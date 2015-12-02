using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using stranddService.DataObjects;
using stranddService.Models;
using System.Data.Entity.Migrations;
using stranddService.Migrations;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Configuration;
using Microsoft.WindowsAzure.Mobile.Service.Config;
using Autofac;
using stranddService.Security;



namespace stranddService
{
    public static class WebApiConfig
    {
         public static void Register()
        {

            // Use this class to set configuration options for your mobile service
            ConfigOptions options = new ConfigOptions();
            options.PushAuthorization = Microsoft.WindowsAzure.Mobile.Service.Security.AuthorizationLevel.User;
            
             var configBuilder = new ConfigBuilder(options, (httpconfig, ioc) =>
            {
                ioc.RegisterInstance(new CORSSignalROwinAppBuilderExtension(httpconfig)).As<IOwinAppBuilderExtension>();
            });

            //Config Setting for Accessible Web Client
            //options.CorsPolicy = new System.Web.Http.Cors.EnableCorsAttribute("http://strandd.azurewebsites.net, http://strandd-dev.azurewebsites.net", "*", "*");
            //config.EnableCors(options.CorsPolicy);

            // Use this class to set WebAPI configuration options
            HttpConfiguration config = ServiceConfig.Initialize(configBuilder);      
           
            // To display errors in the browser during development, uncomment the following
            // line. Comment it out again when you deploy your service for production use.
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            //config.MapHttpAttributeRoutes();

            // Initialize SignalR
            //var idProvider = new ZumoIUserProvider();
            //GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => idProvider );          
            SignalRExtensionConfig.Initialize();

             
            //EF DB-Migrator
            var migrator = new DbMigrator(new Configuration());
            migrator.Update();
            

            //This is for full Scema Drop DB Init [unused]
            //Database.SetInitializer(new stranddInitializer());

            //This tells the local mobile service project to run as if it is being hosted in Azure, including honoring the AuthorizeLevel settings.
            config.SetIsHosted(true);
        }
    }

    public class stranddInitializer : ClearDatabaseSchemaIfModelChanges<stranddContext>
    {
        protected override void Seed(stranddContext context)
        {
            
        }
    }
}

