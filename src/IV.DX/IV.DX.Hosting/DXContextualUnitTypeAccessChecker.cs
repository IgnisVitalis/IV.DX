using IV.DX.Kernel.Enums;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Hosting
{
    internal sealed class DXContextualUnitTypeAccessChecker(
        IDXExecutionContextAccessor executionContextAccessor,
        IDXSecurityState securityState,
        IDXStructureCache structureCache) : IDXUnitTypeAccessChecker
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

            if (!securityState.IsEnabled)
                return DXAccessDecision.Allowed;

            var context = executionContextAccessor.Current;

            if (context?.IsSystem == true)
                return DXAccessDecision.Allowed;

            if (IsCoreUnit(typeName))
                return DXAccessDecision.Denied;

            if (operation == DXUnitTypeAccessOperation.Read && IsPublicReadUnit(typeName))
                return DXAccessDecision.Allowed;

            if (context == null)
                return DXAccessDecision.Denied;

            // An explicit denial outranks grants, the create flag and the ownership fallback alike.
            if (context.Access.IsExplicitlyDenied(operation, typeName))
                return DXAccessDecision.Denied;

            if (context.Access.Allows(operation, typeName))
                return DXAccessDecision.Allowed;

            if (operation == DXUnitTypeAccessOperation.Create
                && context.IdentityId.HasValue
                && AllowsAuthenticatedCreate(typeName))
                return DXAccessDecision.Allowed;

            return FallbackToOwnership(context);
        }

        private static DXAccessDecision FallbackToOwnership(DXExecutionContext context)
        {
            return context.IdentityId.HasValue
                ? DXAccessDecision.AllowedOwnedOnly
                : DXAccessDecision.Denied;
        }

        private bool IsCoreUnit(string typeName)
        {
            var unit = structureCache.GetDXUnit(typeName);
            return unit?.Kind == DXObjectKindEnum.Core;
        }

        private bool IsPublicReadUnit(string typeName)
        {
            var unit = structureCache.GetDXUnit(typeName);
            return unit?.IsPublicRead == true;
        }

        private bool AllowsAuthenticatedCreate(string typeName)
        {
            var unit = structureCache.GetDXUnit(typeName);
            return unit?.AllowAuthenticatedCreate == true;
        }

        private static void ThrowDenied(DXExecutionContext? context, string typeName, DXUnitTypeAccessOperation operation)
        {
            var subject = context == null || string.IsNullOrWhiteSpace(context.SubjectId)
                ? "anonymous"
                : context.SubjectId;
            throw new UnauthorizedAccessException($"Access denied for '{subject}' to '{typeName}' ({operation}).");
        }
    }
}
