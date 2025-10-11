using IV.DX.Application.Contracts.Handlers;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Pipeline
{
    public interface IDXUnitGetHandlerProvider
    {
        IEnumerable<IDXIsItemExisting<T>> GetIsItemExistingHandlers<T>() where T : DXUnit;
        IEnumerable<IDXBeforeGet<T>> GetBeforeGetHandlers<T>() where T : DXUnit;
        IEnumerable<IDXAfterGet<T>> GetAfterGetHandlers<T>() where T : DXUnit;
        void Register<T>(IDXBeforeGet<T> handler) where T : DXUnit;
        void Register<T>(IDXAfterGet<T> handler) where T : DXUnit;
        void Register<T>(IDXIsItemExisting<T> handler) where T : DXUnit;
        bool TryResolveType(string typeName, out Type type);
    }
}