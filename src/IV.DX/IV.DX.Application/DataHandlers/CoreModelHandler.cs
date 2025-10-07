using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    internal sealed class CoreModelHandler : IDXCoreHandler
    {
        private readonly IDXCoreRepository _coreRepo;
        private readonly IDXGenericRepository _genericRepo;

        public CoreModelHandler(IServiceProvider serviceProvider)
        {
            this._coreRepo = serviceProvider.GetService<IDXCoreRepository>();
            this._genericRepo = serviceProvider.GetService<IDXGenericRepository>();
        }

        public bool OnDeleting(string typeName, Guid id, DXUnitHandlerBaseContext context)
        {
            return this._coreRepo.Delete(typeName, id);
        }

        public void OnDeleted(string typeName, Guid id, DXUnitHandlerBaseContext context)
        {
        }

        public Guid OnInserting(ESQLModel model, DXUnitHandlerBaseContext context)
        {
            return this._coreRepo.Insert(model);
        }

        public void OnInserted(ESQLModel model, DXUnitHandlerBaseContext context)
        {
        }

        public Guid OnUpdating(ESQLModel model, DXUnitHandlerBaseContext context)
        {
            return this._coreRepo.Update(model);
        }

        public void OnUpdated(ESQLModel model, DXUnitHandlerBaseContext context)
        {
        }

        public T GetItem<T>(Guid id, DXUnitHandlerBaseContext context) where T : DXUnit
        {
            return this._genericRepo.GetItem<T>(id);
        }

        public bool IsItemExisting(string typeName, Guid id, DXUnitHandlerBaseContext context)
        {
            return this._coreRepo.IsItemExisting(typeName, id);
        }

        public void OnGetting(ESQLModel model, DXUnitHandlerBaseContext context)
        {
        }
    }
}