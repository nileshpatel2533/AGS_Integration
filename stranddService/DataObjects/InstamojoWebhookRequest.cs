using stranddService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.DataObjects
{
    public class InstamojoWebhookRequest
    {
        public string Payment_ID { get; set; }
        public string Status { get; set; }
        public string Buyer_Name { get; set; }
        public string Buyer { get; set; }
        public string Buyer_Phone { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public decimal Fees { get; set; }
        public string MAC { get; set; }
        public string Custom_Fields { get; set; }
    }
}
