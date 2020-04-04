using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Navred.Api.Models;
using Navred.Core.Places;
using Navred.Core.Tools;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Navred.Api.Controllers
{
    [Route("api/[controller]")]
    public class PlacesController : Controller
    {
        private readonly IPlacesManager placesManager;
        private readonly ILogger<ItinerariesController> logger;

        public PlacesController(
            IPlacesManager placesManager, ILogger<ItinerariesController> logger)
        {
            this.placesManager = placesManager;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(string country, string prefix)
        {
            try
            {
                Validator.ThrowIfNullOrEmpty(country);

                var places = this.placesManager.LoadPlacesFor(country);
                var filteredPlaces = places.Where(
                    p => p.Name.ToLower().StartsWith(prefix.ToLower()))
                    .Select(p => new PlaceViewModel
                    {
                        Id = p.GetId(),
                        Place = p
                    })
                    .ToList();

                return Ok(await Task.FromResult(filteredPlaces));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.ToString());

                return StatusCode(500);
            }
        }
    }
}
