namespace IV.DX.Application.Contracts.Abstractions
{
    /// <summary>
    /// Read-only surface over one element type, in terms of its response DTO. Every call needs
    /// <c>Read</c> access on the owning unit type, and is narrowed to the units the caller may see.
    /// </summary>
    public interface IDXElementQueryService<TResponse>
    {
        /// <summary>
        /// One element by its own id, or <c>default</c> when it does not exist. Use when the element
        /// is addressed on its own, without naming the unit it belongs to.
        /// </summary>
        Task<TResponse?> GetAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// One element of a named unit. An element belonging to a different unit reads as
        /// <c>default</c>, so a nested address never resolves to something living elsewhere.
        /// </summary>
        Task<TResponse?> GetAsync(Guid dxUnitId, Guid id, CancellationToken ct = default);

        /// <summary>Every element of this type belonging to one unit.</summary>
        Task<IEnumerable<TResponse>> GetByUnitAsync(Guid dxUnitId, CancellationToken ct = default);
    }
}
