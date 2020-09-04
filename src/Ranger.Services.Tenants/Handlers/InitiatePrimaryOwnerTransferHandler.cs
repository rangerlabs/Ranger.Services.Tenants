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
    public class InitiatePrimaryOwnerTransferHandler : ICommandHandler<InitiatePrimaryOwnerTransfer>
    {
        private readonly ILogger<InitiatePrimaryOwnerTransferHandler> _logger;
        private readonly ITenantService _tenantService;
        private readonly ITenantsRepository _tenantsRepository;
        private readonly IBusPublisher _busPublisher;

        public InitiatePrimaryOwnerTransferHandler(ILogger<InitiatePrimaryOwnerTransferHandler> logger, ITenantService tenantService, ITenantsRepository tenantsRepository, IBusPublisher busPublisher)
        {
            this._logger = logger;
            this._tenantService = tenantService;
            this._tenantsRepository = tenantsRepository;
            this._busPublisher = busPublisher;
        }

        public async Task HandleAsync(InitiatePrimaryOwnerTransfer message, ICorrelationContext context)
        {
            _logger.LogInformation("Handling InitiatePrimaryOwnerTransfer message");
            var primaryOwnerTransfer = PrimaryOwnerTransfer.Create(message.CommandingUserEmail, message.TransferUserEmail, context.CorrelationContextId);
            try
            {
                var domain = await _tenantsRepository.AddPrimaryOwnerTransferAsync(message.CommandingUserEmail, message.TenantId, primaryOwnerTransfer);
                await _tenantService.RemoveTenantResponseModelsFromRedis(message.TenantId, domain);
            }
            catch (ConcurrencyException ex)
            {
                _logger.LogDebug(ex, "Failed to intiate the domain transfer");
                throw new RangerException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred intiating the domain transfer");
                throw new RangerException("An unexpected error occurred intiating the domain transfer");
            }
            _busPublisher.Publish(new PrimaryOwnerTransferInitiated(), context);
        }
    }
}