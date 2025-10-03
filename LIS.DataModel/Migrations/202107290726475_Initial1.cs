namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TestMappingMaster", "DepartmentCode", c => c.String(maxLength: 15));
            CreateIndex("dbo.TestMappingMaster", "DepartmentCode");
            AddForeignKey("dbo.TestMappingMaster", "DepartmentCode", "dbo.Department", "Code");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TestMappingMaster", "DepartmentCode", "dbo.Department");
            DropIndex("dbo.TestMappingMaster", new[] { "DepartmentCode" });
            DropColumn("dbo.TestMappingMaster", "DepartmentCode");
        }
    }
}
