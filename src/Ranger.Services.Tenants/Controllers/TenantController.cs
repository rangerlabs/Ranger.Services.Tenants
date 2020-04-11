using System;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants
{
    [ApiController]
    [ApiVersion("1.0")]
    public class TenantController : Controller
    {
        private readonly ITenantService tenantService;
        private readonly ITenantsRepository tenantRepository;
        private readonly ILogger<TenantController> logger;

        public TenantController(ITenantsRepository tenantRepository, ITenantService tenantService, ILogger<TenantController> logger)
        {
            this.tenantService = tenantService;
            this.tenantRepository = tenantRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Gets a Tenant for the given TenantId
        /// </summary>
        /// <param name="tenantId">The tenant's unique identitfier</param>
        [HttpGet("/tenants/{tenantId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetTenantByTenantId(string tenantId)
        {
            Tenant tenant = await this.tenantRepository.FindTenantByTenantIdAsync(tenantId);
            if (tenant is null)
            {
                return new ApiResponse($"No tenant was found for tenant id {tenantId}", statusCode: StatusCodes.Status404NotFound, apiVersion: "1.0.");
            }
            return new ApiResponse($"Tenant was found for tenant id {tenantId}", result: tenant, statusCode: StatusCodes.Status200OK, apiVersion: "1.0");
        }

        /// <summary>
        /// Gets a tenant's unique identifier for the given domain
        /// </summary>
        /// <param name="domain">The tenant's domain</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/tenants/{domain}/id")]
        public async Task<ApiResponse> GetTenantIdByDomain(string domain)
        {
            Tenant tenant = await this.tenantRepository.FindNotDeletedTenantByDomainAsync(domain);
            if (tenant is null)
            {
                return new ApiResponse($"No tenant was found for domain {domain}", result: tenant.TenantId, statusCode: StatusCodes.Status404NotFound, apiVersion: "1.0");
            }
            return new ApiResponse($"Tenant was found for domain {domain}", result: tenant.TenantId, statusCode: StatusCodes.Status200OK, apiVersion: "1.0");
        }

        /// <summary>
        /// Determines whether a domain has been reserved
        /// </summary>
        /// <param name="domain">The tenant's domain</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/tenants/{domain}/exists")]
        public async Task<ApiResponse> GetExists(string domain)
        {
            if (await this.tenantRepository.ExistsAsync(domain))
            {
                return new ApiResponse($"The domain {domain} is available", result: true, statusCode: StatusCodes.Status200OK, apiVersion: "1.0");
            }
            else
            {
                return new ApiResponse($"The domain {domain} is NOT available", result: false, statusCode: StatusCodes.Status200OK, apiVersion: "1.0");
            }
        }

        /// <summary>
        /// Determines whether the tenant with the requested domain has been confirmed
        /// </summary>
        /// <param name="domain">The tenant's domain</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/tenants/{domain}/confirmed")]
        public async Task<ApiResponse> GetConfirmed(string domain)
        {
            var (exists, confirmed) = await this.tenantRepository.IsTenantConfirmedAsync(domain);
            if (!exists)
            {
                return new ApiResponse($"No tenant was found for domain {domain}", statusCode: StatusCodes.Status404NotFound, apiVersion: "1.0");
            }
            return new ApiResponse($"Success", result: confirmed, statusCode: StatusCodes.Status200OK, apiVersion: "1.0");
        }

        /// <summary>
        /// Gets details of a pending Primary Owner Transfer
        /// </summary>
        /// <param name="domain">The tenant's domain</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/tenants/{domain}/primary-owner-transfer")]
        public async Task<ApiResponse> GetPrimaryOwnerTransfer(string domain)
        {
            Tenant tenant = await this.tenantRepository.FindNotDeletedTenantByDomainAsync(domain);
            if (tenant is null)
            {
                return new ApiResponse($"No tenant was found for the specified domain", statusCode: StatusCodes.Status404NotFound, apiVersion: "1.0");
            }
            if (tenant.PrimaryOwnerTransfer is null || (!(tenant.PrimaryOwnerTransfer.State is PrimaryOwnerTransferStateEnum.Pending) || tenant.PrimaryOwnerTransfer.InitiatedAt.Add(TimeSpan.FromDays(1)) <= DateTime.UtcNow))
            {
                return new ApiResponse($"There is no pending Primary Owner Transfer", statusCode: StatusCodes.Status200OK, apiVersion: "1.0");
            }
            var result = new { CorrelationId = tenant.PrimaryOwnerTransfer.CorrelationId, TransferTo = tenant.PrimaryOwnerTransfer.TransferingToEmail };
            return new ApiResponse($"Success", result: result, statusCode: StatusCodes.Status200OK, apiVersion: "1.0");
        }

        /// <summary>
        /// Confirms a new tenant's domain
        /// </summary>
        /// <param name="domain">The tenant's domain</param>
        /// <param name="confirmModel">The confirmation model</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("/tenants/{domain}/confirm")]
        public async Task<ApiResponse> Confirm(string domain, ConfirmModel confirmModel)
        {
            TenantConfirmStatusEnum status = await tenantService.ConfirmTenantAsync(domain, confirmModel.Token);
            var errors = new ApiErrorContent();
            return status switch
            {
                TenantConfirmStatusEnum.InvalidToken => new ApiResponse($"The registration key is invalid. Failed to confirm the domain", statusCode: StatusCodes.Status400BadRequest, apiVersion: "1.0"),
                TenantConfirmStatusEnum.TenantNotFound => new ApiResponse($"No tenant was found for the specified domain", statusCode: StatusCodes.Status200OK, apiVersion: "1.0"),
                TenantConfirmStatusEnum.Confirmed => new ApiResponse($"Success", statusCode: StatusCodes.Status200OK, apiVersion: "1.0"),
                TenantConfirmStatusEnum.PreviouslyConfirmed => new ApiResponse($"The tenant was previously confirmed", statusCode: StatusCodes.Status200OK, apiVersion: "1.0"),
                _ => throw new ApiException($"Unable to determine whether the tenant was confirmed", StatusCodes.Status500InternalServerError)
            };
        }
    }
}