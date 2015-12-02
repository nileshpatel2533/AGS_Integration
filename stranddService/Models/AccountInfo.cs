using System;
using System.Linq;

namespace stranddService.Models
{
    public class AccountInfo
    {
        public string AccountGUID { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ProviderUserID { get; set; }
        public DateTimeOffset? RegisteredAt { get; set; }

        //Constructor built on ProviderUserID LookUp
        public AccountInfo(string providerUserID)
        {
            if (string.IsNullOrEmpty(providerUserID))
            {
                this.AccountGUID = "NO ASSOCIATED USER";
                this.Name = "NO USER";
                this.Phone = "NO PHONE";
                this.Email = "NO EMAIL";
                this.ProviderUserID = providerUserID;
                this.RegisteredAt = System.DateTime.Now;
            }
            else
            {
                stranddContext context = new stranddContext();

                //[RECONSIDER] Look up User based upon ProviderUserID
                Account returnUser = context.Accounts
                    .Where(u => u.ProviderUserID == providerUserID)
                    .FirstOrDefault();

                if (returnUser == null)
                {
                    this.AccountGUID = "NO USER ACCOUNT";
                    this.Name = providerUserID;
                    this.Phone = "NO PHONE";
                    this.Email = "NO EMAIL";
                    this.ProviderUserID = providerUserID;
                    this.RegisteredAt = System.DateTime.Now;
                }
                else
                {
                    this.AccountGUID = returnUser.Id;
                    this.Name = returnUser.Name;
                    this.Phone = returnUser.Phone;
                    this.Email = returnUser.Email;
                    this.ProviderUserID = returnUser.ProviderUserID;
                    this.RegisteredAt = returnUser.CreatedAt;
                }
            }
        }

    }
}