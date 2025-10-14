namespace LIS.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class heartbeat : DbMigration
    {
        public override void Up()
        {
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
            
        }
        
        public override void Down()
        {
            DropTable("dbo.EquipmentHeartBeat");
        }
    }
}
