using System.IO;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ranger.Services.Tenants;
using Ranger.Services.Tenants.Data;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Startup>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Production);

        builder.ConfigureAppConfiguration((context, conf) =>
        {
            conf.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
        });

        builder.ConfigureServices(services =>
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            services.AddDbContext<TenantsDbContext>(options =>
                {
                    options.UseNpgsql(configuration["cloudSql:ConnectionString"]);
                });

            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var logger = scope.ServiceProvider.GetService<ILogger<CustomWebApplicationFactory>>();
                logger.LogInformation("ConnectionString: " + configuration["cloudSql:ConnectionString"]);
                var context = scope.ServiceProvider.GetRequiredService<TenantsDbContext>();
                context.Database.Migrate();
            }
        });
    }
}