namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial5 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TestRequestDetails", "BedNo", c => c.String(maxLength: 20));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TestRequestDetails", "BedNo", c => c.Int(nullable: false));
        }
    }
}
