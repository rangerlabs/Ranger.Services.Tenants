using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using Ranger.Common;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
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
            var random = new Random();
            var databasePassword = Crypto.GenerateSudoRandomPasswordString();
            var tenant = new Tenant()
            {
                TenantId = Guid.NewGuid().ToString("N"),
                CreatedOn = DateTime.UtcNow,
                OrganizationName = command.OrganizationName,
                Domain = command.Domain,
                DatabasePassword = databasePassword,
                Token = Crypto.GenerateSudoRandomAlphaNumericString(random.Next(64, 64)),
            };

            try
            {
                await this.tenantRepository.AddTenant(command.Email, tenant);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create tenant for domain {Domain}", command.Domain);
                throw new RangerException($"An unexpected error occurred creating domain '{command.Domain}'");
            }

            logger.LogInformation($"Tenant created for domain: '{command.Domain}'");
            busPublisher.Publish<TenantCreated>(new TenantCreated(tenant.TenantId, command.Email, command.FirstName, command.LastName, command.Password, command.OrganizationName, databasePassword, tenant.Token), context);
        }
    }
}