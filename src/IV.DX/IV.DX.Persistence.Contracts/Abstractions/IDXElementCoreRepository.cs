using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXElementCoreRepository
    {
        Guid InsertOrUpdate(DXDataBlock<DXElementRecord> block);

        /// <summary>
        /// Deletes element rows by their own ids, in one transaction. Reports whether anything was
        /// deleted.
        /// </summary>
        bool Delete(string typeName, IEnumerable<Guid> ids);

        DXElementRecord? GetItemRecord(DXTableDefinition container, Guid id);

        /// <summary>
        /// The unit a stored element belongs to, or <see cref="Guid.Empty"/> when it is not there.
        /// </summary>
        Guid GetOwnerDXUnitId(string dxUnitTypeName, string elementTypeName, Guid id);

        /// <summary>Element rows owned by the given units.</summary>
        IEnumerable<DXElementRecord> GetItemsRecordByUnits(DXTableDefinition container, IEnumerable<Guid> dxUnitIds);

        /// <summary>
        /// Element rows of every unit matching <paramref name="dxFilter"/>. The filter is evaluated
        /// against the unit table.
        /// </summary>
        IEnumerable<DXElementRecord> GetItemsRecord(DXTableDefinition container, string dxFilter);
    }
}
