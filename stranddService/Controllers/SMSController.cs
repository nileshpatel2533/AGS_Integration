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
using System.Configuration;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;



namespace stranddService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class SMSController : ApiController
    {
        public ApiServices Services { get; set; }


        //piyush work


        //--------------------------------------------------INBOUND AND OUTBOUND MESSAGE CODE START -------------------------------------------------------------// 

       

        [HttpGet]
        [Route("api/SMS/inboundsms")]
        public async Task<HttpResponseMessage> inboundSms()
        {
            Microsoft.AspNet.SignalR.IHubContext hubContext = Services.GetRealtime<IncidentHub>();
            Services.Log.Info("Inbound SMS Request");

            String Type = "INBOUND SMS";
            String ID = Guid.NewGuid().ToString();

            var queryStrings = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
            String from = String.Empty;
            String message = String.Empty;
            String ts = String.Empty;


            if (queryStrings.ContainsKey("from"))
            {
                from = queryStrings["from"];
            }
            if (queryStrings.ContainsKey("message"))
            {
                message = queryStrings["message"];
            }
            if (queryStrings.ContainsKey("ts"))
            {
                ts = queryStrings["ts"];
            }

        
            //store at DB Level
           // LogInboundOutboundMessageToDB(ID, Type, from, "SUBMITTED", "Source", message);

            CommunicationEntry newCommunication = new CommunicationEntry()
            {
                Id = ID,
                Type = Type,
                Tag = from,
                Status = "SUBMITTED",
                Text = message,
                StartTime = DateTime.Now
            };


            stranddContext context = new stranddContext();
            context.CommunicationLog.Add(newCommunication);

            await context.SaveChangesAsync();
            String responseText = ("New Communication Log Created #" + ID);
            Services.Log.Info(responseText);
         
          //  return this.Request.CreateResponse(HttpStatusCode.Created, responseText);


            //----------------------------NOTIFICATION INBOUND SMS CODE-------------------------------//
            responseText = "";

            if (from != null)
            {
                responseText += "Please call [" + from + "] ";
            }

            if (message != null)
            {
                responseText += "[" + message + "]";
                //hubContext.Clients.All.updateIncidentCustomerError(responseText);
            }

            Services.Log.Warn(responseText);

           // hubContext.Clients.All.notifyCustomerClientExceptionContact(new CommunicationInfo(newCommunication));
            hubContext.Clients.All.notifyCustomerNewInboundSmsContact(new InboundsmsInfo(newCommunication));
            Services.Log.Info("Inbound Notification Cerated.");


           // Outbound SMS triggered to above Number with following text from MM-STRNDD:
            String strdownloadlink = ConfigurationManager.AppSettings["Appdownloadlink"];
            strdownloadlink = "<" + "a href='" + strdownloadlink + "'>" + strdownloadlink + "<a>";
          
            CommunicationsLogOutboundRequest communicationRequest = new CommunicationsLogOutboundRequest();
            communicationRequest.MobileNo = from;
            communicationRequest.Message = "Please download the StrandD App [" + strdownloadlink + "] so we can better assist you incase you need us again.";

            await OutboundSms(communicationRequest);

            //Return Successful Response
            return this.Request.CreateResponse(HttpStatusCode.OK, responseText);

            //----------------------------NOTIFICATION INBOUND SMS CODE END-------------------------------//

        }



        //send out bound message method OutboundSms
        [HttpPost]
        [Route("api/SMS/outboundsms")]
        public async Task<HttpResponseMessage> OutboundSms(CommunicationsLogOutboundRequest communicationRequest)
        {

            String Type = "OUTBOUND SMS";
            String ID = Guid.NewGuid().ToString();
            //String MobileNo, String Message
            String strreturn;
            String strAPI = ConfigurationManager.AppSettings["OutboundSmsAPI"];
            String senderid = ConfigurationManager.AppSettings["OutboundSmsSenderID"];

            // String sURL = "http://global.sinfini.com/api/v3/index.php?method=sms" + "&api_key=" + strAPI + "&to=" + communicationRequest.MobileNo + "&sender=" + senderid + "&message=" + communicationRequest.Message + "&format=json&custom=1,2&flash=0";
            String sURL = "http://global.sinfini.com/api/v3/index.php?method=sms" + "&api_key=" + strAPI + "&to=" + communicationRequest.MobileNo + "&sender=" + senderid + "&message=" + communicationRequest.Message + "&unicode=1";



            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sURL);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    //return reader.ReadToEnd();
                    strreturn = reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    // log errorText
                }
                throw;
            }

            string responseText = strreturn;

            JObject obj = JObject.Parse(strreturn);
            string Messageid = String.Empty;

            try
            {
                Messageid = obj["data"]["0"]["id"].ToString();
            }
            catch
            {

            }

            if (Messageid != null)
            {
                string Status = obj["status"].ToString();
                LogOutboundMessageToDB(ID, Type, communicationRequest.MobileNo, Status, Messageid, communicationRequest.Message);
            }

            string responseLog = ("New Out Bound message Send to #" + communicationRequest.MobileNo);
            Services.Log.Info(responseLog);

            return this.Request.CreateResponse(HttpStatusCode.Created, responseText);

          
        }



        private void LogOutboundMessageToDB(String ID, String Type, String Tag, String Status, String Source, String Text)
        {
            CommunicationEntry newCommlog = new CommunicationEntry()
            {
                Id = ID,
                Type = Type,
                Tag = Tag,
                Status = "SUBMITTED",
                Source = Source,
                Text = Text,
                StartTime = DateTime.Now
            };

            stranddContext context = new stranddContext();
            context.CommunicationLog.Add(newCommlog);

            //Save record
            context.SaveChangesAsync();


        }

        //--------------------------------------------------INBOUND AND OUTBOUND MESSAGE CODE END-------------------------------------------------------------// 
    }
}
