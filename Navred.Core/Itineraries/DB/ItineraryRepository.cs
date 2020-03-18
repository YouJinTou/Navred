using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Navred.Core.Cultures;
using Navred.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Navred.Core.Itineraries.DB
{
    public class ItineraryRepository
    {
        private readonly IAmazonDynamoDB db;
        private readonly ICultureProvider cultureProvider;
        private readonly Settings settings;

        public ItineraryRepository(IAmazonDynamoDB db, ICultureProvider cultureProvider, Settings settings)
        {
            this.db = new AmazonDynamoDBClient();
            this.cultureProvider = cultureProvider;
            this.settings = settings;
        }

        public async Task UpdateItinerariesAsync(IEnumerable<Itinerary> itineraries)
        {
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
                var stampGroups = fromGroup.GroupBy(fg => fg.Departure.ToUtcTimestamp());

                foreach (var stampGroup in stampGroups)
                {
                    dbItineraries.Add(new DBItinerary
                    {
                        From = fromGroup.Key,
                        UtcTimestamp = stampGroup.Key,
                        Tos = stampGroup.Select(i => new DBTo
                        {
                            Arrival = i.Arrival,
                            Carrier = i.Carrier,
                            Departure = i.Departure,
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

        private string GetUpdateExp(DBItinerary i)
        {
            var equalities = i.Tos
                .Select(t =>
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
                map[nameof(DBTo.Arrival)] = new AttributeValue { S = to.Arrival.ToString() };
                map[nameof(DBTo.Carrier)] = new AttributeValue { S = to.Carrier };
                map[nameof(DBTo.Departure)] = new AttributeValue { S = to.Departure.ToString() };
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
