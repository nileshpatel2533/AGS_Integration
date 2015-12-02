using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.DataObjects
{
    public class VehicleRequest
    {
        public string VehicleGUID { get; set; } // For removal and perhaps later editting - null for new Vehicle submissions
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public string RegistrationNumber { get; set; }
    }
    public class VehicleRequestResponse
    {
        public string VehicleGUID { get; set; }
    }
}