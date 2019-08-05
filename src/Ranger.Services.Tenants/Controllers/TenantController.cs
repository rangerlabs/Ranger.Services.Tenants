using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants {
    [AllowAnonymous]
    public class TenantController : Controller {

        private readonly ITenantRepository tenantRepository;
        private readonly ILogger<TenantController> logger;
        public TenantController (ITenantRepository tenantRepository, ILogger<TenantController> logger) {
            this.tenantRepository = tenantRepository;
            this.logger = logger;
        }

        [HttpGet ("/tenant/exists/{domain}")]
        public async Task<IActionResult> Exists (string domain) {
            if (String.IsNullOrWhiteSpace (domain)) {
                return BadRequest (new { errors = $"{nameof(domain)} cannot be null or empty." });
            }
            if (await this.tenantRepository.ExistsAsync (domain)) {
                return Ok ();
            } else {
                return NotFound ();
            }
        }

        [HttpGet ("/tenant/{domain}")]
        public async Task<IActionResult> Index (string domain) {
            if (String.IsNullOrWhiteSpace (domain)) {
                return BadRequest (new { errors = $"{nameof(domain)} cannot be null or empty." });
            }
            Tenant tenant = await this.tenantRepository.FindTenantByDomainAsync (domain);
            if (tenant is null) {
                return NotFound ();
            }
            return Ok (tenant);
        }
    }
}