using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests.Factories.Test;
using IV.DX.Shared.IntTests.Models.Test;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IV.DX.Application.IntTests.Services
{
    /// <summary>
    /// Covers <see cref="IDXOwnershipReader"/> and the <c>GetOwnedAsync</c> it feeds.
    /// </summary>
    /// <remarks>
    /// The distinction under test throughout is between "what may this caller see", which the
    /// access gate answers and which includes records exposed to everybody, and "what is this
    /// caller's own", which is the only question here. The two coincide often enough that a test
    /// asserting only the happy path would pass against either.
    /// </remarks>
    [Collection("DX:one-time")]
    public class DXOwnershipReaderTests : IClassFixture<DXRBACTestFixture>, IDisposable
    {
        private readonly IServiceScope _scope;
        private readonly IDXOwnershipReader _ownership;
        private readonly IDXUnitDataService _dataService;
        private readonly IDXUnitDataReader _reader;
        private readonly IDXUnitQueryService<TUserOwnedDto> _queryService;
        private readonly IDXUnitGenericRepository _genericRepo;
        private readonly IDXStructureCache _structureCache;
        private readonly IDXExecutionContextAccessor _contextAccessor;
        private readonly IDXExecutionContextResolver _contextResolver;
        private readonly DXRBACTestFixture _rbacFx;

        public DXOwnershipReaderTests(DXTestFixture fx, DXRBACTestFixture rbacFx)
        {
            _scope = fx.Root.CreateScope();
            _ownership = _scope.ServiceProvider.GetRequiredService<IDXOwnershipReader>();
            _dataService = _scope.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            _reader = _scope.ServiceProvider.GetRequiredService<IDXUnitDataReader>();
            _queryService = _scope.ServiceProvider.GetRequiredService<IDXUnitQueryService<TUserOwnedDto>>();
            _genericRepo = _scope.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
            _structureCache = _scope.ServiceProvider.GetRequiredService<IDXStructureCache>();
            _contextAccessor = _scope.ServiceProvider.GetRequiredService<IDXExecutionContextAccessor>();
            _contextResolver = _scope.ServiceProvider.GetRequiredService<IDXExecutionContextResolver>();
            _rbacFx = rbacFx;
        }

        // ===== A. What counts as owned =====

        [Fact]
        public async Task GetOwnedIds_ReturnsOnlyRecordsOwnedByCurrentIdentity()
        {
            await SetSupportsOwnershipAsync("TUserUnit", true);

            var mineId = await InsertAsSystemAsync("Owned", "Mine");
            var theirsId = await InsertAsSystemAsync("Owned", "Theirs");
            var ownershipId = await CreateIdentityOwnershipAsync(mineId, read: true, update: true, delete: true);

            try
            {
                var owned = await RunAsIdentityAsync(() => _ownership.GetOwnedIdsAsync<TUserUnit>());

                Assert.Contains(mineId, owned);
                Assert.DoesNotContain(theirsId, owned);
            }
            finally
            {
                await SetSupportsOwnershipAsync("TUserUnit", false);
                await CleanupAsync(
                    ("DXIdentityOwnershipUnit", ownershipId),
                    ("TUserUnit", mineId),
                    ("TUserUnit", theirsId));
            }
        }

        [Fact]
        public async Task GetOwnedIds_ExcludesPubliclyExposedRecords()
        {
            await SetSupportsOwnershipAsync("TUserUnit", true);

            var unitDefId = GetUnitDefinitionId("TUserUnit");
            var publicId = await InsertAsSystemAsync("Owned", "PublicToEveryone");
            var publicAccessId = Guid.Empty;

            await RunAsSystemAsync(async () =>
                publicAccessId = await _dataService.InsertAsync(new DXPublicAccessUnit
                {
                    TimeStamp = DateTime.UtcNow,
                    DXUnitDefinition = unitDefId,
                    PublicDXUnitId = publicId
                }));

            try
            {
                var owned = await RunAsIdentityAsync(() => _ownership.GetOwnedIdsAsync<TUserUnit>());

                // The access gate would hand this record back - it is readable by anyone. Ownership
                // is a different question, and a record exposed to everybody belongs to nobody.
                Assert.DoesNotContain(publicId, owned);
            }
            finally
            {
                await SetSupportsOwnershipAsync("TUserUnit", false);
                await CleanupAsync(
                    ("DXPublicAccessUnit", publicAccessId),
                    ("TUserUnit", publicId));
            }
        }

        [Fact]
        public async Task GetOwnedIds_IncludesRecordCreatedByTheIdentity()
        {
            await SetSupportsOwnershipAsync("TUserUnit", true);
            await SetAllowAuthenticatedCreateAsync("TUserUnit", true);

            var createdId = Guid.Empty;

            try
            {
                // The creator's ownership row is written by the data service, not by this test -
                // this is the path a book takes when an author posts one.
                createdId = await RunAsIdentityAsync(() => _dataService.InsertAsync(
                    TUserUnitFactory.GetItem("Owned", "ByCreation", DateTime.UtcNow.Date)));

                var owned = await RunAsIdentityAsync(() => _ownership.GetOwnedIdsAsync<TUserUnit>());
                var editable = await RunAsIdentityAsync(() =>
                    _ownership.GetOwnedIdsAsync<TUserUnit>(DXUnitTypeAccessOperation.Update));

                Assert.Contains(createdId, owned);
                Assert.Contains(createdId, editable);
            }
            finally
            {
                await SetAllowAuthenticatedCreateAsync("TUserUnit", false);
                await SetSupportsOwnershipAsync("TUserUnit", false);
                await CleanupOwnershipRowsAsync(createdId);
                await CleanupAsync(("TUserUnit", createdId));
            }
        }

        [Fact]
        public async Task GetOwnedIds_IncludesRecordsOwnedThroughAGroup()
        {
            await SetSupportsOwnershipAsync("TUserUnit", true);

            var membershipId = await CreateMembershipAsync(_rbacFx.IdentityId, _rbacFx.TenantId);
            var groupId = await CreateGroupAsync(_rbacFx.TenantId);
            var groupMembershipId = await CreateGroupMembershipAsync(groupId, membershipId);

            var groupOwnedId = await InsertAsSystemAsync("Owned", "ByGroup");
            var ownershipId = await CreateGroupOwnershipAsync(groupId, groupOwnedId, read: true, update: false, delete: false);

            try
            {
                var owned = await RunAsIdentityAsync(() => _ownership.GetOwnedIdsAsync<TUserUnit>());
                var editable = await RunAsIdentityAsync(() =>
                    _ownership.GetOwnedIdsAsync<TUserUnit>(DXUnitTypeAccessOperation.Update));

                Assert.Contains(groupOwnedId, owned);
                Assert.DoesNotContain(groupOwnedId, editable);
            }
            finally
            {
                await SetSupportsOwnershipAsync("TUserUnit", false);
                await CleanupAsync(
                    ("DXGroupOwnershipUnit", ownershipId),
                    ("TUserUnit", groupOwnedId),
                    ("DXGroupMembershipUnit", groupMembershipId),
                    ("DXGroupUnit", groupId),
                    ("DXMembershipUnit", membershipId));
            }
        }

        // ===== B. Which rows count for which operation =====

        [Fact]
        public async Task GetOwnedIds_HonoursTheOperationFlagsOnTheRow()
        {
            await SetSupportsOwnershipAsync("TUserUnit", true);

            var readOnlyId = await InsertAsSystemAsync("Owned", "ReadOnlyRow");
            var ownershipId = await CreateIdentityOwnershipAsync(readOnlyId, read: true, update: false, delete: false);

            try
            {
                var readable = await RunAsIdentityAsync(() =>
                    _ownership.GetOwnedIdsAsync<TUserUnit>(DXUnitTypeAccessOperation.Read));
                var editable = await RunAsIdentityAsync(() =>
                    _ownership.GetOwnedIdsAsync<TUserUnit>(DXUnitTypeAccessOperation.Update));
                var deletable = await RunAsIdentityAsync(() =>
                    _ownership.GetOwnedIdsAsync<TUserUnit>(DXUnitTypeAccessOperation.Delete));

                Assert.Contains(readOnlyId, readable);
                Assert.DoesNotContain(readOnlyId, editable);
                Assert.DoesNotContain(readOnlyId, deletable);
            }
            finally
            {
                await SetSupportsOwnershipAsync("TUserUnit", false);
                await CleanupAsync(
                    ("DXIdentityOwnershipUnit", ownershipId),
                    ("TUserUnit", readOnlyId));
            }
        }

        [Fact]
        public async Task GetOwnedIds_NeverReportsOwnershipForCreate()
        {
            await SetSupportsOwnershipAsync("TUserUnit", true);

            var recordId = await InsertAsSystemAsync("Owned", "FullRights");
            var ownershipId = await CreateIdentityOwnershipAsync(recordId, read: true, update: true, delete: true);

            try
            {
                // Ownership is a grant over a record that exists; it cannot authorise bringing new
                // ones into being, so no row ever covers Create however wide its flags are.
                var forCreate = await RunAsIdentityAsync(() =>
                    _ownership.GetOwnedIdsAsync<TUserUnit>(DXUnitTypeAccessOperation.Create));

                Assert.Empty(forCreate);
            }
            finally
            {
                await SetSupportsOwnershipAsync("TUserUnit", false);
                await CleanupAsync(
                    ("DXIdentityOwnershipUnit", ownershipId),
                    ("TUserUnit", recordId));
            }
        }

        [Fact]
        /// <remarks>
        /// One identity holds at most one row per record - <c>DXIdentityOwnershipUnit</c> is unique
        /// over <c>Identity, DXUnitDefinition, OwnedDXUnitId</c> - so an Allow and a Deny from the
        /// same identity cannot coexist to be resolved against each other. A lone Deny row is what
        /// the identity route can actually produce; Deny beating Allow is reachable only across two
        /// routes, which the group test below covers.
        /// </remarks>
        public async Task GetOwnedIds_ExcludesRecordWhoseOwnershipRowDenies()
        {
            await SetSupportsOwnershipAsync("TUserUnit", true);

            var recordId = await InsertAsSystemAsync("Owned", "Denied");
            var denyId = await CreateIdentityOwnershipAsync(
                recordId, read: true, update: true, delete: true, effect: DXGrantEffectEnum.Deny);

            try
            {
                // The flags are wide open; the effect is what settles it.
                var owned = await RunAsIdentityAsync(() => _ownership.GetOwnedIdsAsync<TUserUnit>());

                Assert.DoesNotContain(recordId, owned);
            }
            finally
            {
                await SetSupportsOwnershipAsync("TUserUnit", false);
                await CleanupAsync(
                    ("DXIdentityOwnershipUnit", denyId),
                    ("TUserUnit", recordId));
            }
        }

        [Fact]
        public async Task GetOwnedIds_DenyThroughAGroupOutranksAnIdentityAllow()
        {
            await SetSupportsOwnershipAsync("TUserUnit", true);

            var membershipId = await CreateMembershipAsync(_rbacFx.IdentityId, _rbacFx.TenantId);
            var groupId = await CreateGroupAsync(_rbacFx.TenantId);
            var groupMembershipId = await CreateGroupMembershipAsync(groupId, membershipId);

            var recordId = await InsertAsSystemAsync("Owned", "GroupDenied");
            var allowId = await CreateIdentityOwnershipAsync(recordId, read: true, update: true, delete: true);
            var denyId = await CreateGroupOwnershipAsync(
                groupId, recordId, read: true, update: true, delete: true, effect: DXGrantEffectEnum.Deny);

            try
            {
                // The denial is subtracted after both routes have been collected, so it wins
                // regardless of which row was read first.
                var owned = await RunAsIdentityAsync(() => _ownership.GetOwnedIdsAsync<TUserUnit>());

                Assert.DoesNotContain(recordId, owned);
            }
            finally
            {
                await SetSupportsOwnershipAsync("TUserUnit", false);
                await CleanupAsync(
                    ("DXIdentityOwnershipUnit", allowId),
                    ("DXGroupOwnershipUnit", denyId),
                    ("TUserUnit", recordId),
                    ("DXGroupMembershipUnit", groupMembershipId),
                    ("DXGroupUnit", groupId),
                    ("DXMembershipUnit", membershipId));
            }
        }

        // ===== C. Callers that own nothing =====

        [Fact]
        public async Task GetOwnedIds_ReturnsEmpty_WhenTypeDoesNotSupportOwnership()
        {
            await SetSupportsOwnershipAsync("TUserUnit", false);

            var recordId = await InsertAsSystemAsync("Owned", "NoOwnershipSupport");
            var ownershipId = await CreateIdentityOwnershipAsync(recordId, read: true, update: true, delete: true);

            try
            {
                // The rows exist, but the type does not declare ownership - so they mean nothing,
                // exactly as the gate treats them.
                var owned = await RunAsIdentityAsync(() => _ownership.GetOwnedIdsAsync<TUserUnit>());

                Assert.Empty(owned);
            }
            finally
            {
                await CleanupAsync(
                    ("DXIdentityOwnershipUnit", ownershipId),
                    ("TUserUnit", recordId));
            }
        }

        [Fact]
        public async Task GetOwnedIds_ReturnsEmpty_ForAnonymousCaller()
        {
            await SetSupportsOwnershipAsync("TUserUnit", true);

            var recordId = await InsertAsSystemAsync("Owned", "AnonymousSees");
            var ownershipId = await CreateIdentityOwnershipAsync(recordId, read: true, update: true, delete: true);

            try
            {
                // No scope opened at all: there is no identity to own anything, and asking is not
                // an error - it is an empty answer.
                var owned = await _ownership.GetOwnedIdsAsync<TUserUnit>();

                Assert.Empty(owned);
            }
            finally
            {
                await SetSupportsOwnershipAsync("TUserUnit", false);
                await CleanupAsync(
                    ("DXIdentityOwnershipUnit", ownershipId),
                    ("TUserUnit", recordId));
            }
        }

        [Fact]
        public async Task GetOwnedIds_ReturnsEmpty_ForSystemContext()
        {
            await SetSupportsOwnershipAsync("TUserUnit", true);

            var recordId = await InsertAsSystemAsync("Owned", "SystemSees");
            var ownershipId = await CreateIdentityOwnershipAsync(recordId, read: true, update: true, delete: true);

            try
            {
                var owned = new HashSet<Guid>();
                await RunAsSystemAsync(async () => owned = await _ownership.GetOwnedIdsAsync<TUserUnit>());

                // A system principal carries no identity, so it owns nothing. It is not denied -
                // system code reads what it needs directly rather than asking whose records these are.
                Assert.Empty(owned);
            }
            finally
            {
                await SetSupportsOwnershipAsync("TUserUnit", false);
                await CleanupAsync(
                    ("DXIdentityOwnershipUnit", ownershipId),
                    ("TUserUnit", recordId));
            }
        }

        // ===== D. Through the query service =====

        [Fact]
        public async Task GetOwnedAsync_ReturnsOnlyOwnedRecords_WhenTypeIsPublicRead()
        {
            await SetSupportsOwnershipAsync("TUserUnit", true);
            var originalIsPublicRead = await SetIsPublicReadAsync("TUserUnit", true);

            var mineId = await InsertAsSystemAsync("Query", "Mine");
            var theirsId = await InsertAsSystemAsync("Query", "Theirs");
            var ownershipId = await CreateIdentityOwnershipAsync(mineId, read: true, update: true, delete: true);

            try
            {
                var all = await RunAsIdentityAsync(() => _queryService.GetAllAsync());
                var owned = await RunAsIdentityAsync(() => _queryService.GetOwnedAsync());

                var allIds = all.Select(x => x.Id).ToHashSet();
                var ownedIds = owned.Select(x => x.Id).ToHashSet();

                // IsPublicRead makes the type-level decision Allowed, so the gate never narrows and
                // GetAllAsync hands back both records. This is precisely the case ownership has to
                // be asked for explicitly, and the reason GetOwnedAsync exists.
                Assert.Contains(mineId, allIds);
                Assert.Contains(theirsId, allIds);

                Assert.Contains(mineId, ownedIds);
                Assert.DoesNotContain(theirsId, ownedIds);

                // The read mapper still ran - GetOwnedAsync goes through the same pipeline and
                // mapper as every other method on the service.
                Assert.Equal("Query", owned.Single(x => x.Id == mineId).Name);
            }
            finally
            {
                await SetIsPublicReadAsync("TUserUnit", originalIsPublicRead);
                await SetSupportsOwnershipAsync("TUserUnit", false);
                await CleanupAsync(
                    ("DXIdentityOwnershipUnit", ownershipId),
                    ("TUserUnit", mineId),
                    ("TUserUnit", theirsId));
            }
        }

        [Fact]
        public async Task GetOwnedAsync_ReturnsEmpty_ForAnonymousCaller()
        {
            await SetSupportsOwnershipAsync("TUserUnit", true);
            var originalIsPublicRead = await SetIsPublicReadAsync("TUserUnit", true);

            var recordId = await InsertAsSystemAsync("Query", "AnonymousSees");
            var ownershipId = await CreateIdentityOwnershipAsync(recordId, read: true, update: true, delete: true);

            try
            {
                // No scope opened: the record is public, so the anonymous caller reads it happily -
                // and still owns nothing. The empty short-circuit is the branch under test.
                var all = await _queryService.GetAllAsync();
                var owned = await _queryService.GetOwnedAsync();

                Assert.Contains(recordId, all.Select(x => x.Id));
                Assert.Empty(owned);
            }
            finally
            {
                await SetIsPublicReadAsync("TUserUnit", originalIsPublicRead);
                await SetSupportsOwnershipAsync("TUserUnit", false);
                await CleanupAsync(
                    ("DXIdentityOwnershipUnit", ownershipId),
                    ("TUserUnit", recordId));
            }
        }

        // ===== Private Helpers =====

        private async Task<T> RunAsIdentityAsync<T>(Func<Task<T>> action)
        {
            var ctx = await _contextResolver.ResolveAsync(_rbacFx.LoginId, _rbacFx.SessionId, "ownership-tests");
            using var _ = _contextAccessor.BeginScope(ctx);
            return await action();
        }

        private async Task RunAsSystemAsync(Func<Task> action)
        {
            using var _ = _contextAccessor.BeginScope(new DXExecutionContext
            {
                SubjectId = "system:ownership-tests",
                IsSystem = true
            });
            await action();
        }

        private async Task<Guid> InsertAsSystemAsync(string name, string surname)
        {
            var id = Guid.Empty;
            await RunAsSystemAsync(async () =>
                id = await _dataService.InsertAsync(
                    TUserUnitFactory.GetItem(name, surname, DateTime.UtcNow.Date)));
            return id;
        }

        private Guid GetUnitDefinitionId(string typeName)
        {
            var unit = _structureCache.GetDXUnit(typeName);
            Assert.NotNull(unit);
            return unit.Id;
        }

        private async Task SetSupportsOwnershipAsync(string typeName, bool value)
        {
            await RunAsSystemAsync(() =>
            {
                var unitDef = _genericRepo.GetDXUnits<DXUnitDefinitionUnit>($"Name = '{typeName}'").First();
                unitDef.SupportsOwnership = value;
                _genericRepo.Update(unitDef);
                return Task.CompletedTask;
            });
            await _structureCache.RefreshAsync();
        }

        private async Task SetAllowAuthenticatedCreateAsync(string typeName, bool value)
        {
            await RunAsSystemAsync(() =>
            {
                var unitDef = _genericRepo.GetDXUnits<DXUnitDefinitionUnit>($"Name = '{typeName}'").First();
                unitDef.AllowAuthenticatedCreate = value;
                _genericRepo.Update(unitDef);
                return Task.CompletedTask;
            });
            await _structureCache.RefreshAsync();
        }

        /// <summary>Sets the flag and returns what it was, so the caller can put it back.</summary>
        private async Task<bool> SetIsPublicReadAsync(string typeName, bool value)
        {
            var original = false;
            await RunAsSystemAsync(() =>
            {
                var unitDef = _genericRepo.GetDXUnits<DXUnitDefinitionUnit>($"Name = '{typeName}'").First();
                original = unitDef.IsPublicRead;
                unitDef.IsPublicRead = value;
                _genericRepo.Update(unitDef);
                return Task.CompletedTask;
            });
            await _structureCache.RefreshAsync();
            return original;
        }

        private async Task<Guid> CreateIdentityOwnershipAsync(
            Guid ownedUnitId,
            bool read,
            bool update,
            bool delete,
            DXGrantEffectEnum effect = DXGrantEffectEnum.Allow)
        {
            var id = Guid.CreateVersion7();
            var unitDefId = GetUnitDefinitionId("TUserUnit");

            await RunAsSystemAsync(() =>
            {
                _genericRepo.Insert(new DXIdentityOwnershipUnit
                {
                    Id = id,
                    TimeStamp = DateTime.UtcNow,
                    Identity = _rbacFx.IdentityId,
                    DXUnitDefinition = unitDefId,
                    OwnedDXUnitId = ownedUnitId,
                    Read = read,
                    Update = update,
                    Delete = delete,
                    Effect = effect
                });
                return Task.CompletedTask;
            });
            return id;
        }

        private async Task<Guid> CreateGroupOwnershipAsync(
            Guid groupId,
            Guid ownedUnitId,
            bool read,
            bool update,
            bool delete,
            DXGrantEffectEnum effect = DXGrantEffectEnum.Allow)
        {
            var id = Guid.CreateVersion7();
            var unitDefId = GetUnitDefinitionId("TUserUnit");

            await RunAsSystemAsync(() =>
            {
                _genericRepo.Insert(new DXGroupOwnershipUnit
                {
                    Id = id,
                    TimeStamp = DateTime.UtcNow,
                    Group = groupId,
                    DXUnitDefinition = unitDefId,
                    OwnedDXUnitId = ownedUnitId,
                    Read = read,
                    Update = update,
                    Delete = delete,
                    Effect = effect
                });
                return Task.CompletedTask;
            });
            return id;
        }

        private async Task<Guid> CreateMembershipAsync(Guid identityId, Guid tenantId)
        {
            var insertedId = Guid.Empty;
            await RunAsSystemAsync(async () =>
                insertedId = await _dataService.InsertAsync(new DXMembershipUnit
                {
                    TimeStamp = DateTime.UtcNow,
                    Name = $"ownership-membership-{Guid.NewGuid():N}",
                    Identity = identityId,
                    Tenant = tenantId
                }));
            return insertedId;
        }

        private async Task<Guid> CreateGroupAsync(Guid tenantId)
        {
            var insertedId = Guid.Empty;
            await RunAsSystemAsync(async () =>
                insertedId = await _dataService.InsertAsync(new DXGroupUnit
                {
                    TimeStamp = DateTime.UtcNow,
                    Name = $"ownership-group-{Guid.NewGuid():N}",
                    Tenant = tenantId
                }));
            return insertedId;
        }

        private async Task<Guid> CreateGroupMembershipAsync(Guid groupId, Guid membershipId)
        {
            var id = Guid.CreateVersion7();
            await RunAsSystemAsync(() =>
            {
                _genericRepo.Insert(new DXGroupMembershipUnit
                {
                    Id = id,
                    TimeStamp = DateTime.UtcNow,
                    Group = groupId,
                    Membership = membershipId
                });
                return Task.CompletedTask;
            });
            return id;
        }

        /// <summary>Removes ownership rows the data service wrote by itself on create.</summary>
        private async Task CleanupOwnershipRowsAsync(Guid ownedUnitId)
        {
            if (ownedUnitId == Guid.Empty)
                return;

            await RunAsSystemAsync(() =>
            {
                var rows = _genericRepo.GetDXUnits<DXIdentityOwnershipUnit>(
                    $"OwnedDXUnitId = '{ownedUnitId}'");

                foreach (var row in rows)
                {
                    try { _genericRepo.Delete(row); } catch { }
                }
                return Task.CompletedTask;
            });
        }

        private async Task CleanupAsync(params (string typeName, Guid id)[] items)
        {
            await RunAsSystemAsync(async () =>
            {
                foreach (var (typeName, id) in items)
                {
                    if (id == Guid.Empty) continue;
                    try { await DeleteByIdsAsync(typeName, id); } catch { }
                }
            });
        }

        private Task<bool> DeleteByIdsAsync(string typeName, params Guid[] ids)
        {
            var deleteRefs = ids
                .Where(x => x != Guid.Empty)
                .Select(x => new DXDeleteRef { Id = x })
                .ToList();

            if (deleteRefs.Count == 0)
                return Task.FromResult(true);

            return _dataService.DeleteAsync(new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta { Kind = "DXUnit", Type = typeName, Op = "Patch", IsMulti = true },
                Data = new DXData<DXUnitRecord> { Delete = deleteRefs }
            });
        }

        public void Dispose() => _scope.Dispose();
    }
}
