namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Acc : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AccuHealthTestOrders", "REQDATETIME", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AccuHealthTestOrders", "REQDATETIME", c => c.String());
        }
    }
}
