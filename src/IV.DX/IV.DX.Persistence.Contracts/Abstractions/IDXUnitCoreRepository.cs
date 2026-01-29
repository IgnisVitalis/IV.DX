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
        DXModel? GetItem(DXDataSetDefinition dxModelDefinition, Guid id, DXLoadingType typeOfLoading);
        IEnumerable<DXModel> GetItems(string typeName);
        IEnumerable<DXModel> GetItems(string typeName, IEnumerable<Guid> objectIds);
        IEnumerable<DXModel> GetItems(string typeName, string dxFilter);
        DXModel GetItem(string typeName, Guid objectId);
        IEnumerable<DXModel> GetItems(DXDataSetDefinition dxModelDefinition, DXLoadingType typeOfLoading);
        IEnumerable<DXModel> GetItems(DXDataSetDefinition dxModelDefinition, IEnumerable<Guid> objectIds, DXLoadingType typeOfLoading);
        IEnumerable<DXModel> GetItems(DXDataSetDefinition dxModelDefinition, string dxFilter, DXLoadingType typeOfLoading);
        Guid Insert(DXModel dxModel);
        Guid Update(DXModel dxModel);
        Guid InsertOrUpdate(DXModel dxModel);
        Guid InsertOrUpdate(DXDataBlock<DXUnitRecord> block);
        bool Delete(string typeName, Guid objectId);
        IEnumerable<Guid> GetRelations(string leftObjectTypeName, Guid leftObjectId, string rightRelationName);
        Guid? GetRelation(string leftObjectTypeName, Guid leftObjectId, string rightRelationName);
        bool AddRelation(string leftObjectTypeName, Guid leftObjectId, string rightRelationName, string rightObjectTypeName, Guid rightObjectId);
        bool RemoveRelation(string leftObjectTypeName, Guid leftObjectId, string rightRelationName, string rightObjectTypeName, Guid rightObjectId);
    }
}
