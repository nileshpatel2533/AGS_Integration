using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.Models
{
    public class VehicleAccountLink : EntityData
    {
        public string VehicleGUID { get; set; }
        public string UserProviderID { get; set; }
    }
}