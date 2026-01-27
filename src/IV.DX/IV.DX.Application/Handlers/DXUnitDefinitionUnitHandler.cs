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

        public async Task<DXResult> AfterUpdateAsync(DXUnitDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }

        public async Task<DXResult<DXUnitDefinitionUnit>> BeforeDeleteAsync(DXUnitDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit, ctx);

            await this.DeleteRelationsAsync(dxUnit, ctx, ct);

            dataStructureRepo.DropDataStructure(dxUnit);

            return DXResult<DXUnitDefinitionUnit>.OkContinue(dxUnit);
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
            await DeleteDXUnitToElementRelationElements(existingDXUnit, ctx, ct);
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

        private async Task DeleteDXUnitToElementRelationElements(DXUnitDefinitionUnit existingDXUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            foreach (var dxUnitRelation in existingDXUnit.DXUnitToElementRelationElement.Announced)
            {
                var dxElementToUnassign = genericRepo.GetDXUnit<DXElementDefinitionUnit>(dxUnitRelation.TargetDXElement);

                this.UnassingDXElement(existingDXUnit, dxUnitRelation, dxElementToUnassign);
                this.DeleteRevertedDXUnitToElementRelationElement(dxUnitRelation, dxElementToUnassign);
            }
        }

        private async Task ProcessRelationsAsync(DXUnitDefinitionUnit dxUnit, DXUnitDefinitionUnit? dxUnitExisting, CancellationToken ct)
        {
            this.ProcessDXElementsInDXUnitElements(dxUnit, dxUnitExisting);
            this.ProcessDXUnitToUnitRelationElements(dxUnit, dxUnitExisting);
            this.ProcessDXUnitToElementRelationElements(dxUnit, dxUnitExisting);
            await this.ProcessEnumRelationsAsync(dxUnit, dxUnitExisting, ct);
        }

        private void ProcessDXUnitToElementRelationElements(DXUnitDefinitionUnit dxUnit, DXUnitDefinitionUnit? dxUnitExisting)
        {
            if (dxUnit.DXUnitToElementRelationElement == null)
                return;

            if (dxUnitExisting == null || dxUnit.DXUnitToElementRelationElement.Mode == MultiElementsMode.Target)
            {
                this.ProcessDXUnitToElementRelationElementsUsingTargetMode(dxUnit);
            }
            else
            {
                this.ProcessDXUnitToElementRelationElementsUsingFullMode(dxUnit, dxUnitExisting);
            }
        }

        private void ProcessDXUnitToElementRelationElementsUsingTargetMode(DXUnitDefinitionUnit dxUnit)
        {
            foreach (var announced in dxUnit.DXUnitToElementRelationElement.Announced)
            {
                var announcedId = announced.TargetDXElement;

                var dxElementToAssign = dataStructureRepo.GetDXElementDefinition(announcedId);

                this.AssignDXElement(dxUnit, announced, dxElementToAssign);
                this.CreateRevertedDXUnitToElementRelationElement(announced);
            }

            foreach (var deleted in dxUnit.DXUnitToElementRelationElement.Deleted)
            {
                var deletedId = deleted.TargetDXElement;

                var dxElementToUnassign = dataStructureRepo.GetDXElementDefinition(deletedId);

                this.UnassingDXElement(dxUnit, deleted, dxElementToUnassign);
                this.DeleteRevertedDXUnitToElementRelationElement(deleted, dxElementToUnassign);
            }

        }

        private void ProcessDXUnitToUnitRelationElements(DXUnitDefinitionUnit dxUnit, DXUnitDefinitionUnit? dxUnitExisting)
        {
            if (dxUnit.DXUnitToUnitRelationElement == null)
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

        private void ProcessDXUnitToElementRelationElementsUsingFullMode(DXUnitDefinitionUnit dxUnit, DXUnitDefinitionUnit existingdxUnit)
        {
            var newAnnouncedIds = dxUnit.DXUnitToElementRelationElement.Announced.Select(x => x.TargetDXElement);
            var existingAnnouncedIds = existingdxUnit.DXUnitToElementRelationElement.Announced.Select(x => x.TargetDXElement);

            var announcedIds = newAnnouncedIds.Except(existingAnnouncedIds);
            var deletedIds = existingAnnouncedIds.Except(newAnnouncedIds);

            var dxElementsToUnassign = dataStructureRepo.GetDXElementDefinitions(deletedIds);

            foreach (var announcedId in announcedIds)
            {
                var dxUnitRelation = dxUnit.DXUnitToElementRelationElement.Announced.Single(x => x.TargetDXElement == announcedId);

                var dxElementToAssign = dataStructureRepo.GetDXElementDefinition(announcedId);

                this.AssignDXElement(dxUnit, dxUnitRelation, dxElementToAssign);
                this.CreateRevertedDXUnitToElementRelationElement(dxUnitRelation);
            }

            foreach (var deletedId in deletedIds)
            {
                var dxUnitRelation = existingdxUnit.DXUnitToElementRelationElement.Announced.Single(x => x.TargetDXElement == deletedId);

                var dxElementToUnassign = dataStructureRepo.GetDXElementDefinition(deletedId);

                this.UnassingDXElement(dxUnit, dxUnitRelation, dxElementToUnassign);
                this.DeleteRevertedDXUnitToElementRelationElement(dxUnitRelation, dxElementToUnassign);
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

        private void CreateRevertedDXUnitToUnitRelationElement(DXUnitToUnitRelationElement dxUnitToUnitRelationElement)
        {
            var revertedDXUnitToUnitRelationElement = dxUnitToUnitRelationElement.GetReverted();

            revertedDXUnitToUnitRelationElement.ID = Guid.NewGuid();
            revertedDXUnitToUnitRelationElement.DXUnitID = dxUnitToUnitRelationElement.TargetDXUnit;

            dxElementGenericRepo.Insert("DXUnitDefinitionUnit", revertedDXUnitToUnitRelationElement);
        }

        private void CreateRevertedDXUnitToElementRelationElement(DXUnitToElementRelationElement dxUnitToElementRelationElement)
        {
            var revertedDXUnitToUnitRelationElement = dxUnitToElementRelationElement.GetReverted();

            revertedDXUnitToUnitRelationElement.ID = Guid.NewGuid();
            revertedDXUnitToUnitRelationElement.DXUnitID = dxUnitToElementRelationElement.TargetDXElement;

            dxElementGenericRepo.Insert("DXElementDefinitionUnit", revertedDXUnitToUnitRelationElement);
        }

        private void DeleteRevertedDXUnitToUnitRelationElement(DXUnitToUnitRelationElement DXUnitToUnitRelationElement, DXUnitDefinitionUnit relatedDXUnit)
        {
            var revertedDXElementToDelete =
                relatedDXUnit.DXUnitToUnitRelationElement.Announced.SingleOrDefault(x => x.TargetDXUnit == DXUnitToUnitRelationElement.DXUnitID);

            dxElementGenericRepo.Delete(revertedDXElementToDelete);
        }

        private void DeleteRevertedDXUnitToElementRelationElement(DXUnitToElementRelationElement dxUnitToElementRelationElement, DXElementDefinitionUnit relatedDXElement)
        {
            var revertedDXElementToDelete =
                relatedDXElement.DXElementToUnitRelationElement.Announced.SingleOrDefault(x => x.TargetDXUnit == dxUnitToElementRelationElement.DXUnitID);

            dxElementGenericRepo.Delete(revertedDXElementToDelete);
        }

        private void AssignDXUnit(DXUnitDefinitionUnit dxUnit, DXUnitToUnitRelationElement dxUnitToUnitRelationElement, DXUnitDefinitionUnit dxUnitToAssign)
        {
            var relationType = dxUnit.DXUnitToUnitRelationElement.Announced.Single(x => x.TargetDXUnit == dxUnitToAssign.ID).RelationType;

            var dxRelation = this.GetDXUnitToUnitRelationObject(dxUnit, dxUnitToUnitRelationElement, dxUnitToAssign, relationType);

            dxUnitService.InsertAsync(dxRelation).Wait();
        }

        private void AssignDXElement(DXUnitDefinitionUnit dxUnit, DXUnitToElementRelationElement dxUnitToElementRelationElement, DXElementDefinitionUnit dxElementToAssign)
        {
            var relationType = dxUnit.DXUnitToElementRelationElement.Announced.Single(x => x.TargetDXElement == dxElementToAssign.ID).RelationType;

            var dxRelation = this.GetDXUnitToElementRelationObject(dxUnit, dxUnitToElementRelationElement, dxElementToAssign, relationType);

            dxUnitService.InsertAsync(dxRelation).Wait();
        }

        private void UnassingDXUnit(DXUnitDefinitionUnit dxUnit, DXUnitToUnitRelationElement dxUnitToUnitRelationElement, DXUnitDefinitionUnit dxUnitToUnassign)
        {
            var existingDXUnit = this.GetExistingDXUnitToUnitRelationObject(dxUnit, dxUnitToUnitRelationElement, dxUnitToUnassign);

            if (existingDXUnit == null)
                return;

            var dxRelation = this.GetExistingDXUnitToUnitRelationObject(dxUnit, dxUnitToUnitRelationElement, dxUnitToUnassign);

            dxUnitService.DeleteAsync(dxRelation).Wait();
        }

        private void UnassingDXElement(DXUnitDefinitionUnit dxUnit, DXUnitToElementRelationElement dxUnitToElementRelationElement, DXElementDefinitionUnit dxElementToUnassign)
        {
            var existingDXUnit = this.GetExistingDXUnitToElementRelationObject(dxUnit, dxUnitToElementRelationElement, dxElementToUnassign);

            if (existingDXUnit == null)
                return;

            var dxRelation = this.GetExistingDXUnitToElementRelationObject(dxUnit, dxUnitToElementRelationElement, dxElementToUnassign);

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

        private DXRelationDefinitionUnit GetDXUnitToUnitRelationObject(
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

        private DXRelationDefinitionUnit GetDXUnitToElementRelationObject(
            DXUnitDefinitionUnit dxUnit,
            DXUnitToElementRelationElement DXUnitToUnitRelationElement,
            DXElementDefinitionUnit dxElementRelated,
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
                    relationTable = $"Relation_{dxUnit.Name}_{dxElementRelated.Name}";
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
                ObjectNameRight = dxElementRelated.Name,
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


        private DXRelationDefinitionUnit GetExistingDXUnitToUnitRelationObject(
            DXUnitDefinitionUnit dxUnit,
            DXUnitToUnitRelationElement dxUnitToUnitRelationElement,
            DXUnitDefinitionUnit dxUnitRelated)
        {
            var query = $"ObjectNameLeft = '{dxUnit.Name}' " +
               $"AND ObjectNameRight = '{dxUnitRelated.Name}' " +
               $"AND RelationNameLeft = '{dxUnitToUnitRelationElement.OwnRelationName}' " +
               $"AND RelationNameRight = '{dxUnitToUnitRelationElement.TargetRelationName}'";

            var items = genericRepo.GetDXUnits<DXRelationDefinitionUnit>(query);

            return items.SingleOrDefault();
        }

        private DXRelationDefinitionUnit GetExistingDXUnitToElementRelationObject(
            DXUnitDefinitionUnit dxUnit,
            DXUnitToElementRelationElement dxUnitToElementRelationElement,
            DXElementDefinitionUnit dxElementRelated)
        {
            var query = $"ObjectNameLeft = '{dxUnit.Name}' " +
               $"AND ObjectNameRight = '{dxElementRelated.Name}' " +
               $"AND RelationNameLeft = '{dxUnitToElementRelationElement.OwnRelationName}' " +
               $"AND RelationNameRight = '{dxUnitToElementRelationElement.TargetRelationName}'";

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
