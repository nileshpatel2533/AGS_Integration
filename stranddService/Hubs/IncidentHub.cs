using Exceptions;
using Microsoft.AspNet.SignalR;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Newtonsoft.Json;
using SendGrid;
using stranddService.Controllers;
using stranddService.DataObjects;
using stranddService.Helpers;
using stranddService.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Description;

namespace stranddService.Hubs
{
    public class IncidentHub : Hub
    {
        public ApiServices Services { get; set; }

        public static ConcurrentDictionary<string, AccountInfo> ConnectedUserList = new ConcurrentDictionary<string, AccountInfo>();

        public override Task OnConnected()
        {
            try
            {
                
                var currentUserID = ((ServiceUser)Context.User).Id;
                var connectionID = Context.ConnectionId;

                Groups.Add(connectionID, currentUserID);
                ConnectedUserList.TryAdd(connectionID, new AccountInfo(currentUserID));


                Services.Log.Info("Incident Hub SignalR Connection Account [" + currentUserID + "], Client [" + connectionID + "]");                

            }
            catch (Exception ex)
            {

                Services.Log.Error(ex.ToString());

            }            

            return base.OnConnected();
        
        }
        public override Task OnReconnected()
        {
            try
            {

                var currentUserID = ((ServiceUser)Context.User).Id;
                var connectionID = Context.ConnectionId;

                Groups.Add(connectionID, currentUserID);
                ConnectedUserList.TryAdd(connectionID, new AccountInfo(currentUserID));

                Services.Log.Info("Incident Hub SignalR ReConnection Account [" + currentUserID + "], Client [" + connectionID + "]");

            }
            catch (Exception ex)
            {

                Services.Log.Error(ex.ToString());

            }

            return base.OnReconnected();

        }
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var currentUserID = ((ServiceUser)Context.User).Id;
            var connectionID = Context.ConnectionId;

            AccountInfo outAccount;

            if (stopCalled)
            {
                Services.Log.Warn(String.Format("Client {0} explicitly closed the connection.", connectionID));
            }
            else
            {
                Services.Log.Warn(String.Format("Client {0} timed out .", connectionID));
            }
            
            ConnectedUserList.TryRemove(connectionID, out outAccount);

            return base.OnDisconnected(stopCalled);
        }
        public async Task<string> UpdateDetails(IncidentDetailsRequest detailsRequest)
        {
            Services.Log.Info("Incident Details Update Request [Hub]");
            string responseText;

            var currentUserID = ((ServiceUser)Context.User).Id;

            stranddContext context = new stranddContext();

            //Retrieve Incident
            Incident updateIncident = await (from r in context.Incidents where (r.Id == detailsRequest.IncidentGUID) select r).FirstOrDefaultAsync();

            //Find the Incident to Edit and return Bad Response if not found
            if (updateIncident != null)
            {
                //Check for Submitted Status and Update
                if (detailsRequest.Notes != null)
                {
                    updateIncident.StaffNotes = detailsRequest.Notes;
                }

                //Check for Submitted Pricing and Update
                if (detailsRequest.ConcertoCaseID != null)
                {
                    updateIncident.ConcertoCaseID = detailsRequest.ConcertoCaseID;
                }
            }
            else
            {
                // Return Failed Response
                responseText = "Incident not found [" + detailsRequest.IncidentGUID + "] in the system";
                Services.Log.Warn(responseText);
                return responseText;
            }

            //Save record
            await context.SaveChangesAsync();
            responseText = "Incident [" + updateIncident.Id + "] Details Updated";
            Services.Log.Info(responseText);


            //Notifying Connected WebClients with IncidentInfo Package
            Clients.All.updateIncidentDetailsAdmin(new IncidentInfo(updateIncident));
            Services.Log.Info("Connected Web Clients Updated");

            await HistoryEvent.logHistoryEventAsync("INCIDENT_DETAILS_ADMIN", null, updateIncident.Id, currentUserID, null, null);
            //Return Successful Response
            return responseText;

        }

        public async Task<string> ProviderAcceptJob(string jobTag)
        {
            Services.Log.Info("Provider Job Acceptance Request [Hub]");
            string responseText;

            var currentUserID = ((ServiceUser)Context.User).Id;

            stranddContext context = new stranddContext();

            //Retrieve Incident
            Incident updateIncident = await (from r in context.Incidents where (r.Id == jobTag) select r).FirstOrDefaultAsync();

            if (updateIncident != null)
            {
                    updateIncident.StatusCode = "PROVIDER-FOUND";
                    updateIncident.StatusCustomerConfirm = false;
                    updateIncident.StatusProviderConfirm = false;               
            }
            else
            {
                // Return Failed Response
                responseText = "Incident not found [" + jobTag + "] in the system";
                Services.Log.Warn(responseText);
                return responseText;
            }

            //Save record
            await context.SaveChangesAsync();
            Services.Log.Info("Incident [" + updateIncident.Id + "] Job Accepted by Provider [" + currentUserID + "]");

            Dictionary<string, string> pushData = new Dictionary<string, string>();
            pushData.Add("status", "PROVIDER-FOUND");
            pushData.Add("incidentGUID", jobTag);

            //Notify Particular Connected Customer User through SignalR
            Clients.Group(updateIncident.ProviderUserID).updateMobileClientStatus(pushData);
            Services.Log.Info("Mobile Client [" + updateIncident.ProviderUserID + "] Status Update Payload Sent");

            //Notifying Connect WebClients with IncidentInfo Package            
            Clients.All.updateIncidentStatusAdmin(new IncidentInfo(updateIncident));
            Services.Log.Info("Connected Clients Updated");

            IncidentController.RevokeProviderJobs(updateIncident, Services);
            Services.Log.Info("Provider Jobs Revoked");

            await HistoryEvent.logHistoryEventAsync("INCIDENT_STATUS_PROVIDER", updateIncident.StatusCode, updateIncident.Id, null, null, currentUserID);
            //Return Successful Response
            responseText = "Status Updated";
            return responseText;

        }
        public async Task<string> UpdateStatus(IncidentStatusRequest statusRequest)
        {
            Services.Log.Info("Incident Status Update Request [Hub]");
            string responseText;

            var currentUserID = ((ServiceUser)Context.User).Id;

            stranddContext context = new stranddContext();

            //Retrieve Incident
            Incident updateIncident = await (from r in context.Incidents where (r.Id == statusRequest.IncidentGUID) select r).FirstOrDefaultAsync();

            //Find the Incident to Edit and return Bad Response if not found
            if (updateIncident != null)
            {
                //Check for Submitted Status and Update
                if (statusRequest.NewStatusCode != null)
                {
                    updateIncident.StatusCode = statusRequest.NewStatusCode;
                    updateIncident.StatusCustomerConfirm = false;
                    updateIncident.StatusProviderConfirm = false;
                }

                //Check for Submitted Pricing and Update
                if (statusRequest.ServiceFee != 0)
                {
                    updateIncident.ServiceFee = statusRequest.ServiceFee;
                }

                //Check if ETA and Save Arrival Time
                if (statusRequest.ETA != 0)
                {
                    if (statusRequest.NewStatusCode == "ARRIVED")
                    {
                        updateIncident.ProviderArrivalTime = System.DateTime.Now;
                    }
                    else
                    {
                        DateTimeOffset? priorETA = updateIncident.ProviderArrivalTime;
                        updateIncident.ProviderArrivalTime = System.DateTime.Now.AddMinutes(statusRequest.ETA);

                        if (priorETA != null)
                        {
                            DateTimeOffset compareETA = (DateTimeOffset)priorETA;

                            compareETA = compareETA.AddMinutes(Convert.ToInt32(WebConfigurationManager.AppSettings["RZ_DelayMinuteBuffer"]));
                            if (DateTimeOffset.Compare(compareETA, (DateTimeOffset)updateIncident.ProviderArrivalTime) < 0) { statusRequest.Delayed = true; }
                        }
                    }


                }

            }
            else
            {
                // Return Failed Response
                responseText = "Incident not found [" + statusRequest.IncidentGUID + "] in the system";
                Services.Log.Warn(responseText);
                return responseText;
            }

            //Save record
            await context.SaveChangesAsync();
            Services.Log.Info("Incident [" + updateIncident.Id + "] Status Updated" + " to Code: " + updateIncident.StatusCode);

            await HistoryEvent.logHistoryEventAsync("INCIDENT_STATUS_ADMIN", updateIncident.StatusCode, updateIncident.Id, currentUserID, null, null);

            IncidentController.ProcessStatusRequestBehavior(statusRequest, updateIncident, Services);
            
            //Return Successful Response
            responseText = "Status Updated";
            return responseText;
        }

        public async Task<String> UpdatePayment(IncidentPaymentRequest paymentRequest)
        {
            Services.Log.Info("Incident Payment Update Request [API]");
            string responseText;

            stranddContext context = new stranddContext();

            //Retrieve Incident
            Incident updateIncident = await (from r in context.Incidents where (r.Id == paymentRequest.IncidentGUID) select r).FirstOrDefaultAsync();

            //Find the Incident to Edit and return Bad Response if not found
            if (updateIncident != null)
            {
                string guidnew = Guid.NewGuid().ToString();
                //Check for Submitted Payment Amount             
                Payment newPayment = new Payment
                {
                    Id = guidnew,
                    PaymentPlatform = paymentRequest.PaymentMethod,
                    PlatformPaymentID = guidnew,
                    Amount = paymentRequest.PaymentAmount,
                    Fees = -1,
                    ProviderUserID = IncidentInfo.GetProviderID(paymentRequest.IncidentGUID),
                    Status = "Admin-Entered",
                    BuyerName = "NONE",
                    BuyerEmail = "NONE",
                    BuyerPhone = "NONE",
                    Currency = "INR",
                    AuthenticationCode = "NONE",
                    IncidentGUID = paymentRequest.IncidentGUID
                };

                if (paymentRequest.PaymentMethod == "PAYMENT-CASH")
                {
                    newPayment.PaymentPlatform = "Cash Payment (Admin)";
                    newPayment.Status = "PAYMENT-CASH";
                }

                if (paymentRequest.PaymentMethod == "PAYMENT-FAIL")
                {
                    newPayment.PaymentPlatform = "Payment Failure (Admin)";
                    newPayment.Status = "PAYMENT-FAIL";
                }

                if (paymentRequest.PaymentAmount == 0)
                {
                    newPayment.Amount = -1;
                }

                //Save record
                context.Payments.Add(newPayment);
                await context.SaveChangesAsync();
                responseText = "Incident [" + updateIncident.Id + "] Payment Status Updated";
                Services.Log.Info(responseText);

                //Notify Particular Connected User through SignalR
                IHubContext hubContext = Services.GetRealtime<IncidentHub>();
                hubContext.Clients.Group(updateIncident.ProviderUserID).updateMobileClientStatus(newPayment.GetCustomerPushObject());
                Services.Log.Info("Mobile Client [" + updateIncident.ProviderUserID + "] Status Update Payload Sent");

                //Web Client Notifications
                hubContext.Clients.All.saveNewPayment(newPayment);
                Services.Log.Info("Connected Clients Updated");

                //Return Successful Response
                return responseText;
            }
            else
            {
                // Return Failed Response
                responseText = "Incident [" + paymentRequest.IncidentGUID + "] is not found in the system";
                Services.Log.Warn(responseText);
                return responseText;
            }

        }
        public async Task<IncidentStatusRequest> GetMobileCustomerClientIncidentStatusRequest(string incidentGUID)
        {
            Services.Log.Info("Mobile Customer Current Incident ["+ incidentGUID +"] Status Request [HUB]");

            IncidentStatusRequest generatedStatusRequest = new IncidentStatusRequest(incidentGUID);

            //Return Successful Response
            Services.Log.Info("Incident Status Request Generated and Returned [HUB]");
            return generatedStatusRequest;

        }

        public async Task<string> SendIncidentCurrentInvoice(string incidentGUID)
        {
            Services.Log.Info("Send Current Incident [" + incidentGUID + "] Invoice Initiated [HUB]");
            string responseText;

            //Retrieve Incident
            stranddContext context = new stranddContext();
            Incident returnIncident = context.Incidents.Find(incidentGUID);

            if (returnIncident != null)
            {
                if (SendGridController.SendCurrentIncidentInvoiceEmail(returnIncident, Services) == true) 
                {
                    //Return Successful Response
                    responseText = "Sent Current Incident [" + incidentGUID + "] Invoice [HUB]";
                    Services.Log.Info(responseText);
                    return responseText; 
                }
                else 
                {
                    //Return Successful Response
                    responseText = "No Payment Due for Current Incident [" + incidentGUID + "] Invoice [HUB]";
                    Services.Log.Info(responseText);
                    return responseText;  
                } 
            }

            else
            {
                // Return Failed Response
                responseText = "Incident not found [" + incidentGUID + "] in the system";
                Services.Log.Warn(responseText);
                return responseText;
            }           

        }

        public async Task ConfirmMobileCustomerClientIncidentStatusUpdate(string incidentGUID)
        {
            Services.Log.Info("Mobile Customer Status Receipt Confirmation [" + incidentGUID + "] [HUB]");

            stranddContext context = new stranddContext();

            //Retrieve Incident
            Incident updateIncident = await (from r in context.Incidents where (r.Id == incidentGUID) select r).FirstOrDefaultAsync();

            //Find the Incident to Confirm
            if (updateIncident != null) 
            { 
                updateIncident.StatusCustomerConfirm = true; 
                await context.SaveChangesAsync();
                Services.Log.Info("Customer Incident Status Update Confirmed [HUB]");
            }
            else
            {
                string responseText = "Incident not found [" + incidentGUID + "] in the system";
                Services.Log.Warn(responseText);
            }         
         }

        public async Task MobileProviderClientJobAcceptanceTrigger(bool acceptanceStatus)
        {
            Services.Log.Info("Mobile Provider Client Job Acceptance Trigger [HUB]");

            stranddContext context = new stranddContext();

            /*
            //Retrieve Incident
            Incident updateIncident = await (from r in context.Incidents where (r.Id == incidentGUID) select r).FirstOrDefaultAsync();

            //Find the Incident to Confirm
            if (updateIncident != null)
            {
                updateIncident.StatusCustomerConfirm = true;
                await context.SaveChangesAsync();
                Services.Log.Info("Customer Incident Status Update Confirmed [HUB]");
            }
            else
            {
                string responseText = "Incident not found [" + incidentGUID + "] in the system";
                Services.Log.Warn(responseText);
            }
             */

        }
        public async Task<IEnumerable<IncidentInfo>> GetActiveIncidents()
        {        
            Services.Log.Info("Active Incident Log Requested [HUB]");
            List<Incident> dbIncidentQuery = new List<Incident>();
            List<IncidentInfo> activeIncidentCollection = new List<IncidentInfo>();

            stranddContext context = new stranddContext();

            //Loading List of Active Incidents from DB Context
            dbIncidentQuery = await context.Incidents
                .Where(b => b.StatusCode != "CANCELLED" && b.StatusCode != "DECLINED" && b.StatusCode != "COMPLETED" && b.StatusCode != "FAILED")
                .ToListAsync<Incident>();

            foreach (Incident dbIncident in dbIncidentQuery) { activeIncidentCollection.Add(new IncidentInfo(dbIncident)); }

            //Return Successful Response
            Services.Log.Info("Active Incident Log Returned [HUB]");
            return activeIncidentCollection;
        }
        public async Task<IEnumerable<IncidentInfo>> GetInactiveIncidents(int historyHours)
        {
            Services.Log.Info("Inactive Incident Log Requested [HUB]");
            List<Incident> dbIncidentQuery = new List<Incident>();
            List<IncidentInfo> inactiveIncidentCollection = new List<IncidentInfo>();

            stranddContext context = new stranddContext();
            DateTime floorDate;

            if (historyHours == null || historyHours < 0) { floorDate = Convert.ToDateTime("01/01/2001"); }
            else
            { 
                TimeSpan spanBuffer = new TimeSpan(historyHours, 0, 0);
                floorDate = (DateTime.Now).Subtract(spanBuffer); 
            }
            
            //Loading List of Active Incidents from DB Context
            dbIncidentQuery = await context.Incidents
                .Where(b => b.StatusCode == "CANCELLED" || b.StatusCode == "DECLINED" || b.StatusCode == "COMPLETED" || b.StatusCode == "FAILED")
                .Where(x => x.CreatedAt >= floorDate)
                .ToListAsync<Incident>();

            foreach (Incident dbIncident in dbIncidentQuery) { inactiveIncidentCollection.Add(new IncidentInfo(dbIncident)); }

            //Return Successful Response
            Services.Log.Info("Inactive Incident Log Returned [HUB]");
            return inactiveIncidentCollection;
        }



        public async Task<string> UpdateCosting(IncidentCostingRequest costingRequest)
        {

            Services.Log.Info("Update Incident Costing Request [HUB]");
            string responseText;

            IncidentInfo infoOutput;

            //Get Defaults From Configuration and overrride from Request (To be Modified in Expansions)

            string companyGUID = (costingRequest.ProviderIdentifierGUID == null || costingRequest.ProviderIdentifierGUID == "") ? WebConfigurationManager.AppSettings["RZ_DefaultProviderCompany"] : costingRequest.ProviderIdentifierGUID;
            string policyGUID = (costingRequest.CustomerIdentifierGUID == null || costingRequest.CustomerIdentifierGUID == "") ? WebConfigurationManager.AppSettings["RZ_DefaultCustomerPolicy"] : costingRequest.CustomerIdentifierGUID;

            stranddContext context = new stranddContext();

            CostingSlab providerSlab = new CostingSlab();
            providerSlab = await (from r in context.CostingSlabs where (r.IdentifierGUID == companyGUID && r.ServiceType == costingRequest.ServiceType && r.Status == "CURRENT") select r).FirstOrDefaultAsync();
            if (providerSlab == null) { responseText = "Provider Company Costing Slab Not Found"; Services.Log.Warn(responseText); }
            else { IncidentController.SaveCostingSlabAsync(costingRequest, providerSlab, "PROVIDER", Services); }

            CostingSlab customerSlab = new CostingSlab();
            customerSlab = await (from r in context.CostingSlabs where (r.IdentifierGUID == policyGUID && r.ServiceType == costingRequest.ServiceType && r.Status == "CURRENT") select r).FirstOrDefaultAsync();

            if (customerSlab == null) { responseText = "Customer Policy Costing Slab Not Found"; Services.Log.Warn(responseText); }
            else
            {
                await IncidentController.SaveCostingSlabAsync(costingRequest, customerSlab, "CUSTOMER", Services);
                responseText = "Incident Costings Request Processed";

                infoOutput = new IncidentInfo(costingRequest.IncidentGUID);

                //Web Client Notifications
                IHubContext hubContext = Services.GetRealtime<IncidentHub>();
                hubContext.Clients.All.updateIncidentCostingAdmin(infoOutput);
                Services.Log.Info("Connected Clients Generated");
            }

            return responseText;
        }

        public async Task<string> AssignIncidentOperator(OperatorAssignmentRequest assignmentRequest)
        {

            Services.Log.Info("Update Incident [" + assignmentRequest.IncidentGUID +"] Operator Assignment Request [HUB]");

            await HistoryEvent.logHistoryEventAsync("INCIDENT_STATUS_ADMIN", "OPERATOR-ASSIGNED", assignmentRequest.IncidentGUID, assignmentRequest.OperatorUserProviderID, null, null);

            //Notifying Connected WebClients with IncidentInfo Package
            Clients.All.updateIncidentOperatorAdmin(new IncidentInfo(assignmentRequest.IncidentGUID));
            Services.Log.Info("Connected Web Clients Updated");

            string responseText = "Incident [" + assignmentRequest.IncidentGUID +"] Reassigned to [" + assignmentRequest.OperatorUserProviderID + "]";
            Services.Log.Info(responseText);
            return responseText;
        }

        public async Task<ConcurrentDictionary<string, AccountInfo>> GetConnectedUserList()
        {
            Services.Log.Info("Get Connection Mapping Request [HUB]");
            return ConnectedUserList;
        }



    }
}