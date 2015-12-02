using Exceptions;
using Microsoft.AspNet.SignalR;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Newtonsoft.Json;
using SendGrid;
using stranddService.DataObjects;
using stranddService.Helpers;
using stranddService.Hubs;
using stranddService.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Description;

namespace stranddService.Controllers
{
    public class PaymentController : ApiController
    {
        public ApiServices Services { get; set; }
        
        [Route("api/payments")]
        [ResponseType(typeof(Payment))]
        public async Task<IHttpActionResult> GetAllPayments()
        {
            Services.Log.Info("Payment Log Requested [API]");
            List<Payment> dbPaymentCollection = new List<Payment>();

            stranddContext context = new stranddContext();

            //Loading List of Accounts from DB Context
            dbPaymentCollection = await (context.Payments).ToListAsync<Payment>();

            //Return Successful Response
            Services.Log.Info("Payment Log Returned [API]");
            return Ok(dbPaymentCollection);
        }

        [HttpGet]
        [Route("api/payments/exceloutput")]
        public HttpResponseMessage GeneratePaymentExcel()
        {

            var queryStrings = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);

            string timeZoneDisplayString;
            TimeZoneInfo timeZoneRequest;
            DateTimeOffset startTime;
            DateTimeOffset endTime;

            string responseText;
            responseText = "Excel Output Requested -";



            if (queryStrings.ContainsKey("timezone"))
            {
                try
                {
                    timeZoneRequest = TimeZoneInfo.FindSystemTimeZoneById(queryStrings["timezone"]);
                }
                catch (TimeZoneNotFoundException)
                {
                    Services.Log.Warn("Unable to retrieve the requested Time Zone. Reverting to UTC.");
                    timeZoneRequest = TimeZoneInfo.Utc;
                }
                catch (InvalidTimeZoneException)
                {
                    Services.Log.Warn("Unable to retrieve the requested Time Zone. Reverting to UTC.");
                    timeZoneRequest = TimeZoneInfo.Utc;
                }
            }
            else
            {
                Services.Log.Warn("No Time Zone Requested. Reverting to UTC.");
                timeZoneRequest = TimeZoneInfo.Utc;
            }

            if (queryStrings.ContainsKey("starttime"))
            {
                if (!DateTimeOffset.TryParse(queryStrings["starttime"], out startTime))
                {
                    Services.Log.Warn("Unable to parse the requested Start Time [" + queryStrings["starttime"] + "]. Reverting to Min Value.");
                }
            }
            else
            {
                startTime = DateTimeOffset.MinValue;
                Services.Log.Warn("No Start Time Requested. Reverting to Min Value.");
            }

            if (queryStrings.ContainsKey("endtime"))
            {
                if (!DateTimeOffset.TryParse(queryStrings["endtime"], out endTime))
                {
                    endTime = DateTimeOffset.MaxValue;
                    Services.Log.Warn("Unable to parse the requested End Time [" + queryStrings["endtime"] + "]. Reverting to Max Value.");
                }
            }
            else
            {
                endTime = DateTimeOffset.MaxValue;
                Services.Log.Warn("No End Time Requested. Reverting to Max Value.");
            }

            timeZoneDisplayString = "[" + timeZoneRequest.DisplayName.ToString() + "]";
            responseText += " TimeZone " + timeZoneDisplayString;
            responseText += " StartTime [" + startTime.ToString() + "]";
            responseText += " EndTime [" + endTime.ToString() + "]";
            responseText += " [API]";

            Services.Log.Info(responseText);



            List<Payment> dbPaymentCollection = new List<Payment>();
            List<PaymentExcelData> fullPaymentCollection = new List<PaymentExcelData>();
            stranddContext context = new stranddContext();

            //Loading List of Incidents from DB Context
         


            dbPaymentCollection = context.Payments.Where(a => a.CreatedAt >= startTime)
               .Where(a => a.CreatedAt <= endTime).ToList();

          
            string strData = "PlatformPaymentID,Status,BuyerName,BuyerEmail,CreatedAt,UpdatedAt";
    

            DataTable table = ConvertListToDataTable(dbPaymentCollection);

            ExcelHelper objexcel = new ExcelHelper();

            return objexcel.GetExcel(table, strData, "PaymentHistoryReport");
         
        }
        static DataTable ConvertListToDataTable<Payment>(List<Payment> list)
        {
            //   List<IncidentExcelData[]> myList = list.ToArray();

            DataTable dataTable = new DataTable(typeof(Payment).Name);
            PropertyInfo[] props = typeof(Payment).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ??
                    prop.PropertyType);
            }

            foreach (Payment item in list)
            {
                var values = new object[props.Length];
                for (int i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;

        }

    }
}
