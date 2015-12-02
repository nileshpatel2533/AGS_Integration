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
using SendGrid;
using System.Web.Configuration;
using stranddService.Helpers;
using System.Collections.Generic;
using Exceptions;

namespace stranddService.Controllers
{
    public class SendGridController : ApiController
    {
        public ApiServices Services { get; set; }

        public static void SendIncidentSubmissionAdminEmail(Incident incident, ApiServices Services)
        {

            SendGridMessage submissionMessage = new SendGridMessage();
            IncidentInfo submisionIncident = new IncidentInfo(incident);

            submissionMessage.From = SendGridHelper.GetAppFrom();
            submissionMessage.AddTo(WebConfigurationManager.AppSettings["RZ_SysAdminEmail"]);
            submissionMessage.Html = " ";
            submissionMessage.Text = " ";
            submissionMessage.Subject = " [" + WebConfigurationManager.AppSettings["MS_MobileServiceName"] + "]";

            submissionMessage.EnableTemplateEngine(WebConfigurationManager.AppSettings["RZ_SubmisionTemplateID"]);
            submissionMessage.AddSubstitution("%timestamp%", new List<string> { submisionIncident.CreatedAt.ToString() });
            submissionMessage.AddSubstitution("%incidentguid%", new List<string> { submisionIncident.IncidentGUID });
            submissionMessage.AddSubstitution("%vehicledetails%", new List<string> { submisionIncident.IncidentVehicleInfo.RegistrationNumber });
            submissionMessage.AddSubstitution("%name%", new List<string> { submisionIncident.IncidentUserInfo.Name });
            submissionMessage.AddSubstitution("%phone%", new List<string> { submisionIncident.IncidentUserInfo.Phone });
            submissionMessage.AddSubstitution("%email%", new List<string> { submisionIncident.IncidentUserInfo.Email });
            submissionMessage.AddSubstitution("%jobdescription%", new List<string> { submisionIncident.JobCode });
            submissionMessage.AddSubstitution("%location%", new List<string> { submisionIncident.LocationObj.RGDisplay });
             
            // Create an Web transport for sending email.
            var transportWeb = new Web(SendGridHelper.GetNetCreds());

            // Send the email.
            try
            {
                transportWeb.Deliver(submissionMessage);
                Services.Log.Info("New Incident Submission Email Sent to [" + submisionIncident.IncidentUserInfo.Email + "]");
            }
            catch (InvalidApiRequestException ex)
            {
                for (int i = 0; i < ex.Errors.Length; i++)
                {
                    Services.Log.Error(ex.Errors[i]);
                }
            }
        }

        public static bool SendCurrentIncidentInvoiceEmail(Incident incident, ApiServices Services)
        {

            IncidentInfo invoiceIncident = new IncidentInfo(incident);

            if (invoiceIncident.PaymentAmount < invoiceIncident.ServiceFee) 
            { 

                SendGridMessage invoiceMessage = new SendGridMessage();

                invoiceMessage.From = SendGridHelper.GetAppFrom();
                invoiceMessage.AddTo(invoiceIncident.IncidentUserInfo.Email);
                invoiceMessage.Html = " ";
                invoiceMessage.Text = " ";
                invoiceMessage.Subject = "StrandD Invoice - Payment for Service Due";

                invoiceMessage.EnableTemplateEngine(WebConfigurationManager.AppSettings["RZ_InvoiceTemplateID"]);
                invoiceMessage.AddSubstitution("%invoicestub%", new List<string> { invoiceIncident.IncidentGUID.Substring(0, 5).ToUpper() });
                invoiceMessage.AddSubstitution("%incidentguid%", new List<string> { invoiceIncident.IncidentGUID });
                invoiceMessage.AddSubstitution("%name%", new List<string> { invoiceIncident.IncidentUserInfo.Name });
                invoiceMessage.AddSubstitution("%phone%", new List<string> { invoiceIncident.IncidentUserInfo.Phone });
                invoiceMessage.AddSubstitution("%email%", new List<string> { invoiceIncident.IncidentUserInfo.Email });
                invoiceMessage.AddSubstitution("%jobdescription%", new List<string> { invoiceIncident.JobCode });
                invoiceMessage.AddSubstitution("%servicefee%", new List<string> { (invoiceIncident.ServiceFee - invoiceIncident.PaymentAmount).ToString() });
                invoiceMessage.AddSubstitution("%datesubmitted%", new List<string> { DateTime.Now.ToShortDateString() });
                invoiceMessage.AddSubstitution("%datedue%", new List<string> { (DateTime.Now.AddDays(30)).ToShortTimeString() });
                invoiceMessage.AddSubstitution("%servicepaymentlink%", new List<string> { (WebConfigurationManager.AppSettings["RZ_ServiceBaseURL"].ToString() + "/view/customer/incidentpayment/" + invoiceIncident.IncidentGUID) });

                // Create an Web transport for sending email.
                var transportWeb = new Web(SendGridHelper.GetNetCreds());

                // Send the email.
                try
                {
                    transportWeb.Deliver(invoiceMessage);
                    Services.Log.Info("Incident Invoice Email Sent to [" + invoiceIncident.IncidentUserInfo.Email + "]");
                    return true;
                }
                catch (InvalidApiRequestException ex)
                {
                    for (int i = 0; i < ex.Errors.Length; i++)
                    {
                        Services.Log.Error(ex.Errors[i]);
                        return false;
                    }
                }
                return false;

            }
            else 
            { 
                return false; 
            }
        }

        public static void SendIncidentPaymentReceiptEmail(Payment payment, ApiServices Services)
        {

            SendGridMessage receiptMessage = new SendGridMessage();
            IncidentInfo receiptIncident = new IncidentInfo(payment.IncidentGUID);

            receiptMessage.From = SendGridHelper.GetAppFrom();
            receiptMessage.AddTo(receiptIncident.IncidentUserInfo.Email);
            receiptMessage.Html = " ";
            receiptMessage.Text = " ";
            receiptMessage.Subject = " ";

            receiptMessage.EnableTemplateEngine(WebConfigurationManager.AppSettings["RZ_ReceiptTemplateID"]);
            receiptMessage.AddSubstitution("%invoicestub%", new List<string> { receiptIncident.IncidentGUID.Substring(0, 5).ToUpper() });
            receiptMessage.AddSubstitution("%name%", new List<string> { receiptIncident.IncidentUserInfo.Name });
            receiptMessage.AddSubstitution("%jobdescription%", new List<string> { receiptIncident.JobCode });
            receiptMessage.AddSubstitution("%servicefee%", new List<string> { receiptIncident.ServiceFee.ToString() });
            receiptMessage.AddSubstitution("%datesubmitted%", new List<string> { DateTime.Now.ToShortDateString() });
            receiptMessage.AddSubstitution("%paymentmethod%", new List<string> { receiptIncident.PaymentMethod });
            receiptMessage.AddSubstitution("%paymentamount%", new List<string> { receiptIncident.PaymentAmount.ToString() });

            // Create an Web transport for sending email.
            var transportWeb = new Web(SendGridHelper.GetNetCreds());

            // Send the email.
            try
            {
                transportWeb.Deliver(receiptMessage);
                Services.Log.Info("Payment Receipt Email Sent to [" + receiptIncident.IncidentUserInfo.Email + "]");
            }
            catch (InvalidApiRequestException ex)
            {
                for (int i = 0; i < ex.Errors.Length; i++)
                {
                    Services.Log.Error(ex.Errors[i]);
                }
            }
        }

    }
}
