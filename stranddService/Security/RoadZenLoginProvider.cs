using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Newtonsoft.Json.Linq;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace stranddService.Security
{

    public class RoadZenLoginProvider : LoginProvider
    {
        public const string ProviderName = "RoadZen";

        public override string Name
        {
            get { return ProviderName; }
        }

        public RoadZenLoginProvider(IServiceTokenHandler tokenHandler)
            : base(tokenHandler)
        {
            this.TokenLifetime = new TimeSpan(30, 0, 0, 0);
        }

        public override void ConfigureMiddleware(IAppBuilder appBuilder, ServiceSettingsDictionary settings)
        {
            // Not Applicable - used for federated identity flows
            return;
        }

        public override ProviderCredentials ParseCredentials(JObject serialized)
        {
            if (serialized == null)
            {
                throw new ArgumentNullException("serialized");
            }

            return serialized.ToObject<RoadZenLoginProviderCredentials>();
        }

        public override ProviderCredentials CreateCredentials(ClaimsIdentity claimsIdentity)
        {
            if (claimsIdentity == null)
            {
                throw new ArgumentNullException("claimsIdentity");
            }

            string formattedAccountGUID = (claimsIdentity.FindFirst("AccountGUID").Value).Replace("-","").ToUpper();
            RoadZenLoginProviderCredentials credentials = new RoadZenLoginProviderCredentials
            {
                UserId = this.TokenHandler.CreateUserId(this.Name, formattedAccountGUID)
            };

            return credentials;
        }
    }

}