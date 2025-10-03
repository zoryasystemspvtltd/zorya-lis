namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class s : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TestRequestDetails", "IPNo", c => c.String(maxLength: 20));
            AddColumn("dbo.TestRequestDetails", "BedNo", c => c.Int(nullable: false));
            AddColumn("dbo.TestRequestDetails", "MRNo", c => c.String(maxLength: 20));
            AddColumn("dbo.TestRequestDetails", "HISRequestId", c => c.String(maxLength: 20));
            AddColumn("dbo.TestRequestDetails", "HISRequestNo", c => c.String(maxLength: 20));
            AddColumn("dbo.TestRequestDetails", "DepartmentId", c => c.String(maxLength: 20));
            AddColumn("dbo.TestRequestDetails", "Department", c => c.String(maxLength: 80));
            AlterColumn("dbo.PatientDetails", "Alias", c => c.String(maxLength: 30));
            AlterColumn("dbo.PatientDetails", "Name", c => c.String(maxLength: 100));
            AlterColumn("dbo.PatientDetails", "Gender", c => c.String(maxLength: 10));
            AlterColumn("dbo.PatientDetails", "UHID", c => c.String(maxLength: 20));
            AlterColumn("dbo.PatientDetails", "Phone", c => c.String(maxLength: 15));
            AlterColumn("dbo.PatientDetails", "CreatedBy", c => c.String(maxLength: 80));
            AlterColumn("dbo.TestRequestDetails", "SampleNo", c => c.String(maxLength: 30));
            AlterColumn("dbo.TestRequestDetails", "HISTestCode", c => c.String(maxLength: 20));
            AlterColumn("dbo.TestRequestDetails", "HISTestName", c => c.String(maxLength: 100));
            AlterColumn("dbo.TestRequestDetails", "SpecimenCode", c => c.String(maxLength: 20));
            AlterColumn("dbo.TestRequestDetails", "SpecimenName", c => c.String(maxLength: 100));
            AlterColumn("dbo.TestRequestDetails", "CreatedBy", c => c.String(maxLength: 80));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TestRequestDetails", "CreatedBy", c => c.String());
            AlterColumn("dbo.TestRequestDetails", "SpecimenName", c => c.String());
            AlterColumn("dbo.TestRequestDetails", "SpecimenCode", c => c.String());
            AlterColumn("dbo.TestRequestDetails", "HISTestName", c => c.String());
            AlterColumn("dbo.TestRequestDetails", "HISTestCode", c => c.String());
            AlterColumn("dbo.TestRequestDetails", "SampleNo", c => c.String());
            AlterColumn("dbo.PatientDetails", "CreatedBy", c => c.String());
            AlterColumn("dbo.PatientDetails", "Phone", c => c.String());
            AlterColumn("dbo.PatientDetails", "UHID", c => c.String());
            AlterColumn("dbo.PatientDetails", "Gender", c => c.String());
            AlterColumn("dbo.PatientDetails", "Name", c => c.String());
            AlterColumn("dbo.PatientDetails", "Alias", c => c.String());
            DropColumn("dbo.TestRequestDetails", "Department");
            DropColumn("dbo.TestRequestDetails", "DepartmentId");
            DropColumn("dbo.TestRequestDetails", "HISRequestNo");
            DropColumn("dbo.TestRequestDetails", "HISRequestId");
            DropColumn("dbo.TestRequestDetails", "MRNo");
            DropColumn("dbo.TestRequestDetails", "BedNo");
            DropColumn("dbo.TestRequestDetails", "IPNo");
        }
    }
}
