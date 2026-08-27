using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Services
{
    /// <summary>
    /// How an ownership row resolves. Shared by <see cref="DXUnitAccessGate"/>, which enforces
    /// access, and <see cref="DXOwnershipReader"/>, which reports it.
    /// </summary>
    /// <remarks>
    /// Kept in one place for the same reason the gate itself is: two components answering the same
    /// question from two copies of the rules will drift, and the newer one then becomes a way
    /// around the older one.
    /// </remarks>
    internal static class DXOwnershipRules
    {
        /// <summary>
        /// Whether an ownership row's operation flags cover <paramref name="operation"/>.
        /// </summary>
        public static bool Covers(bool read, bool update, bool delete, DXUnitTypeAccessOperation operation) => operation switch
        {
            DXUnitTypeAccessOperation.Read => read,
            DXUnitTypeAccessOperation.Update => update,
            DXUnitTypeAccessOperation.Delete => delete,
            // Ownership is a grant over a record that already exists; it never authorises creation.
            _ => false
        };

        /// <summary>
        /// Sorts one row's target into the allowed or denied set. Callers subtract the denied set at
        /// the end rather than as they go, so a Deny outranks an Allow regardless of the order the
        /// rows were read in.
        /// </summary>
        public static void Classify(Guid ownedId, DXGrantEffectEnum effect, HashSet<Guid> allowed, HashSet<Guid> denied)
        {
            if (effect == DXGrantEffectEnum.Deny)
                denied.Add(ownedId);
            else if (effect == DXGrantEffectEnum.Allow)
                allowed.Add(ownedId);
        }
    }
}
