using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ranger.Common;

namespace Ranger.Services.Tenants.Data
{
    public class TenantsDbContextInitializer : ITenantsDbContextInitializer
    {
        private readonly TenantsDbContext context;

        public TenantsDbContextInitializer(TenantsDbContext context)
        {
            this.context = context;
        }

        public bool EnsureCreated()
        {
            return context.Database.EnsureCreated();
        }

        public void Migrate()
        {
            context.Database.Migrate();
        }
    }

    public interface ITenantsDbContextInitializer
    {
        bool EnsureCreated();
        void Migrate();
    }
}