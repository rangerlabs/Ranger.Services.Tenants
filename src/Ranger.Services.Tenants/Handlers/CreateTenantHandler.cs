using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using Ranger.Common;
using Ranger.RabbitMQ;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants.Handlers
{
    class CreateTenantHandler : ICommandHandler<CreateTenant>
    {
        private readonly ILogger<CreateTenantHandler> logger;
        private readonly ITenantRepository tenantRepository;
        private readonly IBusPublisher busPublisher;

        public CreateTenantHandler(ILogger<CreateTenantHandler> logger, ITenantRepository tenantRepository, IBusPublisher busPublisher)
        {
            this.logger = logger;
            this.tenantRepository = tenantRepository;
            this.busPublisher = busPublisher;
        }

        public async Task HandleAsync(CreateTenant command, ICorrelationContext context)
        {
            logger.LogInformation("Handling CreateTenant message.");
            var random = new Random();
            var tenant = new Tenant()
            {
                CreatedOn = DateTime.UtcNow,
                OrganizationName = command.Domain.OrganizationName,
                Domain = command.Domain.DomainName,
                DatabaseUsername = Guid.NewGuid().ToString("N"),
                DatabasePassword = Crypto.GenerateSudoRandomPasswordString(),
                RegistrationKey = Crypto.GenerateSudoRandomAlphaNumericString(random.Next(12, 16)),
                Enabled = false
            };

            try
            {
                await this.tenantRepository.AddTenant(tenant);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"Failed to create tenant for domain: '{command.Domain.DomainName}'. Rejecting request.");
                throw new RangerException("Failed to create tenant.", ex);
            }

            logger.LogInformation($"Tenant created for domain: '{command.Domain.DomainName}'.");

            busPublisher.Publish<TenantCreated>(new TenantCreated(command.Domain.DomainName, tenant.DatabaseUsername, tenant.DatabasePassword, tenant.RegistrationKey, command.Owner), context);
        }
    }
}