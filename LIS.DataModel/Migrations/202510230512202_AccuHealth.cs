namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AccuHealth : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PatientDetails", "ROW_ID", c => c.Guid(nullable: false));
            AddColumn("dbo.TestRequestDetails", "IPOPFLAG", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PINNO", c => c.String());
            AddColumn("dbo.TestRequestDetails", "REF_VISITNO", c => c.String());
            AddColumn("dbo.TestRequestDetails", "ADMISSIONNO", c => c.String());
            AddColumn("dbo.TestRequestDetails", "REQDATETIME", c => c.String());
            AddColumn("dbo.TestRequestDetails", "TESTPROF_CODE", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PROCESSED", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PATFNAME", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PATMNAME", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PATLNAME", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PAT_DOB", c => c.String());
            AddColumn("dbo.TestRequestDetails", "GENDER", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PATAGE", c => c.String());
            AddColumn("dbo.TestRequestDetails", "AGEUNIT", c => c.String());
            AddColumn("dbo.TestRequestDetails", "DOC_NAME", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PATIENTTYPECLASS", c => c.String());
            AddColumn("dbo.TestRequestDetails", "SEQNO", c => c.String());
            AddColumn("dbo.TestRequestDetails", "ADDDATE", c => c.String());
            AddColumn("dbo.TestRequestDetails", "ADDTIME", c => c.String());
            AddColumn("dbo.TestRequestDetails", "TITLE", c => c.String());
            AddColumn("dbo.TestRequestDetails", "LABNO", c => c.String());
            AddColumn("dbo.TestRequestDetails", "DATESTAMP", c => c.DateTime(nullable: false));
            AddColumn("dbo.TestRequestDetails", "PARAMCODE", c => c.String());
            AddColumn("dbo.TestRequestDetails", "PARAMNAME", c => c.String());
            AddColumn("dbo.TestRequestDetails", "MRESULT", c => c.String());
            AddColumn("dbo.TestRequestDetails", "BC_PRINTED", c => c.String());
            AddColumn("dbo.TestRequestDetails", "ROW_ID", c => c.Guid(nullable: false));
            AddColumn("dbo.TestRequestDetails", "isSynced", c => c.Boolean(nullable: false));
            AddColumn("dbo.TestRequestDetails", "branch_ID", c => c.Guid(nullable: false));
            AddColumn("dbo.TestResultDetails", "SRNO", c => c.String());
            AddColumn("dbo.TestResultDetails", "SDATE", c => c.DateTime(nullable: false));
            AddColumn("dbo.TestResultDetails", "SAMPLEID", c => c.String());
            AddColumn("dbo.TestResultDetails", "TESTID", c => c.String());
            AddColumn("dbo.TestResultDetails", "MACHINEID", c => c.String());
            AddColumn("dbo.TestResultDetails", "SUFFIX", c => c.String());
            AddColumn("dbo.TestResultDetails", "TRANSFERFLAG", c => c.String());
            AddColumn("dbo.TestResultDetails", "TMPVALUE", c => c.String());
            AddColumn("dbo.TestResultDetails", "DESCRIPTION", c => c.String());
            AddColumn("dbo.TestResultDetails", "RUNDATE", c => c.DateTime(nullable: false));
            AddColumn("dbo.TestResultDetails", "ROW_ID", c => c.Guid(nullable: false));
            AddColumn("dbo.TestResultDetails", "isSynced", c => c.Boolean(nullable: false));
            AddColumn("dbo.TestResults", "ROW_ID", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TestResults", "ROW_ID");
            DropColumn("dbo.TestResultDetails", "isSynced");
            DropColumn("dbo.TestResultDetails", "ROW_ID");
            DropColumn("dbo.TestResultDetails", "RUNDATE");
            DropColumn("dbo.TestResultDetails", "DESCRIPTION");
            DropColumn("dbo.TestResultDetails", "TMPVALUE");
            DropColumn("dbo.TestResultDetails", "TRANSFERFLAG");
            DropColumn("dbo.TestResultDetails", "SUFFIX");
            DropColumn("dbo.TestResultDetails", "MACHINEID");
            DropColumn("dbo.TestResultDetails", "TESTID");
            DropColumn("dbo.TestResultDetails", "SAMPLEID");
            DropColumn("dbo.TestResultDetails", "SDATE");
            DropColumn("dbo.TestResultDetails", "SRNO");
            DropColumn("dbo.TestRequestDetails", "branch_ID");
            DropColumn("dbo.TestRequestDetails", "isSynced");
            DropColumn("dbo.TestRequestDetails", "ROW_ID");
            DropColumn("dbo.TestRequestDetails", "BC_PRINTED");
            DropColumn("dbo.TestRequestDetails", "MRESULT");
            DropColumn("dbo.TestRequestDetails", "PARAMNAME");
            DropColumn("dbo.TestRequestDetails", "PARAMCODE");
            DropColumn("dbo.TestRequestDetails", "DATESTAMP");
            DropColumn("dbo.TestRequestDetails", "LABNO");
            DropColumn("dbo.TestRequestDetails", "TITLE");
            DropColumn("dbo.TestRequestDetails", "ADDTIME");
            DropColumn("dbo.TestRequestDetails", "ADDDATE");
            DropColumn("dbo.TestRequestDetails", "SEQNO");
            DropColumn("dbo.TestRequestDetails", "PATIENTTYPECLASS");
            DropColumn("dbo.TestRequestDetails", "DOC_NAME");
            DropColumn("dbo.TestRequestDetails", "AGEUNIT");
            DropColumn("dbo.TestRequestDetails", "PATAGE");
            DropColumn("dbo.TestRequestDetails", "GENDER");
            DropColumn("dbo.TestRequestDetails", "PAT_DOB");
            DropColumn("dbo.TestRequestDetails", "PATLNAME");
            DropColumn("dbo.TestRequestDetails", "PATMNAME");
            DropColumn("dbo.TestRequestDetails", "PATFNAME");
            DropColumn("dbo.TestRequestDetails", "PROCESSED");
            DropColumn("dbo.TestRequestDetails", "TESTPROF_CODE");
            DropColumn("dbo.TestRequestDetails", "REQDATETIME");
            DropColumn("dbo.TestRequestDetails", "ADMISSIONNO");
            DropColumn("dbo.TestRequestDetails", "REF_VISITNO");
            DropColumn("dbo.TestRequestDetails", "PINNO");
            DropColumn("dbo.TestRequestDetails", "IPOPFLAG");
            DropColumn("dbo.PatientDetails", "ROW_ID");
        }
    }
}
