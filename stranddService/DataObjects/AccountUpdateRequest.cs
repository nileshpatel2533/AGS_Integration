using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.DataObjects
{
    public class AccountRequest
    {
        public string ProviderUserID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}