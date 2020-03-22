using HtmlAgilityPack;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Places;
using Navred.Core.Tools;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Navred.Core.Piecing
{
    public class TimeEstimator : ITimeEstimator
    {
        private const double RoadCurvatureRate = 1.2;
        private const double BusAverageKmPerHour = 80d;
        private const int BusSlackInMinutes = 30;

        private readonly IHttpClientFactory httpClientFactory;
        private readonly IDictionary<string, double> distancesCache;

        public TimeEstimator(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
            this.distancesCache = new Dictionary<string, double>();
        }

        public async Task<DateTime> EstimateArrivalTimeAsync(
            IPlace from, IPlace to, DateTime departure, Mode mode)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(from, to);

            try
            {
                return await this.EstimateWithCrawlAsync(from, to, departure);
            }
            catch (Exception ex)
            {
                return this.EstimateManually(from, to, departure, mode);
            }
        }

        private async Task<DateTime> EstimateWithCrawlAsync(
            IPlace from, IPlace to, DateTime departure)
        {
            var fromId = await this.GetPlaceIdAsync(from);
            var toId = await this.GetPlaceIdAsync(to);
            var url = $"http://bg.toponavi.com/{fromId}-{toId}";
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var distance = doc.DocumentNode.SelectSingleNode("//span[@id='way']").InnerText;
            var x = doc.DocumentNode.SelectNodes("//div[@id='tn_slider']//li");
            distance = string.IsNullOrWhiteSpace(distance) ?
                doc.DocumentNode.SelectSingleNode("//").InnerText :
                distance;

            return new DateTime();
        }

        private async Task<string> GetPlaceIdAsync(IPlace place)
        {
            var url = $"http://bg.toponavi.com/searchcity1/{place.Name}";
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var ps = doc.DocumentNode.SelectNodes("//p");
            Func<string, string> regex = 
                (v) =>  Regex.Match(v, "\\),\\s+?\"(.*)\"\\)").Groups[1].Value;
            var ex = new KeyNotFoundException($"Couldn't get place ID for {place}.");

            if (ps.IsNullOrEmpty())
            {
                throw ex;
            }

            if (ps.ContainsOne())
            {
                var onClickValue = ps[0].GetAttributeValue("onclick", null);
                var id = regex(onClickValue);

                return id;
            }

            var match = default(HtmlNode);

            foreach (var p in ps)
            {
                if (place.RegionCode.SubstringMatchesPartially(p.InnerText))
                {
                    match = p;
                }
            }

            if (match == null)
            {
                throw ex;
            }

            var matchId = regex(match.GetAttributeValue("onclick", null));

            return matchId;
        }

        private DateTime EstimateManually(IPlace from, IPlace to, DateTime departure, Mode mode)
        {
            var distance = from.DistanceToInKm(to);
            double hours;

            switch (mode)
            {
                case Mode.Bus:
                    hours = distance / BusAverageKmPerHour;

                    break;
                default:
                    throw new NotImplementedException();
            }

            var variableDuration =
                (int)(((int)Math.Ceiling(hours * 60)) * RoadCurvatureRate) + BusSlackInMinutes;
            var duration = new TimeSpan(0, variableDuration, 0);
            var arrival = departure + duration;

            return arrival;
        }
    }
}
