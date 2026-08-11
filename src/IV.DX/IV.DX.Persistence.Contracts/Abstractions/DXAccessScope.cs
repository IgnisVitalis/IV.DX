namespace IV.DX.Persistence.Contracts.Abstractions
{
    /// <summary>
    /// Type-level access granted for every <see cref="DXUnitTypeAccessOperation"/>.
    /// Operations are keyed rather than declared as individual members, so adding an
    /// operation to the enum does not change the shape of this type.
    /// <para>
    /// Explicit denials are carried alongside the granted types. "Denied" and "never granted"
    /// are not the same: only an explicit denial overrides access that is granted elsewhere.
    /// </para>
    /// </summary>
    public sealed class DXAccessScope
    {
        private readonly IReadOnlyDictionary<DXUnitTypeAccessOperation, DXUnitTypeAllowSet>? _allowed;
        private readonly IReadOnlyDictionary<DXUnitTypeAccessOperation, DXUnitTypeAllowSet>? _denied;

        private DXAccessScope(
            IReadOnlyDictionary<DXUnitTypeAccessOperation, DXUnitTypeAllowSet>? allowed,
            IReadOnlyDictionary<DXUnitTypeAccessOperation, DXUnitTypeAllowSet>? denied)
        {
            _allowed = allowed;
            _denied = denied;
        }

        /// <summary>No restriction is imposed on any operation, and nothing is denied.</summary>
        public static DXAccessScope Unrestricted { get; } = new(null, null);

        /// <summary>Nothing is granted for any operation. Not the same as everything being denied.</summary>
        public static DXAccessScope None { get; } = FromOperations(static _ => DXUnitTypeAllowSet.None);

        /// <summary>Builds a scope by resolving the granted, and optionally denied, types per operation.</summary>
        public static DXAccessScope FromOperations(
            Func<DXUnitTypeAccessOperation, DXUnitTypeAllowSet?> allowedSelector,
            Func<DXUnitTypeAccessOperation, DXUnitTypeAllowSet?>? deniedSelector = null)
        {
            ArgumentNullException.ThrowIfNull(allowedSelector);

            var allowed = new Dictionary<DXUnitTypeAccessOperation, DXUnitTypeAllowSet>();
            var denied = new Dictionary<DXUnitTypeAccessOperation, DXUnitTypeAllowSet>();

            foreach (var operation in Enum.GetValues<DXUnitTypeAccessOperation>())
            {
                allowed[operation] = allowedSelector(operation) ?? DXUnitTypeAllowSet.Unrestricted;
                denied[operation] = deniedSelector?.Invoke(operation) ?? DXUnitTypeAllowSet.None;
            }

            return new DXAccessScope(allowed, denied);
        }

        /// <summary>
        /// Builds a scope that restricts a single operation to the supplied type names and
        /// leaves every other operation unrestricted.
        /// </summary>
        public static DXAccessScope ForOperation(DXUnitTypeAccessOperation operation, params string[] typeNames)
        {
            return FromOperations(op => op == operation
                ? DXUnitTypeAllowSet.FromTypeNames(typeNames)
                : DXUnitTypeAllowSet.Unrestricted);
        }

        /// <summary>Types granted for the supplied operation.</summary>
        public DXUnitTypeAllowSet For(DXUnitTypeAccessOperation operation)
        {
            return _allowed != null && _allowed.TryGetValue(operation, out var allowSet)
                ? allowSet
                : DXUnitTypeAllowSet.Unrestricted;
        }

        /// <summary>Types explicitly denied for the supplied operation.</summary>
        public DXUnitTypeAllowSet DeniedFor(DXUnitTypeAccessOperation operation)
        {
            return _denied != null && _denied.TryGetValue(operation, out var denySet)
                ? denySet
                : DXUnitTypeAllowSet.None;
        }

        /// <summary>True when the supplied type is granted for the supplied operation.</summary>
        public bool Allows(DXUnitTypeAccessOperation operation, string? typeName)
        {
            return For(operation).Allows(typeName);
        }

        /// <summary>
        /// True when the supplied type carries an explicit denial for the supplied operation.
        /// An explicit denial outranks every other route to access.
        /// </summary>
        public bool IsExplicitlyDenied(DXUnitTypeAccessOperation operation, string? typeName)
        {
            return DeniedFor(operation).Allows(typeName);
        }

        /// <summary>
        /// Narrows this scope by another one, operation by operation.
        /// Grants are intersected; denials are accumulated, since a denial at any level stands.
        /// </summary>
        public DXAccessScope Intersect(DXAccessScope other)
        {
            ArgumentNullException.ThrowIfNull(other);

            return FromOperations(
                op => For(op).Intersect(other.For(op)),
                op => DeniedFor(op).Union(other.DeniedFor(op)));
        }
    }
}
