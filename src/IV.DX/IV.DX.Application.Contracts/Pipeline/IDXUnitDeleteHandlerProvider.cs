using IV.DX.Application.Contracts.Handlers;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Pipeline
{
    public interface IDXUnitDeleteHandlerProvider
    {   
        IEnumerable<IDXBeforeDelete<T>> GetBeforeDeleteHandlers<T>() where T : DXUnit;
        IEnumerable<IDXAfterDelete<T>> GetAfterDeleteHandlers<T>() where T : DXUnit;
        void Register<T>(IDXBeforeDelete<T> handler) where T : DXUnit;
        void Register<T>(IDXAfterDelete<T> handler) where T : DXUnit;
        bool TryResolveType(string typeName, out Type type);
    }
}