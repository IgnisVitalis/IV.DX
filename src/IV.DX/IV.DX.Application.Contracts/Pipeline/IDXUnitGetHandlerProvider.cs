using IV.DX.Application.Contracts.Handlers;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Pipeline
{
    public interface IDXUnitGetHandlerProvider
    {
        IEnumerable<IDXIsItemExistingHandler<T>> GetIsItemExistingHandlers<T>() where T : DXUnit;
        IEnumerable<IDXBeforeGetHandler<T>> GetBeforeGetHandlers<T>() where T : DXUnit;
        IEnumerable<IDXAfterGetHadnler<T>> GetAfterGetHandlers<T>() where T : DXUnit;
        void Register<T>(IDXBeforeGetHandler<T> handler) where T : DXUnit;
        void Register<T>(IDXAfterGetHadnler<T> handler) where T : DXUnit;
        void Register<T>(IDXIsItemExistingHandler<T> handler) where T : DXUnit;
        bool TryResolveType(string typeName, out Type type);
    }
}