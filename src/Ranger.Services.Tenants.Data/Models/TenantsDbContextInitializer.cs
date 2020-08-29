using Microsoft.EntityFrameworkCore;

namespace Ranger.Services.Tenants.Data
{
    public class TenantsDbContextInitializer : ITenantsDbContextInitializer
    {
        private readonly TenantsDbContext context;

        public TenantsDbContextInitializer(TenantsDbContext context)
        {
            this.context = context;
        }
        public void Migrate()
        {
            context.Database.Migrate();
        }
    }

    public interface ITenantsDbContextInitializer
    {
        void Migrate();
    }
}