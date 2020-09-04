using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.Common.Data.Exceptions;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;

namespace Ranger.Services.Tenants.Handlers
{
    public class UpdateTenantOrganizationHandler : ICommandHandler<UpdateTenantOrganization>
    {
        private readonly IBusPublisher _busPublisher;
        private readonly ITenantService _tenantsService;
        private readonly ILogger<UpdateTenantOrganizationHandler> _logger;

        public UpdateTenantOrganizationHandler(IBusPublisher busPublisher, ITenantService tenantsService, ILogger<UpdateTenantOrganizationHandler> logger)
        {
            this._busPublisher = busPublisher;
            this._tenantsService = tenantsService;
            this._logger = logger;
        }

        public async Task HandleAsync(UpdateTenantOrganization message, ICorrelationContext context)
        {
            _logger.LogInformation("Handling UpdateTenantOrganization message");
            try
            {
                var updatedTenantResult = await _tenantsService.UpdateTenantOrganizationDetailsAsync(message.TenantId, message.CommandingUserEmail, message.Version, message.OrganizationName, message.Domain);
                await _tenantsService.RemoveTenantResponseModelsFromRedis(message.TenantId, message.Domain);
                _busPublisher.Publish(new TenantOrganizationUpdated(updatedTenantResult.tenant.OrganizationName, updatedTenantResult.tenant.Domain, updatedTenantResult.domainWasUpdated, updatedTenantResult.oldDomain), context);
            }
            catch (EventStreamDataConstraintException ex)
            {
                _logger.LogDebug(ex, "Failed to update organization details");
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