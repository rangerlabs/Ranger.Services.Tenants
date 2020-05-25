using System.Security.Cryptography.X509Certificates;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json.Serialization;
using Ranger.ApiUtilities;
using Ranger.Monitoring.HealthChecks;
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

            services.AddAutoWrapper();
            services.AddSwaggerGen("Tenants API", "v1");
            services.AddApiVersioning(o => o.ApiVersionReader = new HeaderApiVersionReader("api-version"));

            services.AddDbContext<TenantsDbContext>(options =>
            {
                options.UseNpgsql(configuration["cloudSql:ConnectionString"]);
            },
                ServiceLifetime.Transient
            );

            services.AddTransient<ITenantsDbContextInitializer, TenantsDbContextInitializer>();
            services.AddTransient<ITenantService, TenantsService>();
            services.AddTransient<ITenantsRepository, TenantsRepository>();
            IdentityModelEventSource.ShowPII = true;
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

            services.AddLiveHealthCheck();
            services.AddEntityFrameworkHealthCheck<TenantsDbContext>();
            services.AddDockerImageTagHealthCheck();
            services.AddRabbitMQHealthCheck();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.AddRabbitMq();
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;

            applicationLifetime.ApplicationStopping.Register(OnShutdown);

            app.UseSwagger("v1", "Tenants API");
            app.UseAutoWrapper();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks();
                endpoints.MapLiveTagHealthCheck();
                endpoints.MapEfCoreTagHealthCheck();
                endpoints.MapDockerImageTagHealthCheck();
                endpoints.MapRabbitMQHealthCheck();
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