using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Ranger.Services.Tenants;
using Ranger.Services.Tenants.Data;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Startup>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Create a new service provider.
            // var serviceProvider = new ServiceCollection()
            //     .BuildServiceProvider();

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