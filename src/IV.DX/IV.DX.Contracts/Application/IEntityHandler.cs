using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Models;

namespace IV.DX.Contracts.Application
{
    public interface IEntityHandler<T> where T : ESQLObject
    {
        bool IsItemExisting(Guid id, EntityHandlerBaseContext context);
        void OnGetting(ESQLModel model, EntityHandlerBaseContext context);
        Guid OnInserting(T entity, EntityHandlerBaseContext context);
        void OnInserted(T entity, EntityHandlerBaseContext context);
        Guid OnUpdating(T entity, EntityHandlerBaseContext context);
        void OnUpdated(T entity, EntityHandlerBaseContext context);
        bool OnDeleting(Guid id, EntityHandlerBaseContext context);
        void OnDeleted(Guid id, EntityHandlerBaseContext context);
    }
}