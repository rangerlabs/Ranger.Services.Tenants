using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using Ranger.Common;
using Ranger.Common.Data.Exceptions;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants.Handlers
{
    class DeleteTenantHandler : ICommandHandler<DeleteTenant>
    {
        private readonly ILogger<DeleteTenantHandler> _logger;
        private readonly ITenantService _tenantService;
        private readonly ITenantsRepository _tenantRepository;
        private readonly IBusPublisher _busPublisher;

        public DeleteTenantHandler(ILogger<DeleteTenantHandler> logger, ITenantService tenantService, ITenantsRepository tenantRepository, IBusPublisher busPublisher)
        {
            this._logger = logger;
            this._tenantService = tenantService;
            this._tenantRepository = tenantRepository;
            this._busPublisher = busPublisher;
        }

        public async Task HandleAsync(DeleteTenant command, ICorrelationContext context)
        {
            _logger.LogInformation("Handling DeleteTenant message");
            try
            {
                var (orgNameOfDeleted, domainOfDeleted) = await this._tenantRepository.SoftDelete(command.CommandingUserEmail, command.TenantId);
                await _tenantService.RemoveTenantResponseModelsFromRedis(command.TenantId, domainOfDeleted);
                _logger.LogInformation("Tenant domain deleted {TenantId}", command.TenantId);
                _busPublisher.Publish(new TenantDeleted(command.TenantId, orgNameOfDeleted), context);
            }
            catch (ConcurrencyException ex)
            {
                _logger.LogDebug(ex, "Failed to delete the tenant {TenantId}}", command.TenantId);
                throw new RangerException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred deleting tenant {TenantId}", command.TenantId);
                throw new RangerException($"An unexpected error occurred deleting tenant with TenantId '{command.TenantId}'");
            }
        }
    }
}