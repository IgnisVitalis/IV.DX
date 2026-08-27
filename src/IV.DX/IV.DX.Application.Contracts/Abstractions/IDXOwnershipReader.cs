using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    /// <summary>
    /// Answers "which records of this type does the current principal own", and nothing else.
    /// </summary>
    /// <remarks>
    /// Deliberately returns identifiers rather than units or DTOs. Reading the records themselves
    /// means running the get pipeline so the handlers registered for the type still fire, and that
    /// is <see cref="IDXUnitDataReader"/>'s job - pass these ids to its by-ids overload. Returning
    /// models here would mean duplicating that pipeline, and returning DTOs would mean resolving a
    /// mapper, which is what <see cref="IDXUnitQueryService{TResponse}"/> already does.
    /// <para>
    /// This is not the same question the access gate asks when it narrows a read. The gate answers
    /// "what may this caller see", which includes records exposed through <c>DXPublicAccessUnit</c>
    /// and belongs to nobody. Ownership is strictly the identity's own rows plus those of the groups
    /// it is active in.
    /// </para>
    /// <para>
    /// Needed because a type declared <c>IsPublicRead</c> - or covered by a type-level read grant -
    /// never reaches the gate's ownership narrowing at all: the decision comes back
    /// <c>Allowed</c> and every record is returned. Ownership has to be asked for explicitly.
    /// </para>
    /// </remarks>
    public interface IDXOwnershipReader
    {
        /// <summary>
        /// Identifiers of <typeparamref name="TUnit"/> records the current principal owns with a
        /// grant covering <paramref name="operation"/>.
        /// </summary>
        /// <remarks>
        /// Empty rather than throwing when the caller is anonymous, when the context is a system
        /// one - a system principal holds no identity and therefore owns nothing - or when the type
        /// does not declare <c>SupportsOwnership</c>.
        /// </remarks>
        Task<HashSet<Guid>> GetOwnedIdsAsync<TUnit>(
            DXUnitTypeAccessOperation operation = DXUnitTypeAccessOperation.Read,
            CancellationToken ct = default) where TUnit : DXUnit, new();

        /// <inheritdoc cref="GetOwnedIdsAsync{TUnit}(DXUnitTypeAccessOperation, CancellationToken)"/>
        Task<HashSet<Guid>> GetOwnedIdsAsync(
            string typeName,
            DXUnitTypeAccessOperation operation = DXUnitTypeAccessOperation.Read,
            CancellationToken ct = default);
    }
}
