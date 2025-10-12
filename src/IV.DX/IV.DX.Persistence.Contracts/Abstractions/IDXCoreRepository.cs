using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXCoreRepository
    {
        IEnumerable<Guid> GetItemIDs(string typeName, string? dxsqlWhereExpression = default);
        void DropDataBase();
        void CreateDataBase();
        bool IsItemExisting(string typeName, Guid objectId);
        DXModel GetItem(DXModelDefinition dxModelDefinition, Guid id, DXLoadingType typeOfLoading);
        IEnumerable<DXModel> GetItems(string typeName);
        IEnumerable<DXModel> GetItems(string typeName, IEnumerable<Guid> objectIds);
        IEnumerable<DXModel> GetItems(string typeName, string dxsqlWhereExpression);
        DXModel GetItem(string typeName, Guid objectId);
        IEnumerable<DXModel> GetItems(DXModelDefinition dxModelDefinition, DXLoadingType typeOfLoading);
        IEnumerable<DXModel> GetItems(DXModelDefinition dxModelDefinition, IEnumerable<Guid> objectIds, DXLoadingType typeOfLoading);
        IEnumerable<DXModel> GetItems(DXModelDefinition dxModelDefinition, string dxsqlWhereExpression, DXLoadingType typeOfLoading);
        Guid Insert(DXModel dxModel);
        Guid Update(DXModel dxModel);
        Guid InsertOrUpdate(DXModel dxModel);
        bool Delete(string typeName, Guid objectId);
        IEnumerable<Guid> GetRelations(string leftObjectTypeName, Guid leftObjectId, string rightRelationName);
        Guid? GetRelation(string leftObjectTypeName, Guid leftObjectId, string rightRelationName);
        bool AddRelation(string leftObjectTypeName, Guid leftObjectId, string rightRelationName, string rightObjectTypeName, Guid rightObjectId);
        bool RemoveRelation(string leftObjectTypeName, Guid leftObjectId, string rightRelationName, string rightObjectTypeName, Guid rightObjectId);
        Guid InsertSingleBlock(string esqlModelType, DXSingleElement esqlSingleBlock);
        Guid UpdateSingleBlock(string esqlModelType, DXSingleElement esqlSingleBlock);
        Guid InsertOrUpdateSingleBlock(string esqlModelType, DXSingleElement esqlSingleBlock);
        bool DeleteSingleBlock(string typeName, Guid objectId);
        DXSingleElement GetSingleBlock(DXElementDefinition container, Guid id);
    }
}