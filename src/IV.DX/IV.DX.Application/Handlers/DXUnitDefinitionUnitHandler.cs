using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXUnitDefinitionUnitHandler(IDXUnitDataService dxUnitService, IDXGenericRepository genericRepo, IDXStructureRepository dataStructureRepo) :
        DXObjectDefinitionUnitHandler(dxUnitService, dataStructureRepo, genericRepo),
        IDXBeforeInsert<DXUnitDefinitionUnit>,
        IDXBeforeUpdate<DXUnitDefinitionUnit>,
        IDXBeforeDelete<DXUnitDefinitionUnit>
    {
        public int BeforeOrder => 1;

        public Task<DXResult<DXUnitDefinitionUnit>> BeforeInsertAsync(DXUnitDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit);

            if (ctx is DXUnitHandlerPreInitCoreContextOld)
            {
                dataStructureRepo.CreateDataStructure(dxUnit);

                return Task.Run(() => DXResult<DXUnitDefinitionUnit>.OkSkipProcess(dxUnit));
            }
            else if (ctx is DXUnitHandlerPostInitCoreContextOld)
            {
                return Task.Run(() => DXResult<DXUnitDefinitionUnit>.OkContinue(dxUnit));
            }
            else
            {
                dataStructureRepo.CreateDataStructure(dxUnit);

                this.ProcessRelations(dxUnit);
                return Task.Run(() => DXResult<DXUnitDefinitionUnit>.OkContinue(dxUnit));
            }
        }

        public Task<DXResult<DXUnitDefinitionUnit>> BeforeUpdateAsync(DXUnitDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit);

            dataStructureRepo.UpdatedDataStructure(dxUnit);

            this.ProcessRelations(dxUnit);
            return Task.Run(() => DXResult<DXUnitDefinitionUnit>.OkContinue(dxUnit));
        }

        public Task<DXResult<DXUnitDefinitionUnit>> BeforeDeleteAsync(DXUnitDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit);

            this.DeleteRelations(dxUnit);

            dataStructureRepo.DropDataStructure(dxUnit);

            return Task.Run(() => DXResult<DXUnitDefinitionUnit>.OkContinue(dxUnit));
        }

        private void DeleteRelations(DXUnitDefinitionUnit entity)
        {
            var existingEntity = genericRepo.GetItem<DXUnitDefinitionUnit>(entity.ID);

            if (existingEntity == null)
                return;

            var relatedBlockIds = existingEntity.DXElementInUnitDefinitionMainElement.Announced.Select(x => x.DXElementDefinitionUnit).ToList();

            var relatedBlocks = dataStructureRepo.GetBlocks(relatedBlockIds);

            foreach (var relatedBlock in relatedBlocks)
            {
                dxUnitService.Delete("DXRelationDefinitionUnit", this.GetExistingRelatonObject(entity, relatedBlock).ID);
            }
        }

        private void ProcessRelations(DXUnitDefinitionUnit dxUnit)
        {
            this.ProcessBlocksIndxUnitRelations(dxUnit);
            this.ProcessEnumRelations(dxUnit);
        }

        private void ProcessBlocksIndxUnitRelations(DXUnitDefinitionUnit dxUnit)
        {
            if (dxUnit.DXElementInUnitDefinitionMainElement == null)
                return;

            var objectInfoFromDB = this.GetObjectInfoFromDB(dxUnit);

            if (objectInfoFromDB == null || dxUnit.DXElementInUnitDefinitionMainElement.Mode == MultiElementsMode.Target)
            {
                this.ProcessBlocksIndxUnitRelationsUsingTragetMode(dxUnit);
            }
            else
            {
                this.ProcessBlocksIndxUnitRelationsUsingFullMode(dxUnit, objectInfoFromDB);
            }
        }

        private DXUnitDefinitionUnit GetObjectInfoFromDB(DXUnitDefinitionUnit objectInfoIncome)
        {
            if (systemObjectNames.Contains(objectInfoIncome.DXUnitDefinitionMainElement.Name, StringComparer.OrdinalIgnoreCase))
                return null;

            return genericRepo.GetItem<DXUnitDefinitionUnit>(objectInfoIncome.ID);
        }

        private void ProcessBlocksIndxUnitRelationsUsingFullMode(DXUnitDefinitionUnit dxUnit, DXUnitDefinitionUnit existingdxUnit)
        {
            var newAnnouncedIds = dxUnit.DXElementInUnitDefinitionMainElement.Announced.Select(x => x.DXElementDefinitionUnit);
            var existingAnnouncedIds = existingdxUnit.DXElementInUnitDefinitionMainElement.Announced.Select(x => x.DXElementDefinitionUnit);

            var announcedIds = newAnnouncedIds.Except(existingAnnouncedIds);
            var deletedIds = existingAnnouncedIds.Except(newAnnouncedIds);

            var blocksToAssign = dataStructureRepo.GetBlocks(announcedIds);
            var blocksToUnassign = dataStructureRepo.GetBlocks(deletedIds);

            this.AssignBlocks(dxUnit, blocksToAssign);
            this.UnassingBlocks(dxUnit, blocksToUnassign);
        }

        private void ProcessBlocksIndxUnitRelationsUsingTragetMode(DXUnitDefinitionUnit dxUnit)
        {
            var announcedIds = dxUnit.DXElementInUnitDefinitionMainElement.Announced.Select(x => x.DXElementDefinitionUnit);
            var blocksToAssign = dataStructureRepo.GetBlocks(announcedIds);

            var deletedIds = dxUnit.DXElementInUnitDefinitionMainElement.Deleted.Select(x => x.DXElementDefinitionUnit);
            var blocksToUnassign = dataStructureRepo.GetBlocks(deletedIds);

            this.AssignBlocks(dxUnit, blocksToAssign);
            this.UnassingBlocks(dxUnit, blocksToUnassign);
        }

        private void AssignBlocks(DXUnitDefinitionUnit dxUnit, IEnumerable<DXElementDefinitionUnit> blocksToAssign)
        {
            foreach (var blockToAssign in blocksToAssign)
            {
                var relationType = dxUnit.DXElementInUnitDefinitionMainElement.Announced.Single(x => x.DXElementDefinitionUnit == blockToAssign.ID).RelationType;

                dxUnitService.InsertAsync(this.GetRelationObject(dxUnit, blockToAssign, relationType)).Wait();
            }
        }

        private void UnassingBlocks(DXUnitDefinitionUnit dxUnit, IEnumerable<DXElementDefinitionUnit> blocksToUnassign)
        {
            foreach (var blockToUnassign in blocksToUnassign)
            {
                var existingBlock = this.GetExistingRelatonObject(dxUnit, blockToUnassign);

                if (existingBlock == null)
                    continue;

                dxUnitService.Delete("DXRelationDefinitionUnit", this.GetExistingRelatonObject(dxUnit, blockToUnassign).ID);
            }
        }

        private DXRelationDefinitionUnit GetRelationObject(DXUnitDefinitionUnit dxUnit, DXElementDefinitionUnit block, DXElementInUnitTypeEnum relationType)
        {
            var result = this.GetRelationObject(dxUnit, block);

            result.DXRelationDefinitionMainElement.RelationType = this.ConvertBlockIndxUnitRelationTypeToCommonRelationType(relationType);

            return result;
        }

        private DXRelationDefinitionUnit GetRelationObject(DXUnitDefinitionUnit dxUnit, DXElementDefinitionUnit block)
        {
            return new DXRelationDefinitionUnit()
            {
                ID = Guid.NewGuid(),
                DXRelationDefinitionMainElement = new DXRelationDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectNameLeft = dxUnit.DXUnitDefinitionMainElement.Name,
                    RelationNameLeft = $"{dxUnit.DXUnitDefinitionMainElement.Name}ID",
                    ObjectNameRight = block.DXUnitDefinitionMainElement.Name,
                    RelationNameRight = block.DXUnitDefinitionMainElement.Name,
                    Kind = dxUnit.DXUnitDefinitionMainElement.Kind
                }
            };
        }

        private DXRelationDefinitionUnit GetExistingRelatonObject(DXUnitDefinitionUnit dxUnit, DXElementDefinitionUnit block)
        {
            var query = $"DXRelationDefinitionMainElement.ObjectNameLeft = '{dxUnit.DXUnitDefinitionMainElement.Name}' " +
               $"AND DXRelationDefinitionMainElement.ObjectNameRight = '{block.DXUnitDefinitionMainElement.Name}' " +
               $"AND DXRelationDefinitionMainElement.RelationNameLeft = '{dxUnit.DXUnitDefinitionMainElement.Name}ID' " +
               $"AND DXRelationDefinitionMainElement.RelationNameRight = '{block.DXUnitDefinitionMainElement.Name}'";

            var items = genericRepo.GetItems<DXRelationDefinitionUnit>(query);

            return items.SingleOrDefault();
        }

        private DXRelationTypeEnum ConvertBlockIndxUnitRelationTypeToCommonRelationType(DXElementInUnitTypeEnum relationType)
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
