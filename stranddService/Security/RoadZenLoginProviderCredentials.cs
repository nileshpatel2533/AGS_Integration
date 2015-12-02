using Microsoft.WindowsAzure.Mobile.Service.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.Security
{
    public class RoadZenLoginProviderCredentials : ProviderCredentials
    {
        public RoadZenLoginProviderCredentials()
            : base(RoadZenLoginProvider.ProviderName)
        {
        }
    }
}