using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stranddService.Models
{
    public class StaffAssignmentInfo
    {
        public string UserProviderID { get; set; }
        public string AccountGUID { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public string RoleAssignment { get; set; }

        public StaffAssignmentInfo(AccountRole accountRole)
        {
            stranddContext context = new stranddContext();

            AccountRole returnAccountRole = accountRole;

            if (returnAccountRole != null)
            {
                this.UserProviderID = returnAccountRole.UserProviderID;
                this.RoleAssignment = returnAccountRole.RoleAssignment;

                Account returnUser = context.Accounts
                    .Where(u => u.ProviderUserID == returnAccountRole.UserProviderID)
                    .FirstOrDefault();
                if (returnUser != null)
                {
                    this.AccountGUID = returnUser.Id;
                    this.Name = returnUser.Name;
                    this.Phone = returnUser.Phone;
                    this.Email = returnUser.Email;
                }
                else
                {
                    this.Name = "NONE";
                    this.Phone = "NONE";
                    this.Email = "NONE";
                }

                Company returnCompany = context.Companies
                    .Where(u => u.Id == returnAccountRole.CompanyGUID)
                    .FirstOrDefault();

                if (returnCompany != null)
                {
                    this.CompanyName = returnCompany.Name;
                }
                else { this.CompanyName = "NONE"; }
                
            }
            else
            {
                this.RoleAssignment = "NONE";
                this.CompanyName = "NONE";
            }
        }
    }
}