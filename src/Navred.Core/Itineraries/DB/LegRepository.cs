using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Navred.Core.Configuration;
using Navred.Core.Cultures;
using Navred.Core.Estimation;
using Navred.Core.Extensions;
using Navred.Core.Places;
using Navred.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Navred.Core.Itineraries.DB
{
    public class LegRepository : ILegRepository
    {
        private const int MaxDepth = 3;

        private readonly IAmazonDynamoDB db;
        private readonly ICultureProvider cultureProvider;
        private readonly IPlacesManager placesManager;
        private readonly ITimeEstimator estimator;
        private readonly Settings settings;
        private readonly object locker;

        public LegRepository(
            IAmazonDynamoDB db,
            ICultureProvider cultureProvider,
            IPlacesManager placesManager,
            ITimeEstimator estimator,
            Settings settings)
        {
            this.db = db;
            this.cultureProvider = cultureProvider;
            this.placesManager = placesManager;
            this.estimator = estimator;
            this.settings = settings;
            this.locker = new object();
        }

        public async Task<IEnumerable<Leg>> GetLegsAsync(Place from, Place to, TimeWindow window)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(from, to, window);

            from = this.placesManager.GetPlace(from);
            to = this.placesManager.GetPlace(to);
            var queried = new HashSet<string> { from.GetId(), to.GetId() };
            var threshold = await this.estimator.EstimateArrivalTimeAsync(
                from, to, window.To.DateTime, Mode.Bus);
            var dbLegs = await this.GetLegsRecursiveAsync(
                from, to, window, queried, threshold, MaxDepth, 0);
            var legs = new List<Leg>();

            foreach (var dbl in dbLegs)
            {
                legs.AddRange(dbl.Tos.Select(dl => new Leg(
                    dl.FromId,
                    dl.ToId,
                    dl.UtcDeparture,
                    dl.UtcArrival,
                    dl.Carrier,
                    dl.Mode,
                    dl.Info,
                    dl.Price,
                    dl.FromSpecific,
                    dl.ToSpecific,
                    dl.DepartureEstimated,
                    dl.ArrivalEstimated,
                    dl.PriceEstimated)));
            }

            return legs;
        }

        public async Task UpdateLegsAsync(IEnumerable<Leg> legs)
        {
            legs.ThrowIfNull("Legs empty.");

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

                await new Web().WithBackoffAsync(
                    async () => await this.db.UpdateItemAsync(request));
            }
        }

        public async Task DeleteAllLegsAsync()
        {
            var request = new ScanRequest
            {
                TableName = this.settings.ItinerariesTable,
                ExclusiveStartKey = null
            };
            var response = await this.db.ScanAsync(request);

            while (!response.Items.IsEmpty())
            {
                foreach (var batch in response.Items.ToBatches(25))
                {
                    await this.db.BatchWriteItemAsync(new BatchWriteItemRequest
                    {
                        RequestItems = new Dictionary<string, List<WriteRequest>>
                        {
                            {
                                this.settings.ItinerariesTable,
                                batch.Select(i => new WriteRequest
                                {
                                    DeleteRequest = new DeleteRequest
                                    {
                                        Key = new Dictionary<string, AttributeValue>
                                        {
                                            { "From", i["From"] },
                                            { "UtcTimestamp", i["UtcTimestamp"] }
                                        }
                                    }
                                }).ToList()
                            }
                        }
                    });
                }

                request.ExclusiveStartKey = response.LastEvaluatedKey;
                response = await this.db.ScanAsync(request);
            }
        }

        private IEnumerable<DBLeg> GetDBLegs(IEnumerable<Leg> legs)
        {
            var fromGroups = legs.GroupBy(i => i.From.GetId());
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

        private async Task<IEnumerable<DBLeg>> GetLegsRecursiveAsync(
            Place from,
            Place to,
            TimeWindow window,
            ICollection<string> queried,
            DateTime threshold,
            int maxDepth,
            int currentDepth)
        {
            if (window.To.DateTime >= threshold || currentDepth >= maxDepth)
            {
                return new List<DBLeg>();
            }

            var legs = (await this.GetLegs(from, window)).ToList();
            var toVertices = legs
                .SelectMany(l => l.Tos)
                .Select(t => new VertexWindow
                {
                    Vertex = t.ToId,
                    Window = new TimeWindow(
                        t.UtcArrival.ToUtcDateTimeTz(),
                        t.UtcArrival.ToUtcDateTimeTz() + TimeSpan.FromHours(5))
                })
                .ToList();

            lock (this.locker)
            {
                queried.AddRange(this.DoHeuristicsTrim(from, to, toVertices));
            }

            await toVertices.RunBatchesAsync(10, async (v) =>
            {
                lock (this.locker)
                {
                    if (queried.Contains(v.Vertex))
                    {
                        return;
                    }

                    queried.Add(v.Vertex);
                }

                var nextLegs = await this.GetLegsRecursiveAsync(
                    v.Vertex, to, v.Window, queried, threshold, maxDepth, currentDepth + 1);

                lock (this.locker)
                {
                    legs.AddRange(nextLegs);
                }
            });

            return legs;
        }

        private async Task<IEnumerable<DBLeg>> GetLegs(Place from, TimeWindow window)
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
                { ":v_source", new AttributeValue { S = from.GetId() } },
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

        private string GetUpdateExp(DBLeg i)
        {
            var equalities = i.Tos.Select((l, index) =>
            {
                var value = this.FormatLegId(l, index);
                var result = $"{value} = :{value}";

                return result;
            }).ToList();
            var exp = $"SET {string.Join(", ", equalities)}";

            return exp;
        }

        private Dictionary<string, AttributeValue> GetExpAttributeValues(DBLeg leg)
        {
            var values = new Dictionary<string, AttributeValue>();

            for (int t = 0; t < leg.Tos.Count; t++)
            {
                var to = leg.Tos[t];
                var map = new Dictionary<string, AttributeValue>();
                map[nameof(Leg.FromId)] = new AttributeValue { S = to.From.GetId() };
                map[nameof(Leg.ToId)] = new AttributeValue { S = to.To.GetId() };
                map[nameof(Leg.Carrier)] = new AttributeValue { S = to.Carrier };
                map[nameof(Leg.Mode)] = new AttributeValue { S = to.Mode.ToString() };
                map[nameof(Leg.UtcArrival)] = new AttributeValue { S = to.UtcArrival.ToString() };
                map[nameof(Leg.UtcDeparture)] = new AttributeValue { S = to.UtcDeparture.ToString() };
                map[nameof(Leg.Duration)] = new AttributeValue { S = to.Duration.ToString() };
                map[nameof(Leg.ArrivalEstimated)] = new AttributeValue { BOOL = to.ArrivalEstimated };
                map[nameof(Leg.PriceEstimated)] = new AttributeValue { BOOL = to.PriceEstimated };

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

                values[$":{this.FormatLegId(to, t)}"] = new AttributeValue { M = map };
            }

            return values;
        }

        private string FormatLegId(Leg leg, int index)
        {
            var id = leg.GetUniqueId();
            var latinizedId = this.cultureProvider.Latinize(id);
            var result = Hashing.ComputeSha256Hash(latinizedId);
            var firstLetter = result.First(c => char.IsLetter(c));
            result = firstLetter + result.Substring(0, 5) + index;

            return result;
        }

        private ICollection<string> DoHeuristicsTrim(
            Place from, Place to, IEnumerable<VertexWindow> vertexWindows)
        {
            var toIgnore = new List<string>();
            from = this.placesManager.GetPlace(from);
            to = this.placesManager.GetPlace(to);
            var fromToDistance = from.DistanceToInKm(to);

            foreach (var vw in vertexWindows)
            {
                var vwPlace = this.placesManager.GetPlace(vw.Vertex);
                var fromVwDistance = from.DistanceToInKm(vwPlace);
                var vwToDistance = to.DistanceToInKm(vwPlace);
                var totalDistance = fromVwDistance + vwToDistance;
                var slackRatio = 2.0d;

                if (totalDistance > (fromToDistance * slackRatio))
                {
                    toIgnore.Add(vw.Vertex);
                }
            }

            Console.WriteLine(toIgnore.Count);

            return toIgnore;
        }

        private class VertexWindow
        {
            public string Vertex { get; set; }

            public TimeWindow Window { get; set; }
        }
    }
}
