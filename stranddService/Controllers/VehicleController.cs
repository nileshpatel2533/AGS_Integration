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
    public class VehicleController : ApiController
    {
        public ApiServices Services { get; set; }

        [Route("api/vehicles")]
        [ResponseType(typeof(Vehicle))]
        public async Task<IHttpActionResult> GetAllVehicleInfos()
        {
            Services.Log.Info("Vehicle Log Requested [API]");
            List<Vehicle> dbVehicleCollection = new List<Vehicle>();

            stranddContext context = new stranddContext();

            //Loading List of Incidents from DB Context
            dbVehicleCollection = await (context.Vehicles).ToListAsync<Vehicle>();

            //Return Successful Response
            Services.Log.Info("Vehicle Log Returned [API]");
            return Ok(dbVehicleCollection);
        }

        [Route("api/vehicles/new")]
        public async Task<HttpResponseMessage> CustomerNewVehicle(VehicleRequest vehicleRequest)
        {
            Services.Log.Info("New Vehicle Registration Request [API]");
            bool accountLinkExists = false;

            // Get the logged-in user.
            var currentUser = this.User as ServiceUser;

            stranddContext context = new stranddContext();

            //Checks for unique Registration Number
            Vehicle vehicle = await context.Vehicles.Where(a => a.RegistrationNumber == vehicleRequest.RegistrationNumber).SingleOrDefaultAsync();

            if (vehicle != null)
            {
                //Return Failed Response
                string responseText = "Vehicle Already Exists. Registration Number [" + vehicle.RegistrationNumber + "]";
                Services.Log.Warn(responseText);
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, WebConfigurationManager.AppSettings["RZ_MobileClientUserWarningPrefix"] + responseText);
            }
            else
            {
                //Vehicle Creation 
                Vehicle newVehicle = new Vehicle
                {
                    Id = Guid.NewGuid().ToString(),
                    Make = vehicleRequest.Make,
                    Model = vehicleRequest.Model,
                    Year = vehicleRequest.Year,
                    Color = vehicleRequest.Color,
                    RegistrationNumber = vehicleRequest.RegistrationNumber
                };
                context.Vehicles.Add(newVehicle);

                
                if (currentUser.Id != null)
                {
                    

                    //Adding the VehicleAccountLink
                    VehicleAccountLink newVehicleAccountLink = new VehicleAccountLink
                    {
                        Id = Guid.NewGuid().ToString(),
                        VehicleGUID = newVehicle.Id,
                        UserProviderID = currentUser.Id
                    };

                    context.VehicleAccountLinks.Add(newVehicleAccountLink);
                    accountLinkExists = true;
                }

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Services.Log.Error("Property: " + validationError.PropertyName + " | Error: " + validationError.ErrorMessage);                            
                        }
                    }
                }                

                Services.Log.Info("New Vehicle Created #" + newVehicle.Id);
                if (accountLinkExists)
                {
                    Services.Log.Info("Vehicle Linked to " + currentUser.Id);
                }
                else
                {
                    Services.Log.Info("Vehicle Not Linked to Account " + currentUser.Id);
                }

                VehicleRequestResponse returnObject = new VehicleRequestResponse { VehicleGUID = newVehicle.Id };
                string responseText = JsonConvert.SerializeObject(returnObject);

                return this.Request.CreateResponse(HttpStatusCode.Created, responseText);
            }
        }
        [Route("api/vehicles/update")]
        public async Task<HttpResponseMessage> CustomerUpdateVehicle(VehicleRequest vehicleRequest)
        {                    
            Services.Log.Info("Update Vehicle Request [API]");

            stranddContext context = new stranddContext();

            //Utilizes the Registration Number from Passed in Request as the Identifier
            Vehicle updateVehicle = await context.Vehicles.Where(a => a.RegistrationNumber == vehicleRequest.RegistrationNumber).SingleOrDefaultAsync();

            if (updateVehicle != null)
            {
                //Vehicle Updation 
                updateVehicle.Make = vehicleRequest.Make;
                updateVehicle.Model = vehicleRequest.Model;
                updateVehicle.Year = vehicleRequest.Year;
                updateVehicle.Color = vehicleRequest.Color;

                await context.SaveChangesAsync();

                string responseText = "Updated Vehicle [VehicleGUID: " + updateVehicle.Id + "]";

                //Return Successful Response
                Services.Log.Info(responseText);
                return this.Request.CreateResponse(HttpStatusCode.Created, responseText);
            }
            else
            {
                //Return Failed Response
                Services.Log.Warn("Vehicle not found by Registration Number [" + vehicleRequest.RegistrationNumber + "]");
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Vehicle not found by Registration Number [" + vehicleRequest.RegistrationNumber + "]");              
            }
        }
    }
}
