using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    // Need to refactor.
    // Method to process relation to DBBlocks have duplicated code.
    internal class DXUnitDefinitionUnitHandler : DXObjectDefinitionUnitHandler<DXUnitDefinitionUnit>
    {
        private readonly IDXStructureRepository _dataStructureRepo;
        private readonly IDXUnitDataService _dataService;
        private readonly IDXGenericRepository _genericRepo;

        public DXUnitDefinitionUnitHandler(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._dataStructureRepo = serviceProvider.GetService<IDXStructureRepository>();
            this._dataService = serviceProvider.GetService<IDXUnitDataService>();
            this._genericRepo = serviceProvider.GetService<IDXGenericRepository>();
        }

        public override Guid OnInserting(DXUnitDefinitionUnit entity, DXUnitHandlerBaseContext context)
        {
            base.Validate(entity);
            base.Process(entity);

            if (context is DXUnitHandlerPreInitCoreContext)
            {
                this._dataStructureRepo.CreateDataStructure(entity);

                return Guid.Empty;
            }
            else if (context is DXUnitHandlerPostInitCoreContext)
            {
                return base.OnInserting(entity, context);
            }
            else
            {
                this._dataStructureRepo.CreateDataStructure(entity);

                this.ProcessRelations(entity);
                return base.OnInserting(entity, context);
            }
        }

        public override Guid OnUpdating(DXUnitDefinitionUnit entity, DXUnitHandlerBaseContext context)
        {
            base.Validate(entity);
            base.Process(entity);

            this._dataStructureRepo.UpdatedDataStructure(entity);

            //this._dataStructureRepo.AddOrUpdateEntityInfo(entity);

            this.ProcessRelations(entity);
            return base.OnUpdating(entity, context);
        }

        public override bool OnDeleting(Guid id, DXUnitHandlerBaseContext context)
        {
            var entity = this._genericRepo.GetItem<DXUnitDefinitionUnit>(id);

            if (entity == null)
                return false;

            base.Validate(entity);
            base.Process(entity);

            this.DeleteRelations(entity);

            this._dataStructureRepo.DropDataStructure(entity);
            
            return base.OnDeleting(id, context);
        }

        private void DeleteRelations(DXUnitDefinitionUnit entity)
        {
            var existingEntity = this._genericRepo.GetItem<DXUnitDefinitionUnit>(entity.ID);

            if (existingEntity == null)
                return;

            var relatedBlockIds = existingEntity.DXElementInUnitDefinitionMainElement.Announced.Select(x => x.DXElementDefinitionUnit).ToList();

            var relatedBlocks = this._dataStructureRepo.GetBlocks(relatedBlockIds);

            foreach (var relatedBlock in relatedBlocks)
            {
                this._dataService.Delete("DXRelationDefinitionUnit", this.GetExistingRelatonObject(entity, relatedBlock).ID);
            }
        }

        private void ProcessRelations(DXUnitDefinitionUnit entity)
        {
            this.ProcessBlocksInEntityRelations(entity);
            this.ProcessEnumRelations(entity);
        }

        private void ProcessBlocksInEntityRelations(DXUnitDefinitionUnit entity)
        {
            if (entity.DXElementInUnitDefinitionMainElement == null)
                return;

            var objectInfoFromDB = this.GetObjectInfoFromDB(entity);

            if (objectInfoFromDB == null || entity.DXElementInUnitDefinitionMainElement.Mode == MultiElementsMode.Target)
            {
                this.ProcessBlocksInEntityRelationsUsingTragetMode(entity);
            }
            else
            {
                this.ProcessBlocksInEntityRelationsUsingFullMode(entity, objectInfoFromDB);
            }
        }

        private DXUnitDefinitionUnit GetObjectInfoFromDB(DXUnitDefinitionUnit objectInfoIncome)
        {
            if (systemObjectNames.Contains(objectInfoIncome.DXUnitDefinitionMainElement.Name, StringComparer.OrdinalIgnoreCase))
                return null;

            return this._genericRepo.GetItem<DXUnitDefinitionUnit>(objectInfoIncome.ID);
        }

        private void ProcessBlocksInEntityRelationsUsingFullMode(DXUnitDefinitionUnit entity, DXUnitDefinitionUnit existingEntity)
        {
            var newAnnouncedIds = entity.DXElementInUnitDefinitionMainElement.Announced.Select(x => x.DXElementDefinitionUnit);
            var existingAnnouncedIds = existingEntity.DXElementInUnitDefinitionMainElement.Announced.Select(x => x.DXElementDefinitionUnit);

            var announcedIds = newAnnouncedIds.Except(existingAnnouncedIds);
            var deletedIds = existingAnnouncedIds.Except(newAnnouncedIds);

            var blocksToAssign = this._dataStructureRepo.GetBlocks(announcedIds);
            var blocksToUnassign = this._dataStructureRepo.GetBlocks(deletedIds);

            this.AssignBlocks(entity, blocksToAssign);
            this.UnassingBlocks(entity, blocksToUnassign);
        }

        private void ProcessBlocksInEntityRelationsUsingTragetMode(DXUnitDefinitionUnit entity)
        {
            var announcedIds = entity.DXElementInUnitDefinitionMainElement.Announced.Select(x => x.DXElementDefinitionUnit);
            var blocksToAssign = this._dataStructureRepo.GetBlocks(announcedIds);

            var deletedIds = entity.DXElementInUnitDefinitionMainElement.Deleted.Select(x => x.DXElementDefinitionUnit);
            var blocksToUnassign = this._dataStructureRepo.GetBlocks(deletedIds);

            this.AssignBlocks(entity, blocksToAssign);
            this.UnassingBlocks(entity, blocksToUnassign);
        }

        private void AssignBlocks(DXUnitDefinitionUnit entity, IEnumerable<DXElementDefinitionUnit> blocksToAssign)
        {
            foreach (var blockToAssign in blocksToAssign)
            {
                var relationType = entity.DXElementInUnitDefinitionMainElement.Announced.Single(x => x.DXElementDefinitionUnit == blockToAssign.ID).RelationType;

                this._dataService.Insert(this.GetRelationObject(entity, blockToAssign, relationType));
            }
        }

        private void UnassingBlocks(DXUnitDefinitionUnit entity, IEnumerable<DXElementDefinitionUnit> blocksToUnassign)
        {
            foreach (var blockToUnassign in blocksToUnassign)
            {
                var existingBlock = this.GetExistingRelatonObject(entity, blockToUnassign);

                if (existingBlock == null)
                    continue;

                this._dataService.Delete("DXRelationDefinitionUnit", this.GetExistingRelatonObject(entity, blockToUnassign).ID);
            }
        }

        private DXRelationDefinitionUnit GetRelationObject(DXUnitDefinitionUnit entity, DXElementDefinitionUnit block, DXElementInUnitTypeEnum relationType)
        {
            var result = this.GetRelationObject(entity, block);

            result.DXRelationDefinitionMainElement.RelationType = this.ConvertBlockInEntityRelationTypeToCommonRelationType(relationType);

            return result;
        }

        private DXRelationDefinitionUnit GetRelationObject(DXUnitDefinitionUnit entity, DXElementDefinitionUnit block)
        {
            return new DXRelationDefinitionUnit()
            {
                ID = Guid.NewGuid(),
                DXRelationDefinitionMainElement = new DXRelationDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectNameLeft = entity.DXUnitDefinitionMainElement.Name,
                    RelationNameLeft = $"{entity.DXUnitDefinitionMainElement.Name}ID",
                    ObjectNameRight = block.DXUnitDefinitionMainElement.Name,
                    RelationNameRight = block.DXUnitDefinitionMainElement.Name,
                    Kind = entity.DXUnitDefinitionMainElement.Kind
                }
            };
        }

        private DXRelationDefinitionUnit GetExistingRelatonObject(DXUnitDefinitionUnit entity, DXElementDefinitionUnit block)
        {
            var query = $"DXRelationDefinitionMainElement.ObjectNameLeft = '{entity.DXUnitDefinitionMainElement.Name}' " +
               $"AND DXRelationDefinitionMainElement.ObjectNameRight = '{block.DXUnitDefinitionMainElement.Name}' " +
               $"AND DXRelationDefinitionMainElement.RelationNameLeft = '{entity.DXUnitDefinitionMainElement.Name}ID' " +
               $"AND DXRelationDefinitionMainElement.RelationNameRight = '{block.DXUnitDefinitionMainElement.Name}'";

            var items = this._genericRepo.GetItems<DXRelationDefinitionUnit>(query);

            return items.SingleOrDefault();
        }

        private DXRelationTypeEnum ConvertBlockInEntityRelationTypeToCommonRelationType(DXElementInUnitTypeEnum relationType)
        {
            switch (relationType)
            {
                case DXElementInUnitTypeEnum.SingleMandatory:
                    return DXRelationTypeEnum.ZeroOneToZeroOne;
                case DXElementInUnitTypeEnum.SingleOptional:
                    return DXRelationTypeEnum.ZeroOneToZeroOne;
                case DXElementInUnitTypeEnum.MultiMandatory:
                    return DXRelationTypeEnum.ZeroOneToMany;
                case DXElementInUnitTypeEnum.MultiOptional:
                    return DXRelationTypeEnum.ZeroOneToMany;
                default:
                    throw new Exception($"DXElementInUnitTypeEnum doesn't contain '{relationType}' value");
            }
        }
    }
}