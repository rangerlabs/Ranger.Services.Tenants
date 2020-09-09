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
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Ranger.ApiUtilities;
using Ranger.Common;
using Ranger.Monitoring.HealthChecks;
using Ranger.RabbitMQ;
using Ranger.Redis;
using Ranger.Services.Operations.Messages.Tenants.RejectedEvents;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants
{
    public class Startup
    {
        private readonly IWebHostEnvironment Environment;
        private readonly IConfiguration configuration;

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
                options.Filters.Add<OperationCanceledExceptionFilter>();
            })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });
            services.AddRangerApiVersioning();
            services.ConfigureAutoWrapperModelStateResponseFactory();
            services.AddSwaggerGen("Tenants API", "v1");

            services.AddDbContext<TenantsDbContext>(options =>
            {
                options.UseNpgsql(configuration["cloudSql:ConnectionString"]);
            });

            services.AddRedis(configuration["redis:ConnectionString"], out _);

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
            // Workaround for MAC validation issues on MacOS
            if (configuration.IsIntegrationTesting())
            {
                services.AddDataProtection()
                   .SetApplicationName("Tenants")
                   .PersistKeysToDbContext<TenantsDbContext>();
            }
            else
            {
                services.AddDataProtection()
                    .SetApplicationName("Tenants")
                    .ProtectKeysWithCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                    .UnprotectKeysWithAnyCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                    .PersistKeysToDbContext<TenantsDbContext>();
            }

            services.AddLiveHealthCheck();
            services.AddEntityFrameworkHealthCheck<TenantsDbContext>();
            services.AddDockerImageTagHealthCheck();
            services.AddRabbitMQHealthCheck();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.AddRabbitMq<Startup, TenantsDbContext>();
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime)
        {
            app.UseSwagger("v1", "Tenants API");
            app.UseAutoWrapper();
            app.UseUnhandedExceptionLogger();
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

            app.UseRabbitMQ()
                .SubscribeCommandWithHandler<CreateTenant>((c, e) =>
                   new CreateTenantRejected(e.Message, ""))
                .SubscribeCommandWithHandler<DeleteTenant>((c, e) =>
                   new DeleteTenantRejected(e.Message, ""))
                .SubscribeCommandWithHandler<InitiatePrimaryOwnerTransfer>((c, e) =>
                   new InitiatePrimaryOwnerTransferRejected(e.Message, ""))
                .SubscribeCommandWithHandler<CompletePrimaryOwnerTransfer>((c, e) =>
                   new CompletePrimaryOwnerTransferRejected(e.Message, ""))
                .SubscribeCommandWithHandler<UpdateTenantOrganization>((c, e) =>
                   new UpdateTenantOrganizationRejected(e.Message, ""));
        }
    }
}