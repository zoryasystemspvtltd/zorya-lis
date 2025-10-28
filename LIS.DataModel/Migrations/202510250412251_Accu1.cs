namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Accu1 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.AccuHealthTestOrders");
            DropPrimaryKey("dbo.AccuHealthTestValues");
            AddPrimaryKey("dbo.AccuHealthTestOrders", "ROW_ID");
            AddPrimaryKey("dbo.AccuHealthTestValues", "ROW_ID");
            DropColumn("dbo.AccuHealthTestOrders", "Id");
            DropColumn("dbo.AccuHealthTestValues", "Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AccuHealthTestValues", "Id", c => c.Long(nullable: false, identity: true));
            AddColumn("dbo.AccuHealthTestOrders", "Id", c => c.Long(nullable: false, identity: true));
            DropPrimaryKey("dbo.AccuHealthTestValues");
            DropPrimaryKey("dbo.AccuHealthTestOrders");
            AddPrimaryKey("dbo.AccuHealthTestValues", "Id");
            AddPrimaryKey("dbo.AccuHealthTestOrders", "Id");
        }
    }
}
