namespace stranddService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SlabsTweak : DbMigration
    {
        public override void Up()
        {
            AddColumn("strandd.IncidentCostings", "OffsetDiscount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("strandd.IncidentCostings", "CalculatedSubtotal", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("strandd.IncidentCostings", "CalculatedSubtotal");
            DropColumn("strandd.IncidentCostings", "OffsetDiscount");
        }
    }
}
