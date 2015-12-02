using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Newtonsoft.Json;
using stranddService.DataObjects;
using stranddService.Hubs;
using stranddService.Models;
using stranddService.Controllers;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Configuration;
using stranddService.Helpers;
using Newtonsoft.Json.Linq;

namespace stranddService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class InstamojoController : ApiController
    {
        public ApiServices Services { get; set; }

        [Route("api/instamojo/webhook")]
        public async Task<HttpResponseMessage> InstamojoWebHookReceiver(InstamojoWebhookRequest request)
        {
            Services.Log.Info("Instamojo Webhook Request");

            string incidentPropertyName = WebConfigurationManager.AppSettings["RZ_InstamojoIncidentDataField"];

            //var customFieldsObject = JsonConvert.DeserializeObject<InstamojoCustomFields>(request.Custom_Fields);
            //var parsedIncidentGUID = customFieldsObject.Field_25373.value;

            Services.Log.Info(incidentPropertyName);
            Services.Log.Info(request.Custom_Fields);

            JObject customFieldSetObj = JObject.Parse(request.Custom_Fields);
            Services.Log.Info(customFieldSetObj.ToString());
            JObject customFieldInstanceObj = (JObject) customFieldSetObj[incidentPropertyName];
            Services.Log.Info(customFieldInstanceObj.ToString());
            string parsedIncidentGUID = customFieldInstanceObj["value"].ToString();
            Services.Log.Info(parsedIncidentGUID);

            // Set the Payment Platform.
            string nameInstamojo = "Instamojo";

            Payment newPayment = new Payment()
            {
                Id = Guid.NewGuid().ToString(),
                PlatformPaymentID = request.Payment_ID,
                Status = request.Status,
                BuyerName = request.Buyer_Name,
                BuyerEmail = request.Buyer,
                BuyerPhone = request.Buyer_Phone,
                Currency = request.Currency,
                Amount = request.Amount,
                Fees = request.Fees,
                AuthenticationCode = request.MAC,
                PaymentPlatform = nameInstamojo,
                IncidentGUID = parsedIncidentGUID,
                ProviderUserID = IncidentInfo.GetProviderID(parsedIncidentGUID)

            };

            stranddContext context = new stranddContext();
            context.Payments.Add(newPayment);

            await context.SaveChangesAsync();

            //Initiating Hub Context
            Microsoft.AspNet.SignalR.IHubContext hubContext = Services.GetRealtime<IncidentHub>();

            if (newPayment.ProviderUserID == "NO INCIDENT - NO PROVIDER ID") { Services.Log.Warn("New Instamojo Payment Received - No Incident"); }
            else if (newPayment.ProviderUserID == "INCIDENT NOT FOUND - NO PROVIDER ID") { Services.Log.Warn("New Instamojo Payment Received - No Found Incident"); }
            else if (newPayment.ProviderUserID == "NO ASSOCIATED USER") { Services.Log.Warn("New Instamojo Payment Received - No Associated User"); }
            else
            {
                Services.Log.Info("New Instamojo Payment Received from User [" + newPayment.ProviderUserID + "]");                

                //Notify Particular Connected User through SignalR
                hubContext.Clients.Group(newPayment.ProviderUserID).updateMobileClientStatus(newPayment.GetCustomerPushObject());
                Services.Log.Info("Mobile Client [" + newPayment.ProviderUserID + "] Status Update Payload Sent");

                SendGridController.SendIncidentPaymentReceiptEmail(newPayment, Services);

            }

            //Web Client Notifications
            hubContext.Clients.All.saveNewPayment(newPayment);
            Services.Log.Info("Connected Clients Updated");

            return this.Request.CreateResponse(HttpStatusCode.Created);
        }

    }
}
