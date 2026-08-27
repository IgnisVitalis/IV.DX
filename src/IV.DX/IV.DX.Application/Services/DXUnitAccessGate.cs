using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.Logging;

namespace IV.DX.Application.Services
{
    /// <summary>
    /// Access rules shared by every data service. See <see cref="IDXUnitAccessGate"/> for why they
    /// live in one place.
    /// </summary>
    internal sealed class DXUnitAccessGate(
        IDXUnitTypeAccessChecker unitTypeAccessChecker,
        IDXUnitGenericRepository genericRepo,
        IDXStructureCache structureCache,
        IDXExecutionContextAccessor executionContextAccessor,
        ILogger<DXUnitAccessGate> logger) : IDXUnitAccessGate
    {
        public DXReadScope EnsureReadAccess(string typeName)
        {
            var decision = unitTypeAccessChecker.CheckAccess(typeName, DXUnitTypeAccessOperation.Read);

            // An anonymous caller is denied at the type level, but records exposed one by one
            // through DXPublicAccessUnit stay reachable - narrowed to exactly those records.
            var publicFallback = decision == DXAccessDecision.Denied && executionContextAccessor.Current == null;

            if (decision == DXAccessDecision.Denied && !publicFallback)
                ThrowDenied(typeName, DXUnitTypeAccessOperation.Read);

            return decision == DXAccessDecision.AllowedOwnedOnly || publicFallback
                ? DXReadScope.VisibleOnly
                : DXReadScope.All;
        }

        public bool IsReadVisible(string typeName, Guid instanceId)
        {
            var ctx = executionContextAccessor.Current;

            var unitDef = structureCache.GetDXUnit(typeName);
            if (unitDef == null)
                return false;

            var granted = false;

            if (unitDef.SupportsOwnership && ctx?.IdentityId.HasValue == true)
            {
                var identityOwnerships = genericRepo
                    .GetDXUnits<DXIdentityOwnershipUnit>(
                        $"Identity = '{ctx.IdentityId.Value}' AND DXUnitDefinition = '{unitDef.Id}' AND OwnedDXUnitId = '{instanceId}'");

                foreach (var ownership in identityOwnerships.Where(x => x.Read))
                {
                    if (ownership.Effect == DXGrantEffectEnum.Deny)
                        return false;

                    granted |= ownership.Effect == DXGrantEffectEnum.Allow;
                }
            }

            if (unitDef.SupportsOwnership && ctx?.ActiveGroupIDs != null)
            {
                foreach (var groupId in ctx.ActiveGroupIDs)
                {
                    var groupOwnerships = genericRepo
                        .GetDXUnits<DXGroupOwnershipUnit>(
                            $"Group = '{groupId}' AND DXUnitDefinition = '{unitDef.Id}' AND OwnedDXUnitId = '{instanceId}'");

                    foreach (var ownership in groupOwnerships.Where(x => x.Read))
                    {
                        if (ownership.Effect == DXGrantEffectEnum.Deny)
                            return false;

                        granted |= ownership.Effect == DXGrantEffectEnum.Allow;
                    }
                }
            }

            if (granted)
                return true;

            var publicAccess = genericRepo
                .GetDXUnits<DXPublicAccessUnit>(
                    $"DXUnitDefinition = '{unitDef.Id}' AND PublicDXUnitId = '{instanceId}'")
                .FirstOrDefault();

            return publicAccess != null;
        }

        public HashSet<Guid> CollectVisibleIds(string typeName)
        {
            var ctx = executionContextAccessor.Current;

            var result = new HashSet<Guid>();
            var denied = new HashSet<Guid>();

            var unitDef = structureCache.GetDXUnit(typeName);
            if (unitDef == null)
                return result;

            if (unitDef.SupportsOwnership && ctx?.IdentityId.HasValue == true)
            {
                var identityOwned = genericRepo.GetDXUnits<DXIdentityOwnershipUnit>(
                    $"Identity = '{ctx.IdentityId.Value}' AND DXUnitDefinition = '{unitDef.Id}'");

                foreach (var o in identityOwned.Where(x => x.Read))
                    DXOwnershipRules.Classify(o.OwnedDXUnitId, o.Effect, result, denied);
            }

            if (unitDef.SupportsOwnership && ctx?.ActiveGroupIDs != null)
            {
                foreach (var groupId in ctx.ActiveGroupIDs)
                {
                    var groupOwned = genericRepo.GetDXUnits<DXGroupOwnershipUnit>(
                        $"Group = '{groupId}' AND DXUnitDefinition = '{unitDef.Id}'");

                    foreach (var o in groupOwned.Where(x => x.Read))
                        DXOwnershipRules.Classify(o.OwnedDXUnitId, o.Effect, result, denied);
                }
            }

            var publicAccess = genericRepo.GetDXUnits<DXPublicAccessUnit>(
                $"DXUnitDefinition = '{unitDef.Id}'");

            foreach (var access in publicAccess)
            {
                if (access.PublicDXUnitId != Guid.Empty)
                    result.Add(access.PublicDXUnitId);
            }

            // A denial on a record outranks every route to it, public exposure included.
            result.ExceptWith(denied);

            return result;
        }

        public void EnsureTypeAccess(string? typeName, DXUnitTypeAccessOperation operation)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return;

            if (unitTypeAccessChecker.CheckAccess(typeName, operation) == DXAccessDecision.Allowed)
                return;

            var subject = GetCurrentSubject();
            logger.LogWarning(
                "{Operation} access denied for subject {Subject} to DX unit type {TypeName}.",
                operation,
                subject,
                typeName);
            throw new UnauthorizedAccessException($"Access denied for '{subject}' to '{typeName}' ({operation}).");
        }

        public void EnsureInstanceAccess(string? typeName, Guid instanceId, DXUnitTypeAccessOperation operation)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return;

            var decision = unitTypeAccessChecker.CheckAccess(typeName, operation);

            if (decision == DXAccessDecision.Allowed)
                return;

            if (decision == DXAccessDecision.AllowedOwnedOnly)
            {
                EnsureOwnership(typeName, instanceId, operation);
                return;
            }

            var subject = GetCurrentSubject();
            logger.LogWarning(
                "{Operation} access denied for subject {Subject} to DX unit type {TypeName} and instance {InstanceId}.",
                operation,
                subject,
                typeName,
                instanceId);
            throw new UnauthorizedAccessException($"Access denied for '{subject}' to '{typeName}' ({operation}).");
        }

        public string GetCurrentSubject()
        {
            var ctx = executionContextAccessor.Current;
            return ctx == null || string.IsNullOrWhiteSpace(ctx.SubjectId) ? "anonymous" : ctx.SubjectId;
        }

        /// <summary>
        /// Ownership rows are instance-level grants: each one states which operations its owner may
        /// perform. A Deny row outranks every Allow row on the same record.
        /// </summary>
        private void EnsureOwnership(string typeName, Guid instanceId, DXUnitTypeAccessOperation operation)
        {
            var unitDef = structureCache.GetDXUnit(typeName);
            if (unitDef == null || !unitDef.SupportsOwnership)
            {
                var subject = GetCurrentSubject();
                logger.LogWarning(
                    "Ownership check denied for subject {Subject} because DX unit type {TypeName} does not support ownership.",
                    subject,
                    typeName);
                throw new UnauthorizedAccessException($"Access denied for '{subject}' to '{typeName}' instance '{instanceId}'.");
            }

            var context = executionContextAccessor.Current;
            var granted = false;

            if (context?.IdentityId.HasValue == true)
            {
                var identityOwnerships = genericRepo
                    .GetDXUnits<DXIdentityOwnershipUnit>(
                        $"Identity = '{context.IdentityId.Value}' AND DXUnitDefinition = '{unitDef.Id}' AND OwnedDXUnitId = '{instanceId}'");

                foreach (var ownership in identityOwnerships)
                {
                    if (!DXOwnershipRules.Covers(ownership.Read, ownership.Update, ownership.Delete, operation))
                        continue;

                    if (ownership.Effect == DXGrantEffectEnum.Deny)
                    {
                        ThrowOwnershipDenied(typeName, instanceId, operation);
                    }

                    granted |= ownership.Effect == DXGrantEffectEnum.Allow;
                }
            }

            if (context?.ActiveGroupIDs != null)
            {
                foreach (var groupId in context.ActiveGroupIDs)
                {
                    var groupOwnerships = genericRepo
                        .GetDXUnits<DXGroupOwnershipUnit>(
                            $"Group = '{groupId}' AND DXUnitDefinition = '{unitDef.Id}' AND OwnedDXUnitId = '{instanceId}'");

                    foreach (var ownership in groupOwnerships)
                    {
                        if (!DXOwnershipRules.Covers(ownership.Read, ownership.Update, ownership.Delete, operation))
                            continue;

                        if (ownership.Effect == DXGrantEffectEnum.Deny)
                        {
                            ThrowOwnershipDenied(typeName, instanceId, operation);
                        }

                        granted |= ownership.Effect == DXGrantEffectEnum.Allow;
                    }
                }
            }

            if (granted)
                return;

            ThrowOwnershipDenied(typeName, instanceId, operation);
        }

        private void ThrowOwnershipDenied(string typeName, Guid instanceId, DXUnitTypeAccessOperation operation)
        {
            var subject = GetCurrentSubject();
            logger.LogWarning(
                "Ownership check denied for subject {Subject} to {Operation} DX unit type {TypeName} and instance {InstanceId}.",
                subject,
                operation,
                typeName,
                instanceId);
            throw new UnauthorizedAccessException($"Access denied for '{subject}' to '{typeName}' instance '{instanceId}' ({operation}).");
        }

        private void ThrowDenied(string typeName, DXUnitTypeAccessOperation operation)
        {
            var subject = GetCurrentSubject();
            logger.LogWarning(
                "Read access denied for subject {Subject} to DX unit type {TypeName} and operation {Operation}.",
                subject,
                typeName,
                operation);
            throw new UnauthorizedAccessException($"Access denied for '{subject}' to '{typeName}' ({operation}).");
        }
    }
}
