﻿using System;
using System.Threading;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.Services.Tenants.Data;
using StackExchange.Redis;

namespace Ranger.Services.Tenants
{
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
    public class TenantController : Controller
    {
        private readonly ITenantService _tenantService;
        private readonly ITenantsRepository tenantRepository;
        private readonly ILogger<TenantController> logger;

        public TenantController(ITenantsRepository tenantRepository, ITenantService tenantService, ILogger<TenantController> logger)
        {
            this._tenantService = tenantService;
            this.tenantRepository = tenantRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Gets all Tenants or a specific Tenant for the given TenantId or Domain
        /// </summary>
        /// <param name="tenantId">The tenant's unique identitfier</param>
        /// <param name="domain">The tenant's domain identitfier</param>
        /// <param name="cancellationToken"></param>
        [HttpGet("/tenants")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetTenants([FromQuery] string tenantId, [FromQuery] string domain, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(tenantId) && string.IsNullOrWhiteSpace(domain))
            {
                var tenants = await this.tenantRepository.GetAllNotDeletedAndConfirmedTenantsAsync(cancellationToken);
                return new ApiResponse($"Successfully retrieved all confirmed tenants", result: tenants, statusCode: StatusCodes.Status200OK);
            }

            var tenantVersionTuple = (default(Tenant), default(int));
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                var redisResult = await _tenantService.GetTenantResponseModelOrDefaultFromRedisByDomainAsync(domain);
                if (!(redisResult is null))
                {
                    return new ApiResponse($"Successfully retrieved tenant", result: redisResult, statusCode: StatusCodes.Status200OK);
                }
                tenantVersionTuple = await this.tenantRepository.GetNotDeletedTenantByDomainAsync(domain, cancellationToken);
            }
            else
            {
                var redisResult = await _tenantService.GetTenantResponseModelOrDefaultFromRedisByIdAsync(tenantId);
                if (!(redisResult is null))
                {
                    return new ApiResponse($"Successfully retrieved tenant", result: redisResult, statusCode: StatusCodes.Status200OK);
                }
                tenantVersionTuple = await this.tenantRepository.GetNotDeletedTenantByTenantIdAsync(tenantId, cancellationToken);
            }
            if (tenantVersionTuple.Item1 is null)
            {
                throw new ApiException("No tenant was found for the specified tenant id", StatusCodes.Status404NotFound);
            }

            var result = new TenantResponseModel
            {
                TenantId = tenantVersionTuple.Item1.TenantId,
                CreatedOn = tenantVersionTuple.Item1.CreatedOn,
                OrganizationName = tenantVersionTuple.Item1.OrganizationName,
                Domain = tenantVersionTuple.Item1.Domain,
                DatabasePassword = tenantVersionTuple.Item1.DatabasePassword,
                Confirmed = tenantVersionTuple.Item1.Confirmed,
                PrimaryOwnerTransfer = tenantVersionTuple.Item1.PrimaryOwnerTransfer,
                Version = tenantVersionTuple.Item2
            };
            await _tenantService.SetTenantResponseModelsInRedis(result);
            return new ApiResponse($"Successfully retrieved tenant", result: result, statusCode: StatusCodes.Status200OK);
        }


        /// <summary>
        /// Determines whether a domain has been reserved
        /// </summary>
        /// <param name="domain">The tenant's domain</param>
        /// <param name="cancellationToken"></param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/tenants/{domain}/exists")]
        public async Task<ApiResponse> GetExists(string domain, CancellationToken cancellationToken)
        {
            try
            {
                var exists = await this.tenantRepository.ExistsAsync(domain, cancellationToken);
                return new ApiResponse($"Successfully determined domain existence", result: exists, statusCode: StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                var message = "Failed to determine tenant existence";
                logger.LogError(ex, message);
                throw new ApiException(message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Determines whether the tenant with the requested domain has been confirmed
        /// </summary>
        /// <param name="domain">The tenant's domain</param>
        /// <param name="cancellationToken"></param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/tenants/{domain}/confirmed")]
        public async Task<ApiResponse> GetConfirmed(string domain, CancellationToken cancellationToken)
        {
            var exists = await this.tenantRepository.ExistsAsync(domain, cancellationToken);
            if (!exists)
            {
                throw new ApiException("No tenant was found for the requested domain", StatusCodes.Status404NotFound);
            }
            var confirmed = await this.tenantRepository.IsTenantConfirmedAsync(domain, cancellationToken);
            return new ApiResponse($"Successfully determined domain confirmation ", result: confirmed, statusCode: StatusCodes.Status200OK);
        }

        /// <summary>
        /// Gets details of a pending Primary Owner Transfer
        /// </summary>
        /// <param name="domain">The tenant's domain</param>
        /// <param name="cancellationToken"></param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/tenants/{domain}/primary-owner-transfer")]
        public async Task<ApiResponse> GetPrimaryOwnerTransfer(string domain, CancellationToken cancellationToken)
        {
            var (tenant, _) = await this.tenantRepository.GetNotDeletedTenantByDomainAsync(domain, cancellationToken);
            if (tenant is null)
            {
                throw new ApiException("No tenant was found for the specified tenant id", StatusCodes.Status404NotFound);
            }
            if (tenant.PrimaryOwnerTransfer is null || (!(tenant.PrimaryOwnerTransfer.State is PrimaryOwnerTransferStateEnum.Pending) || tenant.PrimaryOwnerTransfer.InitiatedAt.Add(TimeSpan.FromDays(1)) <= DateTime.UtcNow))
            {
                return new ApiResponse("There currently is no pending primary owner transfer", statusCode: StatusCodes.Status200OK);
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
            TenantConfirmStatusEnum status = await _tenantService.ConfirmTenantAsync(domain, confirmModel.Token);
            return status switch
            {
                TenantConfirmStatusEnum.InvalidToken => throw new ApiException("The registration key is invalid. Failed to confirm the domain", StatusCodes.Status400BadRequest),
                TenantConfirmStatusEnum.TenantNotFound => throw new ApiException("No tenant was found for the specified tenant id", StatusCodes.Status404NotFound),
                TenantConfirmStatusEnum.Confirmed => new ApiResponse($"Successfully confirmed the tenant's domain", statusCode: StatusCodes.Status200OK),
                TenantConfirmStatusEnum.PreviouslyConfirmed => new ApiResponse($"The tenant was previously confirmed", statusCode: StatusCodes.Status200OK),
                _ => throw new ApiException(new RangerApiError($"Unable to determine whether the tenant was confirmed"), StatusCodes.Status500InternalServerError)
            };
        }
    }
}