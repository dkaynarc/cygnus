namespace Cygnus.Migrations
{
    using Cygnus.Models.Api;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Cygnus.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Cygnus.Models.ApplicationDbContext context)
        {
            context.Gateways.AddOrUpdate(x => x.Id,
                new Gateway() { Id = Guid.NewGuid(), Name = "DefaultTestGateway1" },
                new Gateway() { Id = Guid.NewGuid(), Name = "DefaultTestGateway2" });
        }
    }
}
