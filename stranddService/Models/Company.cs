using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.Models
{
    public class Company : EntityData
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}