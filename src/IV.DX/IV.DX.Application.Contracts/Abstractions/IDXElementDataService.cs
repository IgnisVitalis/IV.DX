using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    /// <summary>
    /// Reads and writes single DXElements without going through the unit that owns them.
    /// </summary>
    /// <remarks>
    /// Editing one element of a unit through <see cref="IDXUnitDataService"/> means reading the whole
    /// unit, changing a part of it and writing it all back - which needs Read access on top of
    /// Update, carries every untouched field along for the ride, and lets a second caller's edit be
    /// overwritten by a stale copy. This service touches only the element's own row.
    /// <para>
    /// Access is decided against the <em>owning unit type</em>, since an element has no grants of its
    /// own: reading one needs Read on the unit, and creating, changing or removing one needs Update
    /// on the specific unit that owns it. Update rather than Create or Delete, because none of these
    /// bring a unit into being or end it - they all amount to changing a unit's contents, which is
    /// exactly what a whole-unit write with a modified element container already does.
    /// </para>
    /// <para>
    /// Writes do not move the owning unit's <c>TimeStamp</c>: the unit's own row is not touched. A
    /// client that caches a unit together with its elements therefore cannot tell from the unit alone
    /// that an element changed, and should track the element timestamps it cares about.
    /// </para>
    /// <para>
    /// No handler pipeline runs here yet - unlike the unit services, which execute before and after
    /// handlers around every operation. Handlers registered for a unit will not see an element
    /// written through this service.
    /// </para>
    /// </remarks>
    public interface IDXElementDataService
    {
        /// <summary>
        /// One element by its own id, or <c>null</c> when it does not exist or the caller may not
        /// read the unit that owns it.
        /// </summary>
        Task<T?> GetItemAsync<T>(string dxUnitTypeName, Guid id, CancellationToken ct = default) where T : DXElement, new();

        /// <summary>
        /// Every element of the given type belonging to one unit. Empty when the caller may not read
        /// that unit.
        /// </summary>
        Task<IEnumerable<T>> GetItemsByUnitAsync<T>(string dxUnitTypeName, Guid dxUnitId, CancellationToken ct = default) where T : DXElement, new();

        /// <summary>
        /// Elements of every unit matching <paramref name="dxFilter"/>.
        /// </summary>
        /// <remarks>
        /// The filter is evaluated against the <em>unit</em> table, not the element table - it
        /// selects which units to take elements from. It is also concatenated into the WHERE clause
        /// rather than parameterised, so it must never be built from caller input; the overloads
        /// taking ids are the safe way to ask for a known unit.
        /// </remarks>
        Task<IEnumerable<T>> GetItemsByUnitFilterAsync<T>(string dxUnitTypeName, string dxFilter, CancellationToken ct = default) where T : DXElement, new();

        /// <summary>
        /// Adds an element to a unit and returns its server-assigned id. An id on
        /// <paramref name="dxElement"/> is ignored; <c>DXUnitId</c> is required and must name a unit
        /// that exists.
        /// </summary>
        Task<Guid> InsertAsync<T>(string dxUnitTypeName, T dxElement, CancellationToken ct = default) where T : DXElement;

        /// <summary>
        /// Updates an existing element and returns its id, or <see cref="Guid.Empty"/> when no
        /// element with that id exists. Access is checked before existence.
        /// </summary>
        Task<Guid> UpdateAsync<T>(string dxUnitTypeName, T dxElement, CancellationToken ct = default) where T : DXElement;

        /// <summary>
        /// Writes one element, inserting it when its id is unknown and updating it otherwise.
        /// </summary>
        Task<Guid> InsertOrUpdateAsync<T>(string dxUnitTypeName, T dxElement, CancellationToken ct = default) where T : DXElement;

        /// <summary>
        /// Writes a block of elements in one transaction. <c>Meta.DXUnitContext</c> names the owning
        /// unit type and <c>Meta.Type</c> the element type; both are required.
        /// </summary>
        /// <returns>The ids written, in block order.</returns>
        Task<IEnumerable<Guid>> InsertOrUpdateAsync(DXDataBlock<DXElementRecord> block, CancellationToken ct = default);

        /// <summary>
        /// Removes the elements listed in the block's <c>Data.Delete</c>, in one transaction.
        /// Removing an element that is not there is not an error.
        /// </summary>
        Task<bool> DeleteAsync(DXDataBlock<DXElementRecord> block, CancellationToken ct = default);
    }
}
