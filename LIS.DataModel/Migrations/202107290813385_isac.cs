namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class isac : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.HISTestMaster", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.HISTestMaster", "IsActive");
        }
    }
}
