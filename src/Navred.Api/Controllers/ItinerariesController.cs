using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Navred.Api.Models;
using Navred.Core;
using Navred.Core.Itineraries;
using Navred.Core.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Navred.Api.Controllers
{
    [Route("api/[controller]")]
    public class ItinerariesController : Controller
    {
        private readonly IItineraryFinder finder;
        private readonly ILogger<ItinerariesController> logger;

        public ItinerariesController(
            IItineraryFinder finder, ILogger<ItinerariesController> logger)
        {
            this.finder = finder;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(
            string from, string to, DateTime start, DateTime end)
        {
            try
            {
                var window = new TimeWindow(
                    new DateTimeTz(start, Constants.BulgariaTimeZone), 
                    new DateTimeTz(end, Constants.BulgariaTimeZone));
                var paths = await this.finder.FindItinerariesAsync(from, to, window);
                var itineraries = paths.Select(p => new ItineraryViewModel
                {
                    Legs = p.Path.Select(e => e.Leg),
                    Weight = p.Weight
                }).ToList();

                return Ok(itineraries);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.ToString());

                return StatusCode(500);
            }
        }
    }
}
