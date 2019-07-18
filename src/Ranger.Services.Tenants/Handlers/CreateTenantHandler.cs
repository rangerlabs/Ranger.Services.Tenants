using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using Ranger.Common;
using Ranger.RabbitMQ;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants.Handlers {
    class CreateTenantHandler : ICommandHandler<CreateTenant> {
        private readonly ILogger<CreateTenantHandler> logger;
        private readonly ITenantRepository tenantRepository;
        private readonly IBusPublisher busPublisher;

        public CreateTenantHandler (ILogger<CreateTenantHandler> logger, ITenantRepository tenantRepository, IBusPublisher busPublisher) {
            this.logger = logger;
            this.tenantRepository = tenantRepository;
            this.busPublisher = busPublisher;
        }

        public async Task HandleAsync (CreateTenant command) {
            logger.LogInformation ("Handling CreateTenant message.");
            var random = new Random ();
            var tenant = new Tenant () {
                CreatedOn = DateTime.UtcNow,
                OrganizationName = command.Domain.OrganizationName,
                Domain = command.Domain.DomainName,
                DatabaseUsername = command.Domain.DomainName,
                DatabasePassword = Crypto.GenerateSudoRandomPasswordString (),
                RegistrationKey = Crypto.GenerateSudoRandomAlphaNumericString (random.Next (12, 16)),
                DomainConfirmed = false
            };
            await this.tenantRepository.AddTenant (tenant);
            logger.LogInformation ($"Tenant created for domain: '{command.Domain.DomainName}'.");

            busPublisher.Publish<TenantCreated> (new TenantCreated (command.CorrelationContext, command.Domain.DomainName, tenant.DatabaseUsername, tenant.DatabasePassword, command.User));
        }
    }
}