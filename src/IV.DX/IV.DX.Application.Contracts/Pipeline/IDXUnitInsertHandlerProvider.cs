using IV.DX.Application.Contracts.Handlers;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Pipeline
{
    public interface IDXUnitInsertHandlerProvider
    {
        IEnumerable<IDXBeforeInsert<T>> GetBeforeInsertHandlers<T>() where T : DXUnit;
        IEnumerable<IDXAfterInsert<T>> GetAfterInsertHandlers<T>() where T : DXUnit;
        void Register<T>(IDXBeforeInsert<T> handler) where T : DXUnit;
        void Register<T>(IDXAfterInsert<T> handler) where T : DXUnit;
        bool TryResolveType(string typeName, out Type type);
    }
}
