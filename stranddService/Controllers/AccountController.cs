using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Newtonsoft.Json;
using stranddService.DataObjects;
using stranddService.Helpers;
using stranddService.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Description;
namespace stranddService.Controllers
{
    public class AccountController : ApiController
    {
        public ApiServices Services { get; set; }

        [Route("api/accounts")]
        [ResponseType(typeof(Account))]
        public async Task<IHttpActionResult> GetAllAccountInfos()
        {
            Services.Log.Info("Account Log Requested [API]");
            List<Account> dbAccountCollection = new List<Account>();

            stranddContext context = new stranddContext();

            //Loading List of Accounts from DB Context
            dbAccountCollection = await (context.Accounts).ToListAsync<Account>();

            //Return Successful Response
            Services.Log.Info("Account Log Returned [API]");
            return Ok(dbAccountCollection);
        }

        [Route("api/accounts/userprovider/{providerUserID}")]
        [ResponseType(typeof(AccountInfo))]
        public async Task<IHttpActionResult> GetAccountInfo(string providerUserID)
        {
            Services.Log.Info("ProviderUserID [" + providerUserID + "] Account Information Requested [API]");

            AccountInfo returnAccountInfo = new AccountInfo(providerUserID);

            //Return Successful Response
            Services.Log.Info("ProviderUserID [" + providerUserID + "] Account Information Returned");
            return Ok(returnAccountInfo);
        }

        [Route("api/accounts/findbyprovideruserid")]
        [ResponseType(typeof(AccountInfo))]
        public async Task<IHttpActionResult> FindAccountInfo(AccountRequest request)
        {
            Services.Log.Info("Find Account Information Posted for ProviderUserID [" + request.ProviderUserID + "] [API]");

            AccountInfo returnAccountInfo = new AccountInfo(request.ProviderUserID);

            //Return Successful Response
            Services.Log.Info("ProviderUserID [" + request.ProviderUserID + "] Account Information Returned [API]");
            return Ok(returnAccountInfo);
        }

        [Route("api/accounts/current")]
        [ResponseType(typeof(AccountInfo))]
        public async Task<IHttpActionResult> GetCurrentAccountInfo()
        {
            // Get the logged-in user.
            var currentUser = this.User as ServiceUser;

            Services.Log.Info("Find Account Information for Current User");

            AccountInfo returnAccountInfo = new AccountInfo(currentUser.Id);

            //Return Successful Response
            Services.Log.Info("ProviderUserID [" + currentUser.Id + "] Account Information Returned [API]");
            return Ok(returnAccountInfo);
        }

        [Route("api/accounts/current/roles")]
        [ResponseType(typeof(AccountRole))]
        public async Task<IHttpActionResult> GetCurrentAccountRoles()
        {
            List<AccountRole> dbRoleCollection = new List<AccountRole>();

            // Get the logged-in user.
            var currentUser = this.User as ServiceUser;

            Services.Log.Info("Find Account Roles Information for Current User");

            stranddContext context = new stranddContext();

            //Loading List of Account Roles from DB Context
            dbRoleCollection = await context.AccountRoles.Where(a => a.UserProviderID == currentUser.Id).ToListAsync<AccountRole>();

            if (dbRoleCollection.Count > 0)
            {
                string responseText = "Returned Account Roles for Account [" + currentUser.Id + "] " + currentUser.Id;
                Services.Log.Info(responseText);
                return Ok(dbRoleCollection);
            }
            else
            {
                string responseText = "No Account Roles for Account [" + currentUser.Id + "] " + currentUser.Id;
                Services.Log.Info(responseText);
                AccountRole noneRole = new AccountRole { CompanyGUID="NONE", RoleAssignment="NONE", UserProviderID=currentUser.Id };
                dbRoleCollection.Add(noneRole);
                return Ok(dbRoleCollection);
            }

        }

        [Route("api/accounts/current/incidentstatusrequest")]
        [ResponseType(typeof(IncidentStatusRequest))]
        public async Task<IHttpActionResult> GetCurrentUserMobileCustomerClientIncidentStatusRequest()
        {
            // Get the logged-in user.
            var currentUser = this.User as ServiceUser;

            Services.Log.Info("Current User Mobile Customer Current Incident Status Request [API]");

            if (currentUser.Id != null)
            {

                stranddContext context = new stranddContext();
                Incident accountincidentcheck = await context.Incidents
                    .Where(b => b.StatusCode != "CANCELLED" && b.StatusCode != "DECLINED" && b.StatusCode != "COMPLETED" && b.StatusCode != "FAILED")
                    .Where(c => c.ProviderUserID == currentUser.Id)
                    .SingleOrDefaultAsync();

                if (accountincidentcheck == null) { return NotFound(); }
                else 
                {
                    IncidentStatusRequest generatedStatusRequest = new IncidentStatusRequest(accountincidentcheck.Id);
                    //Return Successful Response
                    Services.Log.Info("Current User Mobile Customer Current Incident Status Request Generated and Returned [API]");
                    return Ok(generatedStatusRequest);
                }               
            }
            else { return NotFound(); }
            
        }

        [Route("api/accounts/current/uservehicles")]
        [ResponseType(typeof(Vehicle))]
        public async Task<IHttpActionResult> GetCurrentUserVehicleInfos()
        {
            // Get the logged-in user.
            var currentUser = this.User as ServiceUser;

            Services.Log.Info("Account-Vehicle Link Log Requested for Current User");

            if (currentUser.Id != null)
            {
                List<VehicleAccountLink> dbAccountLinkCollection = new List<VehicleAccountLink>();
                List<Vehicle> dbVehicleCollection = new List<Vehicle>();

                stranddContext context = new stranddContext();

                //Loading List of VehicleAccountLinks from DB Context
                dbAccountLinkCollection = await context.VehicleAccountLinks.Where(a => a.UserProviderID == currentUser.Id).ToListAsync<VehicleAccountLink>();

                if (dbAccountLinkCollection.Count > 0)
                {
                    for (int i = 0; i < dbAccountLinkCollection.Count; i++)
                    {
                        dbVehicleCollection.Add(context.Vehicles.Find(dbAccountLinkCollection[i].VehicleGUID));
                    }

                    string responseText = "Returned Account-Vehicle Links for Account [" + currentUser.Id + "] " + currentUser.Id;
                    Services.Log.Info(responseText);
                    return Ok(dbVehicleCollection);
                }
                else
                {
                    string responseText = "No Account-Vehicle Links for Account [" + currentUser.Id + "] " + currentUser.Id;
                    Services.Log.Info(responseText);
                    return Ok(responseText);
                }
            }
            else
            {
                string responseText = "Request made without Current User Logged In";
                Services.Log.Warn(responseText);
                return BadRequest(responseText);
            }
        }

        [Route("api/accounts/current/removevehicle")]
        public async Task<IHttpActionResult> RemoveAccountVehicleLink(VehicleRequest request)
        {
            Services.Log.Info("Account Vehicle Link Removal Requested Vehicle [" + request.VehicleGUID + "]");

            string vehicleGUID = request.VehicleGUID;
            // Get the logged-in user.
            var currentUser = this.User as ServiceUser;

            if (currentUser.Id != null)
            {
                VehicleAccountLink dbLink = new VehicleAccountLink();
                stranddContext context = new stranddContext();

                //Loading List of VehicleAccountLinks from DB Context
                dbLink = await (from r in context.VehicleAccountLinks where (r.UserProviderID == currentUser.Id && r.VehicleGUID == vehicleGUID) select r).FirstOrDefaultAsync();


                if (dbLink != null)
                {
                    context.VehicleAccountLinks.Remove(dbLink);
                    await context.SaveChangesAsync();

                    string responseText = "Removed Vehicle [" + vehicleGUID + "] from Account [" + currentUser.Id + "]";
                    Services.Log.Info(responseText);
                    return Ok(responseText);
                }
                else
                {
                    string responseText = "No Link found for Vehicle [" + vehicleGUID + "] from Account [" + currentUser.Id + "]";
                    Services.Log.Info(responseText);
                    return BadRequest(responseText);
                }
            }
            else
            {
                string responseText = "Request made without Current User Logged In";
                Services.Log.Warn(responseText);
                return BadRequest(responseText);
            }
        }

        [Route("api/accounts/update")]
        public async Task<HttpResponseMessage> UpdateAccount(AccountRequest accountRequest)
        {
            Services.Log.Info("Update Account Request");

            stranddContext context = new stranddContext();

            //Determine Account ProviderUserID for Updation based upon Request (or Authenticated Token)
            string accountID;

            if (accountRequest.ProviderUserID != null) { accountID = accountRequest.ProviderUserID; }
            else
            {
                var currentUser = this.User as ServiceUser;

                if (currentUser != null) { accountID = currentUser.Id; }
                else
                {
                    string responsetext = "No User Account or Provided ID";
                    Services.Log.Warn(responsetext);
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, responsetext);

                }
            }

            //Looks up the Account for Update by ProviderUserID
            Account updateAccount = await context.Accounts.Where(a => a.ProviderUserID == accountID).SingleOrDefaultAsync();

            if (updateAccount != null)
            {
                string responseText;

                if (updateAccount.Phone != accountRequest.Phone)
                {
                    Account accountphonecheck = await context.Accounts.Where(a => a.Phone == accountRequest.Phone).SingleOrDefaultAsync();

                    if (accountphonecheck != null)
                    {
                        if (updateAccount.Id != accountphonecheck.Id)
                        {
                            responseText = "Phone Number Already Registered";
                            Services.Log.Warn(responseText);
                            return this.Request.CreateResponse(HttpStatusCode.BadRequest, WebConfigurationManager.AppSettings["RZ_MobileClientUserWarningPrefix"] + responseText);
                        }
                    }
                }

                //Account Updation 
                updateAccount.Name = accountRequest.Name;
                updateAccount.Email = accountRequest.Email;
                updateAccount.Phone = accountRequest.Phone;

                await context.SaveChangesAsync();

                //Return Successful Response
                responseText = "Updated Account [" + accountID + "]";
                Services.Log.Info(responseText);
                return this.Request.CreateResponse(HttpStatusCode.Created, responseText);



            }
            else
            {
                //Return Failed Response
                string responseText;
                responseText = "Account not found by ProviderUserID [" + accountID + "]";
                Services.Log.Warn(responseText);
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, responseText);
            }
        }


        [HttpGet]
        [Route("api/accounts/exceloutput")]
        public HttpResponseMessage GenerateAccountExcel()
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

            List<Account> dbAccountCollection = new List<Account>() ;
            List<AccountExcelData> fullAccountCollection = new List<AccountExcelData>() ;


            stranddContext context = new stranddContext();


            //     List<Incident> objincident = context.Incidents.ToList();

            //Loading List of Accounts from DB Context
            dbAccountCollection = context.Accounts
                .Where(a => a.CreatedAt >= startTime)
               .Where(a => a.CreatedAt <= endTime)
                .ToList();

            foreach (var dbAccount in dbAccountCollection)
            {
                fullAccountCollection.Add(new AccountExcelData(dbAccount));
            }

            string strData = string.Empty;
            strData = "Id,Phone,Name,Email,ProviderUserID,Salt,SaltedAndHashedPassword,Version,RegisterDate,UpdatedAt,Deleted,IncidentSubmittedCount";

            DataTable table = ConvertListToDataTable(fullAccountCollection);

            ExcelHelper objexcel = new ExcelHelper();

            return objexcel.GetExcel(table, strData, "AccountHistoryReport");
        }
        static DataTable ConvertListToDataTable<AccountExcelData>(List<AccountExcelData> list)
        {
            //   List<IncidentExcelData[]> myList = list.ToArray();

            DataTable dataTable = new DataTable(typeof(AccountExcelData).Name);
            PropertyInfo[] props = typeof(AccountExcelData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ??
                    prop.PropertyType);
            }

            foreach (AccountExcelData item in list)
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
    }
}

