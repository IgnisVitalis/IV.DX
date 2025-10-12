using IV.DX.Application.Contracts.Handlers;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Pipeline
{
    public interface IDXUnitDeleteHandlerProvider
    {   
        IEnumerable<IDXBeforeDeleteHandler<T>> GetBeforeDeleteHandlers<T>() where T : DXUnit;
        IEnumerable<IDXAfterDeleteHandler<T>> GetAfterDeleteHandlers<T>() where T : DXUnit;
        void Register<T>(IDXBeforeDeleteHandler<T> handler) where T : DXUnit;
        void Register<T>(IDXAfterDeleteHandler<T> handler) where T : DXUnit;
        bool TryResolveType(string typeName, out Type type);
    }
}