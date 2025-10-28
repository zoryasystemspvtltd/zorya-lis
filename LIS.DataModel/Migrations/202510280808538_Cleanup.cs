namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Cleanup : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ControlResults", "EquipmentId", "dbo.EquipmentMaster");
            DropForeignKey("dbo.ControlResultDetails", "ControlResultId", "dbo.ControlResults");
            DropForeignKey("dbo.HISTestMaster", "DepartmentCode", "dbo.Department");
            DropForeignKey("dbo.HISParameterMaster", "HisTestId", "dbo.HISTestMaster");
            DropForeignKey("dbo.HISParameterRangMaster", "HisParameterId", "dbo.HISParameterMaster");
            DropForeignKey("dbo.TestMappingMaster", "EquipmentId", "dbo.EquipmentMaster");
            DropForeignKey("dbo.TestRequestDetails", "PatientId", "dbo.PatientDetails");
            DropForeignKey("dbo.TestParameters", "TestRequestDetailsId", "dbo.TestRequestDetails");
            DropForeignKey("dbo.TestResults", "EquipmentId", "dbo.EquipmentMaster");
            DropForeignKey("dbo.TestResults", "PatientId", "dbo.PatientDetails");
            DropForeignKey("dbo.TestResults", "TestRequestId", "dbo.TestRequestDetails");
            DropForeignKey("dbo.TestResultDetails", "TestResultId", "dbo.TestResults");
            DropIndex("dbo.ControlResultDetails", new[] { "ControlResultId" });
            DropIndex("dbo.ControlResults", new[] { "EquipmentId" });
            DropIndex("dbo.HISParameterMaster", new[] { "HisTestId" });
            DropIndex("dbo.HISTestMaster", new[] { "DepartmentCode" });
            DropIndex("dbo.HISParameterRangMaster", new[] { "HisParameterId" });
            DropIndex("dbo.TestMappingMaster", new[] { "EquipmentId" });
            DropIndex("dbo.TestParameters", new[] { "TestRequestDetailsId" });
            DropIndex("dbo.TestRequestDetails", new[] { "SampleNo", "HISTestCode", "ReportStatus" });
            DropIndex("dbo.TestRequestDetails", new[] { "PatientId" });
            DropIndex("dbo.TestResultDetails", new[] { "TestResultId" });
            DropIndex("dbo.TestResults", new[] { "PatientId" });
            DropIndex("dbo.TestResults", new[] { "TestRequestId" });
            DropIndex("dbo.TestResults", new[] { "EquipmentId" });
            DropTable("dbo.ControlResultDetails");
            DropTable("dbo.ControlResults");
            DropTable("dbo.Department");
            DropTable("dbo.EquipmentHeartBeat");
            DropTable("dbo.HISParameterMaster");
            DropTable("dbo.HISTestMaster");
            DropTable("dbo.HISParameterRangMaster");
            DropTable("dbo.HISSpecimenMaster");
            DropTable("dbo.PatientDetails");
            DropTable("dbo.TestMappingMaster");
            DropTable("dbo.TestParameters");
            DropTable("dbo.TestRequestDetails");
            DropTable("dbo.TestResultDetails");
            DropTable("dbo.TestResults");
        }
        
        public override void Down()
        {
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
                .PrimaryKey(t => t.Id);
            
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
                        TestResultId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TestRequestDetails",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        SampleNo = c.String(maxLength: 30),
                        HISTestCode = c.String(maxLength: 20),
                        HISTestName = c.String(maxLength: 100),
                        SampleCollectionDate = c.DateTime(nullable: false),
                        SampleReceivedDate = c.DateTime(nullable: false),
                        SpecimenCode = c.String(maxLength: 20),
                        SpecimenName = c.String(maxLength: 100),
                        CreatedBy = c.String(maxLength: 80),
                        CreatedOn = c.DateTime(nullable: false),
                        ReportStatus = c.Int(nullable: false),
                        IPNo = c.String(maxLength: 20),
                        BedNo = c.String(maxLength: 20),
                        MRNo = c.String(maxLength: 20),
                        HISRequestId = c.String(maxLength: 20),
                        HISRequestNo = c.String(maxLength: 20),
                        DepartmentId = c.String(maxLength: 20),
                        Department = c.String(maxLength: 80),
                        PatientId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
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
                        GroupName = c.String(),
                        EquipmentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PatientDetails",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        HisPatientId = c.String(maxLength: 20),
                        Name = c.String(maxLength: 100),
                        Age = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Gender = c.String(maxLength: 10),
                        Phone = c.String(maxLength: 15),
                        IsActive = c.Boolean(nullable: false),
                        DateOfBirth = c.DateTime(nullable: false),
                        CreatedBy = c.String(maxLength: 80),
                        CreatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
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
                "dbo.HISParameterRangMaster",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HISRangeCode = c.String(),
                        HISRangeValue = c.String(),
                        Gender = c.String(),
                        AgeFrom = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AgeTo = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AgeType = c.String(),
                        MinValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaxValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedOn = c.DateTime(nullable: false),
                        HisParameterId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
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
                        IsActive = c.Boolean(nullable: false),
                        DepartmentCode = c.String(maxLength: 15),
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
                        HISParamUnit = c.String(),
                        HISParamMethod = c.String(),
                        LISParamCode = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        HisTestId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EquipmentHeartBeat",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccessKey = c.String(nullable: false, maxLength: 50),
                        IsAlive = c.Boolean(nullable: false),
                        CreatedBy = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Department",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 15),
                        Name = c.String(nullable: false, maxLength: 55),
                    })
                .PrimaryKey(t => t.Code);
            
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
                .PrimaryKey(t => t.Id);
            
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
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.TestResults", "EquipmentId");
            CreateIndex("dbo.TestResults", "TestRequestId");
            CreateIndex("dbo.TestResults", "PatientId");
            CreateIndex("dbo.TestResultDetails", "TestResultId");
            CreateIndex("dbo.TestRequestDetails", "PatientId");
            CreateIndex("dbo.TestRequestDetails", new[] { "SampleNo", "HISTestCode", "ReportStatus" }, unique: true);
            CreateIndex("dbo.TestParameters", "TestRequestDetailsId");
            CreateIndex("dbo.TestMappingMaster", "EquipmentId");
            CreateIndex("dbo.HISParameterRangMaster", "HisParameterId");
            CreateIndex("dbo.HISTestMaster", "DepartmentCode");
            CreateIndex("dbo.HISParameterMaster", "HisTestId");
            CreateIndex("dbo.ControlResults", "EquipmentId");
            CreateIndex("dbo.ControlResultDetails", "ControlResultId");
            AddForeignKey("dbo.TestResultDetails", "TestResultId", "dbo.TestResults", "Id");
            AddForeignKey("dbo.TestResults", "TestRequestId", "dbo.TestRequestDetails", "Id");
            AddForeignKey("dbo.TestResults", "PatientId", "dbo.PatientDetails", "Id");
            AddForeignKey("dbo.TestResults", "EquipmentId", "dbo.EquipmentMaster", "Id");
            AddForeignKey("dbo.TestParameters", "TestRequestDetailsId", "dbo.TestRequestDetails", "Id");
            AddForeignKey("dbo.TestRequestDetails", "PatientId", "dbo.PatientDetails", "Id");
            AddForeignKey("dbo.TestMappingMaster", "EquipmentId", "dbo.EquipmentMaster", "Id");
            AddForeignKey("dbo.HISParameterRangMaster", "HisParameterId", "dbo.HISParameterMaster", "Id");
            AddForeignKey("dbo.HISParameterMaster", "HisTestId", "dbo.HISTestMaster", "Id");
            AddForeignKey("dbo.HISTestMaster", "DepartmentCode", "dbo.Department", "Code");
            AddForeignKey("dbo.ControlResultDetails", "ControlResultId", "dbo.ControlResults", "Id");
            AddForeignKey("dbo.ControlResults", "EquipmentId", "dbo.EquipmentMaster", "Id");
        }
    }
}
