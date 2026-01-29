using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXEnumCoreRepository
    {
        bool IsItemExisting(string typeName, Guid objectId);
        IEnumerable<DXModel> GetItems(string enumType);
        DXModel? GetItem(string typeName, Guid objectId);
        Guid Insert(DXModel dxModel);
        Guid Update(DXModel dxModel);
        Guid InsertOrUpdate(DXModel dxModel);
        Guid InsertOrUpdate(DXDataBlock<DXEnumRecord> block);
        bool Delete(string typeName, Guid objectId);
    }
}
