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
            var tenantVersionTuple = (default(Tenant), default(int));
            try
            {
                tenantVersionTuple = await this.tenantRepository.FindNotDeletedTenantByDomainAsync(command.TenantId);
                if (tenantVersionTuple.Item1 is null)
                {
                    throw new RangerException($"No tenant found for domain {command.TenantId}");
                }
                await this.tenantRepository.SoftDelete(command.CommandingUserEmail, command.TenantId);
            }
            catch (ConcurrencyException ex)
            {
                logger.LogDebug(ex, "Failed to delete the tenant {TenantId}}", command.TenantId);
                throw new RangerException(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred deleting domain {Domain}", tenantVersionTuple.Item1.Domain);
                throw new RangerException($"An unexpected error occurred deleting domain '{tenantVersionTuple.Item1.Domain}'");
            }
            logger.LogInformation("Tenant domain deleted {TenantId}", command.TenantId);
            busPublisher.Publish(new TenantDeleted(command.TenantId, tenantVersionTuple.Item1.OrganizationName), context);
        }
    }
}