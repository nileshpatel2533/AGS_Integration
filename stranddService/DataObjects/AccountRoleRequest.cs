using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.DataObjects
{
    public class AccountRoleRequest
    {
        public string RoleAssignment { get; set; }
        public string UserProviderID { get; set; }
        public string CompanyGUID { get; set; }
    }
}