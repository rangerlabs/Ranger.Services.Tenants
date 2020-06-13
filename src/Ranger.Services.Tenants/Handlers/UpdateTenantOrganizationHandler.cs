using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.Common.Data.Exceptions;
using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants.Handlers
{
    public class UpdateTenantOrganizationHandler : ICommandHandler<UpdateTenantOrganization>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ITenantService tenantsService;
        private readonly ILogger<UpdateTenantOrganizationHandler> logger;

        public UpdateTenantOrganizationHandler(IBusPublisher busPublisher, ITenantService tenantsService, ILogger<UpdateTenantOrganizationHandler> logger)
        {
            this.busPublisher = busPublisher;
            this.tenantsService = tenantsService;
            this.logger = logger;
        }

        public async Task HandleAsync(UpdateTenantOrganization message, ICorrelationContext context)
        {
            logger.LogInformation("Handling UpdateTenantOrganization message");
            try
            {
                var updatedTenantResult = await tenantsService.UpdateTenantOrganizationDetailsAsync(message.TenantId, message.CommandingUserEmail, message.Version, message.OrganizationName, message.Domain);
                busPublisher.Publish(new TenantOrganizationUpdated(updatedTenantResult.tenant.OrganizationName, updatedTenantResult.tenant.Domain, updatedTenantResult.domainWasUpdated), context);
            }
            catch (EventStreamDataConstraintException ex)
            {
                logger.LogDebug(ex, "Failed to update organization details");
                throw new RangerException(ex.Message);
            }
            catch (ConcurrencyException ex)
            {
                throw new RangerException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new RangerException("An unexpected error occurred updating the organization details", ex);
            }
        }
    }
}