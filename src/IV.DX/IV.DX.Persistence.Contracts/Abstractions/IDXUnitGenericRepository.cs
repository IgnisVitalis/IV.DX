using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXUnitGenericRepository
    {
        IEnumerable<T> GetDXUnits<T>() where T : DXUnit;
        IEnumerable<T> GetDXUnits<T>(IEnumerable<Guid> ids) where T : DXUnit;
        IEnumerable<T> GetDXUnits<T>(string dxFilter) where T : DXUnit;
        T GetDXUnit<T>(Guid id) where T : DXUnit;
        Guid Insert(DXUnit dxUnit);
        Guid Update(DXUnit dxUnit);
        bool Delete(DXUnit dxUnit);
        Guid InsertOrUpdate(DXUnit dxUnit);
        bool AddDXRelation(DXRelationItemUnit relationItem);
        bool RemoveDXRelation(DXRelationItemUnit relationItem);       
    }
}