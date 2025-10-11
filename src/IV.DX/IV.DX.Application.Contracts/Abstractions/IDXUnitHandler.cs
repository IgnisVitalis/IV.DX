using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitHandler<T> where T : DXUnit
    {
        bool IsItemExisting(Guid id, IDXHandlerContext context);
        void OnGetting(DXModel model, IDXHandlerContext context);
        Guid OnInserting(T entity, IDXHandlerContext context);
        void OnInserted(T entity, IDXHandlerContext context);
        Guid OnUpdating(T entity, IDXHandlerContext context);
        void OnUpdated(T entity, IDXHandlerContext context);
        bool OnDeleting(Guid id, IDXHandlerContext context);
        void OnDeleted(Guid id, IDXHandlerContext context);
    }
}