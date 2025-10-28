namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AccuHealth1 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.AccuHealthTestValues");
            AddColumn("dbo.AccuHealthTestOrders", "Value", c => c.String());
            AddColumn("dbo.AccuHealthTestOrders", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.AccuHealthTestValues", "Id", c => c.Long(nullable: false, identity: true));
            AddPrimaryKey("dbo.AccuHealthTestValues", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.AccuHealthTestValues");
            DropColumn("dbo.AccuHealthTestValues", "Id");
            DropColumn("dbo.AccuHealthTestOrders", "Status");
            DropColumn("dbo.AccuHealthTestOrders", "Value");
            AddPrimaryKey("dbo.AccuHealthTestValues", "ROW_ID");
        }
    }
}
