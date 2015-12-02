using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Newtonsoft.Json;
using stranddService.DataObjects;
using stranddService.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Description;

namespace stranddService.Controllers
{
    public class HistoryEventController : ApiController
    {
        public ApiServices Services { get; set; }

        [Route("api/historylog")]
        [ResponseType(typeof(HistoryEvent))]
        public async Task<IHttpActionResult> GetAllHistoryLog()
        {
            Services.Log.Info("History Log Requested [API]");
            List<HistoryEvent> dbHistoryLog = new List<HistoryEvent>();

            stranddContext context = new stranddContext();

            //Loading Log of History Events from DB Context
            dbHistoryLog = await (context.HistoryLog).ToListAsync<HistoryEvent>();

            //Return Successful Response
            Services.Log.Info("History Log Returned [API]");
            return Ok(dbHistoryLog);
        }

        /*
        [Route("api/historylog/{incidentID}")]
        [ResponseType(typeof(HistoryEvent))]
        public async Task<IHttpActionResult> GetAccountInfo(string providerUserID)
        {
            Services.Log.Info("ProviderUserID [" + providerUserID + "] Account Information Requested [API]");

            AccountInfo returnAccountInfo = new AccountInfo(providerUserID);

            //Return Successful Response
            Services.Log.Info("ProviderUserID [" + providerUserID + "] Account Information Returned");
            return Ok(returnAccountInfo);
        }
        */

    }
}