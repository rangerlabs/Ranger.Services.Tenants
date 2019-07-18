using System;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Ranger.RabbitMQ;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants {
    public class Startup {
        private readonly IConfiguration configuration;
        private readonly ILogger<Startup> logger;
        private IContainer container;
        private IBusSubscriber busSubscriber;

        public Startup (IConfiguration configuration, ILogger<Startup> logger) {
            this.configuration = configuration;
            this.logger = logger;
        }

        public IServiceProvider ConfigureServices (IServiceCollection services) {
            services.AddMvcCore (options => {
                    var policy = ScopePolicy.Create ("tenantScope");
                    options.Filters.Add (new AuthorizeFilter (policy));
                })
                .AddAuthorization ()
                .AddJsonFormatters ()
                .AddJsonOptions (options => {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver ();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            services.AddEntityFrameworkNpgsql ().AddDbContext<TenantDbContext> (options => {
                    options.UseNpgsql (configuration["cloudSql:ConnectionString"]);
                },
                ServiceLifetime.Transient
            );

            services.AddTransient<ITenantDbContextInitializer, TenantDbContextInitializer> ();
            services.AddTransient<ITenantRepository, TenantRepository> ();

            services.AddAuthentication ("Bearer")
                .AddIdentityServerAuthentication (options => {
                    options.Authority = "http://identity:5000/auth";
                    options.ApiName = "tenantsApi";

                    //TODO: Change these to true
                    options.EnableCaching = false;
                    options.RequireHttpsMetadata = false;
                });

            if (Environment.GetEnvironmentVariable ("ASPNETCORE_ENVIRONMENT") == EnvironmentName.Production) {
                services.AddDataProtection ()
                    .ProtectKeysWithCertificate (new X509Certificate2 (configuration["DataProtectionCertPath:Path"]))
                    .PersistKeysToDbContext<TenantDbContext> ();
                this.logger.LogInformation ("Production data protection certificate loaded.");
            } else {
                services.AddDataProtection ();
            }

            var builder = new ContainerBuilder ();
            builder.Populate (services);
            builder.AddRabbitMq ();
            container = builder.Build ();
            return new AutofacServiceProvider (container);
        }

        public void Configure (IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime) {
            applicationLifetime.ApplicationStopping.Register (OnShutdown);
            app.UseAuthentication ();
            app.UseMvcWithDefaultRoute ();
            this.busSubscriber = app.UseRabbitMQ ()
                .SubscribeCommand<CreateTenant> ((c, e) =>
                    new CreateTenantRejected (c.CorrelationContext, e.Message, ""));
        }

        private void OnShutdown () {
            this.busSubscriber.Dispose ();
        }
    }
}