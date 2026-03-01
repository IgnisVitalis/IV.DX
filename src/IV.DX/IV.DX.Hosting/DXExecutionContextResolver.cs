using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Hosting
{
    internal sealed class DXExecutionContextResolver(
        IDXUnitGenericRepository dxUnitGenericRepository,
        IDXElementGenericRepository dxElementGenericRepository,
        IDXStructureCache dxStructureCache) : IDXExecutionContextResolver
    {
        private const string DenyMarker = "__dx_deny__";

        public Task<DXExecutionContext> ResolveAsync(Guid identityLoginId, Guid sessionId, string? subjectId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (identityLoginId == Guid.Empty || sessionId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Invalid security claims.");
            }

            var now = DateTime.UtcNow;
            var session = dxUnitGenericRepository
                .GetDXUnits<DXAuthSessionUnit>($"SessionId = '{sessionId}'")
                .FirstOrDefault();

            if (session == null
                || session.IdentityLogin != identityLoginId
                || session.RevokedAt.HasValue
                || session.ExpiresAt <= now)
            {
                throw new UnauthorizedAccessException("Session is invalid.");
            }

            var identityLogin = dxUnitGenericRepository.GetDXUnit<DXIdentityLoginUnit>(identityLoginId);
            if (identityLogin == null)
            {
                throw new UnauthorizedAccessException("Identity login is not found.");
            }

            var memberships = dxUnitGenericRepository
                .GetDXUnits<DXMembershipUnit>($"Identity = '{identityLogin.Identity}'")
                .ToList();

            var tenantRoleIds = new HashSet<Guid>();
            var membershipRoleIds = new HashSet<Guid>();
            var groupRoleIds = new HashSet<Guid>();
            var activeGroupIDs = new HashSet<Guid>();

            foreach (var membership in memberships)
            {
                membershipRoleIds.UnionWith(GetRoleIdsForMember(membership.ID));
                tenantRoleIds.UnionWith(GetRoleIdsForMember(membership.Tenant));

                var groupMemberships = dxUnitGenericRepository
                    .GetDXUnits<DXGroupMembershipUnit>($"Membership = '{membership.ID}'");

                foreach (var groupMembership in groupMemberships)
                {
                    groupRoleIds.UnionWith(GetRoleIdsForMember(groupMembership.Group));
                    activeGroupIDs.Add(groupMembership.Group);
                }
            }

            var tenantRead = ResolveAllowedUnitTypes(tenantRoleIds, static x => x.Read);
            var tenantWrite = ResolveAllowedUnitTypes(tenantRoleIds, static x => x.Write);

            var membershipRead = ResolveAllowedUnitTypes(membershipRoleIds, static x => x.Read);
            var membershipWrite = ResolveAllowedUnitTypes(membershipRoleIds, static x => x.Write);

            var groupRead = ResolveAllowedUnitTypes(groupRoleIds, static x => x.Read);
            var groupWrite = ResolveAllowedUnitTypes(groupRoleIds, static x => x.Write);

            var applyGroupRestrictions = groupRoleIds.Count > 0;

            var finalRead = ComputeFinalAllowedTypes(
                tenantRead,
                membershipRead,
                applyGroupRestrictions ? groupRead : null);

            var finalWrite = ComputeFinalAllowedTypes(
                tenantWrite,
                membershipWrite,
                applyGroupRestrictions ? groupWrite : null);

            var resolvedSubject = string.IsNullOrWhiteSpace(subjectId)
                ? identityLogin.Subject
                : subjectId;

            return Task.FromResult(new DXExecutionContext
            {
                SubjectId = resolvedSubject,
                IsSystem = false,
                IdentityID = identityLogin.Identity,
                ActiveGroupIDs = activeGroupIDs.Count > 0 ? activeGroupIDs : null,
                AllowedReadUnitTypes = finalRead,
                AllowedWriteUnitTypes = finalWrite,
                TenantReadUnitTypes = tenantRead,
                TenantWriteUnitTypes = tenantWrite,
                MembershipReadUnitTypes = membershipRead,
                MembershipWriteUnitTypes = membershipWrite,
                GroupReadUnitTypes = groupRead,
                GroupWriteUnitTypes = groupWrite,
                ApplyGroupRestrictions = applyGroupRestrictions
            });
        }

        private HashSet<Guid> GetRoleIdsForMember(Guid memberId)
        {
            if (memberId == Guid.Empty)
            {
                return new HashSet<Guid>();
            }

            var roleElements = dxElementGenericRepository
                .GetItems<DXRoleElement>("DXSecurityMemberUnit", $"DXUnitID = '{memberId}'");

            return roleElements
                .Select(x => x.Role)
                .Where(x => x != Guid.Empty)
                .ToHashSet();
        }

        private IReadOnlyCollection<string>? ResolveAllowedUnitTypes(
            HashSet<Guid> roleIds,
            Func<DXUnitGrantElement, bool> operationSelector)
        {
            if (roleIds.Count == 0)
            {
                return null;
            }

            var effectByUnit = new Dictionary<Guid, DXGrantEffectEnum>();

            foreach (var roleId in roleIds)
            {
                var grants = dxElementGenericRepository
                    .GetItems<DXUnitGrantElement>("DXRoleUnit", $"DXUnitID = '{roleId}'");

                foreach (var grant in grants)
                {
                    if (!operationSelector(grant) || grant.TargetDXUnitID == Guid.Empty)
                    {
                        continue;
                    }

                    if (grant.Effect == DXGrantEffectEnum.Deny)
                    {
                        effectByUnit[grant.TargetDXUnitID] = DXGrantEffectEnum.Deny;
                        continue;
                    }

                    if (!effectByUnit.TryGetValue(grant.TargetDXUnitID, out var currentEffect)
                        || currentEffect != DXGrantEffectEnum.Deny)
                    {
                        effectByUnit[grant.TargetDXUnitID] = DXGrantEffectEnum.Allow;
                    }
                }
            }

            var allowedUnitIds = effectByUnit
                .Where(x => x.Value == DXGrantEffectEnum.Allow)
                .Select(x => x.Key)
                .ToHashSet();

            if (allowedUnitIds.Count == 0)
            {
                return DenySet();
            }

            var names = dxStructureCache.DXUnits
                .Where(x => allowedUnitIds.Contains(x.ID))
                .Select(x => x.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return names.Count == 0 ? DenySet() : names;
        }

        private static IReadOnlyCollection<string> ComputeFinalAllowedTypes(
            IReadOnlyCollection<string>? tenantTypes,
            IReadOnlyCollection<string>? membershipTypes,
            IReadOnlyCollection<string>? groupTypes)
        {
            HashSet<string>? result = null;

            Apply(ref result, tenantTypes);
            Apply(ref result, membershipTypes);
            Apply(ref result, groupTypes);

            if (result == null || result.Count == 0)
            {
                return DenySet();
            }

            return result;
        }

        private static void Apply(ref HashSet<string>? result, IReadOnlyCollection<string>? set)
        {
            if (set == null)
            {
                return;
            }

            if (result == null)
            {
                result = new HashSet<string>(set, StringComparer.OrdinalIgnoreCase);
                return;
            }

            result.IntersectWith(set);
        }

        private static HashSet<string> DenySet()
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { DenyMarker };
        }
    }
}
