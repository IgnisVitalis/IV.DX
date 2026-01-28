using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXElementGenericRepository
    {
        Guid Insert(string dxUnitTypeName, DXElement dxElement);
        Guid Update(string dxUnitTypeName, DXElement dxElement);
        bool Delete(DXElement dxElement);
        T GetItem<T>(string dxUnitTypeName, Guid id) where T : DXElement;
        IEnumerable<T> GetItems<T>(string dxUnitTypeName, string dxFilter) where T : DXElement;
    }
}