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
        private readonly ILogger<DeleteTenantHandler> logger;
        private readonly ITenantsRepository tenantRepository;
        private readonly IBusPublisher busPublisher;

        public DeleteTenantHandler(ILogger<DeleteTenantHandler> logger, ITenantsRepository tenantRepository, IBusPublisher busPublisher)
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
                var orgNameOfDeleted = await this.tenantRepository.SoftDelete(command.CommandingUserEmail, command.TenantId);
                logger.LogInformation("Tenant domain deleted {TenantId}", command.TenantId);
                busPublisher.Publish(new TenantDeleted(command.TenantId, orgNameOfDeleted), context);
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