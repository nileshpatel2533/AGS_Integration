using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.DataObjects
{
    public class CostingSlabRequest
    {
        public string ID { get; set; }
        public string IdentifierGUID { get; set; }
        public string ServiceType { get; set; }
        public string Status { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public decimal BaseCharge { get; set; }
        public decimal BaseKilometersFloor { get; set; }
        public decimal ExtraKilometersCharge { get; set; }
    }
}