using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.Models
{
    public class ExceptionEntry : EntityData
    {
        public string ExceptionText { get; set; }
        public string Source { get; set; }
    }
}