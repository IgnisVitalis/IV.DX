using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXCoreHandler
    {
        bool IsItemExisting(string typeName, Guid id, DXUnitHandlerBaseContextOld context);

        void OnGetting(DXModel model, DXUnitHandlerBaseContextOld context);

        bool OnDeleting(string typeName, Guid id, DXUnitHandlerBaseContextOld context);

        void OnDeleted(string typeName, Guid id, DXUnitHandlerBaseContextOld context);

        Guid OnInserting(DXModel model, DXUnitHandlerBaseContextOld context);

        void OnInserted(DXModel model, DXUnitHandlerBaseContextOld context);

        Guid OnUpdating(DXModel model, DXUnitHandlerBaseContextOld context);

        void OnUpdated(DXModel model, DXUnitHandlerBaseContextOld context);
    }
}