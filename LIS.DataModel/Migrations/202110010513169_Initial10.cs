namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial10 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.TestRequestDetails", new[] { "SampleNo", "HISTestCode" }, unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.TestRequestDetails", new[] { "SampleNo", "HISTestCode" });
        }
    }
}
