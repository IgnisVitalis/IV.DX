using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXCoreHandler
    {
        bool IsItemExisting(string typeName, Guid id, DXUnitHandlerBaseContext context);

        void OnGetting(DXModel model, DXUnitHandlerBaseContext context);

        bool OnDeleting(string typeName, Guid id, DXUnitHandlerBaseContext context);

        void OnDeleted(string typeName, Guid id, DXUnitHandlerBaseContext context);

        Guid OnInserting(DXModel model, DXUnitHandlerBaseContext context);

        void OnInserted(DXModel model, DXUnitHandlerBaseContext context);

        Guid OnUpdating(DXModel model, DXUnitHandlerBaseContext context);

        void OnUpdated(DXModel model, DXUnitHandlerBaseContext context);
    }
}