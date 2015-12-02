using stranddService.Models;

namespace stranddService.DataObjects
{
    public class IncidentRequest
    {
        public string VehicleGUID { get; set; }
        public string JobCode { get; set; }
        public string AdditionalDetails { get; set; }
        public IncidentLocation Location { get; set; }
        public decimal ServiceFee { get; set; }
    }
    public class IncidentRequestResponse
    {
        public string IncidentGUID { get; set; }
    }
}