using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.DataObjects
{
    public class LoginRequest
    {
        public String Phone { get; set; }
        public String Password { get; set; }
    }
}