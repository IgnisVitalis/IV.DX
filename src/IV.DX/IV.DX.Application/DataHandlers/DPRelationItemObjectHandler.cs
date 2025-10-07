using IV.DX.Contracts.Application;
using IV.DX.Contracts.Common.Helpers;
using IV.DX.Contracts.Common.Models;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    public class DPRelationItemObjectHandler : BaseEntityHandler<DPRelationItemObject>
    {
        private readonly IGenericRepository _genericRepo;

        public DPRelationItemObjectHandler(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._genericRepo = serviceProvider.GetService<IGenericRepository>();
        }

        public override bool IsItemExisting(Guid id, EntityHandlerBaseContext context)
        {
            return false;
        }

        public override Guid OnInserting(DPRelationItemObject entity, EntityHandlerBaseContext context)
        {
            this._genericRepo.AddRelation(entity);
            return Guid.Empty;
        }

        public override Guid OnUpdating(DPRelationItemObject entity, EntityHandlerBaseContext context)
        {
            this.ThrowNotSupportedExceptionForOnUpdatingMethod();
            return Guid.Empty;
        }

        public override bool OnDeleting(Guid id, EntityHandlerBaseContext context)
        {
            var entity = this._genericRepo.GetItem<DPRelationItemObject>(id);

            return this._genericRepo.Delete(entity);
        }
    }
}