using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXElementGenericRepository
    {
        Guid InsertBlock(string esqlModelType, DXElement dxElement);
        Guid UpdateBlock(string esqlModelType, DXElement dxElement);
        bool DeleteBlock(DXElement dxElement);
        T GetBlock<T>(Guid id) where T : DXElement;
    }
}