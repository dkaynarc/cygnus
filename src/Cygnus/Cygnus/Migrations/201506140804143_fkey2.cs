namespace Cygnus.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fkey2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Resources", "ResourceGroupId", "dbo.ResourceGroups");
            DropForeignKey("dbo.Resources", "GatewayId", "dbo.Gateways");
            DropIndex("dbo.Resources", new[] { "ResourceGroupId" });
            AlterColumn("dbo.Resources", "ResourceGroupId", c => c.Guid());
            CreateIndex("dbo.Resources", "ResourceGroupId");
            AddForeignKey("dbo.Resources", "ResourceGroupId", "dbo.ResourceGroups", "Id");
            AddForeignKey("dbo.Resources", "GatewayId", "dbo.Gateways", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Resources", "GatewayId", "dbo.Gateways");
            DropForeignKey("dbo.Resources", "ResourceGroupId", "dbo.ResourceGroups");
            DropIndex("dbo.Resources", new[] { "ResourceGroupId" });
            AlterColumn("dbo.Resources", "ResourceGroupId", c => c.Guid(nullable: false));
            CreateIndex("dbo.Resources", "ResourceGroupId");
            AddForeignKey("dbo.Resources", "GatewayId", "dbo.Gateways", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Resources", "ResourceGroupId", "dbo.ResourceGroups", "Id", cascadeDelete: true);
        }
    }
}
