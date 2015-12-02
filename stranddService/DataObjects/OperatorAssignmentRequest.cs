using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.DataObjects
{
    public class OperatorAssignmentRequest
    {
        public string IncidentGUID { get; set; }
        public string OperatorUserProviderID { get; set; }
    }
}