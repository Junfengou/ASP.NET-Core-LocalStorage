using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.Models
{
    public class CityDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int NumberOfPointOfInterest
        {
            get
            {
                return PointOfInterest.Count;
            }
        }

        public ICollection<PointOfInterestDto> PointOfInterest { get; set; } = new List<PointOfInterestDto>(); // good idea to initialize it to empty in the constructor
    }
}
