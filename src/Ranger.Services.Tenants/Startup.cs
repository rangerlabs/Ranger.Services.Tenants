﻿using System;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Ranger.RabbitMQ;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants
{
    public class Startup
    {
        private readonly IWebHostEnvironment Environment;
        private readonly IConfiguration configuration;
        private ILoggerFactory loggerFactory;
        private IBusSubscriber busSubscriber;

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            this.Environment = environment;
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
                {
                    options.EnableEndpointRouting = false;
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });
            services.AddAuthorization(options =>
                options.AddPolicy("tenantPolicy", policyBuilder =>
                {
                    policyBuilder.RequireScope("tenantsApi");
                }
            ));


            services.AddEntityFrameworkNpgsql().AddDbContext<TenantsDbContext>(options =>
            {
                options.UseNpgsql(configuration["cloudSql:ConnectionString"]);
            },
                ServiceLifetime.Transient
            );

            services.AddTransient<ITenantsDbContextInitializer, TenantsDbContextInitializer>();
            services.AddTransient<ITenantService, TenantsService>();
            services.AddTransient<ITenantsRepository, TenantsRepository>();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://identity:5000/auth";
                    options.ApiName = "tenantsApi";

                    options.RequireHttpsMetadata = false;
                });

            services.AddDataProtection()
                .ProtectKeysWithCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                .PersistKeysToDbContext<TenantsDbContext>();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.AddRabbitMq();
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;

            applicationLifetime.ApplicationStopping.Register(OnShutdown);

            app.UseRouting();
            app.UseAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            this.busSubscriber = app.UseRabbitMQ()
                .SubscribeCommand<CreateTenant>((c, e) =>
                    new CreateTenantRejected(e.Message, ""))
                .SubscribeCommand<DeleteTenant>((c, e) =>
                    new DeleteTenantRejected(e.Message, ""))
                .SubscribeCommand<InitiatePrimaryOwnerTransfer>((c, e) =>
                    new InitiatePrimaryOwnerTransferRejected(e.Message, ""))
                .SubscribeCommand<CompletePrimaryOwnerTransfer>((c, e) =>
                    new CompletePrimaryOwnerTransferRejected(e.Message, ""));
        }

        private void OnShutdown()
        {
            this.busSubscriber.Dispose();
        }
    }
}