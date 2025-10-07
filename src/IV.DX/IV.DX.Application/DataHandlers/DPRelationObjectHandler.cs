using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    internal class DXRelationDefinitionUnitHandler : BaseEntityHandler<DXRelationDefinitionUnit>
    {
        private readonly IGenericRepository _genericRepo;
        private readonly IDataStructureRepository _dataStructureRepo;

        public DXRelationDefinitionUnitHandler(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._genericRepo = serviceProvider.GetService<IGenericRepository>();
            this._dataStructureRepo = serviceProvider.GetService<IDataStructureRepository>();
        }

        public override Guid OnInserting(DXRelationDefinitionUnit entity, EntityHandlerBaseContext context)
        {
            var existingRelation = this._dataStructureRepo.GetRelation(entity.DXRelationDefinitionMainElement.ObjectNameLeft, entity.DXRelationDefinitionMainElement.RelationNameLeft, entity.DXRelationDefinitionMainElement.ObjectNameRight, entity.DXRelationDefinitionMainElement.RelationNameRight);

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

        public override Guid OnUpdating(DXRelationDefinitionUnit entity, EntityHandlerBaseContext context)
        {
            base.ThrowNotSupportedExceptionForOnUpdatingMethod();
            return Guid.Empty;
        }

        public override bool OnDeleting(Guid id, EntityHandlerBaseContext context)
        {
            var entity = this._genericRepo.GetItem<DXRelationDefinitionUnit>(id);

            this._dataStructureRepo.DropDataStructure(entity);

            var existingRelation = this._dataStructureRepo.GetRelation(entity.DXRelationDefinitionMainElement.ObjectNameLeft, entity.DXRelationDefinitionMainElement.RelationNameLeft, entity.DXRelationDefinitionMainElement.ObjectNameRight, entity.DXRelationDefinitionMainElement.RelationNameRight);

            if (existingRelation == null)
                return false;

            entity = existingRelation;

            base.OnDeleting(existingRelation.ID, context);

            var invertedRelation = this.GetInvertedRelationObject(entity);

            return this._genericRepo.Delete(invertedRelation);
        }

        private DXRelationDefinitionUnit GetInvertedRelationObject(DXRelationDefinitionUnit entity)
        {
            var modelDefinition = this._genericRepo.GetItems<DXRelationDefinitionUnit>(entity.GetQueryForInvertedRelationObject());

            return modelDefinition.SingleOrDefault();
        }
    }
}