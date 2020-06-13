using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.Common.Data.Exceptions;
using Ranger.RabbitMQ;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants.Handlers
{
    public class CompletePrimaryOwnerTransferHandler : ICommandHandler<CompletePrimaryOwnerTransfer>
    {
        private readonly ILogger<CompletePrimaryOwnerTransferHandler> logger;
        private readonly ITenantsRepository tenantsRepository;
        private readonly IBusPublisher busPublisher;

        public CompletePrimaryOwnerTransferHandler(ILogger<CompletePrimaryOwnerTransferHandler> logger, ITenantsRepository tenantsRepository, IBusPublisher busPublisher)
        {
            this.logger = logger;
            this.tenantsRepository = tenantsRepository;
            this.busPublisher = busPublisher;
        }

        public async Task HandleAsync(CompletePrimaryOwnerTransfer message, ICorrelationContext context)
        {
            logger.LogInformation("Handling InitiatePrimaryOwnerTransfer message");
            try
            {
                await tenantsRepository.CompletePrimaryOwnerTransferAsync(message.CommandingUserEmail, message.Tenantid, message.State);
            }
            catch (ConcurrencyException ex)
            {
                logger.LogDebug(ex, "Failed to complete the primary owner transfer");
                throw new RangerException(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred completing the primary owner transfer");
                throw new RangerException("An unexpected error occurred completing the primary owner transfer");
            }
            busPublisher.Publish(new PrimaryOwnerTransferCompleted(), context);
        }
    }
}