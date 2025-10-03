namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GroupName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TestMappingMaster", "GroupName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TestMappingMaster", "GroupName");
        }
    }
}
