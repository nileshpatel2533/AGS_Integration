using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.Models
{
    public class CommunicationEntry : EntityData
    {
        public string Type { get; set; }
        public string Tag { get; set; }
        public string Status { get; set; }
        public string OperatorID { get; set; }
        public string IncidentID { get; set; }
        public string Notes { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public string Source { get; set; }
        public string Text { get; set; }
    }
}