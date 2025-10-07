using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXCoreHandler
    {
        bool IsItemExisting(string typeName, Guid id, DXUnitHandlerBaseContext context);

        void OnGetting(ESQLModel model, DXUnitHandlerBaseContext context);

        bool OnDeleting(string typeName, Guid id, DXUnitHandlerBaseContext context);

        void OnDeleted(string typeName, Guid id, DXUnitHandlerBaseContext context);

        Guid OnInserting(ESQLModel model, DXUnitHandlerBaseContext context);

        void OnInserted(ESQLModel model, DXUnitHandlerBaseContext context);

        Guid OnUpdating(ESQLModel model, DXUnitHandlerBaseContext context);

        void OnUpdated(ESQLModel model, DXUnitHandlerBaseContext context);
    }
}