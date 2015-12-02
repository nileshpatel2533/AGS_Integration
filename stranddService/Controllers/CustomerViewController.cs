using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using stranddService.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;

namespace stranddService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class CustomerViewController : ApiController
    {
        public ApiServices Services { get; set; }

        [HttpGet]
        [Route("api/viewgenerator/customer/incidentpayment/{incidentGUID}")]
        [Route("view/customer/incidentpayment/{incidentGUID}")]
        public HttpResponseMessage CustomerIncidentPaymentView(string incidentGUID)
        {

            HttpResponseMessage response;
            Services.Log.Info("Customer Incident [" + incidentGUID + "] Payment View Requested [API]");
           
            Uri redirectURI;
            string paymentService;
            IncidentInfo paymentIncident;

            stranddContext context = new stranddContext();
            Incident returnIncident = context.Incidents.Find(incidentGUID);

            if (returnIncident != null)
            {
                paymentService = "Instamojo";  //WILL ADAPT AS NEW PAYMENT SERVICES ADDED
                paymentIncident = new IncidentInfo(incidentGUID);
            }
            else { paymentService = "Not Found"; paymentIncident = new IncidentInfo(); }


            if (paymentService == "Instamojo")
            {

                string instamojoIncidentDataField = WebConfigurationManager.AppSettings["RZ_InstamojoIncidentDataField"]; // "data_Field_25373"

                var redirectQuery = HttpUtility.ParseQueryString(string.Empty);

                redirectQuery["data_readonly"] = "data_amount";
                redirectQuery["embed"] = "form";

                redirectQuery["data_amount"] = (paymentIncident.ServiceFee - paymentIncident.PaymentAmount).ToString();
                redirectQuery["data_email"] = paymentIncident.IncidentUserInfo.Email.ToString();
                redirectQuery["data_name"] = paymentIncident.IncidentUserInfo.Name.ToString();
                redirectQuery["data_phone"] = paymentIncident.IncidentUserInfo.Phone.ToString();

                redirectQuery[("data_" + instamojoIncidentDataField)] = incidentGUID;
                redirectQuery["data_hidden"] = ("data_" + instamojoIncidentDataField);

                string urlFullString = WebConfigurationManager.AppSettings["RZ_InstamojoBaseURL"].ToString() + "?" + redirectQuery.ToString();
                redirectURI = new Uri(urlFullString);

                response = Request.CreateResponse(HttpStatusCode.Moved);
                response.Headers.Location = redirectURI;
                Services.Log.Info("Customer Incident Payment View [" + urlFullString + "] Returned [VIEW]");

            }
            else
            {
                response = Request.CreateResponse("<H1>No Incident Found</H1>");
                Services.Log.Warn("Customer Incident [" + incidentGUID  + "] Not Found [VIEW]");
            }

            //Return Generated Response           
            return response;
        }

    }
}
