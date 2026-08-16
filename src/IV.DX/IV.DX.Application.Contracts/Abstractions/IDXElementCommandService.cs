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
    /// </remarks>
    public interface IDXElementCommandService<TRequest>
    {
        /// <summary>
        /// Adds an element to a unit and returns its server-assigned id.
        /// </summary>
        /// <remarks>
        /// The owner is an argument rather than a property of the DTO so the caller decides where it
        /// came from - a route segment for a nested resource, the body for a flat one - without every
        /// request shape having to carry it.
        /// </remarks>
        Task<Guid> CreateAsync(Guid dxUnitId, TRequest dto, CancellationToken ct = default);

        /// <summary>
        /// Updates an existing element. Returns false when no element with that id exists. The owner
        /// is resolved from storage, so the element cannot be moved to another unit through this.
        /// </summary>
        Task<bool> UpdateAsync(TRequest dto, CancellationToken ct = default);

        /// <summary>
        /// Removes an element. Removing one that is not there is not an error.
        /// </summary>
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
