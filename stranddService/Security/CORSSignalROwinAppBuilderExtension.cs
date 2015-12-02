using Microsoft.Owin.Cors;
using Microsoft.WindowsAzure.Mobile.Service.Config;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace stranddService.Security
{
    public class CORSSignalROwinAppBuilderExtension : OwinAppBuilderExtension
    {
        public CORSSignalROwinAppBuilderExtension(HttpConfiguration config)
            : base(config)
        {
        }

        public override void Configure(IAppBuilder appBuilder)
        {
            appBuilder.UseCors(CorsOptions.AllowAll);
            base.Configure(appBuilder);
        }
    }
}