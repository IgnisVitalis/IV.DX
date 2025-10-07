using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Models;

namespace IV.DX.Contracts.Common.Helpers
{
    public interface IGenericRepository
    {
        IEnumerable<T> GetItems<T>() where T : ESQLObject;
        IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids) where T : ESQLObject;
        IEnumerable<T> GetItems<T>(string esqlWhereExpression) where T : ESQLObject;
        T GetItem<T>(Guid id) where T : ESQLObject;
        Guid Insert(ESQLObject esqlObject);
        Guid Update(ESQLObject esqlObject);
        bool Delete(ESQLObject esqlObject);
        Guid InsertOrUpdate(ESQLObject esqlObject);
        bool AddRelation(DPRelationItemObject relationItem);
        bool RemoveRelation(DPRelationItemObject relationItem);

        Guid InsertBlock(string esqlModelType, ESQLBlock esqlBlock);
        Guid UpdateBlock(string esqlModelType, ESQLBlock esqlBlock);
        bool DeleteBlock(ESQLBlock esqlBlock);
        T GetBlock<T>(Guid id) where T : ESQLBlock;
    }
}