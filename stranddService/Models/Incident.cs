using Microsoft.WindowsAzure.Mobile.Service;
using System;

namespace stranddService.Models
{
    public class Incident : EntityData
    {
        public string ProviderUserID { get; set; }
        public string VehicleGUID { get; set; }
        public string JobCode { get; set; }
        public string LocationObj { get; set; }
        public double CoordinateX { get; set; }
        public double CoordinateY { get; set; }
        public string StatusCode { get; set; }
        public bool StatusCustomerConfirm { get; set; }
        public string ErrorCustomer { get; set; }
        public bool StatusProviderConfirm { get; set; }
        public string ErrorProvider { get; set; }
        public decimal ServiceFee { get; set; }
        public string AdditionalDetails { get; set; }
        public string CustomerComments { get; set; }
        public string StaffNotes { get; set; }
        public int Rating { get; set; }
        public string ConcertoCaseID { get; set; } //From Concerto - 10 digit ID that starts with 25
        public DateTimeOffset? ProviderArrivalTime { get; set; }
    }
}