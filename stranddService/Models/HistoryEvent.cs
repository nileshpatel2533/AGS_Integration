using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace stranddService.Models
{
    public class HistoryEvent : EntityData
    {
        public string Code { get; set; }
        public string Attribute { get; set; }
        public string ReferenceID { get; set; }
        public string AdminID { get; set; }
        public string CustomerID { get; set; }
        public string ProviderID { get; set; }

        public static async Task logHistoryEventAsync(string code, string attribute, string referenceID, string adminID, string customerID, string providerID)
        {
            HistoryEvent newEvent = new HistoryEvent()
            {
                Id = Guid.NewGuid().ToString(),
                Code = code,
                Attribute = attribute,
                ReferenceID = referenceID,
                AdminID = adminID,
                CustomerID = customerID,
                ProviderID = providerID
            };

            stranddContext context = new stranddContext();
            context.HistoryLog.Add(newEvent);
            await context.SaveChangesAsync();
        }

    }

}