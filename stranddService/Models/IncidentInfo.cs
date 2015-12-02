using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace stranddService.Models
{
    public class IncidentInfo
    {        
        public string IncidentGUID { get; set; }
        public AccountInfo IncidentUserInfo { get; set; }
        public VehicleInfo IncidentVehicleInfo { get; set; }
        public string JobCode { get; set; }
        public IncidentLocation LocationObj { get; set; }
        public string CustomerComments { get; set; }
        public string StaffNotes { get; set; }
        public double CoordinateX { get; set; }
        public double CoordinateY { get; set; }
        public string StatusCode { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentMethod { get; set; }
        public AccountInfo ConfirmedAdminAccount { get; set; }
        public int Rating { get; set; }
        public string AdditionalDetails { get; set; }
        public string ConcertoCaseID { get; set; } //From Concerto - 10 digit ID that starts with 25
        public DateTimeOffset? ProviderArrivalTime { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }


        //Costing variable
        public string ServiceType { get; set; }
        public decimal ServiceKilometers { get; set; }
        public decimal CalculatedBaseServiceCost { get; set; }
        public decimal ParkingCosts { get; set; }
        public decimal TollCosts { get; set; }
        public decimal OtherCosts { get; set; }
        public decimal OffsetDiscount { get; set; }
        public decimal CalculatedSubtotal { get; set; }
        public decimal TaxZoneRate { get; set; }
        public decimal CalculatedTaxes { get; set; }
        public decimal CalculatedTotalCost { get; set; }


        /*
        public bool StatusCustomerConfirm { get; set; }
        public string ErrorCustomer { get; set; }
        public bool StatusProviderConfirm { get; set; }
        public string ErrorProvider { get; set; }
        public string AdditionalDetails { get; set; }
         */

        public IncidentInfo() { }
        public IncidentInfo(String baseIncidentGUID)
            : this(GetIncident(baseIncidentGUID)) { }


        public IncidentInfo(Incident baseIncident)
        {
            //Create UserInfo Object based upon Passed in ProviderUserID [Call to DB Made in Constructor]
            AccountInfo lookupUser = new AccountInfo(baseIncident.ProviderUserID);

            //Create VehicleInfo Object based upon Passed in VehicleGUID [Call to DB Made in Constructor]
            VehicleInfo lookupVehicle = new VehicleInfo(baseIncident.VehicleGUID);

            stranddContext context = new stranddContext();

            IncidentCosting lookupIncidentCosting = context.IncidentCostings
              .Where(u => u.IncidentGUID == baseIncident.Id)
              .Where(v => v.Type == "CUSTOMER")
              .FirstOrDefault();

            //Payment Information
            List<Payment> lookupPaymentList = context.Payments
                .Where(u => u.IncidentGUID == baseIncident.Id)
                .ToList<Payment>();

            string paymentMethodString = null;

            if (lookupPaymentList.Count != 0) 
            { 
                if (lookupPaymentList.Count > 1) { paymentMethodString = "Multiple Payments [" + lookupPaymentList.Count.ToString() + "]"; }
                else { paymentMethodString = lookupPaymentList[0].PaymentPlatform; }
            }

            decimal sumPaymentTotal = lookupPaymentList.Sum(a => a.Amount);

            //Confirmed Admin Information
            HistoryEvent lookupAdminEvent = context.HistoryLog
                .Where(u => u.ReferenceID == baseIncident.Id)
                .OrderByDescending(d => d.CreatedAt)
                .Where(v => v.Code == "INCIDENT_STATUS_ADMIN")
                .Where(x => x.Attribute == "CONFIRMED" || x.Attribute == "OPERATOR-ASSIGNED")
                .FirstOrDefault();

            AccountInfo lookupAdmin;

            if (lookupAdminEvent != null) { lookupAdmin = new AccountInfo(lookupAdminEvent.AdminID); }
            else { lookupAdmin = new AccountInfo(null); }

            this.IncidentGUID = baseIncident.Id;
            this.IncidentUserInfo = lookupUser;
            this.IncidentVehicleInfo = lookupVehicle;
            this.ConfirmedAdminAccount = lookupAdmin;
            this.JobCode = baseIncident.JobCode;
            this.LocationObj = (baseIncident.LocationObj != null) ? JsonConvert.DeserializeObject<IncidentLocation>(baseIncident.LocationObj) : null;
            this.ConcertoCaseID = baseIncident.ConcertoCaseID;
            this.StatusCode = baseIncident.StatusCode;
            this.Rating = baseIncident.Rating;
            this.ServiceFee = baseIncident.ServiceFee;
            this.CoordinateX = baseIncident.CoordinateX;
            this.CoordinateY = baseIncident.CoordinateY;
            this.ProviderArrivalTime = baseIncident.ProviderArrivalTime;
            this.CreatedAt = baseIncident.CreatedAt;
            this.UpdatedAt = baseIncident.UpdatedAt;
            this.CustomerComments = baseIncident.CustomerComments;
            this.StaffNotes = baseIncident.StaffNotes;
            this.PaymentAmount = sumPaymentTotal; //(lookupPayment != null) ? lookupPayment.Amount : 0;
            this.PaymentMethod = paymentMethodString; //(lookupPayment != null) ? lookupPayment.PaymentPlatform : null;
            this.AdditionalDetails = baseIncident.AdditionalDetails;

            //retrive data IncidentCostings
            this.ServiceType = (lookupIncidentCosting != null) ? lookupIncidentCosting.ServiceType   : null;
            this.ServiceKilometers = (lookupIncidentCosting != null) ? lookupIncidentCosting.ServiceKilometers : 0;
            this.CalculatedBaseServiceCost = (lookupIncidentCosting != null) ? lookupIncidentCosting.CalculatedBaseServiceCost : 0;
            this.ParkingCosts = (lookupIncidentCosting != null) ? lookupIncidentCosting.ParkingCosts : 0;
            this.TollCosts = (lookupIncidentCosting != null) ? lookupIncidentCosting.TollCosts : 0;
            this.OtherCosts = (lookupIncidentCosting != null) ? lookupIncidentCosting.OtherCosts : 0;
            this.OffsetDiscount = (lookupIncidentCosting != null) ? lookupIncidentCosting.OffsetDiscount : 0;
            this.CalculatedSubtotal = (lookupIncidentCosting != null) ? lookupIncidentCosting.CalculatedSubtotal : 0;
            this.TaxZoneRate = (lookupIncidentCosting != null) ? lookupIncidentCosting.TaxZoneRate : 0;
            this.CalculatedTaxes = (lookupIncidentCosting != null) ? lookupIncidentCosting.CalculatedTaxes : 0;
            this.CalculatedTotalCost = (lookupIncidentCosting != null) ? lookupIncidentCosting.CalculatedTotalCost : 0;

        }


        public static Incident GetIncident(string incidentGUID)
        {
            stranddContext context = new stranddContext();
            Incident returnIncident = context.Incidents.Find(incidentGUID);
            return returnIncident;
        }

        public static string GetProviderID(string incidentGUID)
        {

            if (string.IsNullOrEmpty(incidentGUID))
            {
                return "NO INCIDENT - NO PROVIDER ID";
            }
            else
            {

                stranddContext context = new stranddContext();
                Incident returnIncident = context.Incidents.Find(incidentGUID);


                if (returnIncident == null)
                {
                    return "INCIDENT NOT FOUND - NO PROVIDER ID";
                }
                else
                {
                    if (string.IsNullOrEmpty(returnIncident.ProviderUserID))
                    {
                        return "NO ASSOCIATED USER";
                    }
                    else
                    {
                        return returnIncident.ProviderUserID;
                    }
                }
            }
        }
    }
}

