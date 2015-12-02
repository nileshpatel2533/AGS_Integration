using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace stranddService.Models
{
    public class AccountExcelData
    {
        public string Id { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ProviderUserID { get; set; }
        public string Salt { get; set; }
        public string SaltedAndHashedPassword { get; set; }
        public string Version { get; set; }
        public DateTimeOffset RegisterDate { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string Deleted { get; set; }
        public string IncidentSubmittedCount { get; set; }
        public AccountExcelData(Account baseAccount)
        {
            this.Id = baseAccount.Id;
            this.Phone = baseAccount.Phone;
            this.Name = baseAccount.Name;
            this.Email = baseAccount.Email;
            this.ProviderUserID = baseAccount.ProviderUserID;
            this.Salt = System.Convert.ToString( baseAccount.Salt);
            this.SaltedAndHashedPassword = System.Convert.ToString(baseAccount.SaltedAndHashedPassword);
            this.Version = System.Convert.ToString( baseAccount.Version);
           
            this.RegisterDate = (baseAccount.CreatedAt != null)  ? (DateTimeOffset) baseAccount.CreatedAt : DateTimeOffset.MinValue;
            this.UpdatedAt = (baseAccount.UpdatedAt != null) ? (DateTimeOffset)baseAccount.UpdatedAt : DateTimeOffset.MinValue;
            this.Deleted = System.Convert.ToString( baseAccount.Deleted);

            stranddContext context = new stranddContext();
            int SubmittedCount = context.Incidents.Count(u => u.ProviderUserID == baseAccount.ProviderUserID);

            this.IncidentSubmittedCount = System.Convert.ToString(SubmittedCount);


        }
    }
}