namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Data : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EquipmentMaster",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Model = c.String(),
                        AccessKey = c.String(nullable: false, maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.HISParameterMaster",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HISTestCode = c.String(),
                        HISParamCode = c.String(),
                        HISParamDescription = c.String(),
                        LISParamCode = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        HisTestId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.HISTestMaster", t => t.HisTestId)
                .Index(t => t.HisTestId);
            
            CreateTable(
                "dbo.HISTestMaster",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HISTestCode = c.String(),
                        HISTestCodeDescription = c.String(),
                        HISSpecimenCode = c.String(),
                        HISSpecimenName = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.HISParameterRangMaster",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HISRangeCode = c.String(),
                        HISRangeValue = c.String(),
                        Gender = c.String(),
                        AgeFrom = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AgeTo = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MinValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaxValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedOn = c.DateTime(nullable: false),
                        HisParameterId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.HISParameterMaster", t => t.HisParameterId)
                .Index(t => t.HisParameterId);
            
            CreateTable(
                "dbo.HISSpecimenMaster",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        Name = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PatientDetails",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Alias = c.String(),
                        Name = c.String(),
                        Age = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Gender = c.String(),
                        UHID = c.String(),
                        Phone = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        DateOfBirth = c.DateTime(nullable: false),
                        SiteId = c.String(),
                        CreatedBy = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TestMappingMaster",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HISTestCode = c.String(),
                        HISTestCodeDescription = c.String(),
                        SpecimenCode = c.String(),
                        SpecimenName = c.String(),
                        LISTestCode = c.String(),
                        LISTestCodeDescription = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        EquipmentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EquipmentMaster", t => t.EquipmentId)
                .Index(t => t.EquipmentId);
            
            CreateTable(
                "dbo.TestParameters",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        HISParamCode = c.String(),
                        HISParamName = c.String(),
                        HISTestCode = c.String(),
                        CreatedBy = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        TestRequestDetailsId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TestRequestDetails", t => t.TestRequestDetailsId)
                .Index(t => t.TestRequestDetailsId);
            
            CreateTable(
                "dbo.TestRequestDetails",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        SampleNo = c.String(),
                        HISTestCode = c.String(),
                        HISTestName = c.String(),
                        SampleCollectionDate = c.DateTime(nullable: false),
                        SampleReceivedDate = c.DateTime(nullable: false),
                        SpecimenCode = c.String(),
                        SpecimenName = c.String(),
                        CreatedBy = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        ReportStatus = c.Int(nullable: false),
                        PatientId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PatientDetails", t => t.PatientId)
                .Index(t => t.PatientId);
            
            CreateTable(
                "dbo.TestResultDetails",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        LISParamCode = c.String(),
                        LISParamValue = c.String(),
                        LISParamUnit = c.String(),
                        CreatedBy = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        ReviewDate = c.DateTime(),
                        ReportStatus = c.Int(nullable: false),
                        ReviewedBy = c.String(),
                        RunIndex = c.Int(nullable: false),
                        TestResultId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TestResults", t => t.TestResultId)
                .Index(t => t.TestResultId);
            
            CreateTable(
                "dbo.TestResults",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        SampleNo = c.String(),
                        HISTestCode = c.String(),
                        LISTestCode = c.String(),
                        SpecimenCode = c.String(),
                        SpecimenName = c.String(),
                        ResultDate = c.DateTime(nullable: false),
                        SampleCollectionDate = c.DateTime(nullable: false),
                        SampleReceivedDate = c.DateTime(nullable: false),
                        AuthorizationDate = c.DateTime(),
                        AuthorizedBy = c.String(),
                        ReviewDate = c.DateTime(),
                        ReviewedBy = c.String(),
                        TechnicianNote = c.String(),
                        DoctorNote = c.String(),
                        CreatedBy = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        PatientId = c.Long(nullable: false),
                        TestRequestId = c.Long(nullable: false),
                        EquipmentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EquipmentMaster", t => t.EquipmentId)
                .ForeignKey("dbo.PatientDetails", t => t.PatientId)
                .ForeignKey("dbo.TestRequestDetails", t => t.TestRequestId)
                .Index(t => t.PatientId)
                .Index(t => t.TestRequestId)
                .Index(t => t.EquipmentId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TestResultDetails", "TestResultId", "dbo.TestResults");
            DropForeignKey("dbo.TestResults", "TestRequestId", "dbo.TestRequestDetails");
            DropForeignKey("dbo.TestResults", "PatientId", "dbo.PatientDetails");
            DropForeignKey("dbo.TestResults", "EquipmentId", "dbo.EquipmentMaster");
            DropForeignKey("dbo.TestParameters", "TestRequestDetailsId", "dbo.TestRequestDetails");
            DropForeignKey("dbo.TestRequestDetails", "PatientId", "dbo.PatientDetails");
            DropForeignKey("dbo.TestMappingMaster", "EquipmentId", "dbo.EquipmentMaster");
            DropForeignKey("dbo.HISParameterRangMaster", "HisParameterId", "dbo.HISParameterMaster");
            DropForeignKey("dbo.HISParameterMaster", "HisTestId", "dbo.HISTestMaster");
            DropIndex("dbo.TestResults", new[] { "EquipmentId" });
            DropIndex("dbo.TestResults", new[] { "TestRequestId" });
            DropIndex("dbo.TestResults", new[] { "PatientId" });
            DropIndex("dbo.TestResultDetails", new[] { "TestResultId" });
            DropIndex("dbo.TestRequestDetails", new[] { "PatientId" });
            DropIndex("dbo.TestParameters", new[] { "TestRequestDetailsId" });
            DropIndex("dbo.TestMappingMaster", new[] { "EquipmentId" });
            DropIndex("dbo.HISParameterRangMaster", new[] { "HisParameterId" });
            DropIndex("dbo.HISParameterMaster", new[] { "HisTestId" });
            DropTable("dbo.TestResults");
            DropTable("dbo.TestResultDetails");
            DropTable("dbo.TestRequestDetails");
            DropTable("dbo.TestParameters");
            DropTable("dbo.TestMappingMaster");
            DropTable("dbo.PatientDetails");
            DropTable("dbo.HISSpecimenMaster");
            DropTable("dbo.HISParameterRangMaster");
            DropTable("dbo.HISTestMaster");
            DropTable("dbo.HISParameterMaster");
            DropTable("dbo.EquipmentMaster");
        }
    }
}
