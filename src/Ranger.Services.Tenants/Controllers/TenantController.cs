using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants
{
    [ApiController]
    public class TenantController : Controller
    {
        private readonly ITenantService tenantService;
        private readonly ITenantRepository tenantRepository;
        private readonly ILogger<TenantController> logger;

        public TenantController(ITenantRepository tenantRepository, ITenantService tenantService, ILogger<TenantController> logger)
        {
            this.tenantService = tenantService;
            this.tenantRepository = tenantRepository;
            this.logger = logger;
        }

        [HttpGet("/tenant/{domain}")]
        public async Task<IActionResult> Index(string domain)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                return BadRequest(new { errors = $"{nameof(domain)} cannot be null or empty." });
            }
            Tenant tenant = await this.tenantRepository.FindTenantByDomainAsync(domain);
            if (tenant is null)
            {
                return NotFound();
            }
            return Ok(tenant);
        }

        [HttpGet("/tenant/{domain}/exists")]
        public async Task<IActionResult> Exists(string domain)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                return BadRequest(new { errors = $"{nameof(domain)} cannot be null or empty." });
            }
            if (await this.tenantRepository.ExistsAsync(domain))
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("/tenant/{domain}/enabled")]
        public async Task<IActionResult> Enabled(string domain)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                return BadRequest(new { errors = $"{nameof(domain)} cannot be null or empty." });
            }
            Tenant tenant = await this.tenantRepository.FindTenantByDomainAsync(domain);
            if (tenant is null)
            {
                return NotFound();
            }
            return Ok(new TenantEnabledModel { Enabled = tenant.Enabled });
        }

        [HttpPut("tenant/{domain}/confirm")]
        public async Task<IActionResult> Confirm(string domain, ConfirmModel confirmModel)
        {
            TenantConfirmStatusEnum status = await tenantService.ConfirmTenantAsync(domain, confirmModel.RegistrationKey);
            switch (status)
            {
                case TenantConfirmStatusEnum.InvalidRegistrationKey:
                    return StatusCode(StatusCodes.Status409Conflict, (new { error = "The registration key is invalid. Failed to confirm the domain." }));
                case TenantConfirmStatusEnum.TenantNotFound:
                    return NotFound();

                case TenantConfirmStatusEnum.Confirmed:
                case TenantConfirmStatusEnum.PreviouslyConfirmed:
                default:
                    return Ok();
            }
        }
    }
}