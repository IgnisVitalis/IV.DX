using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitHandler<T> where T : DXUnit
    {
        bool IsItemExisting(Guid id, DXUnitHandlerBaseContextOld context);
        void OnGetting(DXModel model, DXUnitHandlerBaseContextOld context);
        Guid OnInserting(T entity, DXUnitHandlerBaseContextOld context);
        void OnInserted(T entity, DXUnitHandlerBaseContextOld context);
        Guid OnUpdating(T entity, DXUnitHandlerBaseContextOld context);
        void OnUpdated(T entity, DXUnitHandlerBaseContextOld context);
        bool OnDeleting(Guid id, DXUnitHandlerBaseContextOld context);
        void OnDeleted(Guid id, DXUnitHandlerBaseContextOld context);
    }
}