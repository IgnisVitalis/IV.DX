using IV.DX.Application.Contracts.Handlers;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Pipeline
{
    public interface IDXUnitInsertHandlerProvider
    {
        IEnumerable<IDXBeforeInsertHandler<T>> GetBeforeInsertHandlers<T>() where T : DXUnit;
        IEnumerable<IDXAfterInsertHandler<T>> GetAfterInsertHandlers<T>() where T : DXUnit;
        void Register<T>(IDXBeforeInsertHandler<T> handler) where T : DXUnit;
        void Register<T>(IDXAfterInsertHandler<T> handler) where T : DXUnit;
        bool TryResolveType(string typeName, out Type type);
    }
}
