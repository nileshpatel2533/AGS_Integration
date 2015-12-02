using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.DataObjects
{
    public class ServiceLogSCMResponse
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Level { get; set; }
        public string PID { get; set; }
        public string Message { get; set; }
    }

}