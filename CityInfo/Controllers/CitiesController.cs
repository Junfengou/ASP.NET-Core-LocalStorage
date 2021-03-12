using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.Controllers
{
    [ApiController]
    //[Route("api/[controller]")]  ==> Dynamic routing
    [Route("api/cities")]
    public class CitiesController : ControllerBase //ControllerBase class contains basic functionalities that the controller need 
    {
        [HttpGet] // Specify the route to the api with a request/ CRUD
        public IActionResult GetCities()
        {
            return Ok(CitiesDataStore.Current.Cities); 
            // No [not found error] need to be accounted for here because an empty list of cities is valid 
        }

        [HttpGet("{id}")] // Specify the route to the api with a request/ CRUD
        public IActionResult GetCity(int id) // This will automatically pass in the URI
        {
            // Look for this particular city
            var cityToReturn = CitiesDataStore.Current.Cities.FirstOrDefault(item => item.Id == id); // For every items in the CitiesDataStore, we'll check if the item.id matches the passed in id

            if(cityToReturn == null)
            {
                return NotFound(); // status code: 404 error
            }
            return Ok(cityToReturn); // status code: 200 
        }
    }
} 
