using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Navred.Core.Configuration;
using Navred.Core.Cultures;
using Navred.Core.Extensions;
using Navred.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Navred.Core.Itineraries.DB
{
    public class LegRepository : ILegRepository
    {
        private readonly IAmazonDynamoDB db;
        private readonly ICultureProvider cultureProvider;
        private readonly Settings settings;

        public LegRepository(
            IAmazonDynamoDB db, ICultureProvider cultureProvider, Settings settings)
        {
            this.db = db;
            this.cultureProvider = cultureProvider;
            this.settings = settings;
        }

        public async Task<IEnumerable<Leg>> GetLegsAsync(string from, string to, TimeWindow window)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(from, to, window);

            var queried = new HashSet<string> { from, to };
            var dbLegs = await this.GetLegsRecursiveAsync(from, to, window, queried);
            var legs = new List<Leg>();

            foreach (var dbl in dbLegs)
            {
                legs.AddRange(dbl.Tos.Select(dl => new Leg(
                    dl.From, 
                    dl.To, 
                    dl.UtcDeparture, 
                    dl.UtcArrival,
                    dl.Carrier, 
                    dl.Mode,
                    dl.Info,
                    dl.Price, 
                    dl.FromSpecific, 
                    dl.ToSpecific)));
            }

            return legs;
        }

        public async Task UpdateLegsAsync(IEnumerable<Leg> legs)
        {
            Validator.ThrowIfNull(legs);

            var dbLegs = this.GetDBLegs(legs);

            foreach (var dbl in dbLegs)
            {
                var request = new UpdateItemRequest();
                request.TableName = this.settings.ItinerariesTable;
                request.Key = new Dictionary<string, AttributeValue>
                {
                    { "From", new AttributeValue { S = dbl.From } },
                    { "UtcTimestamp", new AttributeValue { N = dbl.UtcTimestamp.ToString() } }
                };
                request.UpdateExpression = this.GetUpdateExp(dbl);
                request.ExpressionAttributeValues = this.GetExpAttributeValues(dbl);
                var response = await this.db.UpdateItemAsync(request);
            }
        }

        private IEnumerable<DBLeg> GetDBLegs(IEnumerable<Leg> legs)
        {
            var fromGroups = legs.GroupBy(i => i.From);
            var dbLegs = new List<DBLeg>();

            foreach (var fromGroup in fromGroups)
            {
                var stampGroups = fromGroup.GroupBy(fg => fg.UtcDeparture.ToUtcTimestamp());

                foreach (var stampGroup in stampGroups)
                {
                    dbLegs.Add(new DBLeg
                    {
                        From = fromGroup.Key,
                        UtcTimestamp = stampGroup.Key,
                        Tos = stampGroup.ToList()
                    });
                }
            }

            return dbLegs;
        }

        private async Task<IEnumerable<DBLeg>> GetLegs(string from, TimeWindow window)
        {
            var request = new QueryRequest();
            request.TableName = this.settings.ItinerariesTable;
            request.KeyConditionExpression =
                "#source = :v_source AND UtcTimestamp BETWEEN :start AND :end";
            request.ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#source", "From" }
            };
            request.ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_source", new AttributeValue { S = from } },
                { ":start", new AttributeValue { N = window.FromUtcTimestamp.ToString() } },
                { ":end", new AttributeValue { N = window.ToUtcTimestmap.ToString() } },
            };
            var response = await this.db.QueryAsync(request);
            var legs = new List<DBLeg>();
            var dbLegType = typeof(DBLeg);
            var legType = typeof(Leg);

            foreach (var item in response.Items)
            {
                var leg = (DBLeg)item.ConvertTo(dbLegType);
                leg.Tos = item
                    .Where(kvp => kvp.Value.IsMSet)
                    .Select(kvp => kvp.ConvertTo(legType))
                    .Cast<Leg>()
                    .ToList();

                legs.Add(leg);
            }

            return legs;
        }

        private async Task<IEnumerable<DBLeg>> GetLegsRecursiveAsync(
            string from, string to, TimeWindow window, ICollection<string> queried)
        {
            var legs = (await this.GetLegs(from, window)).ToList();
            var toVertices = legs
                .SelectMany(l => l.Tos)
                .Select(t => new
                {
                    Vertex = t.To,
                    Window = new TimeWindow(
                        t.UtcArrival.ToUtcDateTimeTz(),
                        t.UtcArrival.ToUtcDateTimeTz() + TimeSpan.FromHours(1))
                })
                .ToList();

            foreach (var v in toVertices)
            {
                if (queried.Contains(v.Vertex))
                {
                    continue;
                }

                queried.Add(v.Vertex);

                var nextItineraries = await this.GetLegsRecursiveAsync(
                    v.Vertex, to, v.Window, queried);

                legs.AddRange(nextItineraries);
            }

            return legs;
        }

        private string GetUpdateExp(DBLeg i)
        {
            var equalities = i.Tos.Select(l =>
            {
                var value = this.FormatLegId(l);
                var result = $"{value} = :{value}";

                return result;
            })
            .ToList();
            var exp = $"SET {string.Join(", ", equalities)}";

            return exp;
        }

        private Dictionary<string, AttributeValue> GetExpAttributeValues(DBLeg leg)
        {
            var values = new Dictionary<string, AttributeValue>();

            foreach (var to in leg.Tos)
            {
                var map = new Dictionary<string, AttributeValue>();
                map[nameof(Leg.From)] = new AttributeValue { S = to.From };
                map[nameof(Leg.To)] = new AttributeValue { S = to.To };
                map[nameof(Leg.Carrier)] = new AttributeValue { S = to.Carrier };
                map[nameof(Leg.Mode)] = new AttributeValue { N = ((int)to.Mode).ToString() };
                map[nameof(Leg.UtcArrival)] = new AttributeValue { S = to.UtcArrival.ToString() };
                map[nameof(Leg.UtcDeparture)] = new AttributeValue { S = to.UtcDeparture.ToString() };
                map[nameof(Leg.Duration)] = new AttributeValue { S = to.Duration.ToString() };
                map[nameof(Leg.ArrivalEstimated)] = new AttributeValue { BOOL = to.ArrivalEstimated };

                if (!string.IsNullOrWhiteSpace(to.FromSpecific))
                {
                    map[nameof(Leg.FromSpecific)] = new AttributeValue { S = to.FromSpecific };
                }

                if (!string.IsNullOrWhiteSpace(to.ToSpecific))
                {
                    map[nameof(Leg.ToSpecific)] = new AttributeValue { S = to.ToSpecific };
                }

                if (to.Price.HasValue)
                {
                    map[nameof(Leg.Price)] = new AttributeValue { N = to.Price.ToString() };
                }

                values[$":{this.FormatLegId(to)}"] = new AttributeValue { M = map };
            }

            return values;
        }

        private string FormatLegId(Leg leg)
        {
            var id = leg.GetUniqueId();
            var latinizedId = this.cultureProvider.Latinize(id);
            var result = Regex.Replace(latinizedId, "[^a-zA-Z]", "_");

            return result;
        }
    }
}
