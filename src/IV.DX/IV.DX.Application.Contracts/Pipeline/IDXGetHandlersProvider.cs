using IV.DX.Application.Contracts.Handlers;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Pipeline
{
    internal interface IDXGetHandlersProvider
    {
        IEnumerable<IDXBeforeGetHandler<T>> GetBefore<T>() where T : DXUnit;
        IEnumerable<IDXAfterGetHadnler<T>> GetAfter<T>() where T : DXUnit;

        IEnumerable<IDXBeforeGetHandler<T>> GetBefore<T>(string typeName) where T : DXUnit;
        IEnumerable<IDXAfterGetHadnler<T>> GetAfter<T>(string typeName) where T : DXUnit;
    }
}
