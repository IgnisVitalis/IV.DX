using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitHandler<T> where T : ESQLObject
    {
        bool IsItemExisting(Guid id, DXUnitHandlerBaseContext context);
        void OnGetting(ESQLModel model, DXUnitHandlerBaseContext context);
        Guid OnInserting(T entity, DXUnitHandlerBaseContext context);
        void OnInserted(T entity, DXUnitHandlerBaseContext context);
        Guid OnUpdating(T entity, DXUnitHandlerBaseContext context);
        void OnUpdated(T entity, DXUnitHandlerBaseContext context);
        bool OnDeleting(Guid id, DXUnitHandlerBaseContext context);
        void OnDeleted(Guid id, DXUnitHandlerBaseContext context);
    }
}