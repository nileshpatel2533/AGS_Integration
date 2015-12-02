namespace stranddService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AccountRolePublicFix : DbMigration
    {
        public override void Up()
        {
            AddColumn("strandd.AccountRoles", "RoleAssignment", c => c.String());
            AddColumn("strandd.AccountRoles", "UserProviderID", c => c.String());
            AddColumn("strandd.AccountRoles", "CompanyGUID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("strandd.AccountRoles", "CompanyGUID");
            DropColumn("strandd.AccountRoles", "UserProviderID");
            DropColumn("strandd.AccountRoles", "RoleAssignment");
        }
    }
}
