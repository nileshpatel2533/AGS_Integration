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
    public class CostingSlabController : ApiController
    {
        public ApiServices Services { get; set; }


        [Route("api/costingslab")]
        [ResponseType(typeof(CostingSlab))]
        public async Task<IHttpActionResult> GetAllCostingSlab()
        {
            Services.Log.Info("Full CostingSlab Log Requested [API]");
            List<CostingSlab> dbCostingSlabCollection = new List<CostingSlab>();

            stranddContext context = new stranddContext();

            //Loading List of CostingSlab from DB Context
            dbCostingSlabCollection = await (context.CostingSlabs.OrderBy(a=>a.Status)).ToListAsync<CostingSlab>();

            //Return Successful Response
            Services.Log.Info("Full CostingSlab Log Returned [API]");
            return Ok(dbCostingSlabCollection);
        }



        [Route("api/costingslab/findbycostingslabid")]
        [ResponseType(typeof(CostingSlab))]
        public async Task<IHttpActionResult> GetCostingSlabInfo(string CostingSlabid)
        {

            List<CostingSlab> dbCostingSlab = new List<CostingSlab>();
           
            // Get the logged-in user.
           // var currentUser = this.User as ServiceUser;

            Services.Log.Info("CostingSlabID [" + CostingSlabid + "] CostingSlab Information Requested [API]");

            stranddContext context = new stranddContext();

                dbCostingSlab = await context.CostingSlabs.Where(a => a.Id == CostingSlabid)
             .ToListAsync<CostingSlab>();

                //Return Successful Response
                Services.Log.Info("CostingSlabID [" + CostingSlabid + "] CostingSlab Information Returned");
                return Ok(dbCostingSlab);
         
        }

         [AuthorizeLevel(AuthorizationLevel.Anonymous)]
        [Route("api/costingslabs/new")]
        public async Task<HttpResponseMessage> NewCostingSlab(CostingSlabRequest costingSlabRequest)
        {

            Services.Log.Info("New CostingSlab Request [API]");
            string responseText;

            stranddContext context = new stranddContext();

            CostingSlab newCostingSlab = new CostingSlab
            {
                
                Id = Guid.NewGuid().ToString(),
                IdentifierGUID = costingSlabRequest.IdentifierGUID,
                ServiceType = costingSlabRequest.ServiceType,
                Status = costingSlabRequest.Status,
                StartTime = costingSlabRequest.StartTime,
                EndTime = costingSlabRequest.EndTime,
                BaseCharge = costingSlabRequest.BaseCharge,
                BaseKilometersFloor = costingSlabRequest.BaseKilometersFloor,
                ExtraKilometersCharge = costingSlabRequest.ExtraKilometersCharge
              
                // Version = new Byte[125],
                //CreatedAt = costingSlabRequest.StartTime,
                //UpdatedAt=costingSlabRequest.StartTime,
                //Deleted= false

            };

            context.CostingSlabs.Add(newCostingSlab);
            await context.SaveChangesAsync();

            responseText = "CostingSlab Successfully Generated";
            Services.Log.Info(responseText);
            return this.Request.CreateResponse(HttpStatusCode.Created, responseText);
        }



        //[Route("api/costingslabs/remove")]
        //public async Task<HttpResponseMessage> RemoveCostingSlabs(CostingSlabRequest costingSlabRequest)
        //{
        //    Services.Log.Info("Remove Costing Slabs Request [API]");
        //    string responseText;

        //    stranddContext context = new stranddContext();
        //    if (costingSlabRequest.ID == null)
        //    {
        //        responseText = "No Costing Slabs Defined";
        //        Services.Log.Warn(responseText);
        //        return this.Request.CreateResponse(HttpStatusCode.BadRequest, responseText);
        //    }

        //    CostingSlab lookupCostingSlabs = await context.CostingSlabs.Where(a => a.Id == costingSlabRequest.ID)
               
        //        .SingleOrDefaultAsync();

        //    if (lookupCostingSlabs == null)
        //    {
        //        responseText = "Costing Slabs Not Found";
        //        Services.Log.Info(responseText);
        //        return this.Request.CreateResponse(HttpStatusCode.NotFound, responseText);
        //    }
        //    else
        //    {
        //        //Staff Assignment Removal
        //        context.CostingSlabs.Remove(lookupCostingSlabs);
        //        await context.SaveChangesAsync();

        //        responseText = "Costing Slabs Successfully Removed";
        //        Services.Log.Info(responseText);
        //        return this.Request.CreateResponse(HttpStatusCode.Created, responseText);
        //    }


        //}



       [Route("api/costingslabs/remove")]
       [ResponseType(typeof(CostingSlab))]
        public async Task<HttpResponseMessage> RemoveCostingSlabs(string id)
        {
            List<CostingSlab> dbCostingSlab = new List<CostingSlab>();

         
            Services.Log.Info("CostingSlabID [" + id + "] Remove Costing Slabs Request [API]");
            string responseText;

            stranddContext context = new stranddContext();

            //if (id == "")
            //{
            //    responseText = "No Costing Slabs Defined";
            //    Services.Log.Warn(responseText);
            //    return this.Request.CreateResponse(HttpStatusCode.BadRequest, responseText);
                
            //}
            CostingSlab lookupCostingSlabs = await context.CostingSlabs.Where(a => a.Id == id).SingleOrDefaultAsync();

            if (lookupCostingSlabs == null)
            {
                responseText = "Costing Slabs Not Found";
                Services.Log.Info(responseText);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, responseText);
            }
            else
            {
                //Staff Assignment Removal
                context.CostingSlabs.Remove(lookupCostingSlabs);
                await context.SaveChangesAsync();

                responseText = "Costing Slabs Successfully Removed";
                Services.Log.Info(responseText);
                return this.Request.CreateResponse(HttpStatusCode.Created, responseText);
            }
     
        }



       [Route("api/costingslabs/update")]
       public async Task<HttpResponseMessage> UpdateCostingSlabs(CostingSlabRequest costingSlabRequest)
       {
           Services.Log.Info("Update Costing Slabs Request");

           stranddContext context = new stranddContext();

           //Determine Account ProviderUserID for Updation based upon Request (or Authenticated Token)
           string costingslabsID;

           if (costingSlabRequest.ID != null)
           {
               costingslabsID = costingSlabRequest.ID; 
           }

           else
           {
               var currentUser = this.User as ServiceUser;

               if (currentUser != null) { costingslabsID = currentUser.Id; }
               else
               {
                   string responsetext = "No Costing Slabs ID";
                   Services.Log.Warn(responsetext);
                   return this.Request.CreateResponse(HttpStatusCode.BadRequest, responsetext);

               }
           }

           //Looks up the Account for Update by ProviderUserID
           CostingSlab updateCostingSlab = await context.CostingSlabs.Where(a => a.Id == costingSlabRequest.ID).SingleOrDefaultAsync();

           if (updateCostingSlab != null)
           {
               string responseText;

               //Account Updation 
               updateCostingSlab.IdentifierGUID = costingSlabRequest.IdentifierGUID;
               updateCostingSlab.ServiceType = costingSlabRequest.ServiceType;
               updateCostingSlab.Status = costingSlabRequest.Status;
               updateCostingSlab.StartTime = costingSlabRequest.StartTime;
               updateCostingSlab.EndTime = costingSlabRequest.EndTime;
               updateCostingSlab.BaseCharge = costingSlabRequest.BaseCharge;
               updateCostingSlab.BaseKilometersFloor = costingSlabRequest.BaseKilometersFloor;
               updateCostingSlab.ExtraKilometersCharge = costingSlabRequest.ExtraKilometersCharge;

               await context.SaveChangesAsync();

               //Return Successful Response
               responseText = "Updated Costing Slabs [" + costingslabsID + "]";
               Services.Log.Info(responseText);
               return this.Request.CreateResponse(HttpStatusCode.Created, responseText);

           }
           else
           {
               //Return Failed Response
               string responseText;
               responseText = "Costing Slabs not found by ID [" + costingslabsID + "]";
               Services.Log.Warn(responseText);
               return this.Request.CreateResponse(HttpStatusCode.BadRequest, responseText);
           }
       }

    }
}
