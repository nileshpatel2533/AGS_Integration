using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.DataObjects
{
    public class ExceptionContactRequest
    {
        public string IncidentGUID { get; set; }
        public string ContactPhone { get; set; }
        public string Notes { get; set; }
    }
}