using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXEnumCoreRepository
    {
        bool IsItemExisting(string typeName, Guid objectId);
        DXDataBlock<DXEnumRecord> GetItemsRecord(string enumType);
        DXDataBlock<DXEnumRecord>? GetItemRecord(string typeName, Guid objectId);
        Guid InsertOrUpdate(DXDataBlock<DXEnumRecord> block);
        bool Delete(string typeName, Guid objectId);
    }
}
