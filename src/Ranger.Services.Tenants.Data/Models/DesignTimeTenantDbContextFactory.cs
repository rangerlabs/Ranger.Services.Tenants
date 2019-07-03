using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Ranger.Services.Tenants.Data {
    public class DesignTimeTenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext> {
        public TenantDbContext CreateDbContext (string[] args) {
            var config = new ConfigurationBuilder ()
                .SetBasePath (System.IO.Directory.GetCurrentDirectory ())
                .AddJsonFile ("appsettings.json")
                .Build ();

            var options = new DbContextOptionsBuilder<TenantDbContext> ();
            options.UseNpgsql (config["CloudSql:TenantConnectionString"]);

            return new TenantDbContext (options.Options);
        }
    }
}