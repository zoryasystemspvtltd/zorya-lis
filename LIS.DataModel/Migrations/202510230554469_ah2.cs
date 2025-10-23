namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ah2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TestRequestDetails", "DATESTAMP", c => c.DateTime());
            AlterColumn("dbo.TestResultDetails", "SDATE", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TestResultDetails", "SDATE", c => c.DateTime(nullable: false));
            AlterColumn("dbo.TestRequestDetails", "DATESTAMP", c => c.DateTime(nullable: false));
        }
    }
}
