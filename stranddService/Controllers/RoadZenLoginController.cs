using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using SendGrid;
using stranddService.DataObjects;
using stranddService.Helpers;
using stranddService.Models;
using stranddService.Security;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.Web.Configuration;
using System.Web.Http;

namespace stranddService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class RoadZenLoginController : ApiController
    {
        public ApiServices Services { get; set; }
        public IServiceTokenHandler handler { get; set; }

        [AuthorizeLevel(AuthorizationLevel.Anonymous)]
        [Route("api/roadzensecurity/login")]
        public HttpResponseMessage RoadZenLogin(LoginRequest loginRequest)
        {
            Services.Log.Info("RoadZen Login Request from Phone# [" + loginRequest.Phone + "]");

            stranddContext context = new stranddContext();
            Account useraccount = context.Accounts.Where(a => a.Phone == loginRequest.Phone).SingleOrDefault();
            if (useraccount != null)
            {
                // Check if Registered Phone Number is through Google-Provider Account
                if (useraccount.ProviderUserID.Substring(0,6)=="Google")
                {
                    string responseText = "Phone Number Registered with Google";
                    Services.Log.Warn(responseText);
                    return this.Request.CreateResponse(HttpStatusCode.Unauthorized, WebConfigurationManager.AppSettings["RZ_MobileClientUserWarningPrefix"] + responseText);
                }

                byte[] incoming = RoadZenSecurityUtils.hash(loginRequest.Password, useraccount.Salt);

                if (RoadZenSecurityUtils.slowEquals(incoming, useraccount.SaltedAndHashedPassword))
                {
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity();
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, loginRequest.Phone));
                    claimsIdentity.AddClaim(new Claim("AccountGUID", useraccount.Id));

                    LoginResult loginResult = new RoadZenLoginProvider(handler).CreateLoginResult(claimsIdentity, Services.Settings.MasterKey);

                    Services.Log.Info("Account [" + useraccount.ProviderUserID + "] has logged-in");
                    return this.Request.CreateResponse(HttpStatusCode.OK, loginResult);
                }
                else
                {
                    string responseText = "Incorrect Password";
                    Services.Log.Warn(responseText);
                    return this.Request.CreateResponse(HttpStatusCode.Unauthorized, WebConfigurationManager.AppSettings["RZ_MobileClientUserWarningPrefix"] + responseText);
                }
            }
            else
            {
                string responseText = "Unregistered Phone Number";
                Services.Log.Warn(responseText);
                return this.Request.CreateResponse(HttpStatusCode.Unauthorized, WebConfigurationManager.AppSettings["RZ_MobileClientUserWarningPrefix"] + responseText);
            }


        }

        [AuthorizeLevel(AuthorizationLevel.Anonymous)]
        [Route("api/roadzensecurity/resetpassword")]
        public HttpResponseMessage RoadZenResetPassword(LoginRequest passwordRequest)
        {
            Services.Log.Info("RoadZen Password Reset Request from Phone# [" + passwordRequest.Phone + "]");

            stranddContext context = new stranddContext();
            Account useraccount = context.Accounts.Where(a => a.Phone == passwordRequest.Phone).SingleOrDefault();
            if (useraccount != null)
            {
                if (useraccount.ProviderUserID.Substring(0, 7) != "RoadZen")
                {
                    string responseText = "Phone# Registered with Google";
                    Services.Log.Warn(responseText);
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, WebConfigurationManager.AppSettings["RZ_MobileClientUserWarningPrefix"] + responseText);
                }
                else
                {
                    //Generate random characters from GUID
                    string newPassword = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);                    

                    //Encrypt new Password
                    byte[] salt = RoadZenSecurityUtils.generateSalt();
                    useraccount.Salt = salt;

                    useraccount.SaltedAndHashedPassword = RoadZenSecurityUtils.hash(newPassword, salt);
                    Services.Log.Info("Password for Phone# [" + passwordRequest.Phone + "] Reset & Saved");

                    //Save Encrypted Password
                    context.SaveChanges();

                    //Prepare SendGrid Mail
                    SendGridMessage resetEmail = new SendGridMessage();

                    resetEmail.From = SendGridHelper.GetAppFrom();
                    resetEmail.AddTo(useraccount.Email);
                    resetEmail.Subject = "StrandD Password Reset";
                    resetEmail.Html = "<h3>New Password</h3><p>"+ newPassword +"</p>";
                    resetEmail.Text = "New Password: " + newPassword;

                    // Create an Web transport for sending email.
                    var transportWeb = new Web(SendGridHelper.GetNetCreds());

                    // Send the email.
                    transportWeb.Deliver(resetEmail);

                    //Send Successful Reponse
                    string responseText = "New Password Email Sent to [" + useraccount.Email + "]";
                    Services.Log.Info(responseText);
                    return this.Request.CreateResponse(HttpStatusCode.OK, responseText);
                }
            }
            else
            {
                string responseText = "Phone Number Not Registered";
                Services.Log.Warn(responseText);
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, WebConfigurationManager.AppSettings["RZ_MobileClientUserWarningPrefix"] + responseText);
            }            
        }
    }
}
