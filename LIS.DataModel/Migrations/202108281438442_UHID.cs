namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UHID : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PatientDetails", "HisPatientId", c => c.String(maxLength: 20));
            DropColumn("dbo.PatientDetails", "Alias");
            DropColumn("dbo.PatientDetails", "UHID");
            DropColumn("dbo.PatientDetails", "SiteId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PatientDetails", "SiteId", c => c.String());
            AddColumn("dbo.PatientDetails", "UHID", c => c.String(maxLength: 20));
            AddColumn("dbo.PatientDetails", "Alias", c => c.String(maxLength: 30));
            AlterColumn("dbo.PatientDetails", "HisPatientId", c => c.Long(nullable: false));
        }
    }
}
