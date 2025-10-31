namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class da : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AccuHealthTestOrders", "CreatedAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AccuHealthTestOrders", "CreatedAt");
        }
    }
}
