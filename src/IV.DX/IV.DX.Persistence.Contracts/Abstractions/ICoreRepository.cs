using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    public interface ICoreRepository
    {
        void DropDataBase();
        void CreateDataBase();
        bool IsItemExisting(string typeName, Guid objectId);
        ESQLModel GetItem(ESQLModelDefinition definitionContainer, Guid id, TypeOfEntityLoading typeOfLoading);
        IEnumerable<ESQLModel> GetItems(string typeName);
        IEnumerable<ESQLModel> GetItems(string typeName, IEnumerable<Guid> objectIds);
        IEnumerable<ESQLModel> GetItems(string typeName, string esqlWhereExpression);
        ESQLModel GetItem(string typeName, Guid objectId);
        IEnumerable<ESQLModel> GetItems(ESQLModelDefinition definitionContainer, TypeOfEntityLoading typeOfLoading);
        IEnumerable<ESQLModel> GetItems(ESQLModelDefinition definitionContainer, IEnumerable<Guid> objectIds, TypeOfEntityLoading typeOfLoading);
        IEnumerable<ESQLModel> GetItems(ESQLModelDefinition definitionContainer, string esqlWhereExpression, TypeOfEntityLoading typeOfLoading);
        Guid Insert(ESQLModel model);
        Guid Update(ESQLModel model);
        Guid InsertOrUpdate(ESQLModel model);
        bool Delete(string typeName, Guid objectId);
        IEnumerable<Guid> GetRelations(string leftObjectTypeName, Guid leftObjectId, string rightRelationName);
        Guid? GetRelation(string leftObjectTypeName, Guid leftObjectId, string rightRelationName);
        bool AddRelation(string leftObjectTypeName, Guid leftObjectId, string rightRelationName, string rightObjectTypeName, Guid rightObjectId);
        bool RemoveRelation(string leftObjectTypeName, Guid leftObjectId, string rightRelationName, string rightObjectTypeName, Guid rightObjectId);


        Guid InsertSingleBlock(string esqlModelType, ESQLSingleItem esqlSingleBlock);
        Guid UpdateSingleBlock(string esqlModelType, ESQLSingleItem esqlSingleBlock);
        Guid InsertOrUpdateSingleBlock(string esqlModelType, ESQLSingleItem esqlSingleBlock);
        bool DeleteSingleBlock(string typeName, Guid objectId);
        ESQLSingleItem GetSingleBlock(ESQLBlockDefinition container, Guid id);
    }
}