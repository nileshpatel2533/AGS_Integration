using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using System.Web.Http.Description;
using stranddService.DataObjects;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace stranddService.Controllers
{
    public class SCMController : ApiController
    {
        public ApiServices Services { get; set; }

        [Route("api/servicelogs")]
        [ResponseType(typeof(ServiceLogSCMResponse))]
        public async Task<IHttpActionResult> GetSCMServiceLogs()
        {
            Services.Log.Info("Azure SCM Service Logs Requested [API]");

            var serviceName = WebConfigurationManager.AppSettings["MS_MobileServiceName"];
            var requestBaseURI = new Uri("https://" + serviceName + ".scm.azure-mobile.net/");

            var authToken = WebConfigurationManager.AppSettings["RZ_SCMAuthToken"];

            using (var client = new HttpClient())
            {
                client.BaseAddress = requestBaseURI;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                HttpResponseMessage response = await client.GetAsync("api/logs/recent");
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    List<ServiceLogSCMResponse> deserializedLogs = await JsonConvert.DeserializeObjectAsync<List<ServiceLogSCMResponse>>(content);

                    //Return Successful Responses
                    Services.Log.Info("Azure SCM Service Logs Returned [API]");
                    return Ok(content);
                    //JUST RETURNING STRING (Not Deserialized... for NOW)
                }
                else
                {
                    Services.Log.Warn("Unable to Retreive Azure SCM Service Logs [API]");
                    return BadRequest();
                }     

            }
            
        }

        [Route("api/serviceversion")]
        [ResponseType(typeof(String))]
        public async Task<IHttpActionResult> GetServiceVersion()
        {
            Services.Log.Info("Service Version Requested [API]");

            var serviceVersion = WebConfigurationManager.AppSettings["RZ_ServiceVersion"];
            return Ok(serviceVersion);

        }

        [Route("api/systemtimezones")]
        [ResponseType(typeof(String))]
        public async Task<IHttpActionResult> GetSystemTimeZones()
        {
            Services.Log.Info("System Time Zones Requested [API]");
            return Ok(TimeZoneInfo.GetSystemTimeZones());

        }

    }
}
