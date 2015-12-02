namespace stranddService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IncidentAdditionalDetails : DbMigration
    {
        public override void Up()
        {
            AddColumn("strandd.Incidents", "AdditionalDetails", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("strandd.Incidents", "AdditionalDetails");
        }
    }
}
