namespace stranddService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConfirmChecks : DbMigration
    {
        public override void Up()
        {
            AddColumn("strandd.Incidents", "StatusCustomerConfirm", c => c.Boolean(nullable: false));
            AddColumn("strandd.Incidents", "ErrorCustomer", c => c.String());
            AddColumn("strandd.Incidents", "StatusProviderConfirm", c => c.Boolean(nullable: false));
            AddColumn("strandd.Incidents", "ErrorProvider", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("strandd.Incidents", "ErrorProvider");
            DropColumn("strandd.Incidents", "StatusProviderConfirm");
            DropColumn("strandd.Incidents", "ErrorCustomer");
            DropColumn("strandd.Incidents", "StatusCustomerConfirm");
        }
    }
}
