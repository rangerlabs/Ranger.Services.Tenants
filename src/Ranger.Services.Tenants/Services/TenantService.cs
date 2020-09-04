using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ranger.Services.Tenants.Data;
using StackExchange.Redis;

namespace Ranger.Services.Tenants
{

    public class TenantsService : ITenantService
    {
        private readonly ITenantsRepository _tenantRepository;
        private readonly IDatabase redisDb;

        public TenantsService(ITenantsRepository tenantRepository, IConnectionMultiplexer connectionMultiplexer)
        {
            _tenantRepository = tenantRepository;
            redisDb = connectionMultiplexer.GetDatabase();
        }

        public async Task<TenantResponseModel> GetTenantResponseModelOrDefaultFromRedisByIdAsync(string tenantId)
        {
            string result = await redisDb.StringGetAsync(RedisKeys.GetTenantId(tenantId));
            if (String.IsNullOrWhiteSpace(result))
            {
                return default;
            }
            else
            {
                return JsonConvert.DeserializeObject<TenantResponseModel>(result);
            }
        }

        public async Task<TenantResponseModel> GetTenantResponseModelOrDefaultFromRedisByDomainAsync(string domain)
        {
            string result = await redisDb.StringGetAsync(RedisKeys.GetTenantDomain(domain));
            if (String.IsNullOrWhiteSpace(result))
            {
                return default;
            }
            else
            {
                return JsonConvert.DeserializeObject<TenantResponseModel>(result);
            }
        }

        public async Task<TenantConfirmStatusEnum> ConfirmTenantAsync(string domain, string token)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"'{nameof(domain)}' was null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new System.ArgumentException($"'{nameof(token)}' was null or whitespace");
            }

            var (tenant, _) = await _tenantRepository.GetNotDeletedTenantByDomainAsync(domain);
            if (tenant is null)
            {
                return TenantConfirmStatusEnum.TenantNotFound;
            }
            if (tenant.Confirmed)
            {
                return TenantConfirmStatusEnum.PreviouslyConfirmed;
            }

            if (tenant.Token == token)
            {
                tenant.Token = "";
                tenant.Confirmed = true;
                await _tenantRepository.UpdateTenantAsync("Anonymous", "TenantConfirmed", 1, tenant);
                return TenantConfirmStatusEnum.Confirmed;
            }
            return TenantConfirmStatusEnum.InvalidToken;
        }

        public async Task<(Tenant tenant, bool domainWasUpdated, string oldDomain)> UpdateTenantOrganizationDetailsAsync(string tenantId, string commandingUserEmail, int version, string organizationName = "", string domain = "")
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"'{nameof(tenantId)}' cannot be null or whitespace", nameof(tenantId));
            }

            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new ArgumentException($"'{nameof(commandingUserEmail)}' cannot be null or whitespace", nameof(commandingUserEmail));
            }

            if (string.IsNullOrWhiteSpace(organizationName) && string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"'{nameof(organizationName)}' or '{nameof(domain)}' must not null or whitespace", nameof(organizationName));
            }

            if (version <= 1)
            {
                throw new ArgumentException($"'{nameof(version)}' must be greater than 1", nameof(version));
            }

            var tenantVersion = await _tenantRepository.GetNotDeletedTenantByTenantIdAsync(tenantId);
            if (!String.IsNullOrWhiteSpace(organizationName))
            {
                tenantVersion.tenant.OrganizationName = organizationName;
            }
            var domainWasUpdated = false;
            var oldDomain = "";
            if (!String.IsNullOrWhiteSpace(domain))
            {
                oldDomain = tenantVersion.tenant.Domain;
                tenantVersion.tenant.Domain = domain;
                domainWasUpdated = true;
            }

            await _tenantRepository.UpdateTenantAsync(commandingUserEmail, "TenantOrganizationUpdated", version, tenantVersion.tenant);
            return (tenantVersion.tenant, domainWasUpdated, oldDomain);
        }

        public void SetTenantResponseModelInRedisById(TenantResponseModel model)
        {
            redisDb.SetAdd(RedisKeys.GetTenantId(model.TenantId), JsonConvert.SerializeObject(model), CommandFlags.FireAndForget);
        }

        public void SetTenantResponseModelInRedisByDomain(TenantResponseModel model)
        {
            redisDb.SetAdd(RedisKeys.GetTenantDomain(model.Domain), JsonConvert.SerializeObject(model), CommandFlags.FireAndForget);
        }

        public async Task RemoveTenantResponseModelsFromRedis(string tenantId, string domain)
        {
            var tasks = new Task[2]
            {
                redisDb.KeyDeleteAsync(RedisKeys.GetTenantId(tenantId)),
                redisDb.KeyDeleteAsync(RedisKeys.GetTenantDomain(domain))
            };
            await Task.WhenAll(tasks);
        }
    }
}
