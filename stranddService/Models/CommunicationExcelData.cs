using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.Models
{
    public class CommunicationExcelData
    {
         public string Id { get; set; }
         public string Type { get; set; }
         public string Tag { get; set; }
         public string Status { get; set; }
         public string OperatorID { get; set; }
         public string IncidentID { get; set; }
         public string Notes { get; set; }
         public DateTimeOffset StartTime { get; set; }
         public DateTimeOffset EndTime { get; set; }
         public string Version { get; set; }
         public DateTimeOffset CreatedAt { get; set; }
         public DateTimeOffset UpdatedAt { get; set; }
         public string Deleted { get; set; }

    }
}