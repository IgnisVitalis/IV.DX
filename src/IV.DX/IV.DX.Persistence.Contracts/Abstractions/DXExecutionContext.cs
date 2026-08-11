namespace IV.DX.Persistence.Contracts.Abstractions
{
    public sealed class DXExecutionContext
    {
        public string? SubjectId { get; init; }
        public bool IsSystem { get; init; }
        public Guid? IdentityId { get; init; }
        public IReadOnlyCollection<Guid>? ActiveGroupIDs { get; init; }

        /// <summary>
        /// Effective type-level access for this principal.
        /// <see cref="IDXExecutionContextResolver"/> computes it by narrowing the tenant,
        /// membership and group levels against each other; a level that grants nothing
        /// yields <see cref="DXAccessScope.None"/> rather than widening access.
        /// </summary>
        public DXAccessScope Access { get; init; } = DXAccessScope.Unrestricted;
    }
}
