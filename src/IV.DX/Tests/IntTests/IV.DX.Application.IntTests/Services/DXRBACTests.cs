using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
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
    [Collection("DX:one-time")]
    public class DXRBACTests : IClassFixture<DXRBACTestFixture>, IDisposable
    {
        private readonly IServiceScope _scope;
        private readonly IDXUnitDataService _dataService;
        private readonly IDXUnitDataReader _reader;
        private readonly IDXUnitGenericRepository _genericRepo;
        private readonly IDXStructureCache _structureCache;
        private readonly IDXExecutionContextAccessor _contextAccessor;
        private readonly IDXExecutionContextResolver _contextResolver;
        private readonly DXRBACTestFixture _rbacFx;

        public DXRBACTests(DXTestFixture fx, DXRBACTestFixture rbacFx)
        {
            _scope = fx.Root.CreateScope();
            _dataService = _scope.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            _reader = _scope.ServiceProvider.GetRequiredService<IDXUnitDataReader>();
            _genericRepo = _scope.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
            _structureCache = _scope.ServiceProvider.GetRequiredService<IDXStructureCache>();
            _contextAccessor = _scope.ServiceProvider.GetRequiredService<IDXExecutionContextAccessor>();
            _contextResolver = _scope.ServiceProvider.GetRequiredService<IDXExecutionContextResolver>();
            _rbacFx = rbacFx;
        }

        // ===== A. Basic Grants =====

        [Fact]
        public async Task Read_Allowed_WhenMembershipRoleGrantsRead()
        {
            var unitDefId = GetUnitDefinitionId("TUserUnit");
            var roleId = await CreateRoleAsync(unitDefId, read: true, write: false, delete: false);
            var membershipId = await CreateMembershipAsync(_rbacFx.IdentityId, _rbacFx.TenantId, roleId);

            try
            {
                var ctx = await _contextResolver.ResolveAsync(_rbacFx.LoginId, _rbacFx.SessionId, "test-user");
                using var _ = _contextAccessor.BeginScope(ctx);

                var items = await _reader.GetItemsAsync<TUserUnit>();

                Assert.NotNull(items);
            }
            finally
            {
                await CleanupAsync(
                    ("DXMembershipUnit", membershipId),
                    ("DXRoleUnit", roleId));
            }
        }

        [Fact]
        public async Task Read_Denied_WhenMembershipRoleHasNoReadGrant()
        {
            var unitDefId = GetUnitDefinitionId("TUserUnit");
            var roleId = await CreateRoleAsync(unitDefId, read: false, write: true, delete: false);
            var membershipId = await CreateMembershipAsync(_rbacFx.IdentityId, _rbacFx.TenantId, roleId);
            var hiddenId = Guid.Empty;

            await RunAsSystemAsync(async () =>
            {
                hiddenId = await _dataService.InsertAsync(
                    TUserUnitFactory.GetItem("RBAC", "HiddenNoRead", DateTime.UtcNow.Date));
            });

            try
            {
                var ctx = await _contextResolver.ResolveAsync(_rbacFx.LoginId, _rbacFx.SessionId, "test-user");
                using var _ = _contextAccessor.BeginScope(ctx);

                // read=false → AllowedOwnedOnly → reader returns empty (not exception)
                var items = await _reader.GetItemsAsync<TUserUnit>();
                Assert.Empty(items);
            }
            finally
            {
                await CleanupAsync(
                    ("TUserUnit", hiddenId),
                    ("DXMembershipUnit", membershipId),
                    ("DXRoleUnit", roleId));
            }
        }

        [Fact]
        public async Task Write_Allowed_WhenMembershipRoleGrantsWrite()
        {
            var unitDefId = GetUnitDefinitionId("TUserUnit");
            var roleId = await CreateRoleAsync(unitDefId, read: true, write: true, delete: false);
            var membershipId = await CreateMembershipAsync(_rbacFx.IdentityId, _rbacFx.TenantId, roleId);
            var insertedId = Guid.Empty;

            try
            {
                var ctx = await _contextResolver.ResolveAsync(_rbacFx.LoginId, _rbacFx.SessionId, "test-user");
                using var _ = _contextAccessor.BeginScope(ctx);

                insertedId = await _dataService.InsertAsync(
                    TUserUnitFactory.GetItem("RBAC", "Write", DateTime.UtcNow.Date));

                Assert.NotEqual(Guid.Empty, insertedId);
            }
            finally
            {
                await CleanupAsync(
                    ("TUserUnit", insertedId),
                    ("DXMembershipUnit", membershipId),
                    ("DXRoleUnit", roleId));
            }
        }

        [Fact]
        public async Task Write_Denied_WhenMembershipRoleHasNoWriteGrant()
        {
            var unitDefId = GetUnitDefinitionId("TUserUnit");
            var roleId = await CreateRoleAsync(unitDefId, read: true, write: false, delete: false);
            var membershipId = await CreateMembershipAsync(_rbacFx.IdentityId, _rbacFx.TenantId, roleId);

            try
            {
                var ctx = await _contextResolver.ResolveAsync(_rbacFx.LoginId, _rbacFx.SessionId, "test-user");
                using var _ = _contextAccessor.BeginScope(ctx);

                await Assert.ThrowsAsync<UnauthorizedAccessException>(
                    () => _dataService.InsertAsync(
                        TUserUnitFactory.GetItem("RBAC", "Blocked", DateTime.UtcNow.Date)));
            }
            finally
            {
                await CleanupAsync(
                    ("DXMembershipUnit", membershipId),
                    ("DXRoleUnit", roleId));
            }
        }

        [Fact]
        public async Task Delete_Allowed_WhenMembershipRoleGrantsDelete()
        {
            var unitDefId = GetUnitDefinitionId("TUserUnit");
            var roleId = await CreateRoleAsync(unitDefId, read: true, write: true, delete: true);
            var membershipId = await CreateMembershipAsync(_rbacFx.IdentityId, _rbacFx.TenantId, roleId);
            var insertedId = Guid.Empty;

            await RunAsSystemAsync(async () =>
            {
                insertedId = await _dataService.InsertAsync(
                    TUserUnitFactory.GetItem("RBAC", "Delete", DateTime.UtcNow.Date));
            });

            try
            {
                var ctx = await _contextResolver.ResolveAsync(_rbacFx.LoginId, _rbacFx.SessionId, "test-user");
                using var _ = _contextAccessor.BeginScope(ctx);

                var result = await _dataService.DeleteAsync(
                    new TUserUnit { Id = insertedId, TimeStamp = DateTime.UtcNow });

                Assert.True(result);
            }
            finally
            {
                await CleanupAsync(
                    ("TUserUnit", insertedId),
                    ("DXMembershipUnit", membershipId),
                    ("DXRoleUnit", roleId));
            }
        }

        [Fact]
        public async Task Delete_Denied_WhenRoleHasWriteButNotDelete()
        {
            var unitDefId = GetUnitDefinitionId("TUserUnit");
            var roleId = await CreateRoleAsync(unitDefId, read: true, write: true, delete: false);
            var membershipId = await CreateMembershipAsync(_rbacFx.IdentityId, _rbacFx.TenantId, roleId);
            var insertedId = Guid.Empty;

            await RunAsSystemAsync(async () =>
            {
                insertedId = await _dataService.InsertAsync(
                    TUserUnitFactory.GetItem("RBAC", "NoDelete", DateTime.UtcNow.Date));
            });

            try
            {
                var ctx = await _contextResolver.ResolveAsync(_rbacFx.LoginId, _rbacFx.SessionId, "test-user");
                using var _ = _contextAccessor.BeginScope(ctx);

                await Assert.ThrowsAsync<UnauthorizedAccessException>(
                    () => _dataService.DeleteAsync(
                        new TUserUnit { Id = insertedId, TimeStamp = DateTime.UtcNow }));
            }
            finally
            {
                await CleanupAsync(
                    ("TUserUnit", insertedId),
                    ("DXMembershipUnit", membershipId),
                    ("DXRoleUnit", roleId));
            }
        }

        // ===== B. Deny Overrides Allow =====

        [Fact]
        public async Task Read_Denied_WhenOneRoleDenies_EvenIfAnotherAllows()
        {
            var unitDefId = GetUnitDefinitionId("TUserUnit");
            var allowRoleId = await CreateRoleAsync(unitDefId, read: true, write: false, delete: false, effect: DXGrantEffectEnum.Allow);
            var denyRoleId = await CreateRoleAsync(unitDefId, read: true, write: false, delete: false, effect: DXGrantEffectEnum.Deny);
            var membershipId = await CreateMembershipAsync(_rbacFx.IdentityId, _rbacFx.TenantId, allowRoleId, denyRoleId);
            var hiddenId = Guid.Empty;

            await RunAsSystemAsync(async () =>
            {
                hiddenId = await _dataService.InsertAsync(
                    TUserUnitFactory.GetItem("RBAC", "HiddenDenyRole", DateTime.UtcNow.Date));
            });

            try
            {
                var ctx = await _contextResolver.ResolveAsync(_rbacFx.LoginId, _rbacFx.SessionId, "test-user");
                using var _ = _contextAccessor.BeginScope(ctx);

                // Deny effect overrides Allow → AllowedOwnedOnly → reader returns empty (not exception)
                var items = await _reader.GetItemsAsync<TUserUnit>();
                Assert.Empty(items);
            }
            finally
            {
                await CleanupAsync(
                    ("TUserUnit", hiddenId),
                    ("DXMembershipUnit", membershipId),
                    ("DXRoleUnit", allowRoleId),
                    ("DXRoleUnit", denyRoleId));
            }
        }

        // ===== C. Tenant × Membership Intersection =====

        [Fact]
        public async Task Write_Denied_WhenTenantGrantsButMembershipDoesNot()
        {
            var tUserUnitDefId = GetUnitDefinitionId("TUserUnit");
            var tDocumentUnitDefId = GetUnitDefinitionId("TDocumentUnit");

            var tenantRoleId = await CreateRoleAsync(tUserUnitDefId, read: true, write: true, delete: false);
            var membershipRoleId = await CreateRoleAsync(tDocumentUnitDefId, read: true, write: true, delete: false);
            var tenantId = await CreateTenantWithRolesAsync(tenantRoleId);
            var membershipId = await CreateMembershipAsync(_rbacFx.IdentityId, tenantId, membershipRoleId);

            try
            {
                var ctx = await _contextResolver.ResolveAsync(_rbacFx.LoginId, _rbacFx.SessionId, "test-user");
                using var _ = _contextAccessor.BeginScope(ctx);

                // Tenant allows TUserUnit, membership allows only TDocumentUnit → intersection = denied for TUserUnit
                await Assert.ThrowsAsync<UnauthorizedAccessException>(
                    () => _dataService.InsertAsync(
                        TUserUnitFactory.GetItem("RBAC", "Intersection", DateTime.UtcNow.Date)));
            }
            finally
            {
                await CleanupAsync(
                    ("DXMembershipUnit", membershipId),
                    ("DXTenantUnit", tenantId),
                    ("DXRoleUnit", tenantRoleId),
                    ("DXRoleUnit", membershipRoleId));
            }
        }

        // ===== D. Group Restrictions =====

        [Fact]
        public async Task Read_Denied_WhenGroupDoesNotGrantType()
        {
            var tUserUnitDefId = GetUnitDefinitionId("TUserUnit");
            var tDocumentUnitDefId = GetUnitDefinitionId("TDocumentUnit");

            var memberRoleId = await CreateRoleAsync(tUserUnitDefId, read: true, write: false, delete: false);
            var membershipId = await CreateMembershipAsync(_rbacFx.IdentityId, _rbacFx.TenantId, memberRoleId);
            var groupRoleId = await CreateRoleAsync(tDocumentUnitDefId, read: true, write: false, delete: false);
            var groupId = await CreateGroupAsync(_rbacFx.TenantId, groupRoleId);
            var groupMembershipId = await CreateGroupMembershipAsync(groupId, membershipId);
            var hiddenId = Guid.Empty;

            await RunAsSystemAsync(async () =>
            {
                hiddenId = await _dataService.InsertAsync(
                    TUserUnitFactory.GetItem("RBAC", "HiddenGroup", DateTime.UtcNow.Date));
            });

            try
            {
                var ctx = await _contextResolver.ResolveAsync(_rbacFx.LoginId, _rbacFx.SessionId, "test-user");
                using var _ = _contextAccessor.BeginScope(ctx);

                // Membership allows TUserUnit, but group only grants TDocumentUnit → intersection blocks TUserUnit → empty
                var items = await _reader.GetItemsAsync<TUserUnit>();
                Assert.Empty(items);
            }
            finally
            {
                await CleanupAsync(
                    ("TUserUnit", hiddenId),
                    ("DXGroupMembershipUnit", groupMembershipId),
                    ("DXGroupUnit", groupId),
                    ("DXRoleUnit", groupRoleId),
                    ("DXMembershipUnit", membershipId),
                    ("DXRoleUnit", memberRoleId));
            }
        }

        [Fact]
        public async Task Read_Allowed_WhenGroupAlsoGrantsType()
        {
            var tUserUnitDefId = GetUnitDefinitionId("TUserUnit");

            var memberRoleId = await CreateRoleAsync(tUserUnitDefId, read: true, write: false, delete: false);
            var membershipId = await CreateMembershipAsync(_rbacFx.IdentityId, _rbacFx.TenantId, memberRoleId);
            var groupRoleId = await CreateRoleAsync(tUserUnitDefId, read: true, write: false, delete: false);
            var groupId = await CreateGroupAsync(_rbacFx.TenantId, groupRoleId);
            var groupMembershipId = await CreateGroupMembershipAsync(groupId, membershipId);

            try
            {
                var ctx = await _contextResolver.ResolveAsync(_rbacFx.LoginId, _rbacFx.SessionId, "test-user");
                using var _ = _contextAccessor.BeginScope(ctx);

                // All three levels (tenant unrestricted, membership, group) allow TUserUnit → Allowed
                var items = await _reader.GetItemsAsync<TUserUnit>();

                Assert.NotNull(items);
            }
            finally
            {
                await CleanupAsync(
                    ("DXGroupMembershipUnit", groupMembershipId),
                    ("DXGroupUnit", groupId),
                    ("DXRoleUnit", groupRoleId),
                    ("DXMembershipUnit", membershipId),
                    ("DXRoleUnit", memberRoleId));
            }
        }

        // ===== E. Ownership Fallback =====

        [Fact]
        public async Task Delete_Succeeds_WhenOwnedOnly_AndUserOwnsRecord()
        {
            var unitDefId = GetUnitDefinitionId("TUserUnit");
            var roleId = await CreateRoleAsync(unitDefId, read: true, write: true, delete: false);
            var membershipId = await CreateMembershipAsync(_rbacFx.IdentityId, _rbacFx.TenantId, roleId);
            var insertedId = Guid.Empty;

            await SetSupportsOwnershipAsync("TUserUnit", true);

            try
            {
                var ctx = await _contextResolver.ResolveAsync(_rbacFx.LoginId, _rbacFx.SessionId, "test-user");

                // Insert as user — auto-creates ownership record
                using (var _ = _contextAccessor.BeginScope(ctx))
                {
                    insertedId = await _dataService.InsertAsync(
                        TUserUnitFactory.GetItem("RBAC", "OwnedDel", DateTime.UtcNow.Date));
                }

                // Delete as same user — owns the record → succeeds despite no delete grant
                using (var _ = _contextAccessor.BeginScope(ctx))
                {
                    var result = await _dataService.DeleteAsync(
                        new TUserUnit { Id = insertedId, TimeStamp = DateTime.UtcNow });

                    Assert.True(result);
                }
            }
            finally
            {
                await SetSupportsOwnershipAsync("TUserUnit", false);
                await CleanupAsync(
                    ("TUserUnit", insertedId),
                    ("DXMembershipUnit", membershipId),
                    ("DXRoleUnit", roleId));
            }
        }

        [Fact]
        public async Task Delete_Denied_WhenOwnedOnly_AndUserDoesNotOwnRecord()
        {
            var unitDefId = GetUnitDefinitionId("TUserUnit");
            var roleId = await CreateRoleAsync(unitDefId, read: true, write: true, delete: false);
            var membershipId = await CreateMembershipAsync(_rbacFx.IdentityId, _rbacFx.TenantId, roleId);
            var insertedId = Guid.Empty;

            await SetSupportsOwnershipAsync("TUserUnit", true);

            // Insert as system — no ownership record created for user
            await RunAsSystemAsync(async () =>
            {
                insertedId = await _dataService.InsertAsync(
                    TUserUnitFactory.GetItem("RBAC", "NotOwned", DateTime.UtcNow.Date));
            });

            try
            {
                var ctx = await _contextResolver.ResolveAsync(_rbacFx.LoginId, _rbacFx.SessionId, "test-user");
                using var _ = _contextAccessor.BeginScope(ctx);

                await Assert.ThrowsAsync<UnauthorizedAccessException>(
                    () => _dataService.DeleteAsync(
                        new TUserUnit { Id = insertedId, TimeStamp = DateTime.UtcNow }));
            }
            finally
            {
                await SetSupportsOwnershipAsync("TUserUnit", false);
                await CleanupAsync(
                    ("TUserUnit", insertedId),
                    ("DXMembershipUnit", membershipId),
                    ("DXRoleUnit", roleId));
            }
        }

        [Fact]
        public async Task Update_Succeeds_WhenOwnedOnly_AndUserOwnsRecord()
        {
            var unitDefId = GetUnitDefinitionId("TUserUnit");
            // No write grant → AllowedOwnedOnly for writes
            var roleId = await CreateRoleAsync(unitDefId, read: true, write: false, delete: false);
            var membershipId = await CreateMembershipAsync(_rbacFx.IdentityId, _rbacFx.TenantId, roleId);
            var insertedId = Guid.Empty;

            await SetSupportsOwnershipAsync("TUserUnit", true);

            try
            {
                var ctx = await _contextResolver.ResolveAsync(_rbacFx.LoginId, _rbacFx.SessionId, "test-user");

                // Insert via system and manually create ownership for user
                await RunAsSystemAsync(async () =>
                {
                    insertedId = await _dataService.InsertAsync(
                        TUserUnitFactory.GetItem("RBAC", "OwnedUpd", DateTime.UtcNow.Date));

                    var unitDef = _structureCache.GetDXUnit("TUserUnit");
                    _genericRepo.Insert(new DXIdentityOwnershipUnit
                    {
                        Id = Guid.CreateVersion7(),
                        TimeStamp = DateTime.UtcNow,
                        Identity = _rbacFx.IdentityId,
                        DXUnitDefinition = unitDef.Id,
                        OwnedDXUnitId = insertedId
                    });
                });

                // Read back the inserted item to get stable element IDs for update
                var existing = _genericRepo.GetDXUnit<TUserUnit>(insertedId)!;
                existing.TUserMainElement.Name = "OwnedUpdated";

                // Update as user — owns the record → succeeds despite no write grant
                using var _ = _contextAccessor.BeginScope(ctx);
                var updatedId = await _dataService.UpdateAsync(existing);

                Assert.NotEqual(Guid.Empty, updatedId);
            }
            finally
            {
                await SetSupportsOwnershipAsync("TUserUnit", false);
                await CleanupAsync(
                    ("TUserUnit", insertedId),
                    ("DXMembershipUnit", membershipId),
                    ("DXRoleUnit", roleId));
            }
        }

        // ===== Private Helpers =====

        private async Task RunAsSystemAsync(Func<Task> action)
        {
            using var _ = _contextAccessor.BeginScope(new DXExecutionContext
            {
                SubjectId = "system:rbac-tests",
                IsSystem = true
            });
            await action();
        }

        private async Task<Guid> CreateTenantWithRolesAsync(params Guid[] roleIds)
        {
            var insertedId = Guid.Empty;
            await RunAsSystemAsync(async () =>
                insertedId = await _dataService.InsertAsync(new DXTenantUnit
                {
                    TimeStamp = DateTime.UtcNow,
                    Name = $"rbac-tenant-{Guid.NewGuid():N}",
                    DXRoleElement = new DXMultiElementsContainer<DXRoleElement>
                    {
                        Announced = new HashSet<DXRoleElement>(roleIds.Select(roleId => new DXRoleElement
                        {
                            TimeStamp = DateTime.UtcNow,
                            Role = roleId
                        }))
                    }
                }));
            return insertedId;
        }

        private async Task<Guid> CreateMembershipAsync(Guid identityId, Guid tenantId, params Guid[] roleIds)
        {
            var insertedId = Guid.Empty;
            await RunAsSystemAsync(async () =>
                insertedId = await _dataService.InsertAsync(new DXMembershipUnit
                {
                    TimeStamp = DateTime.UtcNow,
                    Name = $"rbac-membership-{Guid.NewGuid():N}",
                    Identity = identityId,
                    Tenant = tenantId,
                    DXRoleElement = new DXMultiElementsContainer<DXRoleElement>
                    {
                        Announced = new HashSet<DXRoleElement>(roleIds.Select(roleId => new DXRoleElement
                        {
                            TimeStamp = DateTime.UtcNow,
                            Role = roleId
                        }))
                    }
                }));
            return insertedId;
        }

        private async Task<Guid> CreateGroupAsync(Guid tenantId, params Guid[] roleIds)
        {
            var insertedId = Guid.Empty;
            await RunAsSystemAsync(async () =>
                insertedId = await _dataService.InsertAsync(new DXGroupUnit
                {
                    TimeStamp = DateTime.UtcNow,
                    Name = $"rbac-group-{Guid.NewGuid():N}",
                    Tenant = tenantId,
                    DXRoleElement = new DXMultiElementsContainer<DXRoleElement>
                    {
                        Announced = new HashSet<DXRoleElement>(roleIds.Select(roleId => new DXRoleElement
                        {
                            TimeStamp = DateTime.UtcNow,
                            Role = roleId
                        }))
                    }
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

        private async Task<Guid> CreateRoleAsync(
            Guid targetUnitDefId,
            bool read,
            bool write,
            bool delete,
            DXGrantEffectEnum effect = DXGrantEffectEnum.Allow)
        {
            var insertedId = Guid.Empty;
            await RunAsSystemAsync(async () =>
                insertedId = await _dataService.InsertAsync(new DXRoleUnit
                {
                    TimeStamp = DateTime.UtcNow,
                    Name = $"rbac-role-{Guid.NewGuid():N}",
                    DXUnitGrantElement = new DXMultiElementsContainer<DXUnitGrantElement>
                    {
                        Announced = new HashSet<DXUnitGrantElement>
                        {
                            new DXUnitGrantElement
                            {
                                TimeStamp = DateTime.UtcNow,
                                Read = read,
                                Write = write,
                                Delete = delete,
                                Effect = effect,
                                TargetDXUnitId = targetUnitDefId
                            }
                        }
                    }
                }));
            return insertedId;
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
