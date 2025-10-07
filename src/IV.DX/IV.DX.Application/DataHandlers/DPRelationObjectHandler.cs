using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    internal class DPRelationObjectHandler : BaseEntityHandler<DPRelationObject>
    {
        private readonly IGenericRepository _genericRepo;
        private readonly IDataStructureRepository _dataStructureRepo;

        public DPRelationObjectHandler(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._genericRepo = serviceProvider.GetService<IGenericRepository>();
            this._dataStructureRepo = serviceProvider.GetService<IDataStructureRepository>();
        }

        public override Guid OnInserting(DPRelationObject entity, EntityHandlerBaseContext context)
        {
            var existingRelation = this._dataStructureRepo.GetRelation(entity.DPRelationGenBlock.ObjectNameLeft, entity.DPRelationGenBlock.RelationNameLeft, entity.DPRelationGenBlock.ObjectNameRight, entity.DPRelationGenBlock.RelationNameRight);

            if (existingRelation != null)
            {
                return Guid.Empty;
            }

            if (context is EntityHandlerPreInitCoreContext)
            {
                this._dataStructureRepo.CreateDataStructure(entity);
                return Guid.Empty;
            }
            else if (context is EntityHandlerPostInitCoreContext)
            {
                var invertedRelation = entity.CreateInvertedRelationObject();

                base.OnInserting(entity, context);
                return this._genericRepo.Insert(invertedRelation);
            }
            else
            {
                this._dataStructureRepo.CreateDataStructure(entity);

                var invertedRelation = entity.CreateInvertedRelationObject();

                base.OnInserting(entity, context);
                return this._genericRepo.Insert(invertedRelation);
            }
        }

        public override Guid OnUpdating(DPRelationObject entity, EntityHandlerBaseContext context)
        {
            base.ThrowNotSupportedExceptionForOnUpdatingMethod();
            return Guid.Empty;
        }

        public override bool OnDeleting(Guid id, EntityHandlerBaseContext context)
        {
            var entity = this._genericRepo.GetItem<DPRelationObject>(id);

            this._dataStructureRepo.DropDataStructure(entity);

            var existingRelation = this._dataStructureRepo.GetRelation(entity.DPRelationGenBlock.ObjectNameLeft, entity.DPRelationGenBlock.RelationNameLeft, entity.DPRelationGenBlock.ObjectNameRight, entity.DPRelationGenBlock.RelationNameRight);

            if (existingRelation == null)
                return false;

            entity = existingRelation;

            base.OnDeleting(existingRelation.ID, context);

            var invertedRelation = this.GetInvertedRelationObject(entity);

            return this._genericRepo.Delete(invertedRelation);
        }

        private DPRelationObject GetInvertedRelationObject(DPRelationObject entity)
        {
            var modelDefinition = this._genericRepo.GetItems<DPRelationObject>(entity.GetQueryForInvertedRelationObject());

            return modelDefinition.SingleOrDefault();
        }
    }
}