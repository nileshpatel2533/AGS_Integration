using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace stranddService.Models
{
    public class IncidentExcelData
    {

        public DateTimeOffset TimeStamp { get; set; }
        public string IncidentGUID { get; set; }
        public string JobCode { get; set; }
        public string ArrivalTime { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string VehicleRegistration { get; set; }
        public string VehicleDescription { get; set; }
        public string ConcertoCaseID { get; set; }
        public string StaffNotes { get; set; }
        public string ServiceFee { get; set; }
        public string CustomerComments { get; set; }
        public string CustomerRating { get; set; }
        public string IncidentStatus { get; set; }
        public string PaymentStatus { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentPlatform { get; set; }
        public string ConfirmedAdminName { get; set; }
        public IncidentExcelData(Incident baseIncident)
        {
            stranddContext context = new stranddContext();

            // Pull Incident data
            this.TimeStamp = (baseIncident.CreatedAt != null) ? (DateTimeOffset) baseIncident.CreatedAt : DateTimeOffset.MinValue;
            this.IncidentGUID = baseIncident.Id;
            this.JobCode = baseIncident.JobCode;
            this.ArrivalTime = System.Convert.ToString(baseIncident.ProviderArrivalTime);
            this.ConcertoCaseID = baseIncident.ConcertoCaseID;
            this.StaffNotes = baseIncident.StaffNotes;
            this.ServiceFee = System.Convert.ToString(baseIncident.ServiceFee);
            this.CustomerComments = baseIncident.CustomerComments;
            this.CustomerRating = baseIncident.Rating.ToString();
            this.IncidentStatus = baseIncident.StatusCode;

            //pull customer Data
            AccountInfo lookupCustomer = new AccountInfo(baseIncident.ProviderUserID);
            this.CustomerName = lookupCustomer.Name;
            this.CustomerPhone = lookupCustomer.Phone;

            //pull Vehicle data
            VehicleInfo lookupVehicle = new VehicleInfo(baseIncident.VehicleGUID);
            this.VehicleDescription = lookupVehicle.Year + " " + lookupVehicle.Color + " " + lookupVehicle.Make + " " + lookupVehicle.Model;
            this.VehicleRegistration = lookupVehicle.RegistrationNumber;

            //Pull Payment data

            Payment lookupPayment = context.Payments
            .Where(u => u.IncidentGUID == baseIncident.Id)
            .FirstOrDefault();
            this.PaymentStatus = (lookupPayment != null) ? lookupPayment.Status : null;
            this.PaymentAmount = (lookupPayment != null) ? lookupPayment.Amount : 0;
            this.PaymentPlatform = (lookupPayment != null) ? lookupPayment.PaymentPlatform : null;

            //Pull Admin from History Event

            HistoryEvent lookupAdminEvent = context.HistoryLog
              .Where(u => u.ReferenceID == baseIncident.Id)
              .Where(v => v.Code == "INCIDENT_STATUS_ADMIN")
              .Where(x => x.Attribute == "CONFIRMED")
              .FirstOrDefault();
            
            AccountInfo lookupAdmin;
            if (lookupAdminEvent != null) { lookupAdmin = new AccountInfo(lookupAdminEvent.AdminID); }
            else { lookupAdmin = new AccountInfo(null); }
            this.ConfirmedAdminName = lookupAdmin.Name;

        }
    }
}