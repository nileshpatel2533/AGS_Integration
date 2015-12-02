using SendGrid;
using stranddService.Helpers;
using stranddService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.DataObjects
{
    public class IncidentPaymentRequest
    {
        public string IncidentGUID { get; set; }
        public string PaymentMethod { get; set; }
        public decimal PaymentAmount { get; set; }

        public Dictionary<string, string> GetCustomerPushObject()
        {
            //Prepare Notification Title & Message from IncidentStatusRequest Details
            string notificationMessage =  "NONE";
            string notificationTitle = "NONE";

            //Setup Push Message
            Dictionary<string, string> pushData = new Dictionary<string, string>();

            if (notificationMessage != "NONE")
                pushData.Add("message", notificationMessage);

            if (notificationTitle != "NONE")
                pushData.Add("title", notificationTitle);

            if (this.PaymentMethod != null)
                pushData.Add("status", this.PaymentMethod);

            pushData.Add("incidentGUID", this.IncidentGUID.ToString());


            return pushData;
        }        
    }
}