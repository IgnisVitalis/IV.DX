using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
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

        public virtual bool OnDeleting(Guid id, DXUnitHandlerBaseContext context)
        {
            return this._coreModelHandler.OnDeleting(TypeName, id, context);
        }

        public virtual void OnDeleted(Guid id, DXUnitHandlerBaseContext context)
        {
            this._coreModelHandler.OnDeleted(TypeName, id, context);
        }

        public virtual Guid OnInserting(T entity, DXUnitHandlerBaseContext context)
        {
            var esqlModel = entity.ConvertToESQLModel();
            return this._coreModelHandler.OnInserting(esqlModel, context);
        }

        public virtual void OnInserted(T entity, DXUnitHandlerBaseContext context)
        {
            var esqlModel = entity.ConvertToESQLModel();
            this._coreModelHandler.OnInserted(esqlModel, context);
        }

        public virtual Guid OnUpdating(T entity, DXUnitHandlerBaseContext context)
        {
            var esqlModel = entity.ConvertToESQLModel();
            return this._coreModelHandler.OnUpdating(esqlModel, context);
        }

        public virtual void OnUpdated(T entity, DXUnitHandlerBaseContext context)
        {
            var esqlModel = entity.ConvertToESQLModel();
            this._coreModelHandler.OnUpdated(esqlModel, context);
        }

        public virtual bool IsItemExisting(Guid id, DXUnitHandlerBaseContext context)
        {
            return this._coreModelHandler.IsItemExisting(TypeName, id, context);
        }

        public void OnGetting(ESQLModel model, DXUnitHandlerBaseContext context)
        {
            this._coreModelHandler.OnGetting(model, context);
        }
    }
}