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
            base.Process(dxUnit, ctx);

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
            base.Process(dxUnit, ctx);

            dataStructureRepo.UpdatedDataStructure(dxUnit);

            var objectInfoFromDB = this.GetObjectInfoFromDB<DXUnitDefinitionUnit>(dxUnit, ctx);

            await this.ProcessRelationsAsync(dxUnit, objectInfoFromDB, ct);

            dataStructureRepo.UpdateUniqueColumns(dxUnit);

            return DXResult<DXUnitDefinitionUnit>.OkContinue(dxUnit);
        }

        public async Task<DXResult<DXUnitDefinitionUnit>> BeforeDeleteAsync(DXUnitDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit, ctx);

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
            await DeleteDXUnitToUnitRelationElements(existingDXUnit, ctx, ct);
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

        private async Task DeleteDXUnitToUnitRelationElements(DXUnitDefinitionUnit existingDXUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            foreach (var dxUnitRelation in existingDXUnit.DXUnitToUnitRelationElement.Announced)
            {
                var dxUnitToUnassign = genericRepo.GetDXUnit<DXUnitDefinitionUnit>(dxUnitRelation.TargetDXUnit);

                this.UnassingDXUnit(existingDXUnit, dxUnitRelation, dxUnitToUnassign);
                this.DeleteRevertedDXUnitToUnitRelationElement(dxUnitRelation, dxUnitToUnassign);
            }
        }

        private async Task ProcessRelationsAsync(DXUnitDefinitionUnit dxUnit, DXUnitDefinitionUnit? dxUnitExisting, CancellationToken ct)
        {
            this.ProcessDXElementsInDXUnitElements(dxUnit, dxUnitExisting);
            this.ProcessDXUnitToUnitRelationElements(dxUnit, dxUnitExisting);
            await this.ProcessEnumRelationsAsync(dxUnit, dxUnitExisting, ct);
        }

        private void ProcessDXUnitToUnitRelationElements(DXUnitDefinitionUnit dxUnit, DXUnitDefinitionUnit? dxUnitExisting)
        {
            if (dxUnit.DXElementInUnitDefinitionElement == null)
                return;

            if (dxUnitExisting == null || dxUnit.DXUnitToUnitRelationElement.Mode == MultiElementsMode.Target)
            {
                this.ProcessDXUnitToUnitRelationElementsUsingTargetMode(dxUnit);
            }
            else
            {
                this.ProcessDXUnitToUnitRelationElementsUsingFullMode(dxUnit, dxUnitExisting);
            }
        }

        private void ProcessDXUnitToUnitRelationElementsUsingFullMode(DXUnitDefinitionUnit dxUnit, DXUnitDefinitionUnit existingdxUnit)
        {
            var newAnnouncedIds = dxUnit.DXUnitToUnitRelationElement.Announced.Select(x => x.TargetDXUnit);
            var existingAnnouncedIds = existingdxUnit.DXUnitToUnitRelationElement.Announced.Select(x => x.TargetDXUnit);

            var announcedIds = newAnnouncedIds.Except(existingAnnouncedIds);
            var deletedIds = existingAnnouncedIds.Except(newAnnouncedIds);

            var dxUnitsToUnassign = dataStructureRepo.GetDXUnitDefinitions(deletedIds);

            foreach (var announcedId in announcedIds)
            {
                var dxUnitRelation = dxUnit.DXUnitToUnitRelationElement.Announced.Single(x => x.TargetDXUnit == announcedId);

                var dxUnitToAssign = dataStructureRepo.GetDXUnitDefinition(announcedId);

                this.AssignDXUnit(dxUnit, dxUnitRelation, dxUnitToAssign);
                this.CreateRevertedDXUnitToUnitRelationElement(dxUnitRelation);
            }

            foreach (var deletedId in deletedIds)
            {
                var dxUnitRelation = existingdxUnit.DXUnitToUnitRelationElement.Announced.Single(x => x.TargetDXUnit == deletedId);

                var dxUnitToUnassign = dataStructureRepo.GetDXUnitDefinition(deletedId);

                this.UnassingDXUnit(dxUnit, dxUnitRelation, dxUnitToUnassign);
                this.DeleteRevertedDXUnitToUnitRelationElement(dxUnitRelation, dxUnitToUnassign);
            }
        }

        private void ProcessDXUnitToUnitRelationElementsUsingTargetMode(DXUnitDefinitionUnit dxUnit)
        {
            foreach (var announced in dxUnit.DXUnitToUnitRelationElement.Announced)
            {
                var announcedId = announced.TargetDXUnit;

                var dxUnitToAssign = dataStructureRepo.GetDXUnitDefinition(announcedId);

                this.AssignDXUnit(dxUnit, announced, dxUnitToAssign);
                this.CreateRevertedDXUnitToUnitRelationElement(announced);
            }

            foreach (var deleted in dxUnit.DXUnitToUnitRelationElement.Deleted)
            {
                var deletedId = deleted.TargetDXUnit;

                var dxUnitToUnassign = dataStructureRepo.GetDXUnitDefinition(deletedId);

                this.UnassingDXUnit(dxUnit, deleted, dxUnitToUnassign);
                this.DeleteRevertedDXUnitToUnitRelationElement(deleted, dxUnitToUnassign);
            }
        }

        private void CreateRevertedDXUnitToUnitRelationElement(DXUnitToUnitRelationElement DXUnitToUnitRelationElement)
        {
            var revertedDXUnitToUnitRelationElement = DXUnitToUnitRelationElement.GetReverted();

            revertedDXUnitToUnitRelationElement.ID = Guid.NewGuid();
            revertedDXUnitToUnitRelationElement.DXUnitID = DXUnitToUnitRelationElement.TargetDXUnit;

            dxElementGenericRepo.Insert("DXUnitDefinitionUnit", revertedDXUnitToUnitRelationElement);
        }

        private void DeleteRevertedDXUnitToUnitRelationElement(DXUnitToUnitRelationElement DXUnitToUnitRelationElement, DXUnitDefinitionUnit relatedDXUnit)
        {
            var revertedDXElementToDelete =
                relatedDXUnit.DXUnitToUnitRelationElement.Announced.SingleOrDefault(x => x.TargetDXUnit == DXUnitToUnitRelationElement.DXUnitID);

            dxElementGenericRepo.Delete(revertedDXElementToDelete);
        }

        private void AssignDXUnit(DXUnitDefinitionUnit dxUnit, DXUnitToUnitRelationElement DXUnitToUnitRelationElement, DXUnitDefinitionUnit dxUnitToAssign)
        {
            var relationType = dxUnit.DXUnitToUnitRelationElement.Announced.Single(x => x.TargetDXUnit == dxUnitToAssign.ID).RelationType;

            var dxRelation = this.GetDXUnitRelationObject(dxUnit, DXUnitToUnitRelationElement, dxUnitToAssign, relationType);

            dxUnitService.InsertAsync(dxRelation).Wait();
        }

        private void UnassingDXUnit(DXUnitDefinitionUnit dxUnit, DXUnitToUnitRelationElement DXUnitToUnitRelationElement, DXUnitDefinitionUnit dxUnitToUnassign)
        {
            var existingDXUnit = this.GetExistingDXUnitRelationObject(dxUnit, DXUnitToUnitRelationElement, dxUnitToUnassign);

            if (existingDXUnit == null)
                return;

            var dxRelation = this.GetExistingDXUnitRelationObject(dxUnit, DXUnitToUnitRelationElement, dxUnitToUnassign);

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
            };
        }

        private DXRelationDefinitionUnit GetDXUnitRelationObject(
            DXUnitDefinitionUnit dxUnit,
            DXUnitToUnitRelationElement DXUnitToUnitRelationElement,
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
                        relationColumnNameLeft = DXUnitToUnitRelationElement.TargetRelationName;
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
                        relationColumnNameRight = DXUnitToUnitRelationElement.OwnRelationName;
                        relationColumnTypeRight = DXColumnTypeEnum.GUID;
                    }
                    break;
            }

            return new DXRelationDefinitionUnit()
            {
                ID = id,
                ObjectNameLeft = dxUnit.Name,
                RelationNameLeft = DXUnitToUnitRelationElement.OwnRelationName,
                ObjectNameRight = dxUnitRelated.Name,
                RelationNameRight = DXUnitToUnitRelationElement.TargetRelationName,
                Kind = dxUnit.Kind,
                RelationType = relationType,
                RelationColumnNameRight = relationColumnNameRight,
                RelationColumnTypeRight = relationColumnTypeRight,
                RelationColumnNameLeft = relationColumnNameLeft,
                RelationColumnTypeLeft = relationColumnTypeLeft,
                RelationTable = relationTable
            };
        }

        private DXRelationDefinitionUnit GetExistingDXUnitRelationObject(
            DXUnitDefinitionUnit dxUnit,
            DXUnitToUnitRelationElement DXUnitToUnitRelationElement,
            DXUnitDefinitionUnit dxUnitRelated)
        {
            var query = $"ObjectNameLeft = '{dxUnit.Name}' " +
               $"AND ObjectNameRight = '{dxUnitRelated.Name}' " +
               $"AND RelationNameLeft = '{DXUnitToUnitRelationElement.OwnRelationName}' " +
               $"AND RelationNameRight = '{DXUnitToUnitRelationElement.TargetRelationName}'";

            var items = genericRepo.GetDXUnits<DXRelationDefinitionUnit>(query);

            return items.SingleOrDefault();
        }

        private DXRelationDefinitionUnit GetExistingDXElementInDXUnitRelationObject(DXUnitDefinitionUnit dxUnit, DXElementDefinitionUnit dxElement)
        {
            var query = $"ObjectNameLeft = '{dxUnit.Name}' " +
               $"AND ObjectNameRight = '{dxElement.Name}' " +
               $"AND RelationNameLeft = '{dxUnit.Name}ID' " +
               $"AND RelationNameRight = '{dxElement.Name}'";

            var items = genericRepo.GetDXUnits<DXRelationDefinitionUnit>(query);

            return items.SingleOrDefault();
        }
    }
}
