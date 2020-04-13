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
                throw new ApiException($"No tenant was found for the specified tenant id", statusCode: StatusCodes.Status404NotFound);
            }
            return new ApiResponse($"Successfully retrieved tenant", result: tenant, statusCode: StatusCodes.Status200OK);
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
                throw new ApiException($"No tenant was found for the specified domain", statusCode: StatusCodes.Status404NotFound);
            }
            return new ApiResponse($"Successfully retrieved tenant", result: tenant.TenantId, statusCode: StatusCodes.Status200OK);
        }

        /// <summary>
        /// Determines whether a domain has been reserved
        /// </summary>
        /// <param name="domain">The tenant's domain</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/tenants/{domain}/exists")]
        public async Task<ApiResponse> GetExists(string domain)
        {
            var exists = await this.tenantRepository.ExistsAsync(domain);
            return new ApiResponse($"Successfully determined domain existence", result: true, statusCode: StatusCodes.Status200OK);
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
                throw new ApiException($"No tenant was found for the specified domain", statusCode: StatusCodes.Status404NotFound);
            }
            return new ApiResponse($"Successfully determined domain confirmation ", result: confirmed, statusCode: StatusCodes.Status200OK);
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
                throw new ApiException($"No tenant was found for the specified domain", statusCode: StatusCodes.Status404NotFound);
            }
            if (tenant.PrimaryOwnerTransfer is null || (!(tenant.PrimaryOwnerTransfer.State is PrimaryOwnerTransferStateEnum.Pending) || tenant.PrimaryOwnerTransfer.InitiatedAt.Add(TimeSpan.FromDays(1)) <= DateTime.UtcNow))
            {
                throw new ApiException($"There is no pending primary owner transfer", statusCode: StatusCodes.Status404NotFound);
            }
            var result = new { CorrelationId = tenant.PrimaryOwnerTransfer.CorrelationId, TransferTo = tenant.PrimaryOwnerTransfer.TransferingToEmail };
            return new ApiResponse($"Successfully retrieved primary owner transfer", result: result, statusCode: StatusCodes.Status200OK);
        }

        /// <summary>
        /// Confirms a new tenant's domain
        /// </summary>
        /// <param name="domain">The tenant's domain</param>
        /// <param name="confirmModel">The confirmation model</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("/tenants/{domain}/confirm")]
        public async Task<ApiResponse> Confirm(string domain, ConfirmModel confirmModel)
        {
            TenantConfirmStatusEnum status = await tenantService.ConfirmTenantAsync(domain, confirmModel.Token);
            var errors = new ApiErrorContent();
            return status switch
            {
                TenantConfirmStatusEnum.InvalidToken => throw new ApiException($"The registration key is invalid. Failed to confirm the domain", statusCode: StatusCodes.Status400BadRequest),
                TenantConfirmStatusEnum.TenantNotFound => throw new ApiException($"No tenant was found for the specified domain", statusCode: StatusCodes.Status404NotFound),
                TenantConfirmStatusEnum.Confirmed => new ApiResponse($"Successfully retrieved primary owner transfer", statusCode: StatusCodes.Status200OK),
                TenantConfirmStatusEnum.PreviouslyConfirmed => new ApiResponse($"The tenant was previously confirmed", statusCode: StatusCodes.Status200OK),
                _ => throw new ApiException($"Unable to determine whether the tenant was confirmed", StatusCodes.Status500InternalServerError)
            };
        }
    }
}