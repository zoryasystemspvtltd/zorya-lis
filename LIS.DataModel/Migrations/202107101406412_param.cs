namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class param : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.HISParameterMaster", "HISParamUnit", c => c.String());
            AddColumn("dbo.HISParameterMaster", "HISParamMethod", c => c.String());
            AddColumn("dbo.HISParameterRangMaster", "AgeType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.HISParameterRangMaster", "AgeType");
            DropColumn("dbo.HISParameterMaster", "HISParamMethod");
            DropColumn("dbo.HISParameterMaster", "HISParamUnit");
        }
    }
}
