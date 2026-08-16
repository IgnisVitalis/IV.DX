namespace IV.DX.Application.Contracts.Abstractions
{
    /// <summary>
    /// Write-only surface over one element type, in terms of its request DTO.
    /// </summary>
    /// <remarks>
    /// Every verb needs <c>Update</c> access on the unit that owns the element, or ownership of it.
    /// Not <c>Create</c> and not <c>Delete</c>: adding or removing an element does not bring a unit
    /// into being or end it, it changes a unit's contents - the same thing a whole-unit write with a
    /// modified element container does.
    /// <para>
    /// Ids are arguments rather than properties the DTO must carry. An element is addressed by two
    /// coordinates, both of which a nested route already supplies, so the request shape stays pure
    /// payload and no marker interface is needed on it. Anything a mapper puts in
    /// <c>DXElement.Id</c> or <c>DXElement.DXUnitId</c> is overwritten from these arguments.
    /// </para>
    /// </remarks>
    public interface IDXElementCommandService<TRequest>
    {
        /// <summary>
        /// Adds an element to a unit and returns its server-assigned id.
        /// </summary>
        Task<Guid> CreateAsync(Guid dxUnitId, TRequest dto, CancellationToken ct = default);

        /// <summary>
        /// Updates an element by its own id. Returns false when no element with that id exists. The
        /// owning unit is resolved from storage, so this cannot move the element elsewhere.
        /// </summary>
        Task<bool> UpdateAsync(Guid id, TRequest dto, CancellationToken ct = default);

        /// <summary>
        /// Updates an element of a named unit. Returns false when no such element exists under that
        /// unit, including when it exists under a different one.
        /// </summary>
        Task<bool> UpdateAsync(Guid dxUnitId, Guid id, TRequest dto, CancellationToken ct = default);

        /// <summary>
        /// Removes an element by its own id. Removing one that is not there is not an error.
        /// </summary>
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Removes an element of a named unit. Reports false when no such element exists under it.
        /// </summary>
        Task<bool> DeleteAsync(Guid dxUnitId, Guid id, CancellationToken ct = default);
    }
}
