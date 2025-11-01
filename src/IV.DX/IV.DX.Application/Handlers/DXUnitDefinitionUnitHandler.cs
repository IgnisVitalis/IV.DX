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
        IDXStructureCache dxStructureCache) :
        DXObjectDefinitionUnitHandler(dxUnitService, dataStructureRepo, genericRepo),
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

                return DXResult<DXUnitDefinitionUnit>.OkSkipProcess(dxUnit);
            }
            else if (ctx is DXUnitHandlerPostInitCoreContext)
            {
                return DXResult<DXUnitDefinitionUnit>.OkContinue(dxUnit);
            }
            else
            {
                dataStructureRepo.CreateDataStructure(dxUnit);

                this.ProcessRelations(dxUnit);
                return DXResult<DXUnitDefinitionUnit>.OkContinue(dxUnit);
            }
        }

        public async Task<DXResult<DXUnitDefinitionUnit>> BeforeUpdateAsync(DXUnitDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit);

            dataStructureRepo.UpdatedDataStructure(dxUnit);

            this.ProcessRelations(dxUnit);
            return DXResult<DXUnitDefinitionUnit>.OkContinue(dxUnit);
        }

        public async Task<DXResult<DXUnitDefinitionUnit>> BeforeDeleteAsync(DXUnitDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit);

            this.DeleteRelations(dxUnit);

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

        private void DeleteRelations(DXUnitDefinitionUnit dxUnit)
        {
            var existingDXUnit = genericRepo.GetDXUnit<DXUnitDefinitionUnit>(dxUnit.ID);

            if (existingDXUnit == null)
                return;

            var relatedDXElementIds = existingDXUnit.DXElementInUnitDefinitionElement.Announced.Select(x => x.DXElementDefinitionUnit).ToList();

            var relatedDXElements = dataStructureRepo.GetDXElementDefinitions(relatedDXElementIds);

            foreach (var relatedDXElement in relatedDXElements)
            {
                dxUnitService.DeleteAsync(this.GetExistingRelatonObject(dxUnit, relatedDXElement)).Wait();
            }
        }

        private void ProcessRelations(DXUnitDefinitionUnit dxUnit)
        {
            this.ProcessDXElementsIndxUnitRelations(dxUnit);
            this.ProcessEnumRelations(dxUnit);
        }

        private void ProcessDXElementsIndxUnitRelations(DXUnitDefinitionUnit dxUnit)
        {
            if (dxUnit.DXElementInUnitDefinitionElement == null)
                return;

            var objectInfoFromDB = this.GetObjectInfoFromDB(dxUnit);

            if (objectInfoFromDB == null || dxUnit.DXElementInUnitDefinitionElement.Mode == MultiElementsMode.Target)
            {
                this.ProcessDXElementsIndxUnitRelationsUsingTragetMode(dxUnit);
            }
            else
            {
                this.ProcessDXElementsIndxUnitRelationsUsingFullMode(dxUnit, objectInfoFromDB);
            }
        }

        private DXUnitDefinitionUnit GetObjectInfoFromDB(DXUnitDefinitionUnit objectInfoIncome)
        {
            if (systemObjectNames.Contains(objectInfoIncome.DXObjectDefinitionMainElement.Name, StringComparer.OrdinalIgnoreCase))
                return null;

            return genericRepo.GetDXUnit<DXUnitDefinitionUnit>(objectInfoIncome.ID);
        }

        private void ProcessDXElementsIndxUnitRelationsUsingFullMode(DXUnitDefinitionUnit dxUnit, DXUnitDefinitionUnit existingdxUnit)
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

        private void ProcessDXElementsIndxUnitRelationsUsingTragetMode(DXUnitDefinitionUnit dxUnit)
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

                dxUnitService.InsertAsync(this.GetRelationObject(dxUnit, dxElementToAssign, relationType)).Wait();
            }
        }

        private void UnassingDXElements(DXUnitDefinitionUnit dxUnit, IEnumerable<DXElementDefinitionUnit> dxElementsToUnassign)
        {
            foreach (var dxElementToUnassign in dxElementsToUnassign)
            {
                var existingDXElement = this.GetExistingRelatonObject(dxUnit, dxElementToUnassign);

                if (existingDXElement == null)
                    continue;

                dxUnitService.DeleteAsync(this.GetExistingRelatonObject(dxUnit, dxElementToUnassign)).Wait();
            }
        }

        private DXRelationDefinitionUnit GetRelationObject(DXUnitDefinitionUnit dxUnit, DXElementDefinitionUnit dxElement, DXElementInUnitTypeEnum relationType)
        {
            var result = this.GetRelationObject(dxUnit, dxElement);

            result.DXRelationDefinitionMainElement.RelationType = this.ConvertDXElementIndxUnitRelationTypeToCommonRelationType(relationType);

            return result;
        }

        private DXRelationDefinitionUnit GetRelationObject(DXUnitDefinitionUnit dxUnit, DXElementDefinitionUnit dxElement)
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
                    Kind = dxUnit.DXObjectDefinitionMainElement.Kind
                }
            };
        }

        private DXRelationDefinitionUnit GetExistingRelatonObject(DXUnitDefinitionUnit dxUnit, DXElementDefinitionUnit dxElement)
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
