using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    internal class DXRelationItemUnitHandlerOld : BaseEntityHandler<DXRelationItemUnit>
    {
        private readonly IDXGenericRepository _genericRepo;

        public DXRelationItemUnitHandlerOld(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._genericRepo = serviceProvider.GetRequiredService<IDXGenericRepository>();
        }

        public override bool IsItemExisting(Guid id, IDXHandlerContext context)
        {
            return false;
        }

        public override Guid OnInserting(DXRelationItemUnit entity, IDXHandlerContext context)
        {
            this._genericRepo.AddRelation(entity);
            return Guid.Empty;
        }

        public override Guid OnUpdating(DXRelationItemUnit entity, IDXHandlerContext context)
        {
            this.ThrowNotSupportedExceptionForOnUpdatingMethod();
            return Guid.Empty;
        }

        public override bool OnDeleting(Guid id, IDXHandlerContext context)
        {
            var entity = this._genericRepo.GetItem<DXRelationItemUnit>(id);

            return this._genericRepo.Delete(entity);
        }
    }
}