using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using stranddService.DataObjects;
using stranddService.Helpers;
using stranddService.Models;
using stranddService.Security;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Http;

namespace stranddService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class RoadZenRegistrationController : ApiController
    {
        public ApiServices Services { get; set; }

        [AuthorizeLevel(AuthorizationLevel.Anonymous)]
        [Route("api/roadzensecurity/registration")]
        public HttpResponseMessage RoadZenAccountRegistration(RegistrationRequest registrationRequest)
        {
            Services.Log.Info("New Account Registration Request [API]");

            // Phone Number SS Validation
            if (!Regex.IsMatch(registrationRequest.Phone, "^[0-9]{10}$"))
            {
                Services.Log.Warn("Invalid phone number (must be 10 numeric digits");
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid phone number (must be 10 numeric digits");
            }
            if (!RegexUtilities.IsValidEmail(registrationRequest.Email))
            {
                Services.Log.Warn("Invalid e-mail address");
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid e-mail address");
            }

            // Get the logged-in user.
            var currentUser = this.User as ServiceUser;

            stranddContext context = new stranddContext();

            Account account = context.Accounts.Where(a => a.Phone == registrationRequest.Phone).SingleOrDefault();
            if (account != null)
            {
                string responseText = "Phone Number Already Registered";
                Services.Log.Warn(responseText);
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, WebConfigurationManager.AppSettings["RZ_MobileClientUserWarningPrefix"] + responseText);

            }

            if (registrationRequest.Provider == null)
            {
                //Password SS Validation
                if (registrationRequest.Password.Length < 6)
                {
                    Services.Log.Warn("Invalid password (at least 6 chars required)");
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid password (at least 6 chars required)");
                }
            }
            else
            {
                //Existing Provider Check & Updation
                Account accountExistingProvider = context.Accounts.Where(a => a.ProviderUserID == currentUser.Id).SingleOrDefault();
                if (accountExistingProvider != null)
                {
                    accountExistingProvider.Name = registrationRequest.Name;
                    accountExistingProvider.Phone = registrationRequest.Phone;
                    accountExistingProvider.Email = accountExistingProvider.Email;
                    context.SaveChanges();

                    Services.Log.Info("Account for [" + currentUser.Id + "] has been updated");
                    return this.Request.CreateResponse(HttpStatusCode.OK, "Account for [" + currentUser.Id + "] has been updated");
                }
            }

            byte[] salt = RoadZenSecurityUtils.generateSalt();

            Guid guid = Guid.NewGuid();

            Account newUserAccount = new Account
            {
                Id = guid.ToString(),
                Name = registrationRequest.Name,
                Phone = registrationRequest.Phone,
                Email = registrationRequest.Email,
                ProviderUserID = (registrationRequest.Provider == null) ? "RoadZen:" + guid.ToString("N").ToUpper() : currentUser.Id,
                Salt = (registrationRequest.Provider == null) ? salt : null,
                SaltedAndHashedPassword = (registrationRequest.Provider == null) ? RoadZenSecurityUtils.hash(registrationRequest.Password, salt) : null
            };

            context.Accounts.Add(newUserAccount);
            context.SaveChanges();

            Services.Log.Info("Account for [" + newUserAccount.ProviderUserID + "] has been created");
            return this.Request.CreateResponse(HttpStatusCode.Created, "Account for [" + newUserAccount.ProviderUserID + "] has been created");
        }

        [AuthorizeLevel(AuthorizationLevel.Anonymous)]
        [Route("api/roadzensecurity/provider-registration")]
        public HttpResponseMessage RoadZenAccountProviderRegistration(ProviderRegistrationRequest registrationRequest)
        {
            Services.Log.Info("New Account Provider Registration Request [API]");

            // Phone Number SS Validation
            if (!Regex.IsMatch(registrationRequest.Phone, "^[0-9]{10}$"))
            {
                Services.Log.Warn("Invalid phone number (must be 10 numeric digits");
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid phone number (must be 10 numeric digits");
            }
            if (!RegexUtilities.IsValidEmail(registrationRequest.Email))
            {
                Services.Log.Warn("Invalid e-mail address");
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid e-mail address");
            }

            // Get the logged-in user.
            var currentUser = this.User as ServiceUser;

            stranddContext context = new stranddContext();

            Account account = context.Accounts.Where(a => a.Phone == registrationRequest.Phone).SingleOrDefault();
            if (account != null)
            {
                string responseText = "Phone Number Already Registered";
                Services.Log.Warn(responseText);
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, WebConfigurationManager.AppSettings["RZ_MobileClientUserWarningPrefix"] + responseText);

            }

            //Password SS Validation
            if (registrationRequest.Password.Length < 6)
            {
                Services.Log.Warn("Invalid password (at least 6 chars required)");
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid password (at least 6 chars required)");
            }

            byte[] salt = RoadZenSecurityUtils.generateSalt();

            Guid guid = Guid.NewGuid();

            Account newUserAccount = new Account
            {
                Id = guid.ToString(),
                Name = registrationRequest.Name,
                Phone = registrationRequest.Phone,
                Email = registrationRequest.Email,
                ProviderUserID = "RoadZen:" + guid.ToString("N").ToUpper(),
                Salt =  salt,
                SaltedAndHashedPassword = RoadZenSecurityUtils.hash(registrationRequest.Password, salt)
            };

            context.Accounts.Add(newUserAccount);
            context.SaveChanges();

            Services.Log.Info("Account for [" + newUserAccount.ProviderUserID + "] has been created");
            return this.Request.CreateResponse(HttpStatusCode.Created, "Account for [" + newUserAccount.ProviderUserID + "] has been created");
        }
           
    
    }
}
