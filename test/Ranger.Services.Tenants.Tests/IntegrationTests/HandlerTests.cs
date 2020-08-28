using System.Threading.Tasks;
using Ranger.RabbitMQ;
using Ranger.Services.Tenants.Data;
using Shouldly;
using Xunit;

namespace Ranger.Services.Tenants.Tests.IntegrationTests
{
    public class HandlerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ITenantService tenantService;
        private readonly ITenantsRepository tenantsRepository;
        private readonly CustomWebApplicationFactory _factory;

        public HandlerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            this.busPublisher = factory.Services.GetService(typeof(IBusPublisher)) as IBusPublisher;
            this.tenantService = factory.Services.GetService(typeof(ITenantService)) as ITenantService;
            this.tenantsRepository = factory.Services.GetService(typeof(ITenantsRepository)) as ITenantsRepository;
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