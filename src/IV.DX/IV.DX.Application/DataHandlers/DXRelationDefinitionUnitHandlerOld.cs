using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    internal class DXRelationDefinitionUnitHandlerOld : BaseEntityHandler<DXRelationDefinitionUnit>
    {
        private readonly IDXGenericRepository _genericRepo;
        private readonly IDXStructureRepository _dataStructureRepo;

        public DXRelationDefinitionUnitHandlerOld(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._genericRepo = serviceProvider.GetRequiredService<IDXGenericRepository>();
            this._dataStructureRepo = serviceProvider.GetRequiredService<IDXStructureRepository>();
        }

        public override Guid OnInserting(DXRelationDefinitionUnit entity, IDXHandlerContext context)
        {
            var existingRelation = this._dataStructureRepo.GetRelation(entity.DXRelationDefinitionMainElement.ObjectNameLeft, entity.DXRelationDefinitionMainElement.RelationNameLeft, entity.DXRelationDefinitionMainElement.ObjectNameRight, entity.DXRelationDefinitionMainElement.RelationNameRight);

            if (existingRelation != null)
            {
                return Guid.Empty;
            }

            if (context is DXUnitHandlerPreInitCoreContextOld)
            {
                this._dataStructureRepo.CreateDataStructure(entity);
                return Guid.Empty;
            }
            else if (context is DXUnitHandlerPostInitCoreContextOld)
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

        public override Guid OnUpdating(DXRelationDefinitionUnit entity, IDXHandlerContext context)
        {
            base.ThrowNotSupportedExceptionForOnUpdatingMethod();
            return Guid.Empty;
        }

        public override bool OnDeleting(Guid id, IDXHandlerContext context)
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