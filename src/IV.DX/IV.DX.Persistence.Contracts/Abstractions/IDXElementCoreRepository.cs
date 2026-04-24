using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXElementCoreRepository
    {
        Guid InsertOrUpdate(DXDataBlock<DXElementRecord> block);
        bool Delete(string typeName, Guid dxUnitId);
        DXElementRecord? GetItemRecord(DXTableDefinition container, Guid id);
        IEnumerable<DXElementRecord> GetItemsRecord(DXTableDefinition container, string dxFilter);
    }
}
