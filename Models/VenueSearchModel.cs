using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Cuatro.Common;

namespace thunsaker.cuatro.demo.Models {
    public class VenueSearchModel {
        [Display(Name="Query (optional)")]
        public string SearchQuery { get; set; }

        [Display(Name="Search Nearby (optional)")]
        public string Near { get; set; }
        
        [Required]
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [ScaffoldColumn(false)]
        public List<Venue> VenueResults { get; set; }
    }
}