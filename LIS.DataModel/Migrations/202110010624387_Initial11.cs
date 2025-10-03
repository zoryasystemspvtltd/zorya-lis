namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial11 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.TestRequestDetails", new[] { "SampleNo", "HISTestCode" });
            CreateIndex("dbo.TestRequestDetails", new[] { "SampleNo", "HISTestCode", "ReportStatus" }, unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.TestRequestDetails", new[] { "SampleNo", "HISTestCode", "ReportStatus" });
            CreateIndex("dbo.TestRequestDetails", new[] { "SampleNo", "HISTestCode" }, unique: true);
        }
    }
}
