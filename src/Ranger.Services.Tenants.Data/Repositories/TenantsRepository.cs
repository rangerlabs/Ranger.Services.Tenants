using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Ranger.Common;
using Ranger.Common.Data.Exceptions;

namespace Ranger.Services.Tenants.Data
{
    public class TenantsRepository : ITenantsRepository
    {
        private readonly IDataProtector dataProtector;
        private readonly ILogger<TenantsRepository> logger;
        private readonly TenantsDbContext context;
        public TenantsRepository(TenantsDbContext context, ILogger<TenantsRepository> logger, IDataProtectionProvider dataProtectionProvider)
        {
            this.context = context;
            this.logger = logger;
            this.dataProtector = dataProtectionProvider.CreateProtector(nameof(TenantsRepository));
        }

        public async Task AddTenant(string userEmail, Tenant tenant)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                throw new ArgumentException($"{nameof(userEmail)} was null or whitespace.");
            }
            if (tenant is null)
            {
                throw new ArgumentNullException($"{nameof(tenant)} was null.");
            }

            tenant.Domain = tenant.Domain.ToLowerInvariant();
            tenant.DatabasePassword = this.dataProtector.Protect(tenant.DatabasePassword);
            var tenantStream = new TenantStream()
            {
                StreamId = Guid.NewGuid(),
                Version = 0,
                Data = JsonConvert.SerializeObject(tenant),
                Event = "AddTenant",
                InsertedAt = DateTime.UtcNow,
                InsertedBy = userEmail
            };
            this.AddTenantUniqueConstraints(tenantStream, tenant);
            this.context.TenantStreams.Add(tenantStream);
            try
            {
                await this.context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var postgresException = ex.InnerException as PostgresException;
                if (postgresException.SqlState == "23505")
                {
                    var uniqueIndexViolation = postgresException.ConstraintName;
                    switch (uniqueIndexViolation)
                    {
                        case TenantJsonbConstraintNames.Domain:
                            {
                                throw new EventStreamDataConstraintException("The domain name is in use by another tenant.");
                            }
                        default:
                            {
                                throw new EventStreamDataConstraintException("");
                            }
                    }
                }
                throw;
            }
        }

        public async Task SoftDelete(string userEmail, string domain)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                throw new ArgumentException($"{nameof(userEmail)} was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} was null or whitespace.");
            }
            domain = domain.ToLowerInvariant();

            var currentTenantStream = await this.GetNotDeletedTenantStreamByDomainAsync(domain);
            if (!(currentTenantStream is null))
            {
                var currentTenant = JsonConvert.DeserializeObject<Tenant>(currentTenantStream.Data);
                currentTenant.Deleted = true;
                currentTenant.Confirmed = false;
                var deleted = false;
                var maxConcurrencyAttempts = 3;
                while (!deleted && maxConcurrencyAttempts != 0)
                {

                    var updatedTenantStream = new TenantStream
                    {
                        StreamId = currentTenantStream.StreamId,
                        Version = currentTenantStream.Version + 1,
                        Data = JsonConvert.SerializeObject(currentTenant),
                        Event = "TenantDeleted",
                        InsertedAt = DateTime.UtcNow,
                        InsertedBy = userEmail
                    };
                    this.context.TenantUniqueConstraints.Remove(await this.context.TenantUniqueConstraints.Where(_ => _.TenantId == currentTenant.TenantId).SingleAsync());
                    this.context.TenantStreams.Add(updatedTenantStream);
                    try
                    {
                        await this.context.SaveChangesAsync();
                        deleted = true;
                        logger.LogInformation($"Tenant with domain {currentTenant.Domain} deleted.");
                    }
                    catch (DbUpdateException ex)
                    {
                        var postgresException = ex.InnerException as PostgresException;
                        if (postgresException.SqlState == "23505")
                        {
                            var uniqueIndexViolation = postgresException.ConstraintName;
                            switch (uniqueIndexViolation)
                            {
                                case TenantJsonbConstraintNames.TenantId_Version:
                                    {
                                        logger.LogError($"The update version number was outdated. The current and updated stream versions are '{currentTenantStream.Version + 1}'.");
                                        maxConcurrencyAttempts--;
                                        continue;
                                    }
                            }
                        }
                        throw;
                    }
                }
                if (!deleted)
                {
                    throw new ConcurrencyException($"After '{maxConcurrencyAttempts}' attempts, the version was still outdated. Too many updates have been applied in a short period of time. The current stream version is '{currentTenantStream.Version + 1}'. The tenant was not deleted.");
                }
            }
            else
            {
                throw new ArgumentException($"No tenant was found with domain '{domain}'.");
            }
        }


        public async Task<bool> ExistsAsync(string domain)
        {
            return await context.TenantUniqueConstraints.AnyAsync((t => t.Domain == domain.ToLowerInvariant()));
        }

        public async Task<Tenant> FindNotDeletedTenantByDomainAsync(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException("message", nameof(domain));
            }

            var tenantStream = await this.GetNotDeletedTenantStreamByDomainAsync(domain);
            if (!(tenantStream is null))
            {
                var tenant = JsonConvert.DeserializeObject<Tenant>(tenantStream.Data);
                tenant.DatabasePassword = dataProtector.Unprotect(tenant.DatabasePassword);
                return tenant;
            }
            return null;
        }

        public async Task<(bool exists, bool confirmed)> IsTenantConfirmedAsync(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException("message", nameof(domain));
            }

            var tenantStream = await this.GetNotDeletedTenantStreamByDomainAsync(domain);
            if (!(tenantStream is null))
            {
                var tenant = JsonConvert.DeserializeObject<Tenant>(tenantStream.Data);
                if (!tenant.Deleted)
                {
                    return (true, tenant.Confirmed);
                }
            }
            return (false, false);
        }

        public async Task<Tenant> FindTenantByTenantIdAsync(string TenantId)
        {
            if (string.IsNullOrWhiteSpace(TenantId))
            {
                throw new ArgumentException("message", nameof(TenantId));
            }

            var tenantStream = await this.context.TenantStreams.FromSqlInterpolated($"SELECT * FROM tenant_streams WHERE data ->> 'TenantId' = {TenantId} AND data ->> 'Deleted' = 'false' ORDER BY version DESC").FirstOrDefaultAsync();
            var tenant = JsonConvert.DeserializeObject<Tenant>(tenantStream.Data);
            tenant.DatabasePassword = dataProtector.Unprotect(tenant.DatabasePassword);
            return tenant;
        }

        public Task UpdateLastAccessed(string domain)
        {
            throw new System.NotImplementedException();
        }

        public async Task CompletePrimaryOwnerTransferAsync(string userEmail, string domain, PrimaryOwnerTransferStateEnum state)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                throw new ArgumentException($"{nameof(userEmail)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} was null or whitespace.");
            }
            if (state is PrimaryOwnerTransferStateEnum.Pending)
            {
                throw new ArgumentException("A primary owner transfer cannot be completed with a 'Pending' status.");
            }

            var tenantStream = await this.GetNotDeletedTenantStreamByDomainAsync(domain);
            var tenant = JsonConvert.DeserializeObject<Tenant>(tenantStream.Data);

            tenant.PrimaryOwnerTransfer.State = state;

            var serializedTenantData = JsonConvert.SerializeObject(tenant);
            var updatedTenantStream = new TenantStream()
            {
                StreamId = tenantStream.StreamId,
                Version = tenantStream.Version + 1,
                Data = serializedTenantData,
                Event = $"PrimaryOwnerTransferClosed",
                InsertedAt = DateTime.UtcNow,
                InsertedBy = userEmail
            };

            this.context.TenantStreams.Add(updatedTenantStream);
            try
            {
                await this.context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var postgresException = ex.InnerException as PostgresException;
                if (postgresException.SqlState == "23505")
                {
                    var uniqueIndexViolation = postgresException.ConstraintName;
                    switch (uniqueIndexViolation)
                    {
                        case TenantJsonbConstraintNames.Domain:
                            {
                                throw new EventStreamDataConstraintException("The domain name is in use by another tenant.");
                            }
                        case TenantJsonbConstraintNames.TenantId_Version:
                            {
                                throw new ConcurrencyException($"The update version number was outdated. The request update version was '{tenantStream.Version}'.");
                            }
                        default:
                            {
                                throw new EventStreamDataConstraintException("");
                            }
                    }
                }
                throw;
            }

        }

        public async Task AddPrimaryOwnerTransferAsync(string userEmail, string domain, PrimaryOwnerTransfer transfer)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                throw new ArgumentException($"{nameof(userEmail)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} was null or whitespace.");
            }

            var tenantStream = await this.GetNotDeletedTenantStreamByDomainAsync(domain);
            var tenant = JsonConvert.DeserializeObject<Tenant>(tenantStream.Data);

            if (!(tenant.PrimaryOwnerTransfer is null) && tenant.PrimaryOwnerTransfer.State is PrimaryOwnerTransferStateEnum.Pending)
            {
                throw new ConcurrencyException("A primary owner transfer is currently pending.");
            }
            tenant.PrimaryOwnerTransfer = transfer;

            var serializedTenantData = JsonConvert.SerializeObject(tenant);
            var updatedTenantStream = new TenantStream()
            {
                StreamId = tenantStream.StreamId,
                Version = tenantStream.Version + 1,
                Data = serializedTenantData,
                Event = "PrimaryOwnerTransferInitiated",
                InsertedAt = DateTime.UtcNow,
                InsertedBy = userEmail
            };

            this.context.TenantStreams.Add(updatedTenantStream);
            try
            {
                await this.context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var postgresException = ex.InnerException as PostgresException;
                if (postgresException.SqlState == "23505")
                {
                    var uniqueIndexViolation = postgresException.ConstraintName;
                    switch (uniqueIndexViolation)
                    {
                        case TenantJsonbConstraintNames.Domain:
                            {
                                throw new EventStreamDataConstraintException("The domain name is in use by another tenant.");
                            }
                        case TenantJsonbConstraintNames.TenantId_Version:
                            {
                                throw new ConcurrencyException($"The update version number was outdated. The request update version was '{tenantStream.Version}'.");
                            }
                        default:
                            {
                                throw new EventStreamDataConstraintException("");
                            }
                    }
                }
                throw;
            }
        }

        public async Task UpdateTenantAsync(string userEmail, string eventName, int version, Tenant tenant)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                throw new ArgumentException($"{nameof(userEmail)} was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentException($"{nameof(eventName)} was null or whitespace.");
            }
            if (tenant is null)
            {
                throw new ArgumentException($"{nameof(tenant)} was null.");
            }

            var currentTenantStream = await this.GetNotDeletedTenantStreamByDomainAsync(tenant.Domain);
            ValidateRequestVersionIncremented(version, currentTenantStream);

            var outdatedTenant = JsonConvert.DeserializeObject<Tenant>(currentTenantStream.Data);
            tenant.TenantId = outdatedTenant.TenantId; //Domain is used externally as the Id
            tenant.CreatedOn = outdatedTenant.CreatedOn;
            tenant.TenantId = outdatedTenant.TenantId;
            tenant.DatabasePassword = outdatedTenant.DatabasePassword;
            tenant.PrimaryOwnerTransfer = outdatedTenant.PrimaryOwnerTransfer;
            tenant.Deleted = false;

            var serializedNewTenantData = JsonConvert.SerializeObject(tenant);
            var uniqueConstraint = await this.GetTenantUniqueConstraintAsync(tenant.TenantId);
            uniqueConstraint.Domain = tenant.Domain;

            var updatedTenantStream = new TenantStream()
            {
                StreamId = currentTenantStream.StreamId,
                Version = version,
                Data = serializedNewTenantData,
                Event = eventName,
                InsertedAt = DateTime.UtcNow,
                InsertedBy = userEmail
            };

            this.context.Update(uniqueConstraint);
            this.context.TenantStreams.Add(updatedTenantStream);
            try
            {
                await this.context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var postgresException = ex.InnerException as PostgresException;
                if (postgresException.SqlState == "23505")
                {
                    var uniqueIndexViolation = postgresException.ConstraintName;
                    switch (uniqueIndexViolation)
                    {
                        case TenantJsonbConstraintNames.Domain:
                            {
                                throw new EventStreamDataConstraintException("The domain name is in use by another tenant.");
                            }
                        case TenantJsonbConstraintNames.TenantId_Version:
                            {
                                throw new ConcurrencyException($"The update version number was outdated. The current stream version is '{currentTenantStream.Version}' and the request update version was '{version}'.");
                            }
                        default:
                            {
                                throw new EventStreamDataConstraintException("");
                            }
                    }
                }
                throw;
            }
        }

        private static void ValidateDataJsonInequality(TenantStream currentProjectStream, string serializedNewProjectData)
        {
            var currentJObject = JsonConvert.DeserializeObject<JObject>(currentProjectStream.Data);
            var requestJObject = JsonConvert.DeserializeObject<JObject>(serializedNewProjectData);
            if (JToken.DeepEquals(currentJObject, requestJObject))
            {
                throw new NoOpException("No changes were made from the previous version.");
            }
        }

        private static void ValidateRequestVersionIncremented(int version, TenantStream currentProjectStream)
        {
            if (version - currentProjectStream.Version > 1)
            {
                throw new ConcurrencyException($"The update version number was too high. The current stream version is '{currentProjectStream.Version}' and the request update version was '{version}'.");
            }
            if (version - currentProjectStream.Version <= 0)
            {
                throw new ConcurrencyException($"The update version number was outdated. The current stream version is '{currentProjectStream.Version}' and the request update version was '{version}'.");
            }
        }

        private async Task<TenantStream> GetNotDeletedTenantStreamByDomainAsync(string domain)
        {
            var tenantId = (await this.context.TenantUniqueConstraints.Where(_ => _.Domain == domain.ToLowerInvariant()).SingleOrDefaultAsync())?.TenantId;
            if (tenantId is null)
            {
                return null;
            }
            return await this.context.TenantStreams.FromSqlInterpolated($"SELECT * FROM tenant_streams WHERE data ->> 'TenantId' = {tenantId.ToString()} ORDER BY version DESC").FirstOrDefaultAsync();
        }

        private void AddTenantUniqueConstraints(TenantStream tenantStream, Tenant tenant)
        {
            var domainUniqueConstraint = new TenantUniqueConstraint
            {
                TenantId = tenant.TenantId,
                Domain = tenant.Domain
            };
            this.context.TenantUniqueConstraints.Add(domainUniqueConstraint);
        }

        private async Task<TenantUniqueConstraint> GetTenantUniqueConstraintAsync(string tenantId)
        {

            return await this.context.TenantUniqueConstraints.SingleOrDefaultAsync(_ => _.TenantId == tenantId);
        }
    }
}