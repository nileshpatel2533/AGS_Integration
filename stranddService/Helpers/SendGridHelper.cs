using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;

namespace stranddService.Helpers
{
    public static class SendGridHelper
    {
        public static MailAddress GetAppFrom()
        {
            return new MailAddress(WebConfigurationManager.AppSettings["RZ_SysAdminEmail"], WebConfigurationManager.AppSettings["RZ_SysAdminAlias"]);
        }

        public static NetworkCredential GetNetCreds()
        {
            // Create network credentials to access your SendGrid account.
            var username = WebConfigurationManager.AppSettings["RZ_SendGridUser"];
            var pswd = WebConfigurationManager.AppSettings["RZ_SendGridPass"];
            var credentials = new NetworkCredential(username, pswd);

            return credentials;
        }
    }
}