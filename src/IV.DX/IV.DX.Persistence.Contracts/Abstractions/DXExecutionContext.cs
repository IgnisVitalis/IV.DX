namespace IV.DX.Persistence.Contracts.Abstractions
{
    public sealed class DXExecutionContext
    {
        public string? SubjectId { get; init; }
        public bool IsSystem { get; init; }
        public Guid? IdentityID { get; init; }
        public IReadOnlyCollection<Guid>? ActiveGroupIDs { get; init; }

        // Legacy global allow-lists (kept for backward compatibility).
        public IReadOnlyCollection<string>? AllowedReadUnitTypes { get; init; }
        public IReadOnlyCollection<string>? AllowedWriteUnitTypes { get; init; }

        // Hierarchical security allow-lists.
        // Final access is intersection of provided levels:
        // Tenant ∩ Membership ∩ (Group if ApplyGroupRestrictions = true).
        public IReadOnlyCollection<string>? TenantReadUnitTypes { get; init; }
        public IReadOnlyCollection<string>? TenantWriteUnitTypes { get; init; }

        public IReadOnlyCollection<string>? MembershipReadUnitTypes { get; init; }
        public IReadOnlyCollection<string>? MembershipWriteUnitTypes { get; init; }

        public IReadOnlyCollection<string>? GroupReadUnitTypes { get; init; }
        public IReadOnlyCollection<string>? GroupWriteUnitTypes { get; init; }
        public IReadOnlyCollection<string>? GroupDeleteUnitTypes { get; init; }

        public IReadOnlyCollection<string>? TenantDeleteUnitTypes { get; init; }
        public IReadOnlyCollection<string>? MembershipDeleteUnitTypes { get; init; }
        public IReadOnlyCollection<string>? AllowedDeleteUnitTypes { get; init; }

        public bool ApplyGroupRestrictions { get; init; }
    }
}
