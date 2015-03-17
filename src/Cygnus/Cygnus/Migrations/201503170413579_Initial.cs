namespace Cygnus.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Sensors",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        Resource = c.String(),
                        Description = c.String(),
                        GatewayId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Gateways", t => t.GatewayId, cascadeDelete: true)
                .Index(t => t.GatewayId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Sensors", "GatewayId", "dbo.Gateways");
            DropIndex("dbo.Sensors", new[] { "GatewayId" });
            DropTable("dbo.Sensors");
        }
    }
}
