using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.Models
{

    public class Payment : EntityData
    {
        public string PaymentPlatform { get; set; }
        public string PlatformPaymentID { get; set; }
        public string Status { get; set; }
        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerPhone { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public decimal Fees { get; set; }
        public string AuthenticationCode { get; set; }
        public string ProviderUserID { get; set; }
        public string IncidentGUID { get; set; }

        public Dictionary<string, string> GetCustomerPushObject()
        {
            //Prepare Notification Title & Message from IncidentStatusRequest Details
            string notificationTitle = "NONE";
            string notificationMessage = "NONE";

            //Setup Push Message
            Dictionary<string, string> pushData = new Dictionary<string, string>();
            if (notificationMessage != "NONE")
                pushData.Add("message", notificationMessage);
            if (notificationTitle != "NONE")
                pushData.Add("title", notificationTitle);
            if (this.Status == "PAYMENT-FAIL" || this.Status == "PAYMENT-FAIL")
            {
                pushData.Add("status", this.Status);
            }
            else
            {
                pushData.Add("status", "PAYMENT-SUCCESS");
            }

            pushData.Add("incidentGUID", this.IncidentGUID.ToString());

            return pushData;
        }

    }
}