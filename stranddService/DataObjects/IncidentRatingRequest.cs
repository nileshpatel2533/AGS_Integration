using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.DataObjects
{
    public class IncidentRatingRequest
    {
        public string IncidentGUID { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }
    }
}