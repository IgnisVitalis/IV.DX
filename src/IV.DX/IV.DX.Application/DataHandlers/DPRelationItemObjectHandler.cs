using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    internal class DXRelationItemUnitHandler : BaseEntityHandler<DXRelationItemUnit>
    {
        private readonly IDXGenericRepository _genericRepo;

        public DXRelationItemUnitHandler(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._genericRepo = serviceProvider.GetService<IDXGenericRepository>();
        }

        public override bool IsItemExisting(Guid id, DXUnitHandlerBaseContext context)
        {
            return false;
        }

        public override Guid OnInserting(DXRelationItemUnit entity, DXUnitHandlerBaseContext context)
        {
            this._genericRepo.AddRelation(entity);
            return Guid.Empty;
        }

        public override Guid OnUpdating(DXRelationItemUnit entity, DXUnitHandlerBaseContext context)
        {
            this.ThrowNotSupportedExceptionForOnUpdatingMethod();
            return Guid.Empty;
        }

        public override bool OnDeleting(Guid id, DXUnitHandlerBaseContext context)
        {
            var entity = this._genericRepo.GetItem<DXRelationItemUnit>(id);

            return this._genericRepo.Delete(entity);
        }
    }
}