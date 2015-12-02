namespace stranddService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class historyeventfix : DbMigration
    {
        public override void Up()
        {
            DropColumn("strandd.HistoryEvents", "TimeStamp");
        }
        
        public override void Down()
        {
            AddColumn("strandd.HistoryEvents", "TimeStamp", c => c.String());
        }
    }
}
