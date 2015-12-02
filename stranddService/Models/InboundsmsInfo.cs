using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace stranddService.Models
{
    public class InboundsmsInfo : ApiController
    {
         public string CommunicationGUID { get; set; }
        public string Type { get; set; }
        public string Tag { get; set; }
        public string Status { get; set; }
        public AccountInfo OperatorAccountInfo { get; set; }
        public string IncidentGUID { get; set; }
        public string Notes { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }

        public string Source { get; set; }
        public string Text { get; set; }
        public InboundsmsInfo(CommunicationEntry dbCommunication)
        {
          
            this.Type = dbCommunication.Type;
            this.Tag = dbCommunication.Tag;
            this.Text = dbCommunication.Text;
           
            
        }
    }
}
