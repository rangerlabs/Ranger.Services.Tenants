using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.Common.Data.Exceptions;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants.Handlers
{
    public class CompletePrimaryOwnerTransferHandler : ICommandHandler<CompletePrimaryOwnerTransfer>
    {
        private readonly ILogger<CompletePrimaryOwnerTransferHandler> _logger;
        private readonly ITenantService _tenantService;
        private readonly ITenantsRepository _tenantsRepository;
        private readonly IBusPublisher _busPublisher;

        public CompletePrimaryOwnerTransferHandler(ILogger<CompletePrimaryOwnerTransferHandler> logger, ITenantService tenantService, ITenantsRepository tenantsRepository, IBusPublisher busPublisher)
        {
            this._logger = logger;
            this._tenantService = tenantService;
            this._tenantsRepository = tenantsRepository;
            this._busPublisher = busPublisher;
        }

        public async Task HandleAsync(CompletePrimaryOwnerTransfer message, ICorrelationContext context)
        {
            _logger.LogInformation("Handling InitiatePrimaryOwnerTransfer message");
            try
            {
                var domain = await _tenantsRepository.CompletePrimaryOwnerTransferAsync(message.CommandingUserEmail, message.TenantId, message.State);
                await _tenantService.RemoveTenantResponseModelsFromRedis(message.TenantId, domain);
            }
            catch (ConcurrencyException ex)
            {
                _logger.LogDebug(ex, "Failed to complete the primary owner transfer");
                throw new RangerException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred completing the primary owner transfer");
                throw new RangerException("An unexpected error occurred completing the primary owner transfer");
            }
            _busPublisher.Publish(new PrimaryOwnerTransferCompleted(), context);
        }
    }
}