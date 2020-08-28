using System.Threading.Tasks;
using Ranger.RabbitMQ;
using Ranger.Services.Tenants.Data;
using Shouldly;
using Xunit;

namespace Ranger.Services.Tenants.Tests.IntegrationTests
{
    public class HandlerTests
    {
        private readonly IBusPublisher busPublisher;
        private readonly ITenantService tenantService;
        private readonly ITenantsRepository tenantsRepository;

        public HandlerTests(IBusPublisher busPublisher, ITenantService tenantService, ITenantsRepository tenantsRepository)
        {
            this.busPublisher = busPublisher;
            this.tenantService = tenantService;
            this.tenantsRepository = tenantsRepository;
        }

        [Fact]
        public async Task CreateTenantHandler_ReceivesMessage_CreatesTenant()
        {
            var msg = new CreateTenant("domain", "organization", "hello@gmail.com", "John", "Doe", "password");
            this.busPublisher.Send(msg, CorrelationContext.Empty);
            var tenant = await this.tenantsRepository.GetNotDeletedTenantByDomainAsync("domain");
            tenant.tenant.Domain.ShouldBeSameAs(msg.Domain);
        }
    }
}