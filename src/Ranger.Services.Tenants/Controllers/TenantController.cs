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

        [HttpGet]
        public async Task<IActionResult> Exists (string domain) {
            IActionResult response = StatusCode (StatusCodes.Status500InternalServerError);
            if (String.IsNullOrWhiteSpace (domain)) {
                response = BadRequest ("Domain cannot be null or empty.");
            }
            if (await this.tenantRepository.ExistsAsync (domain)) {
                response = Ok (true);
            } else {
                response = Ok (false);
            }
            return response;
        }

        [HttpGet]
        public async Task<IActionResult> Index (string domain) {
            IActionResult response = StatusCode (StatusCodes.Status500InternalServerError);
            if (String.IsNullOrWhiteSpace (domain)) {
                response = BadRequest ("Domain cannot be null or empty.");
            }
            Tenant tenant = await this.tenantRepository.FindTenantByDomainAsync (domain);
            response = Ok (tenant);
            return response;
        }

        [HttpGet]
        public async Task<IActionResult> ConnectionString (string domain) {
            IActionResult response = StatusCode (StatusCodes.Status500InternalServerError);
            if (String.IsNullOrWhiteSpace (domain)) {
                response = BadRequest ("Domain cannot be null or empty.");
            }
            var connectionString = await this.tenantRepository.GetConnectionStringByDomainAsync (domain);
            response = Ok (connectionString);
            return response;
        }
    }
}