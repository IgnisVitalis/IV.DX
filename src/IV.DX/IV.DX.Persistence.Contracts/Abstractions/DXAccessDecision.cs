namespace IV.DX.Persistence.Contracts.Abstractions
{
    public enum DXAccessDecision
    {
        Allowed,          // full type-level access granted
        AllowedOwnedOnly, // no type-level access but ownership check may grant access
        Denied            // no access (no context, no identity, or explicit deny with no ownership)
    }
}
