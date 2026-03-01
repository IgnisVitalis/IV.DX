using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Hosting
{
    internal sealed class DXContextualUnitTypeAccessChecker(IDXExecutionContextAccessor executionContextAccessor) : IDXUnitTypeAccessChecker
    {
        public void EnsureAccess(string typeName, DXUnitTypeAccessOperation operation)
        {
            var decision = CheckAccess(typeName, operation);

            if (decision != DXAccessDecision.Allowed)
            {
                var context = executionContextAccessor.Current;
                ThrowDenied(context, typeName, operation);
            }
        }

        public DXAccessDecision CheckAccess(string typeName, DXUnitTypeAccessOperation operation)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(typeName);

            var context = executionContextAccessor.Current;

            if (context == null)
                return DXAccessDecision.Denied;

            if (context.IsSystem)
                return DXAccessDecision.Allowed;

            var tenantAllowedTypes = operation switch
            {
                DXUnitTypeAccessOperation.Read => context.TenantReadUnitTypes,
                DXUnitTypeAccessOperation.Write => context.TenantWriteUnitTypes,
                _ => null
            };

            if (IsRestrictionProvided(tenantAllowedTypes) && !ContainsType(tenantAllowedTypes, typeName))
            {
                return FallbackToOwnership(context);
            }

            var membershipAllowedTypes = operation switch
            {
                DXUnitTypeAccessOperation.Read => context.MembershipReadUnitTypes,
                DXUnitTypeAccessOperation.Write => context.MembershipWriteUnitTypes,
                _ => null
            };

            if (IsRestrictionProvided(membershipAllowedTypes) && !ContainsType(membershipAllowedTypes, typeName))
            {
                return FallbackToOwnership(context);
            }

            if (context.ApplyGroupRestrictions)
            {
                var groupAllowedTypes = operation switch
                {
                    DXUnitTypeAccessOperation.Read => context.GroupReadUnitTypes,
                    DXUnitTypeAccessOperation.Write => context.GroupWriteUnitTypes,
                    _ => null
                };

                if (!ContainsTypeStrict(groupAllowedTypes, typeName))
                {
                    return FallbackToOwnership(context);
                }
            }

            var globalAllowedTypes = operation switch
            {
                DXUnitTypeAccessOperation.Read => context.AllowedReadUnitTypes,
                DXUnitTypeAccessOperation.Write => context.AllowedWriteUnitTypes,
                _ => null
            };

            if (ContainsType(globalAllowedTypes, typeName))
                return DXAccessDecision.Allowed;

            return FallbackToOwnership(context);
        }

        private static DXAccessDecision FallbackToOwnership(DXExecutionContext context)
        {
            return context.IdentityID.HasValue
                ? DXAccessDecision.AllowedOwnedOnly
                : DXAccessDecision.Denied;
        }

        private static bool IsRestrictionProvided(IReadOnlyCollection<string>? allowedTypes)
        {
            return allowedTypes != null;
        }

        private static void ThrowDenied(DXExecutionContext? context, string typeName, DXUnitTypeAccessOperation operation)
        {
            var subject = context == null || string.IsNullOrWhiteSpace(context.SubjectId)
                ? "anonymous"
                : context.SubjectId;
            throw new UnauthorizedAccessException($"Access denied for '{subject}' to '{typeName}' ({operation}).");
        }

        private static bool ContainsType(IReadOnlyCollection<string>? allowedTypes, string typeName)
        {
            if (allowedTypes == null || allowedTypes.Count == 0)
                return true;

            foreach (var allowedType in allowedTypes)
            {
                if (string.Equals(allowedType, "*", StringComparison.Ordinal))
                    return true;

                if (string.Equals(allowedType, typeName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool ContainsTypeStrict(IReadOnlyCollection<string>? allowedTypes, string typeName)
        {
            if (allowedTypes == null || allowedTypes.Count == 0)
                return false;

            return ContainsType(allowedTypes, typeName);
        }
    }
}
