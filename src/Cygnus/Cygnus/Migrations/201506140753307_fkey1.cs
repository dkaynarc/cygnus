namespace Cygnus.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fkey1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Resources", "ResourceGroupId", "dbo.ResourceGroups");
            AddForeignKey("dbo.Resources", "ResourceGroupId", "dbo.ResourceGroups", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Resources", "ResourceGroupId", "dbo.ResourceGroups");
            AddForeignKey("dbo.Resources", "ResourceGroupId", "dbo.ResourceGroups", "Id");
        }
    }
}
