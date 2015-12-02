using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.Models
{
    public class ProviderJobOffer
    {
        public string JobTag { get; set; }
        public int ExpirySeconds { get; set; }
        public IncidentInfo IncidentInformation { get; set; }
    }
}