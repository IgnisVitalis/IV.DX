using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXUnitDefinitionUnitHandler(
        IDXUnitDataService dxUnitService,
        IDXUnitGenericRepository genericRepo,
        IDXStructureRepository dataStructureRepo,
        IDXElementGenericRepository dxElementGenericRepo,
        IDXStructureCache dxStructureCache) :
        DXObjectDefinitionUnitHandler(dxUnitService, dataStructureRepo, genericRepo, dxElementGenericRepo),
        IDXBeforeInsertHandler<DXUnitDefinitionUnit>, IDXUniqueBeforeInsertHandler,
        IDXBeforeUpdateHandler<DXUnitDefinitionUnit>, IDXUniqueBeforeUpdateHandler,
        IDXBeforeDeleteHandler<DXUnitDefinitionUnit>, IDXUniqueBeforeDeleteHandler,
        IDXAfterInsertHandler<DXUnitDefinitionUnit>, IDXUniqueAfterInsertHandler,
        IDXAfterUpdateHandler<DXUnitDefinitionUnit>, IDXUniqueAfterUpdateHandler,
        IDXAfterDeleteHandler<DXUnitDefinitionUnit>, IDXUniqueAfterDeleteHandler

    {
        public int BeforeOrder => 1;

        public int AfterOrder => 1;

        public async Task<DXResult<DXUnitDefinitionUnit>> BeforeInsertAsync(DXUnitDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit);

            if (ctx is DXUnitHandlerPreInitCoreContext)
            {
                dataStructureRepo.CreateDataStructure(dxUnit);
                dataStructureRepo.UpdateUniqueColumns(dxUnit);

                return DXResult<DXUnitDefinitionUnit>.OkSkipProcess(dxUnit);
            }
            else if (ctx is DXUnitHandlerPostInitCoreContext)
            {
                return DXResult<DXUnitDefinitionUnit>.OkContinue(dxUnit);
            }
            else
            {
                dataStructureRepo.CreateDataStructure(dxUnit);

                await this.ProcessRelationsAsync(dxUnit, ct);

                dataStructureRepo.UpdateUniqueColumns(dxUnit);

                return DXResult<DXUnitDefinitionUnit>.OkContinue(dxUnit);
            }
        }

        public async Task<DXResult<DXUnitDefinitionUnit>> BeforeUpdateAsync(DXUnitDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit);

            dataStructureRepo.UpdatedDataStructure(dxUnit);

            await this.ProcessRelationsAsync(dxUnit, ct);

            dataStructureRepo.UpdateUniqueColumns(dxUnit);

            return DXResult<DXUnitDefinitionUnit>.OkContinue(dxUnit);
        }

        public async Task<DXResult<DXUnitDefinitionUnit>> BeforeDeleteAsync(DXUnitDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit);

            await this.DeleteRelationsAsync(dxUnit, ctx, ct);

            dataStructureRepo.DropDataStructure(dxUnit);

            return DXResult<DXUnitDefinitionUnit>.OkContinue(dxUnit);
        }

        public async Task<DXResult> AfterUpdateAsync(DXUnitDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }

        public async Task<DXResult> AfterInsertAsync(DXUnitDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }

        public async Task<DXResult> AfterDeleteAsync(DXUnitDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }

        private async Task DeleteRelationsAsync(DXUnitDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            var existingDXUnit = genericRepo.GetDXUnit<DXUnitDefinitionUnit>(dxUnit.ID);

            if (existingDXUnit == null)
                return;

            var relatedDXElementIds = existingDXUnit.DXElementInUnitDefinitionElement.Announced.Select(x => x.DXElementDefinitionUnit).ToList();

            var relatedDXElements = dataStructureRepo.GetDXElementDefinitions(relatedDXElementIds);

            foreach (var relatedDXElement in relatedDXElements)
            {
                await dxUnitService.DeleteAsync(this.GetExistingDXElementInDXUnitRelationObject(dxUnit, relatedDXElement), ctx, ct);
            }
        }

        private async Task ProcessRelationsAsync(DXUnitDefinitionUnit dxUnit, CancellationToken ct)
        {
            this.ProcessDXElementsInDXUnitElements(dxUnit);
            this.ProcessDXnitRelationElements(dxUnit);
            await this.ProcessEnumRelationsAsync(dxUnit, ct);
        }

        private void ProcessDXnitRelationElements(DXUnitDefinitionUnit dxUnit)
        {
            if (dxUnit.DXElementInUnitDefinitionElement == null)
                return;

            var objectInfoFromDB = this.GetObjectInfoFromDB(dxUnit);

            if (objectInfoFromDB == null || dxUnit.DXElementInUnitDefinitionElement.Mode == MultiElementsMode.Target)
            {
                this.ProcessDXUnitRelationElementsUsingTargetMode(dxUnit);
            }
            else
            {
                this.ProcessDXUnitRelationElementsUsingFullMode(dxUnit, objectInfoFromDB);
            }
        }

        private void ProcessDXUnitRelationElementsUsingFullMode(DXUnitDefinitionUnit dxUnit, DXUnitDefinitionUnit existingdxUnit)
        {
            var newAnnouncedIds = dxUnit.DXUnitRelationElement.Announced.Select(x => x.TargetUnit);
            var existingAnnouncedIds = existingdxUnit.DXUnitRelationElement.Announced.Select(x => x.TargetUnit);

            var announcedIds = newAnnouncedIds.Except(existingAnnouncedIds);
            var deletedIds = existingAnnouncedIds.Except(newAnnouncedIds);

            var dxUnitsToUnassign = dataStructureRepo.GetDXUnitDefinitions(deletedIds);

            foreach (var announcedId in announcedIds)
            {
                var dxUnitRelation = dxUnit.DXUnitRelationElement.Announced.Single(x => x.TargetUnit == announcedId);

                var dxUnitToAssign = dataStructureRepo.GetDXUnitDefinition(announcedId);

                this.AssignDXUnit(dxUnit, dxUnitRelation, dxUnitToAssign);
                this.CreateRevertedDXUnitRelationElement(dxUnitRelation);
            }

            foreach (var deletedId in deletedIds)
            {
                var dxUnitRelation = existingdxUnit.DXUnitRelationElement.Announced.Single(x => x.TargetUnit == deletedId);

                var dxUnitToUnassign = dataStructureRepo.GetDXUnitDefinition(deletedId);

                this.UnassingDXUnit(dxUnit, dxUnitRelation, dxUnitToUnassign);
                this.DeleteRevertedDXUnitRelationElement(dxUnitRelation, dxUnitToUnassign);
            }
        }

        private void ProcessDXUnitRelationElementsUsingTargetMode(DXUnitDefinitionUnit dxUnit)
        {
            foreach (var announced in dxUnit.DXUnitRelationElement.Announced)
            {
                var announcedId = announced.TargetUnit;

                var dxUnitToAssign = dataStructureRepo.GetDXUnitDefinition(announcedId);

                this.AssignDXUnit(dxUnit, announced, dxUnitToAssign);
                this.CreateRevertedDXUnitRelationElement(announced);
            }

            foreach (var deleted in dxUnit.DXUnitRelationElement.Deleted)
            {
                var deletedId = deleted.TargetUnit;

                var dxUnitToUnassign = dataStructureRepo.GetDXUnitDefinition(deletedId);

                this.UnassingDXUnit(dxUnit, deleted, dxUnitToUnassign);
                this.DeleteRevertedDXUnitRelationElement(deleted, dxUnitToUnassign);
            }
        }

        private void CreateRevertedDXUnitRelationElement(DXUnitRelationElement dxUnitRelationElement)
        {
            var revertedDXUnitRelationElement = dxUnitRelationElement.GetReverted();

            revertedDXUnitRelationElement.ID = Guid.NewGuid();
            revertedDXUnitRelationElement.DXUnitID = dxUnitRelationElement.TargetUnit;

            dxElementGenericRepo.Insert("DXUnitDefinitionUnit", revertedDXUnitRelationElement);
        }

        private void DeleteRevertedDXUnitRelationElement(DXUnitRelationElement dxUnitRelationElement, DXUnitDefinitionUnit relatedDXUnit)
        {
            var revertedDXElementToDelete = 
                relatedDXUnit.DXUnitRelationElement.Announced.SingleOrDefault(x => x.TargetUnit == dxUnitRelationElement.DXUnitID);

            dxElementGenericRepo.Delete(revertedDXElementToDelete);
        }

        private void AssignDXUnit(DXUnitDefinitionUnit dxUnit, DXUnitRelationElement dxUnitRelationElement, DXUnitDefinitionUnit dxUnitToAssign)
        {
            var relationType = dxUnit.DXUnitRelationElement.Announced.Single(x => x.TargetUnit == dxUnitToAssign.ID).RelationType;

            var dxRelation = this.GetDXUnitRelationObject(dxUnit, dxUnitRelationElement, dxUnitToAssign, relationType);

            dxUnitService.InsertAsync(dxRelation).Wait();
        }

        private void UnassingDXUnit(DXUnitDefinitionUnit dxUnit, DXUnitRelationElement dxUnitRelationElement, DXUnitDefinitionUnit dxUnitToUnassign)
        {
            var existingDXUnit = this.GetExistingDXUnitRelationObject(dxUnit, dxUnitRelationElement, dxUnitToUnassign);

            if (existingDXUnit == null)
                return;

            var dxRelation = this.GetExistingDXUnitRelationObject(dxUnit, dxUnitRelationElement, dxUnitToUnassign);

            dxUnitService.DeleteAsync(dxRelation).Wait();
        }

        private void ProcessDXElementsInDXUnitElements(DXUnitDefinitionUnit dxUnit)
        {
            if (dxUnit.DXElementInUnitDefinitionElement == null)
                return;

            var objectInfoFromDB = this.GetObjectInfoFromDB(dxUnit);

            if (objectInfoFromDB == null || dxUnit.DXElementInUnitDefinitionElement.Mode == MultiElementsMode.Target)
            {
                this.ProcessDXElementsInDXUnitElementsUsingTragetMode(dxUnit);
            }
            else
            {
                this.ProcessDXElementsInDXUnitElementsUsingFullMode(dxUnit, objectInfoFromDB);
            }
        }

        private DXUnitDefinitionUnit GetObjectInfoFromDB(DXUnitDefinitionUnit objectInfoIncome)
        {
            if (SystemObjectNames.Contains(objectInfoIncome.DXObjectDefinitionMainElement.Name, StringComparer.OrdinalIgnoreCase))
                return null;

            return genericRepo.GetDXUnit<DXUnitDefinitionUnit>(objectInfoIncome.ID);
        }

        private void ProcessDXElementsInDXUnitElementsUsingFullMode(DXUnitDefinitionUnit dxUnit, DXUnitDefinitionUnit existingdxUnit)
        {
            var newAnnouncedIds = dxUnit.DXElementInUnitDefinitionElement.Announced.Select(x => x.DXElementDefinitionUnit);
            var existingAnnouncedIds = existingdxUnit.DXElementInUnitDefinitionElement.Announced.Select(x => x.DXElementDefinitionUnit);

            var announcedIds = newAnnouncedIds.Except(existingAnnouncedIds);
            var deletedIds = existingAnnouncedIds.Except(newAnnouncedIds);

            var dxElementsToAssign = dataStructureRepo.GetDXElementDefinitions(announcedIds);
            var dxElementsToUnassign = dataStructureRepo.GetDXElementDefinitions(deletedIds);

            this.AssignDXElements(dxUnit, dxElementsToAssign);
            this.UnassingDXElements(dxUnit, dxElementsToUnassign);
        }

        private void ProcessDXElementsInDXUnitElementsUsingTragetMode(DXUnitDefinitionUnit dxUnit)
        {
            var announcedIds = dxUnit.DXElementInUnitDefinitionElement.Announced.Select(x => x.DXElementDefinitionUnit);
            var dxElementsToAssign = dataStructureRepo.GetDXElementDefinitions(announcedIds);

            var deletedIds = dxUnit.DXElementInUnitDefinitionElement.Deleted.Select(x => x.DXElementDefinitionUnit);
            var dxElementsToUnassign = dataStructureRepo.GetDXElementDefinitions(deletedIds);

            this.AssignDXElements(dxUnit, dxElementsToAssign);
            this.UnassingDXElements(dxUnit, dxElementsToUnassign);
        }

        private void AssignDXElements(DXUnitDefinitionUnit dxUnit, IEnumerable<DXElementDefinitionUnit> dxElementsToAssign)
        {
            foreach (var dxElementToAssign in dxElementsToAssign)
            {
                var relationType = dxUnit.DXElementInUnitDefinitionElement.Announced.Single(x => x.DXElementDefinitionUnit == dxElementToAssign.ID).RelationType;

                dxUnitService.InsertAsync(this.GetDXElementInDXUnitRelationObject(dxUnit, dxElementToAssign, relationType)).Wait();
            }
        }

        private void UnassingDXElements(DXUnitDefinitionUnit dxUnit, IEnumerable<DXElementDefinitionUnit> dxElementsToUnassign)
        {
            foreach (var dxElementToUnassign in dxElementsToUnassign)
            {
                var existingDXElement = this.GetExistingDXElementInDXUnitRelationObject(dxUnit, dxElementToUnassign);

                if (existingDXElement == null)
                    continue;

                dxUnitService.DeleteAsync(this.GetExistingDXElementInDXUnitRelationObject(dxUnit, dxElementToUnassign)).Wait();
            }
        }

        private DXRelationDefinitionUnit GetDXElementInDXUnitRelationObject(DXUnitDefinitionUnit dxUnit, DXElementDefinitionUnit dxElement, DXElementInUnitTypeEnum dxElementInUnitRelationType)
        {
            var relationType = this.ConvertDXElementIndxUnitRelationTypeToCommonRelationType(dxElementInUnitRelationType);
            var result = this.GetDXElementsInDXUnitElementsRelationObject(dxUnit, dxElement, relationType);

            return result;
        }

        private DXRelationDefinitionUnit GetDXElementsInDXUnitElementsRelationObject(DXUnitDefinitionUnit dxUnit, DXElementDefinitionUnit dxElement, DXRelationTypeEnum relationType)
        {
            var id = Guid.NewGuid();

            return new DXRelationDefinitionUnit()
            {
                ID = id,
                DXRelationDefinitionMainElement = new DXRelationDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    DXUnitID = id,
                    ObjectNameLeft = dxUnit.DXObjectDefinitionMainElement.Name,
                    RelationNameLeft = $"{dxUnit.DXObjectDefinitionMainElement.Name}ID",
                    ObjectNameRight = dxElement.DXObjectDefinitionMainElement.Name,
                    RelationNameRight = dxElement.DXObjectDefinitionMainElement.Name,
                    Kind = dxUnit.DXObjectDefinitionMainElement.Kind,
                    RelationType = relationType
                }
            };
        }

        private DXRelationDefinitionUnit GetDXUnitRelationObject(
            DXUnitDefinitionUnit dxUnit,
            DXUnitRelationElement dxUnitRelationElement,
            DXUnitDefinitionUnit dxUnitRelated,
            DXRelationTypeEnum relationType)
        {
            var id = Guid.NewGuid();

            return new DXRelationDefinitionUnit()
            {
                ID = id,
                DXRelationDefinitionMainElement = new DXRelationDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    DXUnitID = id,
                    ObjectNameLeft = dxUnit.DXObjectDefinitionMainElement.Name,
                    RelationNameLeft = dxUnitRelationElement.OwnRelationName,
                    ObjectNameRight = dxUnitRelated.DXObjectDefinitionMainElement.Name,
                    RelationNameRight = dxUnitRelationElement.TargetRelationName,
                    Kind = dxUnit.DXObjectDefinitionMainElement.Kind,
                    RelationType = relationType
                }
            };
        }

        private DXRelationDefinitionUnit GetExistingDXUnitRelationObject(
            DXUnitDefinitionUnit dxUnit,
            DXUnitRelationElement dxUnitRelationElement,
            DXUnitDefinitionUnit dxUnitRelated)
        {
            var query = $"DXRelationDefinitionMainElement.ObjectNameLeft = '{dxUnitRelated.DXObjectDefinitionMainElement.Name}' " +
               $"AND DXRelationDefinitionMainElement.ObjectNameRight = '{dxUnitRelated.DXObjectDefinitionMainElement.Name}' " +
               $"AND DXRelationDefinitionMainElement.RelationNameLeft = '{dxUnitRelationElement.OwnRelationName}' " +
               $"AND DXRelationDefinitionMainElement.RelationNameRight = '{dxUnitRelationElement.TargetRelationName}'";

            var items = genericRepo.GetDXUnits<DXRelationDefinitionUnit>(query);

            return items.SingleOrDefault();
        }

        private DXRelationDefinitionUnit GetExistingDXElementInDXUnitRelationObject(DXUnitDefinitionUnit dxUnit, DXElementDefinitionUnit dxElement)
        {
            var query = $"DXRelationDefinitionMainElement.ObjectNameLeft = '{dxUnit.DXObjectDefinitionMainElement.Name}' " +
               $"AND DXRelationDefinitionMainElement.ObjectNameRight = '{dxElement.DXObjectDefinitionMainElement.Name}' " +
               $"AND DXRelationDefinitionMainElement.RelationNameLeft = '{dxUnit.DXObjectDefinitionMainElement.Name}ID' " +
               $"AND DXRelationDefinitionMainElement.RelationNameRight = '{dxElement.DXObjectDefinitionMainElement.Name}'";

            var items = genericRepo.GetDXUnits<DXRelationDefinitionUnit>(query);

            return items.SingleOrDefault();
        }

        private DXRelationTypeEnum ConvertDXElementIndxUnitRelationTypeToCommonRelationType(DXElementInUnitTypeEnum relationType)
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
