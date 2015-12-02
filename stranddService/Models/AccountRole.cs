using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.Models
{
    public class AccountRole : EntityData
    {
        public string RoleAssignment { get; set; }
        public string UserProviderID { get; set; }
        public string CompanyGUID { get; set; }
    }
}