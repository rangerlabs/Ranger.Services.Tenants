﻿using System;
using System.IO;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ranger.Monitoring.Logging;
using Ranger.Services.Tenants.Data;
using Serilog;

namespace Ranger.Services.Tenants
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var host = CreateWebHostBuilder(args).UseUrls(config["serverBindingUrl"]).Build();

            using (var scope = host.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<ITenantsDbContextInitializer>();
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                dbInitializer.Migrate();
            }
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseLogging()
            .UseStartup<Startup>()
            .ConfigureServices(services => services.AddAutofac());
    }
}