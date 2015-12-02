namespace stranddService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PaymentTweak : DbMigration
    {
        public override void Up()
        {
            AddColumn("strandd.Payments", "IncidentGUID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("strandd.Payments", "IncidentGUID");
        }
    }
}
