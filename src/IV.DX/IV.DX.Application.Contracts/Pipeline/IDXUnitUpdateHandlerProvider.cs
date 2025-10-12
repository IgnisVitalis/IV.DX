using IV.DX.Application.Contracts.Handlers;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Pipeline
{
    public interface IDXUnitUpdateHandlerProvider
    {   
        IEnumerable<IDXBeforeUpdateHandler<T>> GetBeforeUpdateHandlers<T>() where T : DXUnit;
        IEnumerable<IDXAfterUpdateHandler<T>> GetAfterUpdateHandlers<T>() where T : DXUnit;
        void Register<T>(IDXBeforeUpdateHandler<T> handler) where T : DXUnit;
        void Register<T>(IDXAfterUpdateHandler<T> handler) where T : DXUnit;
        bool TryResolveType(string typeName, out Type type);
    }
}