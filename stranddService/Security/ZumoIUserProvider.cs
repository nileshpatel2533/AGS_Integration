using Microsoft.AspNet.SignalR;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stranddService.Security
{
    class ZumoIUserProvider : IUserIdProvider
    {
        public string GetUserId(IRequest request)
        {
            // your logic to fetch a user identifier goes here.

            // for example:
            var userID = ((ServiceUser)request.User).Id;

            return userID.ToString();
        }
    }
}
