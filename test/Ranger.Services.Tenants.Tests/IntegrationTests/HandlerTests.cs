using System.Threading.Tasks;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.RabbitMQ.BusSubscriber;
using Ranger.Services.Tenants.Data;
using Shouldly;
using Xunit;

namespace Ranger.Services.Tenants.Tests
{
    public class HandlerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly IBusPublisher busPublisher;
        private readonly IBusSubscriber busSubscriber;
        private readonly ITenantService tenantService;
        private readonly ITenantsRepository tenantsRepository;
        private readonly CustomWebApplicationFactory _factory;

        public HandlerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            this.busPublisher = factory.Services.GetService(typeof(IBusPublisher)) as IBusPublisher;
            this.busSubscriber = factory.Services.GetService(typeof(IBusSubscriber)) as IBusSubscriber;
            this.tenantService = factory.Services.GetService(typeof(ITenantService)) as ITenantService;
            this.tenantsRepository = factory.Services.GetService(typeof(ITenantsRepository)) as ITenantsRepository;
        }
        [Fact]
        public void Tenants_Starts()
        { }

        [Fact]
        public async Task CreateTenantHandler_ReceivesMessage_CreatesTenant()
        {
            var msg = new CreateTenant("domain", "organization", "hello@gmail.com", "John", "Doe", "password");

            var handled = false;
            this.busSubscriber.SubscribeEventWithCallback<TenantCreated>((m, c) =>
            {
                handled = true;
                return Task.CompletedTask;
            });
            this.busPublisher.Send(msg, CorrelationContext.Empty);

            while (!handled) { }

            var tenant = await this.tenantsRepository.GetNotDeletedTenantByDomainAsync("domain");
            tenant.tenant.Domain.ShouldBe(msg.Domain);
            tenant.tenant.OrganizationName.ShouldBe(msg.OrganizationName);
            tenant.tenant.Confirmed.ShouldBeFalse();
            tenant.tenant.Deleted.ShouldBeFalse();
            tenant.tenant.Token.ShouldNotBeEmpty();
            tenant.tenant.DatabasePassword.ShouldNotBeEmpty();
        }
    }
}