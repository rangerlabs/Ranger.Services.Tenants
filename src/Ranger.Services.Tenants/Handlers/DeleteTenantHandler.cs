using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using Ranger.Common;
using Ranger.RabbitMQ;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants.Handlers
{
    class DeleteTenantHandler : ICommandHandler<DeleteTenant>
    {
        private readonly ILogger<CreateTenantHandler> logger;
        private readonly ITenantRepository tenantRepository;
        private readonly IBusPublisher busPublisher;

        public DeleteTenantHandler(ILogger<CreateTenantHandler> logger, ITenantRepository tenantRepository, IBusPublisher busPublisher)
        {
            this.logger = logger;
            this.tenantRepository = tenantRepository;
            this.busPublisher = busPublisher;
        }

        public async Task HandleAsync(DeleteTenant command, ICorrelationContext context)
        {
            logger.LogInformation("Handling DeleteTenant message.");
            try
            {
                var tenant = await this.tenantRepository.FindTenantByDomainAsync(command.Domain);
                if (tenant is null)
                {
                    throw new RangerException($"No tenant found for domain {command.Domain}.");
                }
                await this.tenantRepository.Delete(tenant);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"Failed to delete tenant for domain: '{command.Domain}'.");
            }

            logger.LogInformation($"Tenant domain deleted: '{command.Domain}'.");
            busPublisher.Publish(new TenantDeleted(command.Domain), context);
        }
    }
}