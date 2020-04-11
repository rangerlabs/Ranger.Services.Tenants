using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using Ranger.Common;
using Ranger.Common.Data.Exceptions;
using Ranger.RabbitMQ;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants.Handlers
{
    class DeleteTenantHandler : ICommandHandler<DeleteTenant>
    {
        private readonly ILogger<CreateTenantHandler> logger;
        private readonly ITenantsRepository tenantRepository;
        private readonly IBusPublisher busPublisher;

        public DeleteTenantHandler(ILogger<CreateTenantHandler> logger, ITenantsRepository tenantRepository, IBusPublisher busPublisher)
        {
            this.logger = logger;
            this.tenantRepository = tenantRepository;
            this.busPublisher = busPublisher;
        }

        public async Task HandleAsync(DeleteTenant command, ICorrelationContext context)
        {
            Tenant tenant = null;
            logger.LogInformation("Handling DeleteTenant message.");
            try
            {
                tenant = await this.tenantRepository.FindNotDeletedTenantByDomainAsync(command.TenantId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"Failed to retrieve tenant.");
                throw;
            }

            if (tenant is null)
            {
                throw new RangerException($"No tenant found for domain {command.TenantId}.");
            }

            try
            {
                await this.tenantRepository.SoftDelete(command.CommandingUserEmail, command.TenantId);
            }
            catch (ConcurrencyException ex)
            {
                logger.LogWarning(ex.Message);
                throw new RangerException(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to delete the tenant with domain '{command.TenantId}'.");
                throw new RangerException("Failed to delete the tenant. No additional data could be provided.");
            }
            logger.LogInformation($"Tenant domain deleted: '{command.TenantId}'.");
            busPublisher.Publish(new TenantDeleted(command.TenantId, tenant.OrganizationName), context);
        }
    }
}