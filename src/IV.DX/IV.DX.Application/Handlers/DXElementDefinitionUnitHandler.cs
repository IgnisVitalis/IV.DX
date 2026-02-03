using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXElementDefinitionUnitHandler(
        IDXUnitDataService dxUnitService,
        IDXStructureRepository dataStructureRepo,
        IDXUnitGenericRepository genericRepo,
        IDXElementGenericRepository dxElementGenericRepo,
        IDXStructureCache dxStructureCache) :
        DXObjectDefinitionUnitHandler(dxUnitService, dataStructureRepo, genericRepo, dxElementGenericRepo),
        IDXBeforeInsertHandler<DXElementDefinitionUnit>, IDXUniqueBeforeInsertHandler,
        IDXBeforeUpdateHandler<DXElementDefinitionUnit>, IDXUniqueBeforeUpdateHandler,
        IDXBeforeDeleteHandler<DXElementDefinitionUnit>, IDXUniqueBeforeDeleteHandler,
        IDXAfterInsertHandler<DXElementDefinitionUnit>, IDXUniqueAfterInsertHandler,
        IDXAfterUpdateHandler<DXElementDefinitionUnit>, IDXUniqueAfterUpdateHandler,
        IDXAfterDeleteHandler<DXElementDefinitionUnit>, IDXUniqueAfterDeleteHandler
    {
        public int BeforeOrder => 1;

        public int AfterOrder => 1;

        public async Task<DXResult<DXElementDefinitionUnit>> BeforeInsertAsync(DXElementDefinitionUnit dxElement, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            base.Validate(dxElement);
            base.Process(dxElement, ctx);

            if (ctx is DXUnitHandlerPreInitCoreContext)
            {
                dataStructureRepo.CreateDataStructure(dxElement);
                dataStructureRepo.UpdateUniqueColumns(dxElement);

                return DXResult<DXElementDefinitionUnit>.OkSkipProcess(dxElement);
            }
            else if (ctx is DXUnitHandlerPostInitCoreContext)
            {
                return DXResult<DXElementDefinitionUnit>.OkContinue(dxElement);
            }
            else
            {
                dataStructureRepo.CreateDataStructure(dxElement);

                return DXResult<DXElementDefinitionUnit>.OkContinue(dxElement);
            }
        }

        public async Task<DXResult> AfterInsertAsync(DXElementDefinitionUnit dxElement, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            if (ctx is DXUnitHandlerPreInitCoreContext)
            {

            }
            else if (ctx is DXUnitHandlerPostInitCoreContext)
            {

            }
            else
            {
                // It is necessary to process relations after insert because dxElement should already existing in db to set relations between dxUnits and dxElements.
                // It is urgent to use original income dxElement.
                var originalIncomeDXElement = ctx.OriginalItem as DXElementDefinitionUnit;
                await this.ProcessRelationsAsync(originalIncomeDXElement, null, ct);

                // it is urgent to update unique columns at the end because relation column can be part of unique columns.
                dataStructureRepo.UpdateUniqueColumns(dxElement);
            }

            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }


        public async Task<DXResult<DXElementDefinitionUnit>> BeforeUpdateAsync(DXElementDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit, ctx);

            dataStructureRepo.UpdatedDataStructure(dxUnit);

            var existingDXUnit = genericRepo.GetDXUnit<DXElementDefinitionUnit>(dxUnit.ID);

            await this.ProcessRelationsAsync(dxUnit, existingDXUnit, ct);

            return DXResult<DXElementDefinitionUnit>.OkContinue(dxUnit);
        }

        public async Task<DXResult<DXElementDefinitionUnit>> BeforeDeleteAsync(DXElementDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit, ctx);

            dataStructureRepo.DropDataStructure(dxUnit);

            return DXResult<DXElementDefinitionUnit>.OkContinue(dxUnit);
        }

        private async Task ProcessRelationsAsync(DXElementDefinitionUnit dxElement, DXElementDefinitionUnit? dxElementExisting, CancellationToken ct)
        {
            this.ProcessDXElementToUnitRelationElements(dxElement, dxElementExisting);
            await this.ProcessEnumRelationsAsync(dxElement, dxElementExisting, ct);
        }

        public async Task<DXResult> AfterUpdateAsync(DXElementDefinitionUnit dxElement, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }

        public async Task<DXResult> AfterDeleteAsync(DXElementDefinitionUnit dxElement, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }

        private void ProcessDXElementToUnitRelationElements(DXElementDefinitionUnit dxElement, DXElementDefinitionUnit? dxElementExisting)
        {
            if (dxElement.DXElementToUnitRelationElement == null)
                return;

            if (dxElementExisting == null || dxElement.DXElementToUnitRelationElement.Mode == MultiElementsMode.Target)
            {
                this.ProcessDXElementToUnitRelationElementsUsingTargetMode(dxElement);
            }
            else
            {
                this.ProcessDXElementToUnitRelationElementsUsingFullMode(dxElement, dxElementExisting);
            }
        }

        private void ProcessDXElementToUnitRelationElementsUsingFullMode(DXElementDefinitionUnit dxElement, DXElementDefinitionUnit dxElementExisting)
        {
            var newAnnouncedIds = dxElement.DXElementToUnitRelationElement.Announced.Select(x => x.TargetDXUnit);
            var existingAnnouncedIds = dxElementExisting.DXElementToUnitRelationElement.Announced.Select(x => x.TargetDXUnit);

            var announcedIds = newAnnouncedIds.Except(existingAnnouncedIds);
            var deletedIds = existingAnnouncedIds.Except(newAnnouncedIds);

            var dxElementsToUnassign = dataStructureRepo.GetDXElementDefinitions(deletedIds);

            foreach (var announcedId in announcedIds)
            {
                var dxElementToUnitRelation = dxElement.DXElementToUnitRelationElement.Announced.Single(x => x.TargetDXUnit == announcedId);

                var dxUnitToAssign = dataStructureRepo.GetDXUnitDefinition(announcedId);

                this.AssignDXUnit(dxElement, dxElementToUnitRelation, dxUnitToAssign);
                this.CreateRevertedDXElementToUnitRelationElement(dxElementToUnitRelation);
            }

            foreach (var deletedId in deletedIds)
            {
                var dxElementToUnitRelation = dxElementExisting.DXElementToUnitRelationElement.Announced.Single(x => x.TargetDXUnit == deletedId);

                var dxUnitToUnassign = dataStructureRepo.GetDXUnitDefinition(deletedId);

                this.UnassingDXUnit(dxElement, dxElementToUnitRelation, dxUnitToUnassign);
                this.DeleteRevertedDXElementToUnitRelationElement(dxElementToUnitRelation, dxUnitToUnassign);
            }
        }

        private void ProcessDXElementToUnitRelationElementsUsingTargetMode(DXElementDefinitionUnit dxElement)
        {
            foreach (var announced in dxElement.DXElementToUnitRelationElement.Announced)
            {
                var announcedId = announced.TargetDXUnit;

                var dxUnitToAssign = dataStructureRepo.GetDXUnitDefinition(announcedId);

                this.AssignDXUnit(dxElement, announced, dxUnitToAssign);
                this.CreateRevertedDXElementToUnitRelationElement(announced);
            }

            foreach (var deleted in dxElement.DXElementToUnitRelationElement.Deleted)
            {
                var deletedId = deleted.TargetDXUnit;

                var dxUnitToUnassign = dataStructureRepo.GetDXUnitDefinition(deletedId);

                this.UnassingDXUnit(dxElement, deleted, dxUnitToUnassign);
                this.DeleteRevertedDXElementToUnitRelationElement(deleted, dxUnitToUnassign);
            }
        }

        private void DeleteRevertedDXElementToUnitRelationElement(DXElementToUnitRelationElement dxElementToUnitRelationElement, DXUnitDefinitionUnit relatedDXUnit)
        {
            var revertedDXUnitToDelete =
                relatedDXUnit.DXUnitToElementRelationElement.Announced.SingleOrDefault(x => x.TargetDXElement == dxElementToUnitRelationElement.DXUnitID);

            dxElementGenericRepo.Delete(revertedDXUnitToDelete);
        }

        private void UnassingDXUnit(DXElementDefinitionUnit dxElement, DXElementToUnitRelationElement dxElementToUnitRelationElement, DXUnitDefinitionUnit dxUnitToUnassign)
        {
            var existingDXUnit = this.GetExistingDXElementToUnitRelationObject(dxElement, dxElementToUnitRelationElement, dxUnitToUnassign);

            if (existingDXUnit == null)
                return;

            var dxRelation = this.GetExistingDXElementToUnitRelationObject(dxElement, dxElementToUnitRelationElement, dxUnitToUnassign);

            dxUnitService.DeleteAsync(dxRelation).Wait();
        }

        private DXRelationDefinitionUnit GetExistingDXElementToUnitRelationObject(
            DXElementDefinitionUnit dxElement,
            DXElementToUnitRelationElement dxElementToUnitRelationElement,
            DXUnitDefinitionUnit dxUnitRelated)
        {
            var query = $"ObjectNameLeft = '{dxElement.Name}' " +
               $"AND ObjectNameRight = '{dxUnitRelated.Name}' " +
               $"AND RelationNameLeft = '{dxElementToUnitRelationElement.OwnRelationName}' " +
               $"AND RelationNameRight = '{dxElementToUnitRelationElement.TargetRelationName}'";

            var items = genericRepo.GetDXUnits<DXRelationDefinitionUnit>(query);

            return items.SingleOrDefault();
        }


        private void CreateRevertedDXElementToUnitRelationElement(DXElementToUnitRelationElement dxElementToUnitRelationElement)
        {
            var revertedDXUnitToUnitRelationElement = dxElementToUnitRelationElement.GetReverted();

            revertedDXUnitToUnitRelationElement.ID = Guid.NewGuid();
            revertedDXUnitToUnitRelationElement.DXUnitID = dxElementToUnitRelationElement.TargetDXUnit;

            dxElementGenericRepo.Insert("DXUnitDefinitionUnit", revertedDXUnitToUnitRelationElement);
        }

        private void AssignDXUnit(DXElementDefinitionUnit dxElement, DXElementToUnitRelationElement dxElementToUnitRelationElement, DXUnitDefinitionUnit dxUnitToAssign)
        {
            var existing = this.GetExistingDXElementToUnitRelationObject(dxElement, dxElementToUnitRelationElement, dxUnitToAssign);
            if (existing != null)
                return;

            var relationType = dxElement.DXElementToUnitRelationElement.Announced.Single(x => x.TargetDXUnit == dxUnitToAssign.ID).RelationType;

            var dxRelation = this.GetDXElementToUnitRelationObject(dxElement, dxElementToUnitRelationElement, dxUnitToAssign, relationType);

            dxUnitService.InsertAsync(dxRelation).Wait();
        }

        private DXRelationDefinitionUnit GetDXElementToUnitRelationObject(
            DXElementDefinitionUnit dxElement,
            DXElementToUnitRelationElement dxElementToUnitRelationElement,
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
                    relationTable = $"Relation_{dxElement.Name}_{dxUnitRelated.Name}";
                    relationColumnNameLeft = "ID";
                    relationColumnTypeLeft = DXColumnTypeEnum.GUID;
                    relationColumnNameRight = "ID";
                    relationColumnTypeRight = DXColumnTypeEnum.GUID;
                    break;
                case DXRelationTypeEnum.ManyToOne:
                case DXRelationTypeEnum.ManyToZeroOne:
                case DXRelationTypeEnum.ZeroOneToOne:
                    {
                        relationColumnNameLeft = dxElementToUnitRelationElement.TargetRelationName;
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
                        relationColumnNameRight = dxElementToUnitRelationElement.OwnRelationName;
                        relationColumnTypeRight = DXColumnTypeEnum.GUID;
                    }
                    break;
            }

            return new DXRelationDefinitionUnit()
            {
                ID = id,
                ObjectNameLeft = dxElement.Name,
                RelationNameLeft = dxElementToUnitRelationElement.OwnRelationName,
                ObjectNameRight = dxUnitRelated.Name,
                RelationNameRight = dxElementToUnitRelationElement.TargetRelationName,
                Kind = dxElement.Kind,
                RelationType = relationType,
                RelationColumnNameRight = relationColumnNameRight,
                RelationColumnTypeRight = relationColumnTypeRight,
                RelationColumnNameLeft = relationColumnNameLeft,
                RelationColumnTypeLeft = relationColumnTypeLeft,
                RelationTable = relationTable
            };
        }
    }
}
