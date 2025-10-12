using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXElementGenericRepository
    {
        Guid InsertDXElement(string dxModelType, DXElement dxElement);
        Guid UpdateDXElement(string dxModelType, DXElement dxElement);
        bool DeleteDXElement(DXElement dxElement);
        T GetDXElement<T>(Guid id) where T : DXElement;
    }
}