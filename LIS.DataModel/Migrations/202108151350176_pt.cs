namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class pt : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PatientDetails", "HisPatientId", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PatientDetails", "HisPatientId");
        }
    }
}
