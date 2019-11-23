using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.Common;
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

        [HttpGet("/tenant")]
        public async Task<IActionResult> GetTenantByDatabaseName([FromQuery]string databaseUsername)
        {
            if (String.IsNullOrWhiteSpace(databaseUsername))
            {
                return BadRequest(new { errors = $"{nameof(databaseUsername)} cannot be null or empty." });
            }
            Tenant tenant = await this.tenantRepository.FindTenantEnabledByDatabaseUsernameAsync(databaseUsername);
            if (tenant is null)
            {
                var errors = new ApiErrorContent();
                errors.Errors.Add($"No tenant was found with database username '{databaseUsername}'.");
                return NotFound(errors);
            }
            return Ok(tenant);
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
                var errors = new ApiErrorContent();
                errors.Errors.Add($"No tenant was found for domain '{domain}'.");
                return NotFound(errors);
            }
            return Ok(tenant);
        }

        [HttpGet("/tenant/{domain}/exists")]
        public async Task<IActionResult> Exists(string domain)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                var errors = new ApiErrorContent();
                errors.Errors.Add($"{nameof(domain)} cannot be null or empty.");
                return BadRequest(errors);
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
                var errors = new ApiErrorContent();
                errors.Errors.Add($"{nameof(domain)} cannot be null or empty.");
                return BadRequest(errors);
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
            TenantConfirmStatusEnum status = await tenantService.ConfirmTenantAsync(domain, confirmModel.Token);
            var errors = new ApiErrorContent();
            switch (status)
            {
                case TenantConfirmStatusEnum.InvalidToken:
                    {
                        errors.Errors.Add("The registration key is invalid. Failed to confirm the domain.");
                        return StatusCode(StatusCodes.Status409Conflict, errors);
                    }
                case TenantConfirmStatusEnum.TenantNotFound:
                    errors.Errors.Add($"No tenant was foud for domain '{domain}'.");
                    return NotFound(errors);

                case TenantConfirmStatusEnum.Confirmed:
                case TenantConfirmStatusEnum.PreviouslyConfirmed:
                default:
                    return Ok();
            }
        }
    }
}