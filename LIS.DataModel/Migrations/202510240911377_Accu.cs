namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Accu : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AccuHealthParamMappings",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        HIS_TESTNAME = c.String(),
                        HIS_PARAMCODE = c.String(),
                        HIS_PARAMNAME = c.String(),
                        LIS_PARAMCODE = c.String(),
                        LIS_PARAMNAME = c.String(),
                        LIS_TESTNAME = c.String(),
                        SPECIMEN = c.String(),
                        UNIT = c.String(),
                        EquipmentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EquipmentMaster", t => t.EquipmentId)
                .Index(t => t.EquipmentId);
            
            CreateTable(
                "dbo.AccuHealthTestOrders",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ROW_ID = c.Guid(nullable: false),
                        isSynced = c.Boolean(nullable: false),
                        branch_ID = c.Guid(nullable: false),
                        IPOPFLAG = c.String(),
                        PINNO = c.String(),
                        REF_VISITNO = c.String(),
                        ADMISSIONNO = c.String(),
                        REQDATETIME = c.String(),
                        TESTPROF_CODE = c.String(),
                        PROCESSED = c.String(),
                        PATFNAME = c.String(),
                        PATMNAME = c.String(),
                        PATLNAME = c.String(),
                        PAT_DOB = c.String(),
                        GENDER = c.String(),
                        PATAGE = c.String(),
                        AGEUNIT = c.String(),
                        DOC_NAME = c.String(),
                        PATIENTTYPECLASS = c.String(),
                        SEQNO = c.String(),
                        ADDDATE = c.String(),
                        ADDTIME = c.String(),
                        TITLE = c.String(),
                        LABNO = c.String(),
                        DATESTAMP = c.DateTime(),
                        PARAMCODE = c.String(),
                        PARAMNAME = c.String(),
                        MRESULT = c.String(),
                        BC_PRINTED = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AccuHealthTestValues",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ROW_ID = c.Guid(nullable: false),
                        isSynced = c.Boolean(nullable: false),
                        SRNO = c.String(),
                        SDATE = c.DateTime(),
                        SAMPLEID = c.String(),
                        TESTID = c.String(),
                        MACHINEID = c.String(),
                        SUFFIX = c.String(),
                        TRANSFERFLAG = c.String(),
                        TMPVALUE = c.String(),
                        DESCRIPTION = c.String(),
                        RUNDATE = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropColumn("dbo.PatientDetails", "ROW_ID");
            DropColumn("dbo.TestRequestDetails", "IPOPFLAG");
            DropColumn("dbo.TestRequestDetails", "PINNO");
            DropColumn("dbo.TestRequestDetails", "REF_VISITNO");
            DropColumn("dbo.TestRequestDetails", "ADMISSIONNO");
            DropColumn("dbo.TestRequestDetails", "REQDATETIME");
            DropColumn("dbo.TestRequestDetails", "TESTPROF_CODE");
            DropColumn("dbo.TestRequestDetails", "PROCESSED");
            DropColumn("dbo.TestRequestDetails", "PATFNAME");
            DropColumn("dbo.TestRequestDetails", "PATMNAME");
            DropColumn("dbo.TestRequestDetails", "PATLNAME");
            DropColumn("dbo.TestRequestDetails", "PAT_DOB");
            DropColumn("dbo.TestRequestDetails", "GENDER");
            DropColumn("dbo.TestRequestDetails", "PATAGE");
            DropColumn("dbo.TestRequestDetails", "AGEUNIT");
            DropColumn("dbo.TestRequestDetails", "DOC_NAME");
            DropColumn("dbo.TestRequestDetails", "PATIENTTYPECLASS");
            DropColumn("dbo.TestRequestDetails", "SEQNO");
            DropColumn("dbo.TestRequestDetails", "ADDDATE");
            DropColumn("dbo.TestRequestDetails", "ADDTIME");
            DropColumn("dbo.TestRequestDetails", "TITLE");
            DropColumn("dbo.TestRequestDetails", "LABNO");
            DropColumn("dbo.TestRequestDetails", "DATESTAMP");
            DropColumn("dbo.TestRequestDetails", "PARAMCODE");
            DropColumn("dbo.TestRequestDetails", "PARAMNAME");
            DropColumn("dbo.TestRequestDetails", "MRESULT");
            DropColumn("dbo.TestRequestDetails", "BC_PRINTED");
            DropColumn("dbo.TestRequestDetails", "ROW_ID");
            DropColumn("dbo.TestRequestDetails", "isSynced");
            DropColumn("dbo.TestRequestDetails", "branch_ID");
            DropColumn("dbo.TestResultDetails", "SRNO");
            DropColumn("dbo.TestResultDetails", "SDATE");
            DropColumn("dbo.TestResultDetails", "SAMPLEID");
            DropColumn("dbo.TestResultDetails", "TESTID");
            DropColumn("dbo.TestResultDetails", "MACHINEID");
            DropColumn("dbo.TestResultDetails", "SUFFIX");
            DropColumn("dbo.TestResultDetails", "TRANSFERFLAG");
            DropColumn("dbo.TestResultDetails", "TMPVALUE");
            DropColumn("dbo.TestResultDetails", "DESCRIPTION");
            DropColumn("dbo.TestResultDetails", "RUNDATE");
            DropColumn("dbo.TestResultDetails", "ROW_ID");
            DropColumn("dbo.TestResultDetails", "isSynced");
            DropColumn("dbo.TestResults", "ROW_ID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TestResults", "ROW_ID", c => c.Guid(nullable: false));
            AddColumn("dbo.TestResultDetails", "isSynced", c => c.Boolean(nullable: false));
            AddColumn("dbo.TestResultDetails", "ROW_ID", c => c.Guid(nullable: false));
            AddColumn("dbo.TestResultDetails", "RUNDATE", c => c.DateTime(nullable: false));
            AddColumn("dbo.TestResultDetails", "DESCRIPTION", c => c.String());
            AddColumn("dbo.TestResultDetails", "TMPVALUE", c => c.String());
            AddColumn("dbo.TestResultDetails", "TRANSFERFLAG", c => c.String());
            AddColumn("dbo.TestResultDetails", "SUFFIX", c => c.String());
            AddColumn("dbo.TestResultDetails", "MACHINEID", c => c.String());
            AddColumn("dbo.TestResultDetails", "TESTID", c => c.String());
            AddColumn("dbo.TestResultDetails", "SAMPLEID", c => c.String());
            AddColumn("dbo.TestResultDetails", "SDATE", c => c.DateTime());
            AddColumn("dbo.TestResultDetails", "SRNO", c => c.String());
            AddColumn("dbo.TestRequestDetails", "branch_ID", c => c.Guid(nullable: false));
            AddColumn("dbo.TestRequestDetails", "isSynced", c => c.Boolean(nullable: false));
            AddColumn("dbo.TestRequestDetails", "ROW_ID", c => c.Guid(nullable: false));
            AddColumn("dbo.TestRequestDetails", "BC_PRINTED", c => c.String());
            AddColumn("dbo.TestRequestDetails", "MRESULT", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PARAMNAME", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PARAMCODE", c => c.String());
            AddColumn("dbo.TestRequestDetails", "DATESTAMP", c => c.DateTime());
            AddColumn("dbo.TestRequestDetails", "LABNO", c => c.String());
            AddColumn("dbo.TestRequestDetails", "TITLE", c => c.String());
            AddColumn("dbo.TestRequestDetails", "ADDTIME", c => c.String());
            AddColumn("dbo.TestRequestDetails", "ADDDATE", c => c.String());
            AddColumn("dbo.TestRequestDetails", "SEQNO", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PATIENTTYPECLASS", c => c.String());
            AddColumn("dbo.TestRequestDetails", "DOC_NAME", c => c.String());
            AddColumn("dbo.TestRequestDetails", "AGEUNIT", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PATAGE", c => c.String());
            AddColumn("dbo.TestRequestDetails", "GENDER", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PAT_DOB", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PATLNAME", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PATMNAME", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PATFNAME", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PROCESSED", c => c.String());
            AddColumn("dbo.TestRequestDetails", "TESTPROF_CODE", c => c.String());
            AddColumn("dbo.TestRequestDetails", "REQDATETIME", c => c.String());
            AddColumn("dbo.TestRequestDetails", "ADMISSIONNO", c => c.String());
            AddColumn("dbo.TestRequestDetails", "REF_VISITNO", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PINNO", c => c.String());
            AddColumn("dbo.TestRequestDetails", "IPOPFLAG", c => c.String());
            AddColumn("dbo.PatientDetails", "ROW_ID", c => c.Guid(nullable: false));
            DropForeignKey("dbo.AccuHealthParamMappings", "EquipmentId", "dbo.EquipmentMaster");
            DropIndex("dbo.AccuHealthParamMappings", new[] { "EquipmentId" });
            DropTable("dbo.AccuHealthTestValues");
            DropTable("dbo.AccuHealthTestOrders");
            DropTable("dbo.AccuHealthParamMappings");
        }
    }
}
