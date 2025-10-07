using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXGenericRepository
    {
        IEnumerable<T> GetItems<T>() where T : DXUnit;
        IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids) where T : DXUnit;
        IEnumerable<T> GetItems<T>(string esqlWhereExpression) where T : DXUnit;
        T GetItem<T>(Guid id) where T : DXUnit;
        Guid Insert(DXUnit esqlObject);
        Guid Update(DXUnit esqlObject);
        bool Delete(DXUnit esqlObject);
        Guid InsertOrUpdate(DXUnit esqlObject);
        bool AddRelation(DXRelationItemUnit relationItem);
        bool RemoveRelation(DXRelationItemUnit relationItem);

        Guid InsertBlock(string esqlModelType, DXElement esqlBlock);
        Guid UpdateBlock(string esqlModelType, DXElement esqlBlock);
        bool DeleteBlock(DXElement esqlBlock);
        T GetBlock<T>(Guid id) where T : DXElement;
    }
}