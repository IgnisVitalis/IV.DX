using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXUnitCoreRepository
    {
        IEnumerable<Guid> GetItemIDs(string typeName, string? dxFilter = default);
        void DropDataBase();
        void CreateDataBase();
        bool IsItemExisting(string typeName, Guid objectId);
        DXDataBlock<DXUnitRecord>? GetItemRecord(DXDataSetDefinition dxModelDefinition, Guid id, DXLoadingType typeOfLoading);
        DXDataBlock<DXUnitRecord>? GetItemRecord(string typeName, Guid objectId);
        DXDataBlock<DXUnitRecord> GetItemsRecord(string typeName);
        DXDataBlock<DXUnitRecord> GetItemsRecord(string typeName, IEnumerable<Guid> objectIds);
        DXDataBlock<DXUnitRecord> GetItemsRecord(string typeName, string dxFilter);
        DXDataBlock<DXUnitRecord> GetItemsRecord(DXDataSetDefinition dxModelDefinition, DXLoadingType typeOfLoading);
        DXDataBlock<DXUnitRecord> GetItemsRecord(DXDataSetDefinition dxModelDefinition, IEnumerable<Guid> objectIds, DXLoadingType typeOfLoading);
        DXDataBlock<DXUnitRecord> GetItemsRecord(DXDataSetDefinition dxModelDefinition, string dxFilter, DXLoadingType typeOfLoading);
        Guid InsertOrUpdate(DXDataBlock<DXUnitRecord> block);
        bool Delete(string typeName, Guid objectId);
        IEnumerable<Guid> GetRelations(string leftObjectTypeName, Guid leftObjectId, string rightRelationName);
        Guid? GetRelation(string leftObjectTypeName, Guid leftObjectId, string rightRelationName);
        bool AddRelation(string leftObjectTypeName, Guid leftObjectId, string rightRelationName, string rightObjectTypeName, Guid rightObjectId);
        bool RemoveRelation(string leftObjectTypeName, Guid leftObjectId, string rightRelationName, string rightObjectTypeName, Guid rightObjectId);
    }
}
