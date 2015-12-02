using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.DataObjects
{
    public class IncidentDetailsRequest
    {
        public string IncidentGUID { get; set; }
        public string ConcertoCaseID { get; set; }
        public string Notes { get; set; }
    }
}