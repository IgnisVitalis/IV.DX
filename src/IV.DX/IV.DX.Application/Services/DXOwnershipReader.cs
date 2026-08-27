using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Services
{
    /// <summary>
    /// Reads ownership rows and nothing else. See <see cref="IDXOwnershipReader"/> for why it stops
    /// at identifiers.
    /// </summary>
    /// <remarks>
    /// A leaf on purpose: it knows the two ownership units and the structure cache, and never calls
    /// back into <see cref="IDXUnitDataReader"/> or a data service. Those already depend on
    /// <see cref="IDXUnitAccessGate"/>, which depends on ownership - a call in the other direction
    /// would close that loop and leave it ambiguous which component narrows a read first.
    /// <para>
    /// Reached through <see cref="IDXUnitGenericRepository"/> rather than the unit reader because
    /// the ownership units are <see cref="DXObjectKindEnum.Core"/>, and the type-level checker
    /// denies core types to every non-system caller. Filtering by the identity taken from the
    /// execution context is what keeps the read safe: the value never comes from the caller.
    /// </para>
    /// </remarks>
    internal sealed class DXOwnershipReader(
        IDXUnitGenericRepository genericRepo,
        IDXStructureCache structureCache,
        IDXExecutionContextAccessor executionContextAccessor) : IDXOwnershipReader
    {
        public Task<HashSet<Guid>> GetOwnedIdsAsync<TUnit>(
            DXUnitTypeAccessOperation operation = DXUnitTypeAccessOperation.Read,
            CancellationToken ct = default) where TUnit : DXUnit, new()
            => GetOwnedIdsAsync(AttributeReader.GetDXUnitTypeName(typeof(TUnit)), operation, ct);

        public Task<HashSet<Guid>> GetOwnedIdsAsync(
            string typeName,
            DXUnitTypeAccessOperation operation = DXUnitTypeAccessOperation.Read,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var owned = new HashSet<Guid>();

            if (string.IsNullOrWhiteSpace(typeName))
                return Task.FromResult(owned);

            var unitDef = structureCache.GetDXUnit(typeName);
            if (unitDef == null || !unitDef.SupportsOwnership)
                return Task.FromResult(owned);

            // A system context carries no identity, so it owns nothing - which is right: system code
            // reads what it needs directly rather than asking whose records these are.
            var context = executionContextAccessor.Current;
            if (context == null || context.IsSystem)
                return Task.FromResult(owned);

            var denied = new HashSet<Guid>();

            if (context.IdentityId.HasValue)
            {
                var identityOwned = genericRepo.GetDXUnits<DXIdentityOwnershipUnit>(
                    $"Identity = '{context.IdentityId.Value}' AND DXUnitDefinition = '{unitDef.Id}'");

                foreach (var row in identityOwned)
                {
                    if (DXOwnershipRules.Covers(row.Read, row.Update, row.Delete, operation))
                        DXOwnershipRules.Classify(row.OwnedDXUnitId, row.Effect, owned, denied);
                }
            }

            if (context.ActiveGroupIDs != null)
            {
                foreach (var groupId in context.ActiveGroupIDs)
                {
                    ct.ThrowIfCancellationRequested();

                    var groupOwned = genericRepo.GetDXUnits<DXGroupOwnershipUnit>(
                        $"Group = '{groupId}' AND DXUnitDefinition = '{unitDef.Id}'");

                    foreach (var row in groupOwned)
                    {
                        if (DXOwnershipRules.Covers(row.Read, row.Update, row.Delete, operation))
                            DXOwnershipRules.Classify(row.OwnedDXUnitId, row.Effect, owned, denied);
                    }
                }
            }

            // Applied last so a Deny row outranks an Allow on the same record whichever came first,
            // and whichever route - identity or group - each of them arrived by.
            owned.ExceptWith(denied);

            return Task.FromResult(owned);
        }
    }
}
