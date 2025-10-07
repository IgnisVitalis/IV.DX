using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface ICoreModelHandler
    {
        bool IsItemExisting(string typeName, Guid id, EntityHandlerBaseContext context);

        void OnGetting(ESQLModel model, EntityHandlerBaseContext context);

        bool OnDeleting(string typeName, Guid id, EntityHandlerBaseContext context);

        void OnDeleted(string typeName, Guid id, EntityHandlerBaseContext context);

        Guid OnInserting(ESQLModel model, EntityHandlerBaseContext context);

        void OnInserted(ESQLModel model, EntityHandlerBaseContext context);

        Guid OnUpdating(ESQLModel model, EntityHandlerBaseContext context);

        void OnUpdated(ESQLModel model, EntityHandlerBaseContext context);
    }
}