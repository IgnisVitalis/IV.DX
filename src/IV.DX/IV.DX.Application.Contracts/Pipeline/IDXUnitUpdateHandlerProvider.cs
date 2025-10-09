using IV.DX.Application.Contracts.Handlers;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Pipeline
{
    public interface IDXUnitUpdateHandlerProvider
    {   
        IEnumerable<IDXBeforeUpdate<T>> GetBeforeUpdateHandlers<T>() where T : DXUnit;
        IEnumerable<IDXAfterUpdate<T>> GetAfterUpdateHandlers<T>() where T : DXUnit;
        void Register<T>(IDXBeforeUpdate<T> handler) where T : DXUnit;
        void Register<T>(IDXAfterUpdate<T> handler) where T : DXUnit;
        bool TryResolveType(string typeName, out Type type);
    }
}