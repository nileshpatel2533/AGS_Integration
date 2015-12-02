using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Web;

namespace stranddService.Helpers
{
    public class ExcelHelper
    {

        public HttpResponseMessage GetExcel(DataTable table, string strDataAppend, string ExcelName)
        {

            

            StringBuilder str = new StringBuilder();
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            string[] strArray = strDataAppend.Split(',');

          

            for (int i = 0; i < strArray.Count(); i++)
            {
                str.Append("<td><b><font face=Arial Narrow size=3>" + strArray[i].ToString() + "</font></b></td>");
            }
            str.Append("</tr>");


            foreach (DataRow row in table.Rows)
            {
                str.Append("<tr>");
                for (int i = 0; i < strArray.Count(); i++)
                {
                    str.Append("<td><font face=Arial Narrow size=3>" + row[strArray[i].ToString()] + "</font></td>");
                }

                str.Append("</tr>");
            }

            str.Append("</table>");

            HttpContext.Current.Response.AddHeader("content-disposition", "attachment; filename=" + ExcelName + ".xls");
            HttpContext.Current.Response.SetCookie(new HttpCookie("fileDownload", "true") { Path = "/" });


            byte[] excelData = System.Text.Encoding.UTF8.GetBytes(str.ToString());

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new MemoryStream(excelData);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = ExcelName + ".xls"
            };



            return result; ;
        }


        public static DataTable ConvertListToDataTable<IncidentExcelData>(List<IncidentExcelData> list)
        {
            //   List<IncidentExcelData[]> myList = list.ToArray();

            DataTable dataTable = new DataTable(typeof(IncidentExcelData).Name);
            PropertyInfo[] props = typeof(IncidentExcelData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ??
                    prop.PropertyType);
            }

            foreach (IncidentExcelData item in list)
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