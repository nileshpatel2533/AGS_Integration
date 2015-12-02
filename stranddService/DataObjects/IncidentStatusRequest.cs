using SendGrid;
using stranddService.Helpers;
using stranddService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.DataObjects
{
    public class IncidentStatusRequest
    {
        public string IncidentGUID { get; set; }
        public string NewStatusCode { get; set; }
        public int ETA { get; set; }
        public bool Delayed { get; set; }
        public decimal ServiceFee { get; set; }

        public IncidentStatusRequest(string incidentGUID)
        {
            IncidentInfo lookupIncident =  new IncidentInfo(incidentGUID);
            this.IncidentGUID = lookupIncident.IncidentGUID;
            this.NewStatusCode = lookupIncident.StatusCode;
            this.ServiceFee = lookupIncident.ServiceFee;

            if (lookupIncident.ProviderArrivalTime != null  && lookupIncident.StatusCode != "ARRIVED")
            {
                TimeSpan etaSpan = (DateTimeOffset)lookupIncident.ProviderArrivalTime - System.DateTime.Now;
                if (etaSpan.TotalMinutes > 1) { this.ETA = Convert.ToInt32(etaSpan.TotalMinutes); }
            }

            if (lookupIncident.StatusCode == "ARRIVED" && lookupIncident.PaymentMethod != null)
            {
                this.NewStatusCode = lookupIncident.PaymentMethod;
            }
            
        }

        public Dictionary<string, string> GetCustomerPushObject()
        {
            //Prepare Notification Title & Message from IncidentStatusRequest Details
            string notificationMessage =  "NONE";
            string notificationTitle = "NONE";

            //Create Time String based upon ETA
            string ETANotificationOutput = (this.ETA).ToString();
            switch (this.NewStatusCode)
            {
                case "CONFIRMED":
                    notificationTitle = "Request Confirmed";
                    notificationMessage = "We are processing your request and searching for a nearby provider.";
                    break;
                case "PROVIDER-FOUND":
                    notificationTitle = "Provider Found";
                    notificationMessage = "Your service provider has been found and dispatched to your location."
                        + ETANotificationOutput + " minutes.";
                    break;
                case "IN-PROGRESS":
                    notificationTitle = "Provider En Route";
                    notificationMessage = "Help is on the way! The estimated time of arrival is in "
                        + ETANotificationOutput + " minutes.";
                    break;
                case "ARRIVED":
                    notificationTitle = "Provider Arrived";
                    notificationMessage = "Your provider has arrived. Please proceed to payment.";
                    break;
                case "COMPLETED":
                    notificationTitle = "Job Completed";
                    notificationMessage = "Thank you for using StrandD. Please rate your service.";
                    break;
                case "DECLINED":
                    notificationTitle = "Request Declined";
                    notificationMessage = "Your service request was declined.";
                    break;
                case "PAYMENT-CASH":
                    notificationTitle = "Cash Payment";
                    notificationMessage = "Your cash payment was received.";
                    break;
                case "PAYMENT-SUCCESS":
                    notificationTitle = "Payment Success";
                    notificationMessage = "Your payment was successful.";
                    break;
                default:
                    if (ETA != 0)
                    {
                        if (!Delayed)
                        {
                            notificationTitle = "On Time";
                            notificationMessage = "Your updated time of arrival is in "
                                + ETANotificationOutput + " minutes.";
                        }
                        else
                        {
                            notificationTitle = "Delayed";
                            notificationMessage = "Your provider has been delayed. Your updated time of arrival is in "
                                + ETANotificationOutput + " minutes.";
                        }
                    }
                    break;
            }

            //Setup Push Message
            Dictionary<string, string> pushData = new Dictionary<string, string>();
            if (notificationMessage != "NONE")
                pushData.Add("message", notificationMessage);
            if (notificationTitle != "NONE")
                pushData.Add("title", notificationTitle);
            if (this.NewStatusCode != null)
                pushData.Add("status", this.NewStatusCode);
            if (this.ETA != 0)
                pushData.Add("eta", this.ETA.ToString());
            if (this.ServiceFee != 0)
                pushData.Add("service fee", this.ServiceFee.ToString());
            pushData.Add("incidentGUID", this.IncidentGUID.ToString());


            return pushData;
        }        
    }
}