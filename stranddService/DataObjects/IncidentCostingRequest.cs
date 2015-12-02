using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using stranddService.Models;

namespace stranddService.DataObjects
{
    public class IncidentCostingRequest
    {
        public string IncidentGUID { get; set; }
        public string CustomerIdentifierGUID { get; set; }
        public string ProviderIdentifierGUID { get; set; }
        public string ServiceType { get; set; }
        public decimal ServiceKilometers { get; set; }
        public decimal ParkingCosts { get; set; }
        public decimal TollCosts { get; set; }
        public decimal OtherCosts { get; set; }
        public decimal OffsetDiscount { get; set; }

    }
}