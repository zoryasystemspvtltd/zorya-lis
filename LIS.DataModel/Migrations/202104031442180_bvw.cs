namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class bvw : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ControlResultDetails",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        LISParamCode = c.String(),
                        LISParamValue = c.String(),
                        LISParamUnit = c.String(),
                        CreatedBy = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        ControlResultId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ControlResults", t => t.ControlResultId)
                .Index(t => t.ControlResultId);
            
            CreateTable(
                "dbo.ControlResults",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        SampleNo = c.String(),
                        ResultDate = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        EquipmentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EquipmentMaster", t => t.EquipmentId)
                .Index(t => t.EquipmentId);
            
            DropColumn("dbo.TestResultDetails", "ReviewDate");
            DropColumn("dbo.TestResultDetails", "ReportStatus");
            DropColumn("dbo.TestResultDetails", "ReviewedBy");
            DropColumn("dbo.TestResultDetails", "RunIndex");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TestResultDetails", "RunIndex", c => c.Int(nullable: false));
            AddColumn("dbo.TestResultDetails", "ReviewedBy", c => c.String());
            AddColumn("dbo.TestResultDetails", "ReportStatus", c => c.Int(nullable: false));
            AddColumn("dbo.TestResultDetails", "ReviewDate", c => c.DateTime());
            DropForeignKey("dbo.ControlResultDetails", "ControlResultId", "dbo.ControlResults");
            DropForeignKey("dbo.ControlResults", "EquipmentId", "dbo.EquipmentMaster");
            DropIndex("dbo.ControlResults", new[] { "EquipmentId" });
            DropIndex("dbo.ControlResultDetails", new[] { "ControlResultId" });
            DropTable("dbo.ControlResults");
            DropTable("dbo.ControlResultDetails");
        }
    }
}
