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

namespace stranddService.Controllers
{
    public class ClientExceptionController : ApiController
    {
        public ApiServices Services { get; set; }

        [Route("api/clientexceptions")]
        [ResponseType(typeof(ExceptionEntry))]
        public async Task<IHttpActionResult> GetAllExceptions()
        {
            Services.Log.Info("Exception Log Requested [API]");
            List<ExceptionEntry> dbExceptionCollection = new List<ExceptionEntry>();

            stranddContext context = new stranddContext();

            //Loading List of Accounts from DB Context
            dbExceptionCollection = await (context.ExceptionLog).ToListAsync<ExceptionEntry>();

            //Return Successful Response
            Services.Log.Info("Exception Log Returned [API]");
            return Ok(dbExceptionCollection);
        }

        [Route("api/clientexceptions/mobilecustomer/general")]
        public async Task<HttpResponseMessage> CustomerClientExceptionGeneral(ExceptionLogRequest clientException)
        {
            Services.Log.Warn("Mobile Customer Client General Exception [API]");
            Services.Log.Warn(clientException.Exception);

            ExceptionEntry newException = new ExceptionEntry()
            {
                Id = Guid.NewGuid().ToString(),
                ExceptionText = clientException.Exception,
                Source="MOBILE CUSTOMER CLIENT"
            };

            stranddContext context = new stranddContext();
            context.ExceptionLog.Add(newException);

            await context.SaveChangesAsync();
            
            string responseText;
            responseText = "Exception Logged in Service";

            //Return Successful Response
            return this.Request.CreateResponse(HttpStatusCode.OK, responseText);

        }

        [Route("api/clientexceptions/mobileprovider/general")]
        public async Task<HttpResponseMessage> ProviderClientExceptionGeneral(ExceptionLogRequest clientException)
        {
            Services.Log.Warn("Mobile Provider Client General Exception [API]");
            Services.Log.Warn(clientException.Exception);

            ExceptionEntry newException = new ExceptionEntry()
            {
                Id = Guid.NewGuid().ToString(),
                ExceptionText = clientException.Exception,
                Source = "MOBILE PROVIDER CLIENT"
            };

            stranddContext context = new stranddContext();
            context.ExceptionLog.Add(newException);

            await context.SaveChangesAsync();

            string responseText;
            responseText = "Exception Logged in Service";

            //Return Successful Response
            return this.Request.CreateResponse(HttpStatusCode.OK, responseText);

        }

        [Route("api/clientexceptions/mobilecustomer/contact")]
        public async Task<HttpResponseMessage> CustomerClientExceptionContact(ExceptionContactRequest contactRequest)
        {
            Services.Log.Warn("Mobile Customer Client Exception Contact Request [API]");
            string responseText = "";

            IHubContext hubContext = Services.GetRealtime<IncidentHub>();

            CommunicationEntry newCommunication = new CommunicationEntry()
            {
                Id = Guid.NewGuid().ToString(),
                Tag = contactRequest.ContactPhone,
                IncidentID = contactRequest.IncidentGUID,
                Type = "MOBILE CUSTOMER CLIENT EXCEPTION CONTACT REQUEST",
                Status = "SUBMITTED",
                StartTime = DateTime.Now
            };

            stranddContext context = new stranddContext();
            context.CommunicationLog.Add(newCommunication);

            await context.SaveChangesAsync();
            responseText = "Communication Logged in Service";
            Services.Log.Info(responseText);
            responseText = "";

            if (contactRequest.ContactPhone != null)
            {
                responseText += "CONTACT CUSTOMER on Phone [" + contactRequest.ContactPhone + "] ";
            }

            if (contactRequest.IncidentGUID != null)
            {
                responseText += " | Exception on Incident [" + contactRequest.IncidentGUID + "]" ;
                //hubContext.Clients.All.updateIncidentCustomerError(responseText);
            }

            Services.Log.Warn(responseText);

            hubContext.Clients.All.notifyCustomerClientExceptionContact(new CommunicationInfo(newCommunication));
            Services.Log.Info("Connected Clients Updated");

            //Return Successful Response
            return this.Request.CreateResponse(HttpStatusCode.OK, responseText);

            //PENDING TO ADD: Incident Updation with Error
        }

    }
}