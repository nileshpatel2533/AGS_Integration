using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.Models
{
    public class IncidentCosting : EntityData
    {
        public string IncidentGUID { get; set; }
        public string Type { get; set; }
        public string IdentifierGUID { get; set; }
        public string ServiceType { get; set; }
        public string CostingSlabGUID { get; set; }        
        public decimal TaxZoneRate { get; set; }
        public decimal ServiceKilometers { get; set; }
        public decimal ParkingCosts { get; set; }
        public decimal TollCosts { get; set; }
        public decimal OtherCosts { get; set; }
        public decimal OffsetDiscount { get; set; }
        public decimal CalculatedBaseServiceCost { get; set; }
        public decimal CalculatedSubtotal { get; set; }
        public decimal CalculatedTaxes { get; set; }
        public decimal CalculatedTotalCost { get; set; }
    }

}