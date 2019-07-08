namespace Ranger.Services.Tenants {
    public class Domain {
        public Domain (string domainName, string organizationName) {
            this.DomainName = domainName;
            this.OrganizationName = organizationName;

        }
        public string DomainName { get; }
        public string OrganizationName { get; }
    }
}