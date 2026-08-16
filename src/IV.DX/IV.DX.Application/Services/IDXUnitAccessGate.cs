using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Services
{
    /// <summary>
    /// How much of a type a caller may read once the type-level gate has let them through.
    /// </summary>
    internal enum DXReadScope
    {
        /// <summary>Every record of the type.</summary>
        All = 1,

        /// <summary>
        /// Only records the caller owns or that are publicly exposed. The caller must narrow the
        /// read itself, through <see cref="IDXUnitAccessGate.IsReadVisible"/> for a single record or
        /// <see cref="IDXUnitAccessGate.CollectVisibleIds"/> for a set.
        /// </summary>
        VisibleOnly = 2
    }

    /// <summary>
    /// The single place where a data service decides whether a caller may touch a DX unit type or
    /// one of its records.
    /// </summary>
    /// <remarks>
    /// Both the unit services and the element service answer to the same grants: an element has no
    /// access rules of its own, it inherits the unit that owns it. Keeping the rules in one
    /// component is what stops the two paths from drifting apart, which would turn the newer one
    /// into a way around the older one.
    /// </remarks>
    internal interface IDXUnitAccessGate
    {
        /// <summary>
        /// Settles whether the caller may read this type at all, and how far. Throws
        /// <see cref="UnauthorizedAccessException"/> when they may not.
        /// </summary>
        DXReadScope EnsureReadAccess(string typeName);

        /// <summary>
        /// Whether one record is readable under <see cref="DXReadScope.VisibleOnly"/>. Costs one
        /// targeted query, so it beats <see cref="CollectVisibleIds"/> when there is a single id in
        /// hand.
        /// </summary>
        bool IsReadVisible(string typeName, Guid instanceId);

        /// <summary>
        /// Every record id readable under <see cref="DXReadScope.VisibleOnly"/>.
        /// </summary>
        HashSet<Guid> CollectVisibleIds(string typeName);

        /// <summary>
        /// Requires full type-level access. For operations with no concrete instance to fall back to
        /// an ownership check against - creation, and whole-type operations.
        /// </summary>
        void EnsureTypeAccess(string? typeName, DXUnitTypeAccessOperation operation);

        /// <summary>
        /// Requires full type-level access, or ownership of the record when the type-level decision
        /// is <see cref="DXAccessDecision.AllowedOwnedOnly"/>.
        /// </summary>
        void EnsureInstanceAccess(string? typeName, Guid instanceId, DXUnitTypeAccessOperation operation);

        /// <summary>The caller, for log lines and denial messages.</summary>
        string GetCurrentSubject();
    }
}
