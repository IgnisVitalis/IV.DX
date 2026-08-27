using IV.DX.Kernel.Enums;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitQueryService<TResponse>
    {
        Task<TResponse?> GetAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<TResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<TResponse>> GetAsync(string filter, CancellationToken ct = default);

        /// <summary>
        /// Only the records the current principal owns, mapped through the same read mapper as
        /// every other method here.
        /// </summary>
        /// <remarks>
        /// Not the same thing as <see cref="GetAllAsync"/> narrowing itself. A type declared
        /// <c>IsPublicRead</c>, or covered by a type-level read grant, is read in full - ownership
        /// never enters that decision. This asks for it explicitly, which is what an "edit what I
        /// authored" screen needs.
        /// <para>
        /// <paramref name="operation"/> selects which ownership rows count. The default answers
        /// "records I see as their owner"; pass <see cref="DXUnitTypeAccessOperation.Update"/> for
        /// "records I may actually edit", which differs once a record has co-owners holding
        /// narrower rows.
        /// </para>
        /// </remarks>
        Task<IEnumerable<TResponse>> GetOwnedAsync(
            DXUnitTypeAccessOperation operation = DXUnitTypeAccessOperation.Read,
            CancellationToken ct = default);
    }
}
