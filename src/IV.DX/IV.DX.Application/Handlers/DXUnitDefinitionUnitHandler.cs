using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using System.Xml.Linq;

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

        public async Task<DXResult<DXUnitDefinitionUnit>> BeforeInsertAsync(DXUnitDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
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

                return DXResult<DXUnitDefinitionUnit>.OkContinue(dxUnit);
            }
        }

        public async Task<DXResult<DXUnitDefinitionUnit>> BeforeUpdateAsync(DXUnitDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit);

            dataStructureRepo.UpdatedDataStructure(dxUnit);

            var objectInfoFromDB = this.GetObjectInfoFromDB(dxUnit);

            await this.ProcessRelationsAsync(dxUnit, objectInfoFromDB, ct);

            dataStructureRepo.UpdateUniqueColumns(dxUnit);

            return DXResult<DXUnitDefinitionUnit>.OkContinue(dxUnit);
        }

        public async Task<DXResult<DXUnitDefinitionUnit>> BeforeDeleteAsync(DXUnitDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit);

            await this.DeleteRelationsAsync(dxUnit, ctx, ct);

            dataStructureRepo.DropDataStructure(dxUnit);

            return DXResult<DXUnitDefinitionUnit>.OkContinue(dxUnit);
        }

        public async Task<DXResult> AfterUpdateAsync(DXUnitDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }

        public async Task<DXResult> AfterInsertAsync(DXUnitDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            if (ctx is DXUnitHandlerPreInitCoreContext)
            {

            }
            else if (ctx is DXUnitHandlerPostInitCoreContext)
            {

            }
            else
            {
                // It is necessary to process relations after insert because dxUnit should already existing in db to set relations between dxUnits.
                // It is urgent to use original income dxUnit.
                var originalIncomeDXUnit = ctx.OriginalItem as DXUnitDefinitionUnit;
                await this.ProcessRelationsAsync(originalIncomeDXUnit, null, ct);

                // it is urgent to update unique columns at the end because relation column can be part of unique columns.
                dataStructureRepo.UpdateUniqueColumns(dxUnit);
            }

            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }

        public async Task<DXResult> AfterDeleteAsync(DXUnitDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }

        private async Task DeleteRelationsAsync(DXUnitDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            var existingDXUnit = genericRepo.GetDXUnit<DXUnitDefinitionUnit>(dxUnit.ID);

            if (existingDXUnit == null)
                return;

            await DeleteDXElementInUnitDefinitionElements(existingDXUnit, ctx, ct);
            await DeleteDXUnitRelationElements(existingDXUnit, ctx, ct);
        }

        private async Task DeleteDXElementInUnitDefinitionElements(DXUnitDefinitionUnit existingDXUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            var relatedDXElementIds = existingDXUnit.DXElementInUnitDefinitionElement.Announced.Select(x => x.DXElementDefinitionUnit).ToList();

            var relatedDXElements = dataStructureRepo.GetDXElementDefinitions(relatedDXElementIds);

            foreach (var relatedDXElement in relatedDXElements)
            {
                var dxRelationDefinition = this.GetExistingDXElementInDXUnitRelationObject(existingDXUnit, relatedDXElement);

                await dxUnitService.DeleteAsync(dxRelationDefinition, ctx, ct);
            }
        }

        private async Task DeleteDXUnitRelationElements(DXUnitDefinitionUnit existingDXUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            foreach (var dxUnitRelation in existingDXUnit.DXUnitRelationElement.Announced)
            {
                var dxUnitToUnassign = genericRepo.GetDXUnit<DXUnitDefinitionUnit>(dxUnitRelation.TargetDXUnit);

                this.UnassingDXUnit(existingDXUnit, dxUnitRelation, dxUnitToUnassign);
                this.DeleteRevertedDXUnitRelationElement(dxUnitRelation, dxUnitToUnassign);
            }
        }

        private async Task ProcessRelationsAsync(DXUnitDefinitionUnit dxUnit, DXUnitDefinitionUnit? dxUnitExisting, CancellationToken ct)
        {
            this.ProcessDXElementsInDXUnitElements(dxUnit, dxUnitExisting);
            this.ProcessDXUnitRelationElements(dxUnit, dxUnitExisting);
            await this.ProcessEnumRelationsAsync(dxUnit, dxUnitExisting, ct);
        }

        private void ProcessDXUnitRelationElements(DXUnitDefinitionUnit dxUnit, DXUnitDefinitionUnit? dxUnitExisting)
        {
            if (dxUnit.DXElementInUnitDefinitionElement == null)
                return;

            if (dxUnitExisting == null || dxUnit.DXUnitRelationElement.Mode == MultiElementsMode.Target)
            {
                this.ProcessDXUnitRelationElementsUsingTargetMode(dxUnit);
            }
            else
            {
                this.ProcessDXUnitRelationElementsUsingFullMode(dxUnit, dxUnitExisting);
            }
        }

        private void ProcessDXUnitRelationElementsUsingFullMode(DXUnitDefinitionUnit dxUnit, DXUnitDefinitionUnit existingdxUnit)
        {
            var newAnnouncedIds = dxUnit.DXUnitRelationElement.Announced.Select(x => x.TargetDXUnit);
            var existingAnnouncedIds = existingdxUnit.DXUnitRelationElement.Announced.Select(x => x.TargetDXUnit);

            var announcedIds = newAnnouncedIds.Except(existingAnnouncedIds);
            var deletedIds = existingAnnouncedIds.Except(newAnnouncedIds);

            var dxUnitsToUnassign = dataStructureRepo.GetDXUnitDefinitions(deletedIds);

            foreach (var announcedId in announcedIds)
            {
                var dxUnitRelation = dxUnit.DXUnitRelationElement.Announced.Single(x => x.TargetDXUnit == announcedId);

                var dxUnitToAssign = dataStructureRepo.GetDXUnitDefinition(announcedId);

                this.AssignDXUnit(dxUnit, dxUnitRelation, dxUnitToAssign);
                this.CreateRevertedDXUnitRelationElement(dxUnitRelation);
            }

            foreach (var deletedId in deletedIds)
            {
                var dxUnitRelation = existingdxUnit.DXUnitRelationElement.Announced.Single(x => x.TargetDXUnit == deletedId);

                var dxUnitToUnassign = dataStructureRepo.GetDXUnitDefinition(deletedId);

                this.UnassingDXUnit(dxUnit, dxUnitRelation, dxUnitToUnassign);
                this.DeleteRevertedDXUnitRelationElement(dxUnitRelation, dxUnitToUnassign);
            }
        }

        private void ProcessDXUnitRelationElementsUsingTargetMode(DXUnitDefinitionUnit dxUnit)
        {
            foreach (var announced in dxUnit.DXUnitRelationElement.Announced)
            {
                var announcedId = announced.TargetDXUnit;

                var dxUnitToAssign = dataStructureRepo.GetDXUnitDefinition(announcedId);

                this.AssignDXUnit(dxUnit, announced, dxUnitToAssign);
                this.CreateRevertedDXUnitRelationElement(announced);
            }

            foreach (var deleted in dxUnit.DXUnitRelationElement.Deleted)
            {
                var deletedId = deleted.TargetDXUnit;

                var dxUnitToUnassign = dataStructureRepo.GetDXUnitDefinition(deletedId);

                this.UnassingDXUnit(dxUnit, deleted, dxUnitToUnassign);
                this.DeleteRevertedDXUnitRelationElement(deleted, dxUnitToUnassign);
            }
        }

        private void CreateRevertedDXUnitRelationElement(DXUnitRelationElement dxUnitRelationElement)
        {
            var revertedDXUnitRelationElement = dxUnitRelationElement.GetReverted();

            revertedDXUnitRelationElement.ID = Guid.NewGuid();
            revertedDXUnitRelationElement.DXUnitID = dxUnitRelationElement.TargetDXUnit;

            dxElementGenericRepo.Insert("DXUnitDefinitionUnit", revertedDXUnitRelationElement);
        }

        private void DeleteRevertedDXUnitRelationElement(DXUnitRelationElement dxUnitRelationElement, DXUnitDefinitionUnit relatedDXUnit)
        {
            var revertedDXElementToDelete =
                relatedDXUnit.DXUnitRelationElement.Announced.SingleOrDefault(x => x.TargetDXUnit == dxUnitRelationElement.DXUnitID);

            dxElementGenericRepo.Delete(revertedDXElementToDelete);
        }

        private void AssignDXUnit(DXUnitDefinitionUnit dxUnit, DXUnitRelationElement dxUnitRelationElement, DXUnitDefinitionUnit dxUnitToAssign)
        {
            var relationType = dxUnit.DXUnitRelationElement.Announced.Single(x => x.TargetDXUnit == dxUnitToAssign.ID).RelationType;

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

        private void ProcessDXElementsInDXUnitElements(DXUnitDefinitionUnit dxUnit, DXUnitDefinitionUnit? dxUnitExisting)
        {
            if (dxUnit.DXElementInUnitDefinitionElement == null)
                return;

            if (dxUnitExisting == null || dxUnit.DXElementInUnitDefinitionElement.Mode == MultiElementsMode.Target)
            {
                this.ProcessDXElementsInDXUnitElementsUsingTargetMode(dxUnit);
            }
            else
            {
                this.ProcessDXElementsInDXUnitElementsUsingFullMode(dxUnit, dxUnitExisting);
            }
        }

        private DXUnitDefinitionUnit GetObjectInfoFromDB(DXUnitDefinitionUnit objectInfoIncome)
        {
            if (SystemObjectNames.Contains(objectInfoIncome.Name, StringComparer.OrdinalIgnoreCase))
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

        private void ProcessDXElementsInDXUnitElementsUsingTargetMode(DXUnitDefinitionUnit dxUnit)
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
            var relationType = dxElementInUnitRelationType.ToDXRelationTypeEnum();
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
                    ObjectNameLeft = dxUnit.Name,
                    RelationNameLeft = $"{dxUnit.Name}ID",
                    ObjectNameRight = dxElement.Name,
                    RelationNameRight = dxElement.Name,
                    RelationColumnNameLeft = "ID",
                    RelationColumnNameRight = $"{dxUnit.Name}ID",
                    RelationColumnTypeLeft = DXColumnTypeEnum.GUID,
                    RelationColumnTypeRight = DXColumnTypeEnum.GUID,
                    Kind = dxUnit.Kind,
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

            string relationColumnNameLeft = null;
            string relationColumnNameRight = null;
            DXColumnTypeEnum? relationColumnTypeLeft = null;
            DXColumnTypeEnum? relationColumnTypeRight = null;
            string relationTable = null;

            switch (relationType)
            {
                case DXRelationTypeEnum.ManyToMany:
                    relationTable = $"Relation_{dxUnit.Name}_{dxUnitRelated.Name}";
                    relationColumnNameLeft = "ID";
                    relationColumnTypeLeft = DXColumnTypeEnum.GUID;
                    relationColumnNameRight = "ID";
                    relationColumnTypeRight = DXColumnTypeEnum.GUID;
                    break;
                case DXRelationTypeEnum.ManyToOne:
                case DXRelationTypeEnum.ManyToZeroOne:
                case DXRelationTypeEnum.ZeroOneToOne:
                    {
                        relationColumnNameLeft = dxUnitRelationElement.TargetRelationName;
                        relationColumnTypeLeft = DXColumnTypeEnum.GUID;
                        relationColumnNameRight = "ID";
                        relationColumnTypeRight = DXColumnTypeEnum.GUID;
                    }
                    break;
                case DXRelationTypeEnum.OneToMany:
                case DXRelationTypeEnum.ZeroOneToMany:
                case DXRelationTypeEnum.OneToZeroOne:
                case DXRelationTypeEnum.ZeroOneToZeroOne:
                    {
                        relationColumnNameLeft = "ID";
                        relationColumnTypeLeft = DXColumnTypeEnum.GUID;
                        relationColumnNameRight = dxUnitRelationElement.OwnRelationName;
                        relationColumnTypeRight = DXColumnTypeEnum.GUID;
                    }
                    break;
            }

            return new DXRelationDefinitionUnit()
            {
                ID = id,
                DXRelationDefinitionMainElement = new DXRelationDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    DXUnitID = id,
                    ObjectNameLeft = dxUnit.Name,
                    RelationNameLeft = dxUnitRelationElement.OwnRelationName,
                    ObjectNameRight = dxUnitRelated.Name,
                    RelationNameRight = dxUnitRelationElement.TargetRelationName,
                    Kind = dxUnit.Kind,
                    RelationType = relationType,
                    RelationColumnNameRight = relationColumnNameRight,
                    RelationColumnTypeRight = relationColumnTypeRight,
                    RelationColumnNameLeft = relationColumnNameLeft,
                    RelationColumnTypeLeft = relationColumnTypeLeft,
                    RelationTable = relationTable
                }
            };
        }

        private DXRelationDefinitionUnit GetExistingDXUnitRelationObject(
            DXUnitDefinitionUnit dxUnit,
            DXUnitRelationElement dxUnitRelationElement,
            DXUnitDefinitionUnit dxUnitRelated)
        {
            var query = $"DXRelationDefinitionMainElement.ObjectNameLeft = '{dxUnit.Name}' " +
               $"AND DXRelationDefinitionMainElement.ObjectNameRight = '{dxUnitRelated.Name}' " +
               $"AND DXRelationDefinitionMainElement.RelationNameLeft = '{dxUnitRelationElement.OwnRelationName}' " +
               $"AND DXRelationDefinitionMainElement.RelationNameRight = '{dxUnitRelationElement.TargetRelationName}'";

            var items = genericRepo.GetDXUnits<DXRelationDefinitionUnit>(query);

            return items.SingleOrDefault();
        }

        private DXRelationDefinitionUnit GetExistingDXElementInDXUnitRelationObject(DXUnitDefinitionUnit dxUnit, DXElementDefinitionUnit dxElement)
        {
            var query = $"DXRelationDefinitionMainElement.ObjectNameLeft = '{dxUnit.Name}' " +
               $"AND DXRelationDefinitionMainElement.ObjectNameRight = '{dxElement.Name}' " +
               $"AND DXRelationDefinitionMainElement.RelationNameLeft = '{dxUnit.Name}ID' " +
               $"AND DXRelationDefinitionMainElement.RelationNameRight = '{dxElement.Name}'";

            var items = genericRepo.GetDXUnits<DXRelationDefinitionUnit>(query);

            return items.SingleOrDefault();
        }
    }
}
