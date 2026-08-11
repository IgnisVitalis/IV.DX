namespace IV.DX.Persistence.Contracts.Abstractions
{
    /// <summary>
    /// Immutable set of DX unit type names allowed for a single access operation.
    /// The set has three explicit states:
    /// <list type="bullet">
    /// <item><see cref="Unrestricted"/> — no restriction is imposed; every type is allowed.</item>
    /// <item><see cref="None"/> — an explicit restriction that allows nothing.</item>
    /// <item>a concrete set of type names — only those types are allowed.</item>
    /// </list>
    /// The distinction between <see cref="Unrestricted"/> and <see cref="None"/> is deliberate:
    /// an empty restriction never widens access.
    /// </summary>
    public sealed class DXUnitTypeAllowSet
    {
        private readonly HashSet<string>? _typeNames;

        private DXUnitTypeAllowSet(HashSet<string>? typeNames)
        {
            _typeNames = typeNames;
        }

        /// <summary>No restriction is imposed. Allows every type and is skipped when levels are combined.</summary>
        public static DXUnitTypeAllowSet Unrestricted { get; } = new(null);

        /// <summary>An explicit restriction that allows nothing.</summary>
        public static DXUnitTypeAllowSet None { get; } = new(new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        /// <summary>True when no restriction is imposed by this set.</summary>
        public bool IsUnrestricted => _typeNames == null;

        /// <summary>
        /// Builds a restriction from the supplied type names.
        /// A null or empty sequence produces <see cref="None"/>, never <see cref="Unrestricted"/>.
        /// </summary>
        public static DXUnitTypeAllowSet FromTypeNames(IEnumerable<string>? typeNames)
        {
            if (typeNames == null)
                return None;

            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var typeName in typeNames)
            {
                if (!string.IsNullOrWhiteSpace(typeName))
                    names.Add(typeName);
            }

            return names.Count == 0 ? None : new DXUnitTypeAllowSet(names);
        }

        /// <summary>True when the supplied type name is allowed by this set.</summary>
        public bool Allows(string? typeName)
        {
            if (_typeNames == null)
                return true;

            return !string.IsNullOrWhiteSpace(typeName) && _typeNames.Contains(typeName);
        }

        /// <summary>
        /// Narrows this set by another one. An unrestricted operand imposes nothing and is ignored;
        /// otherwise the result is the intersection, collapsing to <see cref="None"/> when empty.
        /// </summary>
        public DXUnitTypeAllowSet Intersect(DXUnitTypeAllowSet other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (other.IsUnrestricted)
                return this;

            if (IsUnrestricted)
                return other;

            var intersected = new HashSet<string>(_typeNames!, StringComparer.OrdinalIgnoreCase);
            intersected.IntersectWith(other._typeNames!);

            return intersected.Count == 0 ? None : new DXUnitTypeAllowSet(intersected);
        }

        /// <summary>
        /// Widens this set by another one. An unrestricted operand already covers everything,
        /// so the result is unrestricted; otherwise the result is the union.
        /// </summary>
        public DXUnitTypeAllowSet Union(DXUnitTypeAllowSet other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (IsUnrestricted || other.IsUnrestricted)
                return Unrestricted;

            var union = new HashSet<string>(_typeNames!, StringComparer.OrdinalIgnoreCase);
            union.UnionWith(other._typeNames!);

            return union.Count == 0 ? None : new DXUnitTypeAllowSet(union);
        }

        public override string ToString()
        {
            if (_typeNames == null)
                return "unrestricted";

            return _typeNames.Count == 0 ? "none" : string.Join(", ", _typeNames);
        }
    }
}
