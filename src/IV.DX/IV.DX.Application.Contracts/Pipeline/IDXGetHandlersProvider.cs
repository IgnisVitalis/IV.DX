using IV.DX.Application.Contracts.Handlers;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Pipeline
{
    internal interface IDXGetHandlersProvider
    {
        IEnumerable<IDXBeforeGet<T>> GetBefore<T>() where T : DXUnit;
        IEnumerable<IDXAfterGet<T>> GetAfter<T>() where T : DXUnit;

        IEnumerable<IDXBeforeGet<T>> GetBefore<T>(string typeName) where T : DXUnit;
        IEnumerable<IDXAfterGet<T>> GetAfter<T>(string typeName) where T : DXUnit;
    }
}
