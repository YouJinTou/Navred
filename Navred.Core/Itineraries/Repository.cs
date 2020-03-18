using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Navred.Core.Itineraries
{
    public class Repository
    {
        private readonly IAmazonDynamoDB db;

        public Repository(IAmazonDynamoDB db)
        {
            this.db = db;
        }

        public async Task UpdateItinerariesAsync(IEnumerable<Itinerary> itineraries)
        {
            var updateRequests = itineraries.Select(i => new UpdateItemRequest
            {
                TableName = "TN",
                Key = new Dictionary<string, AttributeValue>
                {
                    { "from", new AttributeValue { S = i.From } },
                    { "datestamp", new AttributeValue { N = i.} }
                }
            });

            foreach (var request in updateRequests)
            {
                var response = await this.db.UpdateItemAsync(request);
            }
        }
    }
}
