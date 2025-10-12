using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXGenericRepository
    {
        IEnumerable<T> GetItems<T>() where T : DXUnit;
        IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids) where T : DXUnit;
        IEnumerable<T> GetItems<T>(string dxsqlWhereExpression) where T : DXUnit;
        T GetItem<T>(Guid id) where T : DXUnit;
        Guid Insert(DXUnit dxUnit);
        Guid Update(DXUnit dxUnit);
        bool Delete(DXUnit dxUnit);
        Guid InsertOrUpdate(DXUnit dxUnit);
        bool AddRelation(DXRelationItemUnit relationItem);
        bool RemoveRelation(DXRelationItemUnit relationItem);

        Guid InsertBlock(string esqlModelType, DXElement dxElement);
        Guid UpdateBlock(string esqlModelType, DXElement dxElement);
        bool DeleteBlock(DXElement dxElement);
        T GetBlock<T>(Guid id) where T : DXElement;
    }
}