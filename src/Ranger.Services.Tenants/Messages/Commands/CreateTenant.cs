using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants
{

    [MessageNamespace("tenants")]
    public class CreateTenant : ICommand
    {

        public CreateTenant(string tenantId, string organizationName, string email, string firstName, string lastName, string password)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new System.ArgumentException($"{nameof(organizationName)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new System.ArgumentException($"{nameof(email)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new System.ArgumentException($"{nameof(firstName)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new System.ArgumentException($"{nameof(lastName)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new System.ArgumentException($"{nameof(password)} was null or whitespace.");
            }
            this.TenantId = tenantId;
            this.OrganizationName = organizationName;
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Password = password;

        }
        public string TenantId { get; }
        public string OrganizationName { get; }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Password { get; }

    }
}