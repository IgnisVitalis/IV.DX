using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    internal class BaseEntityHandler<T> : IDXUnitHandler<T> where T : DXUnit
    {
        private readonly IDXCoreHandler _coreModelHandler;

        protected readonly string TypeName = AttributeReader.GetESQLObjectTypeName(typeof(T));

        protected void ThrowNotSupportedExceptionForOnUpdatingMethod()
        {
            throw new Exception($"OnUpdating method isn't supported for '{AttributeReader.GetESQLObjectTypeName(typeof(T))}'");
        }

        public BaseEntityHandler(IServiceProvider serviceProvider)
        {
            this._coreModelHandler = serviceProvider.GetService<IDXCoreHandler>();
        }

        public virtual bool OnDeleting(Guid id, IDXHandlerContext context)
        {
            return this._coreModelHandler.OnDeleting(TypeName, id, context);
        }

        public virtual void OnDeleted(Guid id, IDXHandlerContext context)
        {
            this._coreModelHandler.OnDeleted(TypeName, id, context);
        }

        public virtual Guid OnInserting(T entity, IDXHandlerContext context)
        {
            var esqlModel = entity.ConvertToESQLModel();
            return this._coreModelHandler.OnInserting(esqlModel, context);
        }

        public virtual void OnInserted(T entity, IDXHandlerContext context)
        {
            var esqlModel = entity.ConvertToESQLModel();
            this._coreModelHandler.OnInserted(esqlModel, context);
        }

        public virtual Guid OnUpdating(T entity, IDXHandlerContext context)
        {
            var esqlModel = entity.ConvertToESQLModel();
            return this._coreModelHandler.OnUpdating(esqlModel, context);
        }

        public virtual void OnUpdated(T entity, IDXHandlerContext context)
        {
            var esqlModel = entity.ConvertToESQLModel();
            this._coreModelHandler.OnUpdated(esqlModel, context);
        }

        public virtual bool IsItemExisting(Guid id, IDXHandlerContext context)
        {
            return this._coreModelHandler.IsItemExisting(TypeName, id, context);
        }

        public void OnGetting(DXModel model, IDXHandlerContext context)
        {
            this._coreModelHandler.OnGetting(model, context);
        }
    }
}