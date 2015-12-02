using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using System.Threading.Tasks;
using stranddService.DataObjects;
using Microsoft.AspNet.SignalR;
using stranddService.Hubs;
using stranddService.Models;
using System.Web.Http.Description;
using System.Data.Entity;
using System.Data.Entity.Validation;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using System.IO;
using System.Configuration;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Data;
using stranddService.Helpers;
using System.Reflection;

namespace stranddService.Controllers
{
    public class CommunicationController : ApiController
    {
        public ApiServices Services { get; set; }

        [Route("api/communications")]
        [ResponseType(typeof(CommunicationEntry))]
        public async Task<IHttpActionResult> GetAllCommunications()
        {
            Services.Log.Info("Communication Log Requested [API]");
            List<CommunicationEntry> dbCommunicationCollection = new List<CommunicationEntry>();
            List<CommunicationInfo> fullCommunicationCollection = new List<CommunicationInfo>();

            stranddContext context = new stranddContext();

            //Loading List of Accounts from DB Context
            dbCommunicationCollection = await (context.CommunicationLog).ToListAsync<CommunicationEntry>();
            dbCommunicationCollection.Reverse();
            foreach (CommunicationEntry dbCommunication in dbCommunicationCollection) { fullCommunicationCollection.Add(new CommunicationInfo(dbCommunication)); }

            //Return Successful Response
            Services.Log.Info("Communication Log Returned [API]");
            return Ok(fullCommunicationCollection);




        }

        [Route("api/communications/currentoperator/confirmcallrequest")]
        public async Task<HttpResponseMessage> OperatorConfirmCallRequest(OperatorCallRequestConfirmationRequest communicationRequest)
        {
            Services.Log.Info("Operator Call Request Confirmation Requested [API]");
            string responseText;

            // Get the logged-in user.
            var currentUser = this.User as ServiceUser;

            stranddContext context = new stranddContext();

            //Retrieve Communication
            CommunicationEntry updateCommunication = await (from r in context.CommunicationLog where (r.Id == communicationRequest.CommunicationGUID) select r).FirstOrDefaultAsync();

            if (updateCommunication != null)
            {
                if (updateCommunication.Status == "CONFIRMED")
                {
                    responseText = "Already Confirmed - Communication [" + communicationRequest.CommunicationGUID + "] ";
                    Services.Log.Info(responseText);
                    return this.Request.CreateResponse(HttpStatusCode.OK, responseText);

                }

                else
                {
                    //Edit Communication
                    updateCommunication.OperatorID = currentUser.Id;
                    updateCommunication.Status = "CONFIRMED";
                    updateCommunication.EndTime = DateTime.Now;
                }

            }
            else
            {
                // Return Failed Response
                responseText = "Not Found - Communication [" + communicationRequest.CommunicationGUID + "] ";
                Services.Log.Warn(responseText);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, responseText);
            }

            //Save record
            await context.SaveChangesAsync();

            responseText = "Operator Confirmed - Communication [" + updateCommunication.Id + "] ";
            Services.Log.Info(responseText);

            //await HistoryEvent.logHistoryEventAsync("COMMUNICATION_REQUESTCONFIRMATION_OPERATOR", null, updateCommunication.Id, null, null, null);

            return this.Request.CreateResponse(HttpStatusCode.OK, responseText);
        }

        [HttpGet]
        [Route("api/communication/exceloutput")]
        public HttpResponseMessage GenerateCommunicationExcel()
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


            List<CommunicationEntry> dbCommunicationCollection = new List<CommunicationEntry>();
            List<CommunicationExcelData> fullCommunicationCollection = new List<CommunicationExcelData>();
            stranddContext context = new stranddContext();

            dbCommunicationCollection = context.CommunicationLog.Where(a => a.CreatedAt >= startTime)
               .Where(a => a.CreatedAt <= endTime).ToList();


            string strData = "Type,Tag,Status,Source,Text,CreatedAt,UpdatedAt,Deleted";

            DataTable table = ConvertListToDataTable(dbCommunicationCollection);

            ExcelHelper objexcel = new ExcelHelper();

            return objexcel.GetExcel(table, strData, "CommunicationHistoryReport");
        }

        static DataTable ConvertListToDataTable<CommunicationExcelData>(List<CommunicationExcelData> list)
        {
            //   List<IncidentExcelData[]> myList = list.ToArray();

            DataTable dataTable = new DataTable(typeof(CommunicationExcelData).Name);
            PropertyInfo[] props = typeof(CommunicationExcelData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ??
                    prop.PropertyType);
            }

            foreach (CommunicationExcelData item in list)
            {
                var values = new object[props.Length];
                for (int i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;

        }


        //piyush work


        //--------------------------------------------------INBOUND AND OUTBOUND MESSAGE CODE START -------------------------------------------------------------// 


        [HttpGet]
        [Route("api/communications/inboundsmsnew")]
        public async Task<HttpResponseMessage> inboundsmsnew(WebRequest communicationRequest)
        {

            Services.Log.Info("Inbound SMS Request");

            String Type = "INBOUND SMS";
            String ID = Guid.NewGuid().ToString();


            //store at DB Level
           // LogInboundOutboundMessageToDB(ID, Type, communicationRequest.Tag, "SUBMITTED", communicationRequest.Source, communicationRequest.Text);

            string responseText = ("New Communication Log Created #" + ID);
            Services.Log.Info(responseText);

            return this.Request.CreateResponse(HttpStatusCode.Created, responseText);


        }


        [HttpGet]
        [Route("api/communications/inboundsms")]
        public async Task<HttpResponseMessage> inboundSms()
        {

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
            // String check_value = communicationRequest.GET['check'];
            // String p = HttpUtility.ParseQueryString(communicationRequest); 
            //  String from = communicationRequest["from"];
            //String message = communicationRequest["message"];
            //String ts = communicationRequest["ts"];

            //store at DB Level
            LogInboundOutboundMessageToDB(ID, Type, from, "SUBMITTED", "Source", message);


            //store at DB Level
            //LogInboundOutboundMessageToDB(ID, Type, communicationRequest.Tag, "SUBMITTED", communicationRequest.Source, communicationRequest.Text);

            string responseText = ("New Communication Log Created #" + ID);
            Services.Log.Info(responseText);

            return this.Request.CreateResponse(HttpStatusCode.Created, responseText);


        }

        [HttpPost]
        [Route("api/communications/inboundsms1")]
        public async Task<HttpResponseMessage> InboundSms1(CommunicationsLogInboundRequest communicationRequest)
        {

            Services.Log.Info("Inbound SMS Request");

            String Type = "INBOUND SMS";
            String ID = Guid.NewGuid().ToString();


            //store at DB Level
            LogInboundOutboundMessageToDB(ID, Type, communicationRequest.Tag, "SUBMITTED", communicationRequest.Source, communicationRequest.Text);

            string responseText = ("New Communication Log Created #" + ID);
            Services.Log.Info(responseText);

            return this.Request.CreateResponse(HttpStatusCode.Created, responseText);


        }


        //send out bound message method OutboundSms
        [HttpPost]
        [Route("api/communications/outboundsms")]
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
                LogInboundOutboundMessageToDB(ID, Type, communicationRequest.MobileNo, Status, Messageid, communicationRequest.Message);
            }

            string responseLog = ("New Out Bound message Send to #" + communicationRequest.MobileNo);
            Services.Log.Info(responseLog);

            return this.Request.CreateResponse(HttpStatusCode.Created, responseText);

            //JObject obj = JObject.Parse(strreturn);
            //string Message = (string)obj["message"];
            //return Message;
        }


        [HttpPost]
        [Route("api/communications/outboundsmsStatus")]
        public async Task<HttpResponseMessage> GetSmsStatusByMessageIDs(String MessageIDs)
        {
            String strreturn;
            String strAPI = ConfigurationManager.AppSettings["OutboundSmsAPI"];
            String senderid = ConfigurationManager.AppSettings["OutboundSmsSenderID"];

            String sURL = "http://global.sinfini.com/api/v3/index.php?method=sms" + "&api_key=" + strAPI + "&format=json&id=" + MessageIDs + "&numberinfo=1";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sURL);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    // return reader.ReadToEnd();
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
                    //log errorText
                }
                throw;
            }

            stranddContext context = new stranddContext();

            CommunicationEntry update = await context.CommunicationLog.Where(a => a.Source == MessageIDs).SingleOrDefaultAsync();
            String responseText = String.Empty;
            if (update != null)
            {

                 JObject obj = JObject.Parse(strreturn);
                string Messageid = String.Empty;
              
                update.Source = obj["message"].ToString();
               
                await context.SaveChangesAsync();

                responseText = "Status Update MessgeId: " + MessageIDs;

                //Return Successful Response
                Services.Log.Info(responseText);

            }

            //string responseText = strreturn;

            return this.Request.CreateResponse(HttpStatusCode.Created, responseText);
        }

        private void LogInboundOutboundMessageToLogFile(String ID, String Type, String Tag, String Status, String Source, String Text)
        {
            var filename = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["CommunicationsLogFileName"];
            using (StreamWriter writer = File.AppendText(filename))
            {
                writer.WriteLine("");
                writer.WriteLine("ID :" + ID, writer);
                writer.WriteLine("Type :" + Type, writer);
                writer.WriteLine("Tag :" + Tag, writer);
                writer.WriteLine("Source :" + Source, writer);
                writer.WriteLine("Text :" + Text, writer);
                writer.WriteLine("StartTime :" + DateTime.Now.ToString(), writer);
                writer.WriteLine("------------------------------------------------");
                // Update the underlying file.
                writer.Flush();
                writer.Close();
            }

        }

        private void LogInboundOutboundMessageToDB(String ID, String Type, String Tag, String Status, String Source, String Text)
        {
            CommunicationEntry newCommlog = new CommunicationEntry()
            {
                Id = ID,
                Type = Type,
                Tag = Tag,
                Status = "SUBMITTED",
                Source = Source,
                Text = Text,
                CreatedAt = DateTime.Now
            };

            stranddContext context = new stranddContext();
            context.CommunicationLog.Add(newCommlog);

            //Save record
            context.SaveChangesAsync();


        }

        //--------------------------------------------------INBOUND AND OUTBOUND MESSAGE CODE END-------------------------------------------------------------// 


    }
}
