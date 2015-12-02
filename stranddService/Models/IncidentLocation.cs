using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.Models
{
    public class IncidentLocation
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string RGDisplay { get; set; }
        public string Landmark { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
    }
}