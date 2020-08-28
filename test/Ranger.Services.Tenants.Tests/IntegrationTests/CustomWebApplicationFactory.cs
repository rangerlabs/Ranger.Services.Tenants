using System.IO;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ranger.Services.Tenants;
using Ranger.Services.Tenants.Data;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Startup>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Production);

        builder.ConfigureServices(services =>
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Create a new service provider.
            var serviceProvider = new ServiceCollection()
            .AddDbContext<TenantsDbContext>(options =>
            {
                options.UseNpgsql(config["cloudSql:ConnectionString"]);
            }, ServiceLifetime.Transient)
            .AddTransient<ITenantsDbContextInitializer, TenantsDbContextInitializer>()
            .BuildServiceProvider();


            // Build the service provider.
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database
            // context (ApplicationDbContext).
            using (var scope = sp.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<ITenantsDbContextInitializer>();
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                dbInitializer.Migrate();
            }
        });
    }

    protected override IWebHostBuilder CreateWebHostBuilder() =>
        WebHost.CreateDefaultBuilder()
           .UseStartup<Startup>()
           .ConfigureServices(services => services.AddAutofac());
}