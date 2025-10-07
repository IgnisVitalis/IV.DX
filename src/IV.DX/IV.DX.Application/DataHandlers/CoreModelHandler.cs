using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    public sealed class CoreModelHandler : ICoreModelHandler
    {
        private readonly ICoreRepository _coreRepo;
        private readonly IGenericRepository _genericRepo;

        public CoreModelHandler(IServiceProvider serviceProvider)
        {
            this._coreRepo = serviceProvider.GetService<ICoreRepository>();
            this._genericRepo = serviceProvider.GetService<IGenericRepository>();
        }

        public bool OnDeleting(string typeName, Guid id, EntityHandlerBaseContext context)
        {
            return this._coreRepo.Delete(typeName, id);
        }

        public void OnDeleted(string typeName, Guid id, EntityHandlerBaseContext context)
        {
        }

        public Guid OnInserting(ESQLModel model, EntityHandlerBaseContext context)
        {
            return this._coreRepo.Insert(model);
        }

        public void OnInserted(ESQLModel model, EntityHandlerBaseContext context)
        {
        }

        public Guid OnUpdating(ESQLModel model, EntityHandlerBaseContext context)
        {
            return this._coreRepo.Update(model);
        }

        public void OnUpdated(ESQLModel model, EntityHandlerBaseContext context)
        {
        }

        public T GetItem<T>(Guid id, EntityHandlerBaseContext context) where T : ESQLObject
        {
            return this._genericRepo.GetItem<T>(id);
        }

        public bool IsItemExisting(string typeName, Guid id, EntityHandlerBaseContext context)
        {
            return this._coreRepo.IsItemExisting(typeName, id);
        }

        public void OnGetting(ESQLModel model, EntityHandlerBaseContext context)
        {
        }
    }
}