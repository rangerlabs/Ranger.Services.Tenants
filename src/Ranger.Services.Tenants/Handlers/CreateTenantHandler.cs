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
        private readonly ITenantsRepository tenantRepository;
        private readonly IBusPublisher busPublisher;

        public CreateTenantHandler(ILogger<CreateTenantHandler> logger, ITenantsRepository tenantRepository, IBusPublisher busPublisher)
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
                OrganizationName = command.OrganizationName,
                Domain = command.Domain,
                DatabaseUsername = Guid.NewGuid().ToString("N"),
                DatabasePassword = Crypto.GenerateSudoRandomPasswordString(),
                Token = Crypto.GenerateSudoRandomAlphaNumericString(random.Next(64, 64)),
                Enabled = false
            };

            try
            {
                await this.tenantRepository.AddTenant(command.Email, tenant);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"Failed to create tenant for domain: '{command.Domain}'. Rejecting request.");
                throw new RangerException("Failed to create tenant.");
            }

            logger.LogInformation($"Tenant created for domain: '{command.Domain}'.");

            busPublisher.Publish<TenantCreated>(new TenantCreated(command.Domain, command.Email, command.FirstName, command.LastName, command.Password, command.OrganizationName, tenant.DatabaseUsername, tenant.DatabasePassword, tenant.Token), context);
        }
    }
}