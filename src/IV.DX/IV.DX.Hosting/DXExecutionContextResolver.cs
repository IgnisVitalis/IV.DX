using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Hosting
{
    internal sealed class DXExecutionContextResolver(
        IDXUnitGenericRepository dxUnitGenericRepository,
        IDXElementGenericRepository dxElementGenericRepository,
        IDXStructureCache dxStructureCache) : IDXExecutionContextResolver
    {
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
                membershipRoleIds.UnionWith(GetRoleIdsForMember(membership.Id));
                tenantRoleIds.UnionWith(GetRoleIdsForMember(membership.Tenant));

                var groupMemberships = dxUnitGenericRepository
                    .GetDXUnits<DXGroupMembershipUnit>($"Membership = '{membership.Id}'");

                foreach (var groupMembership in groupMemberships)
                {
                    groupRoleIds.UnionWith(GetRoleIdsForMember(groupMembership.Group));
                    activeGroupIDs.Add(groupMembership.Group);
                }
            }

            var tenantScope = ResolveScope(tenantRoleIds);
            var membershipScope = ResolveScope(membershipRoleIds);
            var groupScope = ResolveScope(groupRoleIds);

            var access = DXAccessScope.FromOperations(
                operation => NarrowLevels(
                    tenantScope.For(operation),
                    membershipScope.For(operation),
                    groupScope.For(operation)),
                operation => tenantScope.DeniedFor(operation)
                    .Union(membershipScope.DeniedFor(operation))
                    .Union(groupScope.DeniedFor(operation)));

            var resolvedSubject = string.IsNullOrWhiteSpace(subjectId)
                ? identityLogin.Subject
                : subjectId;

            return Task.FromResult(new DXExecutionContext
            {
                SubjectId = resolvedSubject,
                IsSystem = false,
                IdentityId = identityLogin.Identity,
                ActiveGroupIDs = activeGroupIDs.Count > 0 ? activeGroupIDs : null,
                Access = access
            });
        }

        /// <summary>
        /// Narrows the levels against each other. A level that imposes no restriction is skipped;
        /// when no level imposed one, nothing was granted anywhere and the result allows nothing.
        /// </summary>
        private static DXUnitTypeAllowSet NarrowLevels(params DXUnitTypeAllowSet[] levels)
        {
            DXUnitTypeAllowSet? result = null;

            foreach (var level in levels)
            {
                if (level.IsUnrestricted)
                    continue;

                result = result == null ? level : result.Intersect(level);
            }

            return result ?? DXUnitTypeAllowSet.None;
        }

        private HashSet<Guid> GetRoleIdsForMember(Guid memberId)
        {
            if (memberId == Guid.Empty)
            {
                return new HashSet<Guid>();
            }

            var roleElements = dxElementGenericRepository
                .GetItems<DXRoleElement>("DXSecurityMemberUnit", $"Id = '{memberId}'");

            return roleElements
                .Select(x => x.Role)
                .Where(x => x != Guid.Empty)
                .ToHashSet();
        }

        /// <summary>
        /// Resolves the access granted by a set of roles. An empty role set imposes no
        /// restriction at this level, which is not the same as granting nothing.
        /// </summary>
        private DXAccessScope ResolveScope(HashSet<Guid> roleIds)
        {
            if (roleIds.Count == 0)
            {
                return DXAccessScope.Unrestricted;
            }

            // Grants are read once per role and every operation is derived from them,
            // rather than re-querying per operation.
            var grants = new List<DXUnitGrantElement>();

            foreach (var roleId in roleIds)
            {
                grants.AddRange(dxElementGenericRepository
                    .GetItems<DXUnitGrantElement>("DXRoleUnit", $"Id = '{roleId}'"));
            }

            var allowed = new Dictionary<DXUnitTypeAccessOperation, DXUnitTypeAllowSet>();
            var denied = new Dictionary<DXUnitTypeAccessOperation, DXUnitTypeAllowSet>();

            foreach (var operation in Enum.GetValues<DXUnitTypeAccessOperation>())
            {
                var effectByUnit = ResolveEffects(grants, operation);

                allowed[operation] = ToAllowSet(effectByUnit, DXGrantEffectEnum.Allow);
                denied[operation] = ToAllowSet(effectByUnit, DXGrantEffectEnum.Deny);
            }

            return DXAccessScope.FromOperations(op => allowed[op], op => denied[op]);
        }

        /// <summary>
        /// Resolves the effect each targeted unit carries for the supplied operation.
        /// Deny outranks Allow within a level regardless of the order grants are seen in.
        /// </summary>
        private static Dictionary<Guid, DXGrantEffectEnum> ResolveEffects(
            IReadOnlyList<DXUnitGrantElement> grants,
            DXUnitTypeAccessOperation operation)
        {
            var effectByUnit = new Dictionary<Guid, DXGrantEffectEnum>();

            foreach (var grant in grants)
            {
                if (!GrantCovers(grant, operation) || grant.TargetDXUnitId == Guid.Empty)
                {
                    continue;
                }

                if (grant.Effect == DXGrantEffectEnum.Deny)
                {
                    effectByUnit[grant.TargetDXUnitId] = DXGrantEffectEnum.Deny;
                    continue;
                }

                if (!effectByUnit.TryGetValue(grant.TargetDXUnitId, out var currentEffect)
                    || currentEffect != DXGrantEffectEnum.Deny)
                {
                    effectByUnit[grant.TargetDXUnitId] = DXGrantEffectEnum.Allow;
                }
            }

            return effectByUnit;
        }

        private DXUnitTypeAllowSet ToAllowSet(
            Dictionary<Guid, DXGrantEffectEnum> effectByUnit,
            DXGrantEffectEnum effect)
        {
            var unitIds = effectByUnit
                .Where(x => x.Value == effect)
                .Select(x => x.Key)
                .ToHashSet();

            if (unitIds.Count == 0)
            {
                return DXUnitTypeAllowSet.None;
            }

            var names = dxStructureCache.DXUnits
                .Where(x => unitIds.Contains(x.Id))
                .Select(x => x.Name);

            return DXUnitTypeAllowSet.FromTypeNames(names);
        }

        private static bool GrantCovers(DXUnitGrantElement grant, DXUnitTypeAccessOperation operation) => operation switch
        {
            DXUnitTypeAccessOperation.Read => grant.Read,
            DXUnitTypeAccessOperation.Create => grant.Create,
            DXUnitTypeAccessOperation.Update => grant.Update,
            DXUnitTypeAccessOperation.Delete => grant.Delete,
            _ => false
        };
    }
}
