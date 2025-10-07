using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXCoreRepository
    {
        void DropDataBase();
        void CreateDataBase();
        bool IsItemExisting(string typeName, Guid objectId);
        DXModel GetItem(DXModelDefinition definitionContainer, Guid id, DXLoadingType typeOfLoading);
        IEnumerable<DXModel> GetItems(string typeName);
        IEnumerable<DXModel> GetItems(string typeName, IEnumerable<Guid> objectIds);
        IEnumerable<DXModel> GetItems(string typeName, string esqlWhereExpression);
        DXModel GetItem(string typeName, Guid objectId);
        IEnumerable<DXModel> GetItems(DXModelDefinition definitionContainer, DXLoadingType typeOfLoading);
        IEnumerable<DXModel> GetItems(DXModelDefinition definitionContainer, IEnumerable<Guid> objectIds, DXLoadingType typeOfLoading);
        IEnumerable<DXModel> GetItems(DXModelDefinition definitionContainer, string esqlWhereExpression, DXLoadingType typeOfLoading);
        Guid Insert(DXModel model);
        Guid Update(DXModel model);
        Guid InsertOrUpdate(DXModel model);
        bool Delete(string typeName, Guid objectId);
        IEnumerable<Guid> GetRelations(string leftObjectTypeName, Guid leftObjectId, string rightRelationName);
        Guid? GetRelation(string leftObjectTypeName, Guid leftObjectId, string rightRelationName);
        bool AddRelation(string leftObjectTypeName, Guid leftObjectId, string rightRelationName, string rightObjectTypeName, Guid rightObjectId);
        bool RemoveRelation(string leftObjectTypeName, Guid leftObjectId, string rightRelationName, string rightObjectTypeName, Guid rightObjectId);


        Guid InsertSingleBlock(string esqlModelType, DXSingleItem esqlSingleBlock);
        Guid UpdateSingleBlock(string esqlModelType, DXSingleItem esqlSingleBlock);
        Guid InsertOrUpdateSingleBlock(string esqlModelType, DXSingleItem esqlSingleBlock);
        bool DeleteSingleBlock(string typeName, Guid objectId);
        DXSingleItem GetSingleBlock(DXElementDefinition container, Guid id);
    }
}