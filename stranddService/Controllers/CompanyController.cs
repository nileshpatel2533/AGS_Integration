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
    public class CompanyController : ApiController
    {
        public ApiServices Services { get; set; }

        [Route("api/companies")]
        [ResponseType(typeof(Company))]
        public async Task<IHttpActionResult> GetAllCompanies()
        {
            Services.Log.Info("Company Log Requested [API]");
            List<Company> dbCompanyCollection = new List<Company>();

            stranddContext context = new stranddContext();

            //Loading List of Incidents from DB Context
            dbCompanyCollection = await (context.Companies).ToListAsync<Company>();

            //Return Successful Response
            Services.Log.Info("Company Log Returned [API]");
            return Ok(dbCompanyCollection);
        }

        [Route("api/companies/new")]
        public async Task<HttpResponseMessage> NewCompany(CompanyRequest companyRequest)
        {
            Services.Log.Info("New Company Request [API]");

            stranddContext context = new stranddContext();

            //Checks for unique Company Name
            Company company = await context.Companies.Where(a => a.Name == companyRequest.Name).SingleOrDefaultAsync();

            if (company != null)
            {
                //Return Failed Response
                string responseText = "Company Already Exists [" + company.Name + "].";
                Services.Log.Warn(responseText);
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, responseText);
            }
            else
            {
                //Vehicle Creation 
                Company newCompany = new Company
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = companyRequest.Name,
                    Type = companyRequest.Type

                };
                context.Companies.Add(newCompany);
                await context.SaveChangesAsync();

                string responseText = ("New Company Created #" + newCompany.Id);
                Services.Log.Info(responseText);

                return this.Request.CreateResponse(HttpStatusCode.Created, responseText);
            }
        }
    }
}
