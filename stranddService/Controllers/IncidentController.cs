using Exceptions;
using Microsoft.AspNet.SignalR;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Newtonsoft.Json;
using SendGrid;
using stranddService.DataObjects;
using stranddService.Helpers;
using stranddService.Hubs;
using stranddService.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Description;

namespace stranddService.Controllers
{
    public class IncidentController : ApiController
    {
        public ApiServices Services { get; set; }

        [Route("api/incidents/currentstatusrequest/{incidentGUID}")]
        [ResponseType(typeof(IncidentStatusRequest))]
        public async Task<IHttpActionResult> GetMobileCustomerClientIncidentStatusRequest(string incidentGUID)
        {
            Services.Log.Info("Mobile Customer Current Incident [" + incidentGUID + "] Status Request [API]");

            IncidentStatusRequest generatedStatusRequest = new IncidentStatusRequest(incidentGUID);

            //Return Successful Response
            Services.Log.Info("Incident Status Request Generated and Returned [API]");
            return Ok(generatedStatusRequest);
        }

        [Route("api/incidents")]
        [ResponseType(typeof(IncidentInfo))]
        public async Task<IHttpActionResult> GetIncidentInfos()
        {
            Services.Log.Info("Incident Log Requested [API]");
            List<Incident> dbIncidentCollection = new List<Incident>();
            List<IncidentInfo> fullIncidentCollection = new List<IncidentInfo>();

            stranddContext context = new stranddContext();

            //Loading List of Incidents from DB Context
            dbIncidentCollection = await (context.Incidents).ToListAsync<Incident>();
            foreach (Incident dbIncident in dbIncidentCollection) { fullIncidentCollection.Add(new IncidentInfo(dbIncident)); }

            //Return Successful Response
            Services.Log.Info("Full Incident Log Returned [API]");
            return Ok(fullIncidentCollection);
        }


        [Route("api/incidents/active")]
        [ResponseType(typeof(IncidentInfo))]
        public async Task<IHttpActionResult> GetActiveIncidentInfos()
        {
            Services.Log.Info("Active Incident Log Requested [API]");
            List<Incident> dbIncidentQuery = new List<Incident>();
            List<IncidentInfo> activeIncidentCollection = new List<IncidentInfo>();

            stranddContext context = new stranddContext();

            //Loading List of Active Incidents from DB Context
            dbIncidentQuery = await context.Incidents
                .Where(b => b.StatusCode != "CANCELLED" && b.StatusCode != "DECLINED" && b.StatusCode != "COMPLETED" && b.StatusCode != "FAILED")
                .ToListAsync<Incident>();

            foreach (Incident dbIncident in dbIncidentQuery) { activeIncidentCollection.Add(new IncidentInfo(dbIncident)); }

            //Return Successful Response
            Services.Log.Info("Active Incident Log Returned [API]");
            return Ok(activeIncidentCollection);
        }
        [Route("api/incidents/inactive")]
        [ResponseType(typeof(IncidentInfo))]
        public async Task<IHttpActionResult> GetInactiveIncidentInfos(int historyHours)
        {
            Services.Log.Info("Inactive Incident Log Requested [API]");
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
                .Where(b => b.StatusCode == "CANCELLED" || b.StatusCode == "DECLINED" || b.StatusCode == "COMPLETED")
                .Where(x => x.CreatedAt >= floorDate)
                .ToListAsync<Incident>();

            foreach (Incident dbIncident in dbIncidentQuery) { inactiveIncidentCollection.Add(new IncidentInfo(dbIncident)); }

            //Return Successful Response
            Services.Log.Info("Inactive Incident Log Returned [API]");
            return Ok(inactiveIncidentCollection);
        }
        [Route("api/incidents/new")]
        public async Task<HttpResponseMessage> CustomerNewIncident(IncidentRequest incidentRequest)
        {
            Services.Log.Info("New Incident Request [API]");

            // Get the logged-in user.
            var currentUser = this.User as ServiceUser;

            double locationX = 0;
            double locationY = 0;

            //Check Provided Location JObject & Process Accordingly
            if (incidentRequest.Location != null)
            {

                if (incidentRequest.Location.X != -1 && incidentRequest.Location.Y != -1)
                {
                    //Coordinates Flow - X & Y Coordinate Passed
                    locationX = incidentRequest.Location.X;
                    locationY = incidentRequest.Location.Y;
                }
                else
                {
                    //No Coordinates
                    //Consider Geocoding
                }
            }
            else
            {
                //No Location Flow - No Location JObject
            }

            Incident newIncident = new Incident()
            {
                Id = Guid.NewGuid().ToString(),
                JobCode = incidentRequest.JobCode,
                LocationObj = await JsonConvert.SerializeObjectAsync(incidentRequest.Location),
                CoordinateX = locationX,
                CoordinateY = locationY,
                ProviderUserID = currentUser.Id,
                VehicleGUID = incidentRequest.VehicleGUID,
                AdditionalDetails = incidentRequest.AdditionalDetails,
                StatusCode = "SUBMITTED", // Set the Initial Status
                StatusCustomerConfirm = true,
                StatusProviderConfirm = false,
                ServiceFee = (incidentRequest.ServiceFee != 0 || incidentRequest.ServiceFee != null) ? incidentRequest.ServiceFee : 0
            };

            stranddContext context = new stranddContext();
            context.Incidents.Add(newIncident);

            await context.SaveChangesAsync();
            Services.Log.Info("New Incident Created [" + newIncident.Id + "]");

            IncidentRequestResponse returnObject = new IncidentRequestResponse { IncidentGUID = newIncident.Id };
            string responseText = JsonConvert.SerializeObject(returnObject);

            //Notifying Connect WebClients with IncidentInfo Package
            IHubContext hubContext = Services.GetRealtime<IncidentHub>();
            hubContext.Clients.All.saveNewIncidentCustomer(new IncidentInfo(newIncident));
            ProcessProviderOutreach(newIncident, Services);

            Services.Log.Info("Connected Clients Updated");

            SendGridController.SendIncidentSubmissionAdminEmail(newIncident, Services);

            await HistoryEvent.logHistoryEventAsync("INCIDENT_NEW_CUSTOMER", null, newIncident.Id, null, currentUser.Id, null);

            return this.Request.CreateResponse(HttpStatusCode.Created, responseText);
        }
        [Route("api/incidents/updaterating")]
        public async Task<HttpResponseMessage> CustomerUpdateRating(IncidentRatingRequest ratingRequest)
        {
            Services.Log.Info("Incident Rating Update Request [API]");
            string responseText;

            // Get the logged-in user.
            var currentUser = this.User as ServiceUser;

            stranddContext context = new stranddContext();

            //Retrieve Incident
            Incident updateIncident = await (from r in context.Incidents where (r.Id == ratingRequest.IncidentGUID) select r).FirstOrDefaultAsync();

            if (updateIncident != null)
            {
                //Edit Incident
                if (ratingRequest.Rating != 0)
                {
                    updateIncident.Rating = ratingRequest.Rating;
                }

                if (ratingRequest.Comments != null)
                {
                    updateIncident.CustomerComments = ratingRequest.Comments;
                }
            }
            else
            {
                // Return Failed Response
                responseText = "Incident [" + ratingRequest.IncidentGUID + "] is not found in the system";
                Services.Log.Warn(responseText);
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, responseText);
            }

            //Save record
            await context.SaveChangesAsync();

            responseText = "Incident [" + updateIncident.Id + "] Rating Updated";
            Services.Log.Info(responseText);

            //Notifying Connect WebClients with IncidentInfo Package
            IHubContext hubContext = Services.GetRealtime<IncidentHub>();
            hubContext.Clients.All.updateIncidentRatingCustomer(new IncidentInfo(updateIncident));
            Services.Log.Info("Connected Web Clients Updated");

            await HistoryEvent.logHistoryEventAsync("INCIDENT_RATING_CUSTOMER", null, updateIncident.Id, null, currentUser.Id, null);

            return this.Request.CreateResponse(HttpStatusCode.OK, responseText);
        }
        [Route("api/incidents/cancel")]
        public async Task<HttpResponseMessage> CustomerCancel(IncidentStatusRequest statusRequest)
        {
            Services.Log.Info("Incident Cancellation Request [API]");
            string responseText;

            // Get the logged-in user.
            var currentUser = this.User as ServiceUser;

            stranddContext context = new stranddContext();

            //Retrieve Incident
            Incident updateIncident = await (from r in context.Incidents where (r.Id == statusRequest.IncidentGUID) select r).FirstOrDefaultAsync();

            //Find the Incident to Cancel and return Bad Response if not found
            if (updateIncident == null)
            {
                // Return Failed Response
                responseText = "Incident [" + statusRequest.IncidentGUID + "] is not found in the system";
                Services.Log.Warn(responseText);
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, responseText);
            }
            else
            {

                updateIncident.StatusCode = "CANCELLED";
                updateIncident.StatusCustomerConfirm = true;
                updateIncident.StatusProviderConfirm = false;

                //Save record
                await context.SaveChangesAsync();
                responseText = "Incident [" + updateIncident.Id + "] Cancelled by Customer";
                Services.Log.Info(responseText);

                //Notifying Connect WebClients with IncidentInfo Package
                IHubContext hubContext = Services.GetRealtime<IncidentHub>();
                hubContext.Clients.All.updateIncidentStatusCustomerCancel(new IncidentInfo(updateIncident));
                Services.Log.Info("Connected Clients Updated");

                await HistoryEvent.logHistoryEventAsync("INCIDENT_CANCEL_CUSTOMER", null, updateIncident.Id, null, currentUser.Id, null);
                //Return Successful Response
                return this.Request.CreateResponse(HttpStatusCode.OK, responseText);
            }

        }
        [Route("api/incidents/updatestatus")]
        public async Task<HttpResponseMessage> UpdateStatus(IncidentStatusRequest statusRequest)
        {
            Services.Log.Info("Incident Status Update Request [API]");
            string responseText;

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
                responseText = "Incident [" + statusRequest.IncidentGUID + "] is not found in the system";
                Services.Log.Warn(responseText);
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, responseText);
            }

            //Save record
            await context.SaveChangesAsync();
            responseText = "Incident [" + updateIncident.Id + "] Status Updated" + " to Code [" + updateIncident.StatusCode + "]";
            Services.Log.Info(responseText);

            ProcessStatusRequestBehavior(statusRequest, updateIncident, Services);

            //Return Successful Response
            return this.Request.CreateResponse(HttpStatusCode.OK, responseText);
        }
        [Route("api/incidents/updatepayment")]
        public async Task<HttpResponseMessage> UpdatePayment(IncidentPaymentRequest paymentRequest)
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
                    newPayment.Status = "Paid";
                }

                if (paymentRequest.PaymentMethod == "PAYMENT-FAIL")
                {
                    newPayment.PaymentPlatform = "Payment Failure (Admin)";
                    newPayment.Status = "FAILED";
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
                return this.Request.CreateResponse(HttpStatusCode.OK, responseText);
            }
            else
            {
                // Return Failed Response
                responseText = "Incident [" + paymentRequest.IncidentGUID + "] is not found in the system";
                Services.Log.Warn(responseText);
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, responseText);
            }

        }
        [Route("api/incidents/updatedetails")]
        public async Task<HttpResponseMessage> UpdateDetails(IncidentDetailsRequest detailsRequest)
        {
            Services.Log.Info("Incident Details Update Request [API]");
            string responseText;

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
                responseText = "Incident [" + detailsRequest.IncidentGUID + "] is not found in the system";
                Services.Log.Warn(responseText);
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, responseText);
            }

            //Save record
            await context.SaveChangesAsync();
            responseText = "Incident [" + updateIncident.Id + "] Details Updated";
            Services.Log.Info(responseText);

            //Notifying Connect WebClients with IncidentInfo Package
            IHubContext hubContext = Services.GetRealtime<IncidentHub>();
            hubContext.Clients.All.updateIncidentDetailsAdmin(new IncidentInfo(updateIncident));
            Services.Log.Info("Connected Clients Updated");

            //Return Successful Response
            return this.Request.CreateResponse(HttpStatusCode.OK, responseText);
        }

        public static void ProcessProviderOutreach(Incident newIncident, ApiServices Services)
        {
            IHubContext hubContext = Services.GetRealtime<IncidentHub>();

            ProviderJobOffer jobOffer = new ProviderJobOffer()
            {
                JobTag = newIncident.Id,
                ExpirySeconds = 30,
                IncidentInformation = new IncidentInfo(newIncident)
            };

            hubContext.Clients.All.offerMobileProviderClientJob(jobOffer);
            Services.Log.Info("All Mobile Providers Offered Job");
        }

        public static void RevokeProviderJobs(Incident updateIncident, ApiServices Services)
        {
            IHubContext hubContext = Services.GetRealtime<IncidentHub>();

            hubContext.Clients.All.revokeMobileProviderClientJob(updateIncident.Id);
            Services.Log.Info("All Mobile Providers Offered Job");
        }

        public static async Task SaveCostingSlabAsync(IncidentCostingRequest costingRequest, CostingSlab inputSlab, string slabType, ApiServices Services)
        {

            string responseText;

            //Get Tax Rate
            decimal taxZoneRate = System.Convert.ToDecimal(WebConfigurationManager.AppSettings["RZ_DefaultTaxZoneRate"]);

            //Calulate Provider
            decimal calculatedBaseServiceCost =
                (costingRequest.ServiceKilometers > inputSlab.BaseKilometersFloor) ? (inputSlab.BaseCharge + ((costingRequest.ServiceKilometers - inputSlab.BaseKilometersFloor)) * inputSlab.ExtraKilometersCharge) : inputSlab.BaseCharge;
            decimal calculatedSubtotal = calculatedBaseServiceCost + costingRequest.ParkingCosts + costingRequest.TollCosts + costingRequest.OtherCosts - costingRequest.OffsetDiscount;
            decimal calculatedTaxes = (calculatedSubtotal * taxZoneRate) / 100;
            decimal calculatedTotalCost = calculatedSubtotal + calculatedTaxes;

            stranddContext context = new stranddContext();

            //Check for Provider Incident in IncidentCosting
            IncidentCosting updateIncidentCosting = await (from r in context.IncidentCostings where (r.IncidentGUID == costingRequest.IncidentGUID && r.Type == slabType) 
                                                          select r).FirstOrDefaultAsync();

            if (updateIncidentCosting != null)
            {
                updateIncidentCosting.IdentifierGUID = inputSlab.IdentifierGUID;
                updateIncidentCosting.ServiceType = costingRequest.ServiceType;
                updateIncidentCosting.CostingSlabGUID = inputSlab.Id;
                updateIncidentCosting.TaxZoneRate = taxZoneRate;
                updateIncidentCosting.ServiceKilometers = costingRequest.ServiceKilometers;
                updateIncidentCosting.ParkingCosts = costingRequest.ParkingCosts;
                updateIncidentCosting.TollCosts = costingRequest.TollCosts;
                updateIncidentCosting.OtherCosts = costingRequest.OtherCosts;
                updateIncidentCosting.OffsetDiscount = (slabType=="CUSTOMER") ? costingRequest.OffsetDiscount : 0;
                updateIncidentCosting.CalculatedSubtotal = calculatedSubtotal;
                updateIncidentCosting.CalculatedBaseServiceCost = calculatedBaseServiceCost;
                updateIncidentCosting.CalculatedTaxes = calculatedTaxes;
                updateIncidentCosting.CalculatedTotalCost = calculatedTotalCost;

                await context.SaveChangesAsync();

                responseText = "IncidentCostings (" + slabType + ") Successfully Updated";
            }

            else
            {
                IncidentCosting newIncidentCosting = new IncidentCosting
                {
                    Id = Guid.NewGuid().ToString(),
                    IncidentGUID = costingRequest.IncidentGUID,
                    IdentifierGUID = inputSlab.IdentifierGUID,
                    Type = slabType,
                    ServiceType = costingRequest.ServiceType,
                    CostingSlabGUID = inputSlab.Id,
                    TaxZoneRate = taxZoneRate,
                    ServiceKilometers = costingRequest.ServiceKilometers,
                    ParkingCosts = costingRequest.ParkingCosts,
                    TollCosts = costingRequest.TollCosts,
                    OtherCosts = costingRequest.OtherCosts,
                    OffsetDiscount = (slabType == "CUSTOMER") ? costingRequest.OffsetDiscount : 0,
                    CalculatedSubtotal = calculatedSubtotal,
                    CalculatedBaseServiceCost = calculatedBaseServiceCost,
                    CalculatedTaxes = calculatedTaxes,
                    CalculatedTotalCost = calculatedTotalCost

                };
                context.IncidentCostings.Add(newIncidentCosting);
                await context.SaveChangesAsync();

                responseText = "IncidentCostings (" + slabType + ") Successfully Generated";
            }

        }


        public static void ProcessStatusRequestBehavior(IncidentStatusRequest statusRequest, Incident updateIncident, ApiServices Services)
        {
            IHubContext hubContext = Services.GetRealtime<IncidentHub>();

            switch (updateIncident.StatusCode)
            {
                case "PROVER-FOUND":

                    //Notify Particular Connected User through SignalR
                    hubContext.Clients.Group(updateIncident.ProviderUserID).updateMobileClientStatus(statusRequest.GetCustomerPushObject());
                    Services.Log.Info("Mobile Client [" + updateIncident.ProviderUserID + "] Status Update Payload Sent");

                    //Notifying Connect WebClients with IncidentInfo Package            
                    hubContext.Clients.All.updateIncidentStatusAdmin(new IncidentInfo(updateIncident));
                    Services.Log.Info("Connected Clients Updated");

                    RevokeProviderJobs(updateIncident, Services);
                    Services.Log.Info("Provider Jobs Revoked");

                    break;

                case "ARRIVED":
                    
                    //Notify Particular Connected User through SignalR
                    hubContext.Clients.Group(updateIncident.ProviderUserID).updateMobileClientStatus(statusRequest.GetCustomerPushObject());
                    Services.Log.Info("Mobile Client [" + updateIncident.ProviderUserID + "] Status Update Payload Sent");

                    //Notifying Connect WebClients with IncidentInfo Package            
                    hubContext.Clients.All.updateIncidentStatusAdmin(new IncidentInfo(updateIncident));
                    Services.Log.Info("Connected Clients Updated");

                    SendGridController.SendCurrentIncidentInvoiceEmail(updateIncident, Services);

                    break;

                case "COMPLETED":

                    //Notify Particular Connected User through SignalR
                    hubContext.Clients.Group(updateIncident.ProviderUserID).updateMobileClientStatus(statusRequest.GetCustomerPushObject());
                    Services.Log.Info("Mobile Client [" + updateIncident.ProviderUserID + "] Status Update Payload Sent");

                    //Notifying Connect WebClients with IncidentInfo Package            
                    hubContext.Clients.All.updateIncidentStatusAdmin(new IncidentInfo(updateIncident));
                    Services.Log.Info("Connected Clients Updated");
                    break;

                case "DECLINED":

                    //Notify Particular Connected User through SignalR
                    hubContext.Clients.Group(updateIncident.ProviderUserID).updateMobileClientStatus(statusRequest.GetCustomerPushObject());
                    Services.Log.Info("Mobile Client [" + updateIncident.ProviderUserID + "] Status Update Payload Sent");

                    //Notifying Connect WebClients with IncidentInfo Package            
                    hubContext.Clients.All.updateIncidentStatusAdmin(new IncidentInfo(updateIncident));
                    Services.Log.Info("Connected Clients Updated");

                    RevokeProviderJobs(updateIncident, Services);
                    Services.Log.Info("Provider Jobs Revoked");

                    break;

                case "CANCELLED":
                    //Notifying Connect WebClients with IncidentInfo Package            
                    hubContext.Clients.All.updateIncidentStatusCustomerCancel(new IncidentInfo(updateIncident));
                    Services.Log.Info("Connected Clients Updated");

                    RevokeProviderJobs(updateIncident, Services);
                    Services.Log.Info("Provider Jobs Revoked");

                    break;

                default:
                    //Notify Particular Connected User through SignalR
                    hubContext.Clients.Group(updateIncident.ProviderUserID).updateMobileClientStatus(statusRequest.GetCustomerPushObject());
                    Services.Log.Info("Mobile Client [" + updateIncident.ProviderUserID + "] Status Update Payload Sent");

                    //Notifying Connect WebClients with IncidentInfo Package            
                    hubContext.Clients.All.updateIncidentStatusAdmin(new IncidentInfo(updateIncident));
                    Services.Log.Info("Connected Clients Updated");

                    break;
            }
        }


        [HttpGet]
        [Route("api/incidents/exceloutput")]

        public HttpResponseMessage GenerateIncidentExcel()
        {
            var queryStrings = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);

            string timeZoneDisplayString;
            TimeZoneInfo timeZoneRequest;
            DateTimeOffset startTime;
            DateTimeOffset endTime;

            string responseText;
            responseText = "Excel Output Requested -";



            if (queryStrings.ContainsKey("timezone"))
            {
                try
                {
                    timeZoneRequest = TimeZoneInfo.FindSystemTimeZoneById(queryStrings["timezone"]);
                }
                catch (TimeZoneNotFoundException)
                {
                    Services.Log.Warn("Unable to retrieve the requested Time Zone. Reverting to UTC.");
                    timeZoneRequest = TimeZoneInfo.Utc;
                }
                catch (InvalidTimeZoneException)
                {
                    Services.Log.Warn("Unable to retrieve the requested Time Zone. Reverting to UTC.");
                    timeZoneRequest = TimeZoneInfo.Utc;
                }
            }
            else
            {
                Services.Log.Warn("No Time Zone Requested. Reverting to UTC.");
                timeZoneRequest = TimeZoneInfo.Utc;
            }

            if (queryStrings.ContainsKey("starttime"))
            {
                if (!DateTimeOffset.TryParse(queryStrings["starttime"], out startTime))
                {
                    Services.Log.Warn("Unable to parse the requested Start Time [" + queryStrings["starttime"] + "]. Reverting to Min Value.");
                }
            }
            else
            {
                startTime = DateTimeOffset.MinValue;
                Services.Log.Warn("No Start Time Requested. Reverting to Min Value.");
            }

            if (queryStrings.ContainsKey("endtime"))
            {
                if (!DateTimeOffset.TryParse(queryStrings["endtime"], out endTime))
                {
                    endTime = DateTimeOffset.MaxValue;
                    Services.Log.Warn("Unable to parse the requested End Time [" + queryStrings["endtime"] + "]. Reverting to Max Value.");
                }
            }
            else
            {
                endTime = DateTimeOffset.MaxValue;
                Services.Log.Warn("No End Time Requested. Reverting to Max Value.");
            }

            timeZoneDisplayString = "[" + timeZoneRequest.DisplayName.ToString() + "]";
            responseText += " TimeZone " + timeZoneDisplayString;
            responseText += " StartTime [" + startTime.ToString() + "]";
            responseText += " EndTime [" + endTime.ToString() + "]";
            responseText += " [API]";

            Services.Log.Info(responseText);

            List<Incident> dbIncidentCollection = new List<Incident>();
            List<IncidentExcelData> fullIncidentCollection = new List<IncidentExcelData>();


            stranddContext context = new stranddContext();


            //     List<Incident> objincident = context.Incidents.ToList();

            //Loading List of Incidents from DB Context
            dbIncidentCollection = context.Incidents
                .Where(a => a.CreatedAt >= startTime)
               .Where(a => a.CreatedAt <= endTime)
                .ToList();

            foreach (Incident dbIncident in dbIncidentCollection) { fullIncidentCollection.Add(new IncidentExcelData(dbIncident)); }

            string strData = string.Empty;
            strData = "IncidentGUID,TimeStamp,IncidentStatus,ArrivalTime,ConcertoCaseID,JobCode,ConfirmedAdminName,CustomerName,CustomerPhone,VehicleRegistration,VehicleDescription,StaffNotes,ServiceFee,CustomerComments,CustomerRating,PaymentStatus,PaymentAmount,PaymentPlatform";

            DataTable table = ExcelHelper.ConvertListToDataTable(fullIncidentCollection);
            ExcelHelper objexcel = new ExcelHelper();

            return objexcel.GetExcel(table, strData, "IncidentHistoryReport");

        }
        

        [Route("api/incidents/updatecostings")]
        public async Task<HttpResponseMessage> UpdateCosting(IncidentCostingRequest costingRequest)
        {

            Services.Log.Info("Update Incident Costing Request [API]");
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

            if (customerSlab == null) { responseText = "Customer Policy Costing Slab Not Found"; Services.Log.Warn(responseText); return this.Request.CreateResponse(HttpStatusCode.NotFound, responseText); }
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

            return this.Request.CreateResponse(HttpStatusCode.OK, responseText);
        }
    }
}
