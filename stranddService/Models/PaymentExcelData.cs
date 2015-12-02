using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace stranddService.Models
{
    public class PaymentExcelData
    {

         public string Id { get; set; }
         public string PlatformPaymentID { get; set; }
         public string Status { get; set; }
         public string BuyerName { get; set; }
         public string BuyerEmail { get; set; }
         public string BuyerPhone { get; set; }
         public string Currency { get; set; }
         public string Amount { get; set; }
         public string Fees { get; set; }
         public string AuthenticationCode { get; set; }
         public string PaymentPlatform { get; set; }
         public string ProviderUserID { get; set; }
         public string Version { get; set; }
         public DateTimeOffset CreatedAt { get; set; }
         public DateTimeOffset UpdatedAt { get; set; }
         public string Deleted { get; set; }
         public string IncidentGUID { get; set; }

    }
}