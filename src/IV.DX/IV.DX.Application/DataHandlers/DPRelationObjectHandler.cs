using IV.DX.Contracts.Application;
using IV.DX.Contracts.Common.Helpers;
using IV.DX.Contracts.Common.Models;
using IV.DX.Contracts.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    public class DPRelationObjectHandler : BaseEntityHandler<DPRelationObject>
    {
        private readonly IGenericRepository _genericRepo;
        private readonly IDataStructureRepository _dataStructureRepo;
        private readonly ISQLQueryHelper _sqlQueryHelper;

        public DPRelationObjectHandler(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._genericRepo = serviceProvider.GetService<IGenericRepository>();
            this._dataStructureRepo = serviceProvider.GetService<IDataStructureRepository>();
            this._sqlQueryHelper = serviceProvider.GetService<ISQLQueryHelper>();
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