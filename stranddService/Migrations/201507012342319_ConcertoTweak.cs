namespace stranddService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConcertoTweak : DbMigration
    {
        public override void Up()
        {
            AlterColumn("strandd.Incidents", "ConcertoCaseID", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("strandd.Incidents", "ConcertoCaseID", c => c.Int(nullable: false));
        }
    }
}
