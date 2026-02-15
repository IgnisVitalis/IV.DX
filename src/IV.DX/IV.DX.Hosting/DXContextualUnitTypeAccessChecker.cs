using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Hosting
{
    internal sealed class DXContextualUnitTypeAccessChecker(IDXExecutionContextAccessor executionContextAccessor) : IDXUnitTypeAccessChecker
    {
        public void EnsureAccess(string typeName, DXUnitTypeAccessOperation operation)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(typeName);

            var context = executionContextAccessor.Current;

            if (context == null || context.IsSystem)
                return;

            var tenantAllowedTypes = operation switch
            {
                DXUnitTypeAccessOperation.Read => context.TenantReadUnitTypes,
                DXUnitTypeAccessOperation.Write => context.TenantWriteUnitTypes,
                _ => null
            };

            if (IsRestrictionProvided(tenantAllowedTypes) && !ContainsType(tenantAllowedTypes, typeName))
            {
                ThrowDenied(context, typeName, operation);
            }

            var membershipAllowedTypes = operation switch
            {
                DXUnitTypeAccessOperation.Read => context.MembershipReadUnitTypes,
                DXUnitTypeAccessOperation.Write => context.MembershipWriteUnitTypes,
                _ => null
            };

            if (IsRestrictionProvided(membershipAllowedTypes) && !ContainsType(membershipAllowedTypes, typeName))
            {
                ThrowDenied(context, typeName, operation);
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
                    ThrowDenied(context, typeName, operation);
                }
            }

            var globalAllowedTypes = operation switch
            {
                DXUnitTypeAccessOperation.Read => context.AllowedReadUnitTypes,
                DXUnitTypeAccessOperation.Write => context.AllowedWriteUnitTypes,
                _ => null
            };

            if (ContainsType(globalAllowedTypes, typeName))
                return;

            ThrowDenied(context, typeName, operation);
        }

        private static bool IsRestrictionProvided(IReadOnlyCollection<string>? allowedTypes)
        {
            return allowedTypes != null;
        }

        private static void ThrowDenied(DXExecutionContext context, string typeName, DXUnitTypeAccessOperation operation)
        {
            var subject = string.IsNullOrWhiteSpace(context.SubjectId) ? "anonymous" : context.SubjectId;
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
