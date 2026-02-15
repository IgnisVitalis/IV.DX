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

            var allowedTypes = operation switch
            {
                DXUnitTypeAccessOperation.Read => context.AllowedReadUnitTypes,
                DXUnitTypeAccessOperation.Write => context.AllowedWriteUnitTypes,
                _ => null
            };

            if (ContainsType(allowedTypes, typeName))
                return;

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
    }
}

