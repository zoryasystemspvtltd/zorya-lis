namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TestMappingMaster", "DepartmentCode", "dbo.Department");
            DropIndex("dbo.TestMappingMaster", new[] { "DepartmentCode" });
            AddColumn("dbo.HISTestMaster", "DepartmentCode", c => c.String(maxLength: 15));
            CreateIndex("dbo.HISTestMaster", "DepartmentCode");
            AddForeignKey("dbo.HISTestMaster", "DepartmentCode", "dbo.Department", "Code");
            DropColumn("dbo.TestMappingMaster", "DepartmentCode");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TestMappingMaster", "DepartmentCode", c => c.String(maxLength: 15));
            DropForeignKey("dbo.HISTestMaster", "DepartmentCode", "dbo.Department");
            DropIndex("dbo.HISTestMaster", new[] { "DepartmentCode" });
            DropColumn("dbo.HISTestMaster", "DepartmentCode");
            CreateIndex("dbo.TestMappingMaster", "DepartmentCode");
            AddForeignKey("dbo.TestMappingMaster", "DepartmentCode", "dbo.Department", "Code");
        }
    }
}
