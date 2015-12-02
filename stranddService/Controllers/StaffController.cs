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
    public class StaffController : ApiController
    {
        public ApiServices Services { get; set; }

        [Route("api/staffassignments")]
        [ResponseType(typeof(AccountRole))]
        public async Task<IHttpActionResult> GetAllStaffAssignments()
        {
            Services.Log.Info("Full Staff Assisgnments Log Requested [API]");
            List<AccountRole> dbAccountRoleCollection = new List<AccountRole>();

            stranddContext context = new stranddContext();

            //Loading List of Incidents from DB Context
            dbAccountRoleCollection = await (context.AccountRoles).ToListAsync<AccountRole>();

            //Return Successful Response
            Services.Log.Info("Full Staff Assisgnments Log Returned [API]");
            return Ok(dbAccountRoleCollection);
        }

        [Route("api/staffassignments/{companyGUID}")]
        [ResponseType(typeof(AccountRole))]
        public async Task<IHttpActionResult> GetCompanyStaffAssignments(string companyGUID)
        {
            Services.Log.Info("Company  [" + companyGUID + "] Staff Assisgnments Log Requested [API]");
            List<AccountRole> dbAccountRoleCollection = new List<AccountRole>();
            List<StaffAssignmentInfo> fullStaffAssignmentCollection = new List<StaffAssignmentInfo>();

            stranddContext context = new stranddContext();

            //Loading List of Incidents from DB Context
            dbAccountRoleCollection = await context.AccountRoles.Where(a => a.CompanyGUID == companyGUID).ToListAsync<AccountRole>();

            //Expanding Account Role Info
            foreach (AccountRole accountRole in dbAccountRoleCollection) { fullStaffAssignmentCollection.Add(new StaffAssignmentInfo(accountRole)); }


            //Return Successful Response
            Services.Log.Info("Company  [" + companyGUID + "] Staff Assisgnments Log Returned [API]");
            return Ok(fullStaffAssignmentCollection);
        }

        [Route("api/staffassignments/currentuser/{companyGUID}")]
        [ResponseType(typeof(AccountRole))]
        public async Task<IHttpActionResult> GetCurrentAccountRoles(string companyGUID)
        {
            List<AccountRole> dbRoleCollection = new List<AccountRole>();

            // Get the logged-in user.
            var currentUser = this.User as ServiceUser;

            Services.Log.Info("Find Company  [" + companyGUID + "] Staff Assignments for Current User");

            stranddContext context = new stranddContext();

            //Loading List of Account Roles from DB Context
            dbRoleCollection = await context.AccountRoles.Where(a => a.UserProviderID == currentUser.Id)
                .Where(b => b.CompanyGUID == companyGUID).ToListAsync<AccountRole>();

            if (dbRoleCollection.Count > 0)
            {
                string responseText = "Returned Company  [" + companyGUID + "] Staff Assignments for Account [" + currentUser.Id + "] " + currentUser.Id;
                Services.Log.Info(responseText);
                return Ok(dbRoleCollection);
            }
            else
            {
                string responseText = "No Company  [" + companyGUID + "] Staff Assignments for Account [" + currentUser.Id + "] " + currentUser.Id;
                Services.Log.Info(responseText);
                AccountRole noneRole = new AccountRole { CompanyGUID = companyGUID, RoleAssignment = "NONE", UserProviderID = currentUser.Id };
                dbRoleCollection.Add(noneRole);
                return Ok(dbRoleCollection);
            }

        }

        [Route("api/staffassignments/new")]
        public async Task<HttpResponseMessage> NewStaffAssignment(AccountRoleRequest accountRoleRequest)
        {
            Services.Log.Info("New Staff Assignment Request [API]");
            string responseText;

            stranddContext context = new stranddContext();            

            //Checks for existing Link References
            Account lookupAccount = await context.Accounts.Where(a => a.ProviderUserID == accountRoleRequest.UserProviderID).SingleOrDefaultAsync();
            Company lookupCompany = await context.Companies.Where(a => a.Id == accountRoleRequest.CompanyGUID).SingleOrDefaultAsync();

            if (lookupAccount == null) { responseText = "Account not found [" + accountRoleRequest.UserProviderID + "]"; Services.Log.Warn(responseText); return this.Request.CreateResponse(HttpStatusCode.BadRequest, responseText); }
            if (lookupCompany == null) { responseText = "Company not found [" + accountRoleRequest.CompanyGUID + "]"; Services.Log.Warn(responseText);  return this.Request.CreateResponse(HttpStatusCode.BadRequest, responseText); }
            if (accountRoleRequest.RoleAssignment == null) { responseText = "No Role Assignment Defined"; Services.Log.Warn(responseText); return this.Request.CreateResponse(HttpStatusCode.BadRequest, responseText); }

            AccountRole lookupAccountRole = await context.AccountRoles.Where(a => a.UserProviderID == accountRoleRequest.UserProviderID)
                .Where(b => b.CompanyGUID == accountRoleRequest.CompanyGUID)
                .Where(c => c.RoleAssignment == accountRoleRequest.RoleAssignment).SingleOrDefaultAsync();

            if (lookupAccountRole == null) { 
            //Staff Assignment Creation 
            AccountRole newAccountRole = new AccountRole
            {
                Id = Guid.NewGuid().ToString(),
                RoleAssignment = accountRoleRequest.RoleAssignment,
                CompanyGUID = accountRoleRequest.CompanyGUID,
                UserProviderID = accountRoleRequest.UserProviderID
            };

            context.AccountRoles.Add(newAccountRole);
            await context.SaveChangesAsync();

            responseText = "Staff Assignment Successfully Generated";
            Services.Log.Info(responseText);
            return this.Request.CreateResponse(HttpStatusCode.Created, responseText);
            }
            else
            {
                responseText = "Staff Assignment Already Exists";
                Services.Log.Info(responseText);
                return this.Request.CreateResponse(HttpStatusCode.OK, responseText);
            }
        }

        [Route("api/staffassignments/remove")]
        public async Task<HttpResponseMessage> RemoveStaffAssignment(AccountRoleRequest accountRoleRequest)
        {
            Services.Log.Info("Remove Staff Assignment Request [API]");
            string responseText;

            stranddContext context = new stranddContext();

            if (accountRoleRequest.RoleAssignment == null) { responseText = "No Role Assignment Defined"; Services.Log.Warn(responseText); return this.Request.CreateResponse(HttpStatusCode.BadRequest, responseText); }

            AccountRole lookupAccountRole = await context.AccountRoles.Where(a => a.UserProviderID == accountRoleRequest.UserProviderID)
                .Where(b => b.CompanyGUID == accountRoleRequest.CompanyGUID)
                .Where(c => c.RoleAssignment == accountRoleRequest.RoleAssignment).SingleOrDefaultAsync();

            if (lookupAccountRole == null)
            {
                responseText = "Staff Assignment Not Found";
                Services.Log.Info(responseText);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, responseText);                
            }
            else
            {
                //Staff Assignment Removal
                context.AccountRoles.Remove(lookupAccountRole);
                await context.SaveChangesAsync();

                responseText = "Staff Assignment Successfully Removed";
                Services.Log.Info(responseText);
                return this.Request.CreateResponse(HttpStatusCode.Created, responseText);
            }
        }
    }
}
