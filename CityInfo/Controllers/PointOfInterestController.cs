using CityInfo.Models;
using CityInfo.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.Controllers
{
    [ApiController]
    [Route("api/cities/{cityId}/pointsofinterest")]
    public class PointOfInterestController : ControllerBase
    {
        private readonly ILogger<PointOfInterestController> _logger;
        private readonly IMailService _mailService;

        public PointOfInterestController(ILogger<PointOfInterestController> logger, IMailService mailService)
        {
            // This is called constructor injection. 
            // All injections are defined in Startup.cs
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Log message
            _mailService = mailService ?? throw new ArgumentException(nameof(mailService)); // Send email
        }

        [HttpGet]
        public IActionResult GetPointOfInterest(int cityId) // Get back a list of cities and its POI
        {
            /*
                Important note: In order to log anything to a file, you need an external party NuGet Package

                Download Nlog.Web.AspNetCore

                On default, Nlog look for nlog.config in the root folder
             */


            try
            {
                //throw new Exception("Example exception"); //If you need to test _logger work
                var city = CitiesDataStore.Current.Cities.FirstOrDefault(item => item.Id == cityId);

                if (city == null)
                {
                    _logger.LogInformation($"City with ID {cityId} wasn't found when accessing POI");
                    return NotFound(); // Since the PointOfInterest is child of City, we need to check if city exist. If there's no city, then there's no POI
                }

                return Ok(city.PointOfInterest);
            }
            catch(Exception ex)
            {
                _logger.LogCritical($"Exception while getting POI for city with ID of {cityId}", ex);
                return StatusCode(500, "A problem happened while handling your request"); // optional
            }
        }

        //---------------------------------------------------------------------------------------------->

        [HttpGet("{id}", Name ="GetPointOfInterest")]
        public IActionResult GetPointOfInterest(int cityId, int id) // Get back a specific POI but needs to check if the city is valid
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(item => item.Id == cityId);

            if (city == null)
            {
                return NotFound(); // Since the PointOfInterest is child of City, we need to check if city exist. If there's no city, then there's no POI
            }



            // find POI
            var pointOfInterest = city.PointOfInterest.FirstOrDefault(POI => POI.Id == id);

            if(pointOfInterest == null)
            {
                return NotFound();
            }

            return Ok(pointOfInterest);
        }

        //---------------------------------------------------------------------------------------------->


        [HttpPost]
        public IActionResult CreatePointOfInterest(int cityId, PointOfInterestForCreationDto pointOfInterest)
        {

            //---------------------------------------------------->
            // This section of code is custom validation to check if the name matches the description
            if (pointOfInterest.Description == pointOfInterest.Name)
            {
                ModelState.AddModelError(
                    "Description",
                    "The provided description should be different from the name"
                 );
            }

            // ModelState must be manually thrown if you add a Model Error
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //---------------------------------------------------->

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(item => item.Id == cityId);

            if (city == null)
            {
                return NotFound(); 
            }

            // Amp through all POI and get the highest value
            var maxPointOfInterestId = CitiesDataStore.Current.Cities.SelectMany(item => item.PointOfInterest).Max(poi => poi.Id); 

            var finalPointOfInterest = new PointOfInterestDto()
            {
                Id = ++maxPointOfInterestId, // increment the existing id by +1
                Name = pointOfInterest.Name, // pointOfInterest.Name = user input
                Description = pointOfInterest.Description // pointOfInterest.Description = user input
            };

            city.PointOfInterest.Add(finalPointOfInterest); // Add the result into List<PointOfInterest> defined in CitiesDto

            // CreatedAtRoute(routeName, routeValues, objectValues)
            return CreatedAtRoute("GetPointOfInterest", new { cityId, id = finalPointOfInterest.Id}, finalPointOfInterest);

            /* To execute an POST request, it requires 201 response code. 
                
             CreatedAtRoute() returns a location header
             */
        }

        //---------------------------------------------------------------------------------------------->
        [HttpPut("{id}")] // Put request update the entire Json list of data
        public IActionResult UpdatePointOfInterest(int cityId, int id,  PointOfInterestForCreationDto pointOfInterest)
        {

            //---------------------------------------------------->
            // This section of code is custom validation to check if the name matches the description
            if (pointOfInterest.Description == pointOfInterest.Name)
            {
                ModelState.AddModelError(
                    "Description",
                    "The provided description should be different from the name"
                 );
            }

            // ModelState must be manually thrown if you add a Model Error
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //---------------------------------------------------->

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(item => item.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            // find POI
            var pointOfInterestFromStore = city.PointOfInterest.FirstOrDefault(POI => POI.Id == id);

            if (pointOfInterest == null)// This will check for both Description and Name input to be validated
            {
                return NotFound();
            }

            pointOfInterestFromStore.Name = pointOfInterest.Name;
            pointOfInterestFromStore.Description = pointOfInterest.Description;

            return NoContent(); // The request passed successfully and there's nothing in return
        }


        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdatePointOfInterest(int cityId, int id, [FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDoc)
        // We use JsonPatchDocument<PointOfInterestForUpdateDto> is because PointOfInterestForUpdateDto class doesn't have ID in it's class 
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(item => item.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            // find POI
            var pointOfInterestFromStore = city.PointOfInterest.FirstOrDefault(POI => POI.Id == id);

            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            var pointOfInterestPatch = new PointOfInterestForUpdateDto()
            {
                Name = pointOfInterestFromStore.Name,
                Description = pointOfInterestFromStore.Description
            };

            patchDoc.ApplyTo(pointOfInterestPatch, ModelState); // The patchDoc document may not exist and it's consumer's fault, we need to to check for that

            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(pointOfInterestPatch.Description == pointOfInterestPatch.Name)
            {
                ModelState.AddModelError(
                    "Description",
                    "The provided description should be different from the name"
                 );
            }

            if (!(TryValidateModel(pointOfInterestPatch)))
            {
                return BadRequest();
            }

            pointOfInterestFromStore.Name = pointOfInterestPatch.Name;
            pointOfInterestFromStore.Description = pointOfInterestPatch.Description;

            return NoContent(); 

        }

        [HttpDelete("{id}")]
        public IActionResult DeletePointOfInterest(int cityId, int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(item => item.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            // find POI
            var pointOfInterestFromStore = city.PointOfInterest.FirstOrDefault(POI => POI.Id == id);

            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            city.PointOfInterest.Remove(pointOfInterestFromStore);

            //Send the mail to Developers to notify them the item in this API has been deleted.
            _mailService.Send("Point of interest deleted", $"Point of Inerest {pointOfInterestFromStore.Name} with id {pointOfInterestFromStore.Id} was deleted");

            return NoContent();
        }
    }
}
