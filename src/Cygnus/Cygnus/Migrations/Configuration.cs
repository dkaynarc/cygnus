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
            ContextKey = "Cygnus.Models.ApplicationDbContext";
        }

        protected override void Seed(Cygnus.Models.ApplicationDbContext context)
        {
            context.Gateways.AddOrUpdate(x => x.Id,
                new Gateway() { Id = 1, Name = "DefaultTestGateway1" },
                new Gateway() { Id = 2, Name = "DefaultTestGateway2" });
        }
    }
}
