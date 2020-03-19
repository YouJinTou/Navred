using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Navred.Core.Configuration;
using Navred.Core.Cultures;
using Navred.Core.Extensions;
using Navred.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Navred.Core.Itineraries.DB
{
    public class ItineraryRepository : IItineraryRepository
    {
        private readonly IAmazonDynamoDB db;
        private readonly ICultureProvider cultureProvider;
        private readonly Settings settings;

        public ItineraryRepository(
            IAmazonDynamoDB db, ICultureProvider cultureProvider, Settings settings)
        {
            this.db = db;
            this.cultureProvider = cultureProvider;
            this.settings = settings;
        }

        public async Task<IEnumerable<Itinerary>> GetItinerariesAsync(
            string from, string to, TimeWindow window)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(from, to);

            var normalizedFrom = this.cultureProvider.NormalizePlaceName(from);
            var dbItineraries = await this.GetItinerariesRecursiveAsync(from, window);
            var itineraries = new List<Itinerary>();

            foreach (var dbi in dbItineraries)
            {
                foreach (var dbTo in dbi.Tos)
                {
                    var itinerary = new Itinerary(dbTo.Carrier, dbTo.Price);

                    itinerary.AddStop(new Stop(dbi.From, dbi.UtcTimestamp.ToUtcDateTime()));

                    itinerary.AddStop(new Stop(dbTo.To, dbTo.UtcArrival));

                    itineraries.Add(itinerary);
                }
            }

            return itineraries;
        }

        public async Task UpdateItinerariesAsync(IEnumerable<Itinerary> itineraries)
        {
            Validator.ThrowIfNull(itineraries);

            var dbItineraries = this.GetDBItineraries(itineraries);

            foreach (var dbi in dbItineraries)
            {
                var request = new UpdateItemRequest();
                request.TableName = this.settings.ItinerariesTable;
                request.Key = new Dictionary<string, AttributeValue>
                {
                    { "From", new AttributeValue { S = dbi.From } },
                    { "UtcTimestamp", new AttributeValue { N = dbi.UtcTimestamp.ToString() } }
                };
                request.UpdateExpression = this.GetUpdateExp(dbi);
                request.ExpressionAttributeValues = this.GetExpAttributeValues(dbi);
                var response = await this.db.UpdateItemAsync(request);
            }
        }

        private IEnumerable<DBItinerary> GetDBItineraries(IEnumerable<Itinerary> itineraries)
        {
            var fromGroups = itineraries.GroupBy(i => i.From);
            var dbItineraries = new List<DBItinerary>();

            foreach (var fromGroup in fromGroups)
            {
                var stampGroups = fromGroup.GroupBy(fg => fg.UtcDeparture.ToUtcTimestamp());

                foreach (var stampGroup in stampGroups)
                {
                    dbItineraries.Add(new DBItinerary
                    {
                        From = fromGroup.Key,
                        UtcTimestamp = stampGroup.Key,
                        Tos = stampGroup.Select(i => new DBTo
                        {
                            UtcArrival = i.UtcArrival,
                            Carrier = i.Carrier,
                            UtcDeparture = i.UtcDeparture,
                            Duration = i.Duration,
                            OnDays = i.OnDays,
                            Price = i.Price,
                            To = i.To
                        }).ToList()
                    });
                }
            }

            return dbItineraries;
        }

        private async Task<IEnumerable<DBItinerary>> GetEdges(string from, TimeWindow window)
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
            var itineraries = new List<DBItinerary>();
            var dbItineraryType = typeof(DBItinerary);
            var dbToType = typeof(DBTo);

            foreach (var item in response.Items)
            {
                var itinerary = (DBItinerary)item.ConvertTo(dbItineraryType);
                itinerary.Tos = item
                    .Where(kvp => kvp.Value.IsMSet)
                    .Select(kvp => kvp.ConvertTo(dbToType))
                    .Cast<DBTo>()
                    .ToList();

                itineraries.Add(itinerary);
            }

            return itineraries;
        }

        private async Task<IEnumerable<DBItinerary>> GetItinerariesRecursiveAsync(
            string from, TimeWindow window)
        {
            var itineraries = (await this.GetEdges(from, window)).ToList();
            var toVertices = itineraries.SelectMany(i => i.Tos).Select(t => new
            {
                Vertex = t.To,
                Window = new TimeWindow(window.LocalTo, window.LocalTo + t.Duration)
            }).ToList();

            foreach (var v in toVertices)
            {
                var nextItineraries = await this.GetItinerariesRecursiveAsync(v.Vertex, v.Window);

                itineraries.AddRange(nextItineraries);
            }

            return itineraries;
        }

        private string GetUpdateExp(DBItinerary i)
        {
            var equalities = i.Tos.Select(t =>
            {
                var id = t.GetUniqueId();
                var latinizedId = this.cultureProvider.Latinize(id);
                var left = latinizedId.Replace(" ", string.Empty);
                var result = $"{left} = {this.GetAttributeValueKey(t)}";

                return result;
            })
            .ToList();
            var exp = $"SET {string.Join(", ", equalities)}";

            return exp;
        }

        private Dictionary<string, AttributeValue> GetExpAttributeValues(DBItinerary itinerary)
        {
            var values = new Dictionary<string, AttributeValue>();

            foreach (var to in itinerary.Tos)
            {
                var map = new Dictionary<string, AttributeValue>();
                map[nameof(DBTo.UtcArrival)] = new AttributeValue { S = to.UtcArrival.ToString() };
                map[nameof(DBTo.Carrier)] = new AttributeValue { S = to.Carrier };
                map[nameof(DBTo.UtcDeparture)] = new AttributeValue { S = to.UtcDeparture.ToString() };
                map[nameof(DBTo.Duration)] = new AttributeValue { S = to.Duration.ToString() };
                map[nameof(DBTo.To)] = new AttributeValue { S = to.To };

                if (to.OnDays.HasValue)
                {
                    map[nameof(DBTo.OnDays)] = new AttributeValue
                    {
                        N = ((long)to.OnDays).ToString()
                    };
                }

                if (to.Price.HasValue)
                {
                    map[nameof(DBTo.Price)] = new AttributeValue { N = to.Price.Value.ToString() };
                }

                values[this.GetAttributeValueKey(to)] = new AttributeValue { M = map };
            }

            return values;
        }

        private string GetAttributeValueKey(DBTo to)
        {
            var id = to.GetUniqueId();
            var latinizedId = this.cultureProvider.Latinize(id);
            var result = $":{latinizedId.Replace(" ", "").ToLower()}";

            return result;
        }
    }
}
