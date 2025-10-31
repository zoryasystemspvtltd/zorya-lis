namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Ac : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LisTestValues",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        REF_VISITNO = c.String(),
                        PARAMCODE = c.String(),
                        Value = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        Equipment = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropColumn("dbo.AccuHealthParamMappings", "HIS_TESTNAME");
            DropColumn("dbo.AccuHealthParamMappings", "LIS_PARAMNAME");
            DropColumn("dbo.AccuHealthParamMappings", "LIS_TESTNAME");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AccuHealthParamMappings", "LIS_TESTNAME", c => c.String());
            AddColumn("dbo.AccuHealthParamMappings", "LIS_PARAMNAME", c => c.String());
            AddColumn("dbo.AccuHealthParamMappings", "HIS_TESTNAME", c => c.String());
            DropTable("dbo.LisTestValues");
        }
    }
}
