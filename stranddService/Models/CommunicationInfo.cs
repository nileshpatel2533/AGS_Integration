using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.Models
{
    public class CommunicationInfo
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

        public CommunicationInfo(CommunicationEntry dbCommunication)
        {
            this.CommunicationGUID = dbCommunication.Id;
            this.Type = dbCommunication.Type;
            this.Tag = dbCommunication.Tag;
            this.Status = dbCommunication.Status;
            this.IncidentGUID = (dbCommunication.IncidentID != null) ? dbCommunication.IncidentID : "NONE";
            this.Notes = dbCommunication.Notes;
            this.StartTime = (dbCommunication.StartTime != null) ? dbCommunication.StartTime : null; //DateTime.Now;
            this.EndTime = (dbCommunication.EndTime != null) ? dbCommunication.EndTime : null; //DateTime.Now;
            
            //Create UserInfo Object based upon Passed in ProviderUserID [Call to DB Made in Constructor]
            AccountInfo lookupUser = new AccountInfo(dbCommunication.OperatorID);
            this.OperatorAccountInfo = lookupUser;
        }
    }

    
}