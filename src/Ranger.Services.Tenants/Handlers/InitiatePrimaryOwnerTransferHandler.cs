using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.Common.Data.Exceptions;
using Ranger.RabbitMQ;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants.Handlers
{
    public class InitiatePrimaryOwnerTransferHandler : ICommandHandler<InitiatePrimaryOwnerTransfer>
    {
        private readonly ILogger<InitiatePrimaryOwnerTransferHandler> logger;
        private readonly ITenantsRepository tenantsRepository;
        private readonly IBusPublisher busPublisher;

        public InitiatePrimaryOwnerTransferHandler(ILogger<InitiatePrimaryOwnerTransferHandler> logger, ITenantsRepository tenantsRepository, IBusPublisher busPublisher)
        {
            this.logger = logger;
            this.tenantsRepository = tenantsRepository;
            this.busPublisher = busPublisher;
        }

        public async Task HandleAsync(InitiatePrimaryOwnerTransfer message, ICorrelationContext context)
        {
            logger.LogInformation("Handling InitiatePrimaryOwnerTransfer message.");
            var primaryOwnerTransfer = PrimaryOwnerTransfer.Create(message.CommandingUserEmail, message.TransferUserEmail, context.CorrelationContextId);
            try
            {
                await tenantsRepository.AddPrimaryOwnerTransferAsync(message.CommandingUserEmail, message.Domain, primaryOwnerTransfer);
            }
            catch (ConcurrencyException ex)
            {
                logger.LogError(ex, "Failed to initiate the primary owner transfer.");
                throw new RangerException(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initiate the primary owner transfer.");
                throw;
            }
            busPublisher.Publish(new PrimaryOwnerTransferInitiated(), context);
        }
    }
}