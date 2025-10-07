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
    internal class DXUnitDefinitionUnitHandler : DPObjectDescObjectHandler<DXUnitDefinitionUnit>
    {
        private readonly IDataStructureRepository _dataStructureRepo;
        private readonly IDataService _dataService;
        private readonly IGenericRepository _genericRepo;

        public DXUnitDefinitionUnitHandler(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._dataStructureRepo = serviceProvider.GetService<IDataStructureRepository>();
            this._dataService = serviceProvider.GetService<IDataService>();
            this._genericRepo = serviceProvider.GetService<IGenericRepository>();
        }

        public override Guid OnInserting(DXUnitDefinitionUnit entity, EntityHandlerBaseContext context)
        {
            base.Validate(entity);
            base.Process(entity);

            if (context is EntityHandlerPreInitCoreContext)
            {
                this._dataStructureRepo.CreateDataStructure(entity);

                return Guid.Empty;
            }
            else if (context is EntityHandlerPostInitCoreContext)
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

        public override Guid OnUpdating(DXUnitDefinitionUnit entity, EntityHandlerBaseContext context)
        {
            base.Validate(entity);
            base.Process(entity);

            this._dataStructureRepo.UpdatedDataStructure(entity);

            //this._dataStructureRepo.AddOrUpdateEntityInfo(entity);

            this.ProcessRelations(entity);
            return base.OnUpdating(entity, context);
        }

        public override bool OnDeleting(Guid id, EntityHandlerBaseContext context)
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

            var relatedBlockIds = existingEntity.DPBlockInEntityDescGenBlock.Announced.Select(x => x.DXElementDefinitionUnit).ToList();

            var relatedBlocks = this._dataStructureRepo.GetBlocks(relatedBlockIds);

            foreach (var relatedBlock in relatedBlocks)
            {
                this._dataService.Delete("DPRelationObject", this.GetExistingRelatonObject(entity, relatedBlock).ID);
            }
        }

        private void ProcessRelations(DXUnitDefinitionUnit entity)
        {
            this.ProcessBlocksInEntityRelations(entity);
            this.ProcessEnumRelations(entity);
        }

        private void ProcessBlocksInEntityRelations(DXUnitDefinitionUnit entity)
        {
            if (entity.DPBlockInEntityDescGenBlock == null)
                return;

            var objectInfoFromDB = this.GetObjectInfoFromDB(entity);

            if (objectInfoFromDB == null || entity.DPBlockInEntityDescGenBlock.Mode == ModeForMultiItems.Target)
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
            if (systemObjectNames.Contains(objectInfoIncome.DPObjectDescGenBlock.Name, StringComparer.OrdinalIgnoreCase))
                return null;

            return this._genericRepo.GetItem<DXUnitDefinitionUnit>(objectInfoIncome.ID);
        }

        private void ProcessBlocksInEntityRelationsUsingFullMode(DXUnitDefinitionUnit entity, DXUnitDefinitionUnit existingEntity)
        {
            var newAnnouncedIds = entity.DPBlockInEntityDescGenBlock.Announced.Select(x => x.DXElementDefinitionUnit);
            var existingAnnouncedIds = existingEntity.DPBlockInEntityDescGenBlock.Announced.Select(x => x.DXElementDefinitionUnit);

            var announcedIds = newAnnouncedIds.Except(existingAnnouncedIds);
            var deletedIds = existingAnnouncedIds.Except(newAnnouncedIds);

            var blocksToAssign = this._dataStructureRepo.GetBlocks(announcedIds);
            var blocksToUnassign = this._dataStructureRepo.GetBlocks(deletedIds);

            this.AssignBlocks(entity, blocksToAssign);
            this.UnassingBlocks(entity, blocksToUnassign);
        }

        private void ProcessBlocksInEntityRelationsUsingTragetMode(DXUnitDefinitionUnit entity)
        {
            var announcedIds = entity.DPBlockInEntityDescGenBlock.Announced.Select(x => x.DXElementDefinitionUnit);
            var blocksToAssign = this._dataStructureRepo.GetBlocks(announcedIds);

            var deletedIds = entity.DPBlockInEntityDescGenBlock.Deleted.Select(x => x.DXElementDefinitionUnit);
            var blocksToUnassign = this._dataStructureRepo.GetBlocks(deletedIds);

            this.AssignBlocks(entity, blocksToAssign);
            this.UnassingBlocks(entity, blocksToUnassign);
        }

        private void AssignBlocks(DXUnitDefinitionUnit entity, IEnumerable<DXElementDefinitionUnit> blocksToAssign)
        {
            foreach (var blockToAssign in blocksToAssign)
            {
                var relationType = entity.DPBlockInEntityDescGenBlock.Announced.Single(x => x.DXElementDefinitionUnit == blockToAssign.ID).RelationType;

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

                this._dataService.Delete("DPRelationObject", this.GetExistingRelatonObject(entity, blockToUnassign).ID);
            }
        }

        private DPRelationObject GetRelationObject(DXUnitDefinitionUnit entity, DXElementDefinitionUnit block, DPBlockInObjectTypeEnum relationType)
        {
            var result = this.GetRelationObject(entity, block);

            result.DPRelationGenBlock.RelationType = this.ConvertBlockInEntityRelationTypeToCommonRelationType(relationType);

            return result;
        }

        private DPRelationObject GetRelationObject(DXUnitDefinitionUnit entity, DXElementDefinitionUnit block)
        {
            return new DPRelationObject()
            {
                ID = Guid.NewGuid(),
                DPRelationGenBlock = new DPRelationGenBlock()
                {
                    ID = Guid.NewGuid(),
                    ObjectNameLeft = entity.DPObjectDescGenBlock.Name,
                    RelationNameLeft = $"{entity.DPObjectDescGenBlock.Name}ID",
                    ObjectNameRight = block.DPObjectDescGenBlock.Name,
                    RelationNameRight = block.DPObjectDescGenBlock.Name,
                    Kind = entity.DPObjectDescGenBlock.Kind
                }
            };
        }

        private DPRelationObject GetExistingRelatonObject(DXUnitDefinitionUnit entity, DXElementDefinitionUnit block)
        {
            var query = $"DPRelationGenBlock.ObjectNameLeft = '{entity.DPObjectDescGenBlock.Name}' " +
               $"AND DPRelationGenBlock.ObjectNameRight = '{block.DPObjectDescGenBlock.Name}' " +
               $"AND DPRelationGenBlock.RelationNameLeft = '{entity.DPObjectDescGenBlock.Name}ID' " +
               $"AND DPRelationGenBlock.RelationNameRight = '{block.DPObjectDescGenBlock.Name}'";

            var items = this._genericRepo.GetItems<DPRelationObject>(query);

            return items.SingleOrDefault();
        }

        private DPRelationTypeEnum ConvertBlockInEntityRelationTypeToCommonRelationType(DPBlockInObjectTypeEnum relationType)
        {
            switch (relationType)
            {
                case DPBlockInObjectTypeEnum.SingleMandatory:
                    return DPRelationTypeEnum.ZeroOneToZeroOne;
                case DPBlockInObjectTypeEnum.SingleOptional:
                    return DPRelationTypeEnum.ZeroOneToZeroOne;
                case DPBlockInObjectTypeEnum.MultiMandatory:
                    return DPRelationTypeEnum.ZeroOneToMany;
                case DPBlockInObjectTypeEnum.MultiOptional:
                    return DPRelationTypeEnum.ZeroOneToMany;
                default:
                    throw new Exception($"DPBlockInObjectTypeEnum doesn't contain '{relationType}' value");
            }
        }
    }
}