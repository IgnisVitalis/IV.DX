using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXCoreHandler
    {
        bool IsItemExisting(string typeName, Guid id, IDXHandlerContext context);

        void OnGetting(DXModel model, IDXHandlerContext context);

        bool OnDeleting(string typeName, Guid id, IDXHandlerContext context);

        void OnDeleted(string typeName, Guid id, IDXHandlerContext context);

        Guid OnInserting(DXModel model, IDXHandlerContext context);

        void OnInserted(DXModel model, IDXHandlerContext context);

        Guid OnUpdating(DXModel model, IDXHandlerContext context);

        void OnUpdated(DXModel model, IDXHandlerContext context);
    }
}