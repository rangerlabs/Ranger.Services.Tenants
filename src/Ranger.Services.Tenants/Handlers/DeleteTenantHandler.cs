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
            logger.LogInformation("Handling DeleteTenant message");
            try
            {
                var tenantVersionTuple = await this.tenantRepository.FindNotDeletedTenantByTenantIdAsync(command.TenantId);
                if (tenantVersionTuple.tenant is null)
                {
                    throw new RangerException($"No tenant found for domain {command.TenantId}");
                }
                await this.tenantRepository.SoftDelete(command.CommandingUserEmail, command.TenantId);
                logger.LogInformation("Tenant domain deleted {TenantId}", command.TenantId);
                busPublisher.Publish(new TenantDeleted(command.TenantId, tenantVersionTuple.tenant.OrganizationName), context);
            }
            catch (ConcurrencyException ex)
            {
                logger.LogDebug(ex, "Failed to delete the tenant {TenantId}}", command.TenantId);
                throw new RangerException(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred deleting tenant {TenantId}", command.TenantId);
                throw new RangerException($"An unexpected error occurred deleting tenant with TenantId '{command.TenantId}'");
            }
        }
    }
}