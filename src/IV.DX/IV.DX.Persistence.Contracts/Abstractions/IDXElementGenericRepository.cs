using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXElementGenericRepository
    {
        Guid Insert(string dxModelType, DXElement dxElement);
        Guid Update(string dxModelType, DXElement dxElement);
        bool Delete(DXElement dxElement);
        T GetItem<T>(Guid id) where T : DXElement;
        IEnumerable<T> GetItems<T>(string dxFilter) where T : DXElement;
    }
}