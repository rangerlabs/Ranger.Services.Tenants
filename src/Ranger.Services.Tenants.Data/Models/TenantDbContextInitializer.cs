using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ranger.Common;

namespace Ranger.Services.Tenants.Data {
    public class TenantDbContextInitializer : ITenantDbContextInitializer {
        private readonly TenantDbContext context;

        public TenantDbContextInitializer (TenantDbContext context) {
            this.context = context;
        }

        public bool EnsureCreated () {
            return context.Database.EnsureCreated ();
        }

        public void Migrate () {
            context.Database.Migrate ();
        }
    }

    public interface ITenantDbContextInitializer {
        bool EnsureCreated ();
        void Migrate ();
    }
}