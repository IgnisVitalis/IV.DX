using IV.DX.Kernel;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Abstractions;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Persistence.CoreData;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.Common;

namespace IV.DX.Persistence
{
    internal partial class CoreRepository : IDXCoreRepository, IDXStructureRepository, IDXEnumCoreRepository
    {
        protected string _connectionStr;
        protected IDXSQLQueryHelper _queryHelper;

        public CoreRepository(
            IConfiguration configuration,
            IDXSQLQueryHelper queryHelper)
        {
            this._connectionStr = configuration["Database:ConnectionString"];
            this._queryHelper = queryHelper;

            this._blockInfos = DXElementDefinitionUnitItems.Items;
            this._entityInfos = DXUnitDefinitionUnitItems.Items;
            this._enumInfos = DXEnumDefinitionUnitItems.Items;
            this._relationInfos = new List<DXRelationDefinitionUnit>();
        }

        public bool Delete(string typeName, Guid id)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(typeName);

            if (id == Guid.Empty)
                throw new ArgumentException("Id must be a non-empty GUID.", nameof(id));

            var mainEntityInfo = this.GetEntity(typeName);

            var entityHierarchy = this.GetHierarchyChainOfBaseEntitiesFromDerivedToBase(mainEntityInfo);

            return this.RunRequestInTransaction((conn) =>
            {
                DataSet dataSet = new DataSet(typeName);

                foreach (var entityInfo in entityHierarchy)
                {
                    var relatedBlocks = this.GetRelatedBlocks(entityInfo);

                    if (relatedBlocks != null)
                    {
                        // Delete related blocks
                        foreach (var relatedBlock in relatedBlocks)
                        {
                            this.DeleteESQLBlocksFromDataSet(relatedBlock.DXUnitDefinitionMainElement.Name, id, dataSet, conn);
                        }
                    }

                    // Delete entity
                    this.DeleteESQLSEntityFromDataSet(entityInfo.DXUnitDefinitionMainElement.Name, id, dataSet, conn);
                }

                dataSet.AcceptChanges();

                return true;
            });
        }

        public ESQLModel GetItem(ESQLModelDefinition container, Guid id, TypeOfEntityLoading typeOfLoading)
        {
            if (container == null)
                return null;

            ESQLModel result = null;

            this.RunRequest((conn) =>
            {
                var dataSet = this.PopulateDataSetForTargetEntity(container, id, conn);

                result = this.GetESQLModel(container, id);

                if (dataSet.Tables[result.OwnSingleItem.ObjectInfo.ObjectName].Rows.Count == 0)
                {
                    result = null;
                }
                else
                {
                    var dataRow = dataSet.Tables[result.OwnSingleItem.ObjectInfo.ObjectName].Rows[0];

                    result = this.ConvertToESQLModel(dataSet, dataRow, container);
                }
            });

            return result;
        }

        private ESQLModel GetESQLModel(ESQLModelDefinition container, Guid id)
        {
            ESQLModel result = new ESQLModel(new ESQLMainItem(new ESQLObjectDefinitionAttribute(container.OwnSingleItem.Name))
            {
                Item = new ESQLItem()
                {
                    ID = id
                }
            });

            return result;
        }

        public IEnumerable<ESQLModel> GetItems(ESQLModelDefinition container, IEnumerable<Guid> objIds, TypeOfEntityLoading typeOfLoading)
        {
            if (container == null)
                return null;

            if (objIds == null)
                return null;

            if (objIds.Count() == 0)
                return new List<ESQLModel>();

            IEnumerable<ESQLModel> resultItems = null;

            this.RunRequest((conn) =>
            {
                resultItems = this.GetItems(conn, container, objIds, typeOfLoading);
            });

            return resultItems;
        }

        public IEnumerable<ESQLModel> GetItems(DbConnection conn, ESQLModelDefinition container, IEnumerable<Guid> objIds, TypeOfEntityLoading typeOfLoading)
        {
            if (container == null)
                return null;

            if (objIds == null)
                return null;

            if (objIds.Count() == 0)
                return new List<ESQLModel>();

            IEnumerable<ESQLModel> resultItems = null;

            var dataSet = this.PopulateDataSetForTargetEntitys(container, objIds, conn);

            var items = dataSet.Tables[container.OwnSingleItem.Type].Rows;

            resultItems = items.Cast<DataRow>().Select(x => this.ConvertToESQLModel(dataSet, x, container)).ToList();

            dataSet.AcceptChanges();

            return resultItems;
        }

        private ESQLModel ConvertToESQLModel(DataSet dataSet, DataRow x, ESQLModelDefinition container)
        {
            ESQLModel resultItem = this.GetESQLModel(container, ConvertHelper.ParseGuid(x[Constants.ID]));

            // Process ESQL model
            this.PopulateESQLMainItem(resultItem.OwnSingleItem, container.OwnSingleItem, x);

            // Process ESQL single items
            resultItem.SingleItems = container.SingleFragmentDefinitions.Select(item =>
            {
                ESQLSingleItem singleItem = this.ConvertToESQLSingleItem(item);

                this.PopulateESQLSingleItem(singleItem, item, dataSet.Tables[singleItem.Name].Rows.Cast<DataRow>()
                    .SingleOrDefault(y => ConvertHelper.ParseGuid(y[Constants.ObjectID]) == resultItem.OwnSingleItem.Item.ID));

                return singleItem;
            }).ToList();

            // Process ESQL multi items
            resultItem.MultiItems = container.MultiFragmentDefinitions.Select(item =>
            {
                ESQLMultiItem multiItem = this.ConvertToESQLMultiItem(item);

                var rows =
                   dataSet.Tables[multiItem.Name].Rows.Cast<DataRow>()
                   .Where(y => ConvertHelper.ParseGuid(y[Constants.ObjectID]) == resultItem.OwnSingleItem.Item.ID).ToList();

                this.PopulateESQLMultiItem(multiItem, item, rows);

                return multiItem;
            }).ToList();

            return resultItem;
        }

        private ESQLSingleItem ConvertToESQLSingleItem(ESQLBlockDefinition item)
        {
            return new ESQLSingleItem()
            {
                Name = item.Name,
                BlockInfo = new ESQLBlockDefinitionAttribute(item.Name)
            };
        }

        private ESQLMultiItem ConvertToESQLMultiItem(ESQLBlockDefinition item)
        {
            return new ESQLMultiItem()
            {
                Name = item.Name,
                BlockInfo = new ESQLBlockDefinitionAttribute(item.Name)
            };
        }

        public IEnumerable<ESQLModel> GetItems(string typeName)
        {
            var modelDefinition = this.GetModelDefinition(typeName);

            if (modelDefinition == null)
                return null;

            return this.GetItems(modelDefinition, TypeOfEntityLoading.Full);
        }

        public IEnumerable<ESQLModel> GetItems(string typeName, IEnumerable<Guid> ids)
        {
            var modelDefinition = this.GetModelDefinition(typeName);

            if (modelDefinition == null)
                return null;

            return this.GetItems(modelDefinition, ids, TypeOfEntityLoading.Full);
        }

        public IEnumerable<ESQLModel> GetItems(string typeName, string esqlWhereExpression)
        {
            var modelDefinition = this.GetModelDefinition(typeName);

            if (modelDefinition == null)
                return null;

            return this.GetItems(modelDefinition, esqlWhereExpression, TypeOfEntityLoading.Full);
        }

        public ESQLModel GetItem(string typeName, Guid id)
        {
            var modelDefinition = this.GetModelDefinition(typeName);

            if (modelDefinition == null)
                return null;

            return this.GetItem(modelDefinition, id, TypeOfEntityLoading.Full);
        }

        private ESQLModelDefinition GetModelDefinition(string type)
        {
            var mainEntity = this.GetEntity(type);

            if (mainEntity == null)
                return null;

            var entities = this.GetHierarchyChainOfBaseEntitiesFromBaseToDerived(mainEntity);

            List<DXElementDefinitionUnit> singleMandatoryBlocks = new List<DXElementDefinitionUnit>();
            List<DXElementDefinitionUnit> singleOptionalBlocks = new List<DXElementDefinitionUnit>();
            List<DXElementDefinitionUnit> multiMandatoryBlocks = new List<DXElementDefinitionUnit>();
            List<DXElementDefinitionUnit> multiOptionalBlocks = new List<DXElementDefinitionUnit>();

            foreach (var entity in entities)
            {
                var singleMandatoryBlocksTemp = this.GetRelatedBlocks(entity, DXElementInUnitTypeEnum.SingleMandatory);

                if (singleMandatoryBlocksTemp != null)
                {
                    singleMandatoryBlocks.AddRange(singleMandatoryBlocksTemp);
                }

                var singleOptionalBlocksTemp = this.GetRelatedBlocks(entity, DXElementInUnitTypeEnum.SingleOptional);

                if (singleOptionalBlocksTemp != null)
                {
                    singleOptionalBlocks.AddRange(singleOptionalBlocksTemp);
                }

                var multiMandatoryBlocksTemp = this.GetRelatedBlocks(entity, DXElementInUnitTypeEnum.MultiMandatory);

                if (multiMandatoryBlocksTemp != null)
                {
                    multiMandatoryBlocks.AddRange(multiMandatoryBlocksTemp);
                }

                var multiOptionalBlocksTemp = this.GetRelatedBlocks(entity, DXElementInUnitTypeEnum.MultiOptional);

                if (multiOptionalBlocksTemp != null)
                {
                    multiOptionalBlocks.AddRange(multiOptionalBlocksTemp);
                }
            }

            var modelDefinition = ESQLModelDefinition.BuildModelDefinition(
                mainEntity,
                singleMandatoryBlocks,
                singleOptionalBlocks,
                multiMandatoryBlocks,
                multiOptionalBlocks);

            return modelDefinition;
        }

        public IEnumerable<ESQLModel> GetItems(ESQLModelDefinition container, TypeOfEntityLoading typeOfLoading)
        {
            return GetItems(container, string.Empty, typeOfLoading);
        }

        public IEnumerable<ESQLModel> GetItems(ESQLModelDefinition container, string esqlWhereExpression, TypeOfEntityLoading typeOfLoading)
        {
            string typeName = container.OwnSingleItem.Type;
            string sqlQuery = this._queryHelper.GetQuery(typeName, esqlWhereExpression, this.RelationInfos);

            return this.RunRequestInTransaction((conn) =>
            {
                var dataSet = new DataSet(typeName);

                var adapter = this._queryHelper.GetDbDataAdapter(conn, sqlQuery);

                adapter.Fill(dataSet, typeName);

                var ids = dataSet.Tables[typeName].Rows.Cast<DataRow>().Select(x => ConvertHelper.ParseGuid(x[Constants.ID]));

                return this.GetItems(conn, container, ids, typeOfLoading);
            });
        }

        private DataSet PopulateDataSetForTargetEntity(ESQLModelDefinition container, Guid id, DbConnection conn)
        {
            DataSet dataSet = new DataSet(container.OwnSingleItem.Type);

            this.PopulateTableToDataSet(conn, dataSet, container.OwnSingleItem.Type,
                whereClause: this._queryHelper.GetWhereExpressionForID(id));

            var whereClauseForObjectID = this._queryHelper.GetWhereExpressionForObjectID(id);

            foreach (var singleItem in container.SingleFragmentDefinitions)
            {
                this.PopulateTableToDataSet(conn, dataSet, singleItem.Type,
                    columnNames: singleItem.Select(x => x.ColumnDefinition.ESQLExpression),
                    whereClause: whereClauseForObjectID);
            }

            foreach (var multiItem in container.MultiFragmentDefinitions)
            {
                this.PopulateTableToDataSet(conn, dataSet, multiItem.Type,
                    multiItem.Select(x => x.ColumnDefinition.ESQLExpression),
                    whereClause: whereClauseForObjectID);
            }

            return dataSet;
        }

        private DataSet PopulateDataSetForTargetEntitys(ESQLModelDefinition container, IEnumerable<Guid> ids, DbConnection conn)
        {
            DataSet dataSet = new DataSet(container.OwnSingleItem.Type);

            if (ids != null && ids.Count() > 0)
            {
                this.PopulateTableToDataSet(conn, dataSet, container.OwnSingleItem.Type, whereClause: this._queryHelper.GetWhereExpressionForID(ids));

                var whereClauseForObjectIDs = this._queryHelper.GetWhereExpressionForObjectID(ids);

                foreach (var singleItem in container.SingleFragmentDefinitions)
                {
                    this.PopulateTableToDataSet(conn, dataSet, singleItem.Type, whereClause: whereClauseForObjectIDs);
                }

                foreach (var multiItem in container.MultiFragmentDefinitions)
                {
                    this.PopulateTableToDataSet(conn, dataSet, multiItem.Type, whereClause: whereClauseForObjectIDs);
                }
            }

            return dataSet;
        }

        private void PopulateESQLMainItem(
            ESQLMainItem ownItem,
            ESQLBlockDefinition fragmentDefinition,
            DataRow dataRow)
        {
            if (dataRow == null || ownItem == null)
                return;

            ownItem.Item =
                this.GetESQLItem(dataRow, fragmentDefinition);
        }

        private void PopulateESQLSingleItem(
            ESQLSingleItem singleItem,
            ESQLBlockDefinition fragmentDefinition,
            DataRow dataRow)
        {
            if (dataRow == null || singleItem == null)
                return;

            singleItem.Item =
                this.GetESQLItem(dataRow, fragmentDefinition);
        }

        private void PopulateESQLMultiItem(
            ESQLMultiItem multiItem,
            ESQLBlockDefinition fragmentDefinition,
            IEnumerable<DataRow> rows)
        {
            multiItem.Announced = rows.OfType<DataRow>().Select(x => this.GetESQLItem(x, fragmentDefinition)).ToList();
            multiItem.Mode = ModeForMultiItems.Full;
        }

        private ESQLItem GetESQLItem(DataRow row, ESQLBlockDefinition structure)
        {
            JObject jObjectContainerCopy = new JObject();

            foreach (DataColumn column in row.Table.Columns)
            {
                var property = structure.SingleOrDefault(x => x.ColumnDefinition.ColumnName == column.ColumnName);

                if (property == null)
                    continue;

                if (row[column] != DBNull.Value)
                {
                    jObjectContainerCopy[property.ColumnDefinition.ColumnName] = GetValueFromRow(row, column);
                }
                else
                {
                    jObjectContainerCopy[property.ColumnDefinition.ColumnName] = null;
                }
            }

            var esqlItem = new ESQLItem()
            {
                ID = ConvertHelper.ParseGuid(row[Constants.ID]),
                Content = jObjectContainerCopy
            };

            if (row.Table.Columns.Contains(Constants.ObjectID))
            {
                esqlItem.ObjectID = ConvertHelper.ParseGuid(row[Constants.ObjectID]);
            }
            else
            {
                esqlItem.ObjectID = esqlItem.ID;
            }

            return esqlItem;
        }

        private JValue GetValueFromRow(DataRow dataRow, DataColumn dataColumn)
        {
            JValue value = null;

            if (dataColumn.DataType == typeof(DateTime))
            {
                var dateTime = ConvertHelper.ParseDateTime(dataRow[dataColumn]);
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

                value = new JValue(dateTime);
            }
            else
            {
                value = new JValue(dataRow[dataColumn]);
            }

            return value;
        }

        public Guid Insert(ESQLModel model)
        {
            return this.InsertOrUpdate(model, ProcessingType.Insert);
        }

        public Guid Update(ESQLModel model)
        {
            return this.InsertOrUpdate(model, ProcessingType.Update);
        }

        public Guid InsertOrUpdate(ESQLModel model)
        {
            var objId = model.OwnSingleItem.Item.ID;
            var type = model.OwnSingleItem.ObjectInfo.ObjectName;

            if (objId.HasValue
                && !string.IsNullOrEmpty(type)
                && this.IsItemExisting(type, objId.Value))
            {
                return this.Update(model);
            }
            else
            {
                return this.Insert(model);
            }
        }

        public bool IsItemExisting(string type, Guid objectId)
        {
            ESQLModelDefinition dd = new ESQLModelDefinition(new ESQLBlockDefinition(type, type));

            var item = this.GetItem(dd, objectId, TypeOfEntityLoading.Base);

            return item != null;
        }

        private Guid InsertOrUpdate(ESQLModel model, ProcessingType processingType)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (!model.OwnSingleItem.Item.ID.HasValue)
            {
                model.OwnSingleItem.Item.ID = Guid.NewGuid();
            }

            var typeName = model.OwnSingleItem.ObjectInfo.ObjectName;

            var mainEntityInfo = this.GetEntity(typeName);

            if (mainEntityInfo != null)
            {
                var entityHierarchy = this.GetHierarchyChainOfBaseEntitiesFromBaseToDerived(mainEntityInfo);

                this.ProcessESQLModelAsESQLEntity(typeName, model, entityHierarchy, processingType);
            }

            var enumInfo = this.GetEnum(typeName);

            if (enumInfo != null)
            {
                this.ProcessESQLModelAsESQLEnum(typeName, enumInfo, model, processingType);
            }

            if (enumInfo == null && mainEntityInfo == null)
            {
                throw new Exception($"Type '{model.OwnSingleItem.ObjectInfo.ObjectName}' is not registered.");
            }

            return model.OwnSingleItem.Item.ID.Value;
        }

        private void ProcessESQLModelAsESQLEntity(string typeName, ESQLModel model, IEnumerable<DXUnitDefinitionUnit> entityHierarchy, ProcessingType processingType)
        {
            this.RunRequestInTransaction((conn) =>
            {
                DataSet dataSet = new DataSet(typeName);

                foreach (var entityInfo in entityHierarchy)
                {
                    this.InsertOrUpdateESQLOwnItemToDataSet(model, entityInfo.DXUnitDefinitionMainElement.Name, dataSet, conn, processingType);

                    var relatedBlocksSM = this.GetRelatedBlocks(entityInfo, DXElementInUnitTypeEnum.SingleMandatory);
                    var relatedBlocksSO = this.GetRelatedBlocks(entityInfo, DXElementInUnitTypeEnum.SingleOptional);
                    var relatedBlocksMM = this.GetRelatedBlocks(entityInfo, DXElementInUnitTypeEnum.MultiMandatory);
                    var relatedBlocksMO = this.GetRelatedBlocks(entityInfo, DXElementInUnitTypeEnum.MultiOptional);

                    var objectID = model.OwnSingleItem.Item.ID.Value;
                    var entityType = entityInfo.DXUnitDefinitionMainElement.Name;
                    // Process ESQL single items
                    if (relatedBlocksSM != null)
                    {
                        foreach (var singleItem in relatedBlocksSM)
                        {
                            var blockName = singleItem.DXUnitDefinitionMainElement.Name.Trim();
                            var block = model.SingleItems.SingleOrDefault(x => x.Name.Trim() == blockName);

                            if (block == null)
                                continue;

                            this.InsertOrUpdateESQLSingleItemToDataSet(block, entityType, objectID, dataSet, conn, processingType);
                        }
                    }

                    if (relatedBlocksSO != null)
                    {
                        foreach (var singleItem in relatedBlocksSO)
                        {
                            var blockName = singleItem.DXUnitDefinitionMainElement.Name.Trim();
                            var block = model.SingleItems.SingleOrDefault(x => x.Name.Trim() == blockName);

                            if (block == null)
                                continue;

                            this.InsertOrUpdateESQLSingleItemToDataSet(block, entityType, objectID, dataSet, conn, processingType);
                        }
                    }

                    // Process ESQL mutli items
                    if (relatedBlocksMM != null)
                    {
                        foreach (var multiItem in relatedBlocksMM)
                        {
                            this.InsertOrUpdateESQLMultiItemToDataSet(model, entityInfo, multiItem, dataSet, conn, processingType);
                        }
                    }

                    if (relatedBlocksMO != null)
                    {
                        foreach (var multiItem in relatedBlocksMO)
                        {
                            this.InsertOrUpdateESQLMultiItemToDataSet(model, entityInfo, multiItem, dataSet, conn, processingType);
                        }
                    }
                }

                dataSet.AcceptChanges();

                return true;
            });
        }

        private void ProcessESQLModelAsESQLEnum(string typeName, DXEnumDefinitionUnit entity, ESQLModel model, ProcessingType processingType)
        {
            this.RunRequestInTransaction((conn) =>
            {
                DataSet dataSet = new DataSet(typeName);

                this.InsertOrUpdateESQLOwnItemToDataSet(model, entity.DXUnitDefinitionMainElement.Name, dataSet, conn, processingType);

                dataSet.AcceptChanges();

                return true;
            });
        }

        private IEnumerable<DXUnitDefinitionUnit> GetHierarchyChainOfBaseEntitiesFromDerivedToBase(DXUnitDefinitionUnit derivedEntity)
        {
            var result = new List<DXUnitDefinitionUnit>() { derivedEntity };

            if (derivedEntity.DXUnitInheritanceElement?.BaseEntity == null)
                return result;

            var derivedEntityInfo = derivedEntity;

            while (true)
            {
                var baseClass = this.GetBaseEntity(derivedEntityInfo);

                result.Add(baseClass);

                if (baseClass.DXUnitInheritanceElement != null)
                {
                    derivedEntityInfo = baseClass;
                }
                else
                {
                    break;
                }
            }

            return result.ToList();
        }

        private IEnumerable<DXUnitDefinitionUnit> GetHierarchyChainOfBaseEntitiesFromBaseToDerived(DXUnitDefinitionUnit derivedEntity)
        {
            return this.GetHierarchyChainOfBaseEntitiesFromDerivedToBase(derivedEntity).Reverse();
        }

        private void DeleteESQLSEntityFromDataSet(string entityName, Guid id, DataSet dataSet, DbConnection conn)
        {
            var esqlModelAdapter = this.PopulateTableToDataSet(conn, dataSet, entityName,
                whereClause: this._queryHelper.GetWhereExpressionForID(id));

            var esqlModelBuilder = this._queryHelper.GetDbCommandBuilder(esqlModelAdapter);

            esqlModelBuilder.GetDeleteCommand();

            DataTable dataTable = dataSet.Tables[entityName];

            foreach (DataRow row in dataTable.Rows)
            {
                row.Delete();
            }

            esqlModelAdapter.Update(dataSet, entityName);
        }

        private void DeleteESQLBlocksFromDataSet(string blockName, Guid objectID, DataSet dataSet, DbConnection conn)
        {
            var esqlModelAdapter = this.PopulateTableToDataSet(conn, dataSet, blockName, whereClause:
                this._queryHelper.GetWhereExpressionForObjectID(objectID));

            var esqlModelBuilder = this._queryHelper.GetDbCommandBuilder(esqlModelAdapter);

            esqlModelBuilder.GetDeleteCommand();

            DataTable dataTable = dataSet.Tables[blockName];

            foreach (DataRow row in dataTable.Rows)
            {
                row.Delete();
            }

            esqlModelAdapter.Update(dataSet, blockName);
        }

        private void InsertOrUpdateESQLOwnItemToDataSet(ESQLModel model, string entityType, DataSet dataSet, DbConnection conn, ProcessingType processingType)
        {
            var entityName = entityType;
            var objectID = model.OwnSingleItem.Item.ID;

            var esqlModelAdapter = this.PopulateTableToDataSet(conn, dataSet, entityName,
                whereClause: this._queryHelper.GetWhereExpressionForID(objectID.Value));

            var esqlModelBuilder = this._queryHelper.GetDbCommandBuilder(esqlModelAdapter);

            switch (processingType)
            {
                case ProcessingType.Insert:
                    esqlModelBuilder.GetInsertCommand();
                    break;
                case ProcessingType.Update:
                    esqlModelBuilder.GetUpdateCommand();
                    break;
                case ProcessingType.Delete:
                default:
                    return;
            }

            DataTable dataTable = dataSet.Tables[entityName];

            if (dataTable.Rows.Count == 0)
            {
                var row = dataTable.NewRow();
                MapESQLItemToRow(model.OwnSingleItem.Item, row, entityName);
                dataTable.Rows.Add(row);
            }
            else
            {
                var row = dataTable.Rows[0];
                MapESQLItemToRow(model.OwnSingleItem.Item, row, entityName);
            }

            esqlModelAdapter.Update(dataSet, entityName);
        }

        private Guid InsertOrUpdateESQLSingleItemToDataSet(
            ESQLSingleItem block,
            string entityType,
            Guid objectID,
            DataSet dataSet,
            DbConnection conn,
            ProcessingType processingType)
        {
            ArgumentNullException.ThrowIfNull(block);

            if (!block.Item.ID.HasValue)
            {
                block.Item.ID = Guid.NewGuid();
            }

            var blockName = block.BlockInfo.BlockName;

            var esqlModelAdapter = this.PopulateTableToDataSet(conn, dataSet, blockName,
                whereClause: this._queryHelper.GetWhereExpressionForID(block.Item.ID.Value));

            var esqlModelBuilder = this._queryHelper.GetDbCommandBuilder(esqlModelAdapter);

            switch (processingType)
            {
                case ProcessingType.Insert:
                    esqlModelBuilder.GetInsertCommand();
                    break;
                case ProcessingType.Update:
                    esqlModelBuilder.GetUpdateCommand();
                    break;
            }

            DataTable dataTable = dataSet.Tables[blockName];

            if (dataTable.Rows.Count == 0)
            {
                var row = dataTable.NewRow();
                MapESQLItemToRow(block.Item, row, entityType);
                dataTable.Rows.Add(row);
            }
            else
            {
                var row = dataTable.Rows[0];
                MapESQLItemToRow(block.Item, row, entityType);
            }

            esqlModelAdapter.Update(dataSet, blockName);

            return block.Item.ID.Value;
        }

        private void InsertOrUpdateESQLMultiItemToDataSet(ESQLModel model, DXUnitDefinitionUnit entityInfo, DXElementDefinitionUnit blockInfo, DataSet dataSet, DbConnection conn, ProcessingType processingType)
        {
            var blockName = blockInfo.DXUnitDefinitionMainElement.Name.Trim();
            var objectID = model.OwnSingleItem.Item.ID;
            var entityType = entityInfo.DXUnitDefinitionMainElement.Name;

            var block = model.MultiItems.SingleOrDefault(x => x.Name.Trim() == blockName);

            if (block == null)
                return;

            var esqlModelAdapter = this.PopulateTableToDataSet(conn, dataSet, blockName, whereClause:
                this._queryHelper.GetWhereExpressionForObjectID(objectID.Value));

            var esqlModelBuilder = this._queryHelper.GetDbCommandBuilder(esqlModelAdapter);

            if (processingType == ProcessingType.Insert)
            {
                esqlModelBuilder.GetInsertCommand();
            }
            else if (processingType == ProcessingType.Update)
            {
                esqlModelBuilder.GetUpdateCommand();
            }

            DataTable dataTable = dataSet.Tables[blockName];

            if (block.Mode == ModeForMultiItems.Full)
            {
                this.ProcessAnnouncedItems(block, dataTable, dataSet.DataSetName);

                var rowsToDelete = dataTable.Rows.Cast<DataRow>()
                    .Where(row =>
                    {
                        var id = Guid.Parse(Convert.ToString(row[Constants.ID]));
                        return !block.Announced.Any(x => x.ID == id);
                    })
                    .ToList();

                foreach (var row in rowsToDelete)
                {
                    row.Delete();
                }

            }
            else if (block.Mode == ModeForMultiItems.Target)
            {
                this.ProcessAnnouncedItems(block, dataTable, dataSet.DataSetName);
                this.ProcessDeletedItems(block, dataTable);
            }

            esqlModelAdapter.Update(dataSet, block.BlockInfo.BlockName);
        }

        private void ProcessAnnouncedItems(ESQLMultiItem esqlMultiItem, DataTable dataTable, string esqlModelType)
        {
            foreach (var announcedItem in esqlMultiItem.Announced)
            {
                var row = dataTable.Rows.Cast<DataRow>().SingleOrDefault(x => Guid.Parse(x[Constants.ID].ToString()) == announcedItem.ID);

                if (row == null)
                {
                    row = dataTable.NewRow();
                    MapESQLItemToRow(announcedItem, row, esqlModelType);
                    dataTable.Rows.Add(row);
                }
                else
                {
                    MapESQLItemToRow(announcedItem, row, esqlModelType);
                }
            }
        }

        private void ProcessDeletedItems(ESQLMultiItem esqlMultiItem, DataTable dataTable)
        {
            var rowIDsToDelete = esqlMultiItem.Deleted.Select(x => x.ID).ToList();

            var rowsToDelete = dataTable.Rows.Cast<DataRow>().Where(x => rowIDsToDelete.Contains(Guid.Parse(x[Constants.ID].ToString()))).ToList();

            foreach (var rowToDelete in rowsToDelete)
            {
                rowToDelete.Delete();
            }
        }

        private void MapESQLItemToRow(ESQLItem esqlItem, DataRow row, string esqlModelType)
        {
            row[Constants.ID] = esqlItem.ID;

            if (row.Table.Columns.Contains(Constants.ObjectID))
            {
                row[Constants.ObjectID] = esqlItem.ObjectID;
            }

            if (row.Table.Columns.Contains($"{esqlModelType}ID"))
            {
                row[$"{esqlModelType}ID"] = esqlItem.ObjectID;
            }

            if (row.Table.Columns.Contains("TimeStamp"))
            {
                row[$"TimeStamp"] = DateTime.UtcNow;
            }

            if (esqlItem.Content == null)
                return;

            var properties = esqlItem.Content.Children().Select(x => x as JProperty).Where(x => x != null);

            foreach (var column in row.Table.Columns.OfType<DataColumn>())
            {
                var jProperty = properties.SingleOrDefault(x => x.Name == column.ColumnName);

                if (column.ColumnName == "ID"
                    || column.ColumnName == "ObjectID"
                    || column.ColumnName == "TimeStamp"
                    || column.ColumnName == $"{esqlModelType}ID")
                {
                    continue;
                }

                if (!column.ReadOnly)
                {
                    if (jProperty != null)
                    {
                        if (this.IsNullOrEmpty(jProperty.Value as JValue))
                        {
                            if (column.AllowDBNull)
                            {
                                this.SetNullValueToRowCell(row, column, jProperty);
                            }
                            else
                            {
                                this.SetNotNullValueToRowCell(row, column);
                            }
                        }
                        else
                        {
                            this.SetJPropertyValueToRowCell(row, column, jProperty);
                        }
                    }
                    else if (
                        row[column] == DBNull.Value
                        && !column.AllowDBNull
                       )
                    {
                        this.SetNotNullValueToRowCell(row, column);
                    }
                }
            }

        }

        private bool IsNullOrEmpty(JValue jValue)
        {
            return jValue == null || string.IsNullOrWhiteSpace(jValue.ToString());
        }

        private void SetNullValueToRowCell(DataRow dataRow, DataColumn dataColumn, JProperty jProperty)
        {
            JValue jValue = jProperty.Value as JValue;

            dataRow[dataColumn] = DBNull.Value;
        }

        private void SetJPropertyValueToRowCell(DataRow dataRow, DataColumn dataColumn, JProperty jProperty)
        {
            JValue jValue = jProperty.Value as JValue;

            if (dataColumn.DataType == typeof(Guid))
            {
                dataRow[dataColumn] = jValue.Value;
            }
            else
            if (dataColumn.DataType == typeof(decimal))
            {
                dataRow[dataColumn] = ConvertHelper.ParseDecimal(jValue.Value);
            }
            else
            if (dataColumn.DataType == typeof(double))
            {
                dataRow[dataColumn] = ConvertHelper.ParseDouble(jValue.Value);
            }
            else
            if (dataColumn.DataType == typeof(sbyte))
            {
                dataRow[dataColumn] = ConvertHelper.ParseSByte(jValue.Value);
            }
            else
            if (dataColumn.DataType == typeof(int))
            {
                dataRow[dataColumn] = ConvertHelper.ParseInt(jValue.Value);
            }
            else
            if (dataColumn.DataType == typeof(DateTime))
            {
                dataRow[dataColumn] = ConvertHelper.ParseDateTime(jValue.Value).ToUniversalTime();
            }
            else
            if (dataColumn.DataType == typeof(bool))
            {
                dataRow[dataColumn] = ConvertHelper.ParseBool(jValue.Value);
            }
            else
            if (dataColumn.DataType == typeof(byte[]))
            {
                dataRow[dataColumn] = (byte[])jValue.Value;
            }
            else if (dataColumn.DataType == typeof(TimeSpan))
            {
                dataRow[dataColumn] = ConvertHelper.ParseTimeSpan(jValue.Value);
            }
            else
            //if (dataColumn.DataType == typeof(string))
            {
                dataRow[dataColumn] = ConvertHelper.ParseString(jValue.Value);
            }
        }

        private void SetNotNullValueToRowCell(DataRow dataRow, DataColumn dataColumn)
        {
            if (dataColumn.DataType == typeof(Guid))
            {
                dataRow[dataColumn] = new Guid();
            }
            else
            if (dataColumn.DataType == typeof(decimal))
            {
                dataRow[dataColumn] = 0;
            }
            else
            if (dataColumn.DataType == typeof(double))
            {
                dataRow[dataColumn] = 0;
            }
            else
            if (dataColumn.DataType == typeof(int))
            {
                dataRow[dataColumn] = 0;
            }
            else
            if (dataColumn.DataType == typeof(DateTime))
            {
                dataRow[dataColumn] = new DateTime(1753, 1, 1);
            }
            else
            if (dataColumn.DataType == typeof(bool))
            {
                dataRow[dataColumn] = false;
            }
            else
            if (dataColumn.DataType == typeof(byte[]))
            {
                dataRow[dataColumn] = new byte[0];
            }
            else
            //if (dataColumn.DataType == typeof(string))
            {
                dataRow[dataColumn] = "";
            }
        }

        public IEnumerable<Guid> GetRelations(string ObjectTypeNameLeft, Guid obj1Id, string relationToObj2Name)
        {
            var relationInfo = this.GetRelationInfo(ObjectTypeNameLeft, relationToObj2Name);

            IEnumerable<Guid> result = null;

            switch (relationInfo.RelationType)
            {
                case DXRelationTypeEnum.ManyToMany:
                    result = this.GetRelationsManyToMany(relationInfo, obj1Id);
                    break;
                case DXRelationTypeEnum.OneToZeroOne:
                    result = this.GetRelationsOneToMany(relationInfo, obj1Id);
                    break;
                case DXRelationTypeEnum.ZeroOneToOne:
                    result = this.GetRelationsManyToOne(relationInfo, obj1Id);
                    break;
                case DXRelationTypeEnum.OneToMany:
                    result = this.GetRelationsOneToMany(relationInfo, obj1Id);
                    break;
                case DXRelationTypeEnum.ManyToOne:
                    result = this.GetRelationsManyToOne(relationInfo, obj1Id);
                    break;
                case DXRelationTypeEnum.ZeroOneToMany:
                    result = this.GetRelationsOneToMany(relationInfo, obj1Id);
                    break;
                case DXRelationTypeEnum.ManyToZeroOne:
                    result = this.GetRelationsManyToOne(relationInfo, obj1Id);
                    break;
                case DXRelationTypeEnum.ZeroOneToZeroOne:
                    result = this.GetRelationsZeroOneToZeroOne(relationInfo, obj1Id);
                    break;
            }

            return result;
        }

        public Guid? GetRelation(string obj1TypeName, Guid obj1Id, string relationToObj2Name)
        {
            var relationInfo = this.GetRelationInfo(obj1TypeName, relationToObj2Name);

            IEnumerable<Guid> ids = null;

            Guid? result = null;

            switch (relationInfo.RelationType)
            {
                case DXRelationTypeEnum.ManyToMany:
                    ids = this.GetRelationsManyToMany(relationInfo, obj1Id);
                    break;
                case DXRelationTypeEnum.OneToZeroOne:
                    ids = this.GetRelationsOneToMany(relationInfo, obj1Id);
                    break;
                case DXRelationTypeEnum.ZeroOneToOne:
                    ids = this.GetRelationsManyToOne(relationInfo, obj1Id);
                    break;
                case DXRelationTypeEnum.OneToMany:
                    ids = this.GetRelationsOneToMany(relationInfo, obj1Id);
                    break;
                case DXRelationTypeEnum.ManyToOne:
                    ids = this.GetRelationsManyToOne(relationInfo, obj1Id);
                    break;
                case DXRelationTypeEnum.ZeroOneToMany:
                    ids = this.GetRelationsOneToMany(relationInfo, obj1Id);
                    break;
                case DXRelationTypeEnum.ManyToZeroOne:
                    ids = this.GetRelationsManyToOne(relationInfo, obj1Id);
                    break;
                case DXRelationTypeEnum.ZeroOneToZeroOne:
                    ids = this.GetRelationsZeroOneToZeroOne(relationInfo, obj1Id);
                    break;
            }

            if (ids != null && ids.Count() > 1)
            {
                throw new Exception($"Object '{obj1TypeName}'('{obj1Id}') for '{relationToObj2Name}' ralation has more than one related entries. Please use 'GetRelations' method instead.");
            }

            if (ids != null && ids.Any())
            {
                result = ids.First();
            }

            return result;
        }

        public bool AddRelation(string obj1TypeName, Guid obj1Id, string relationToObj2Name, string obj2TypeName, Guid obj2Id)
        {
            var relationInfo = this.GetRelationInfo(obj1TypeName, relationToObj2Name);

            switch (relationInfo.RelationType)
            {
                case DXRelationTypeEnum.ManyToMany:
                    return this.AddRelationManyToMany(relationInfo, obj1Id, obj2Id);
                case DXRelationTypeEnum.OneToZeroOne:
                    return this.AddRelationOneToMany(relationInfo, obj1Id, obj2Id);
                case DXRelationTypeEnum.ZeroOneToOne:
                    return this.AddRelationManyToOne(relationInfo, obj1Id, obj2Id);
                case DXRelationTypeEnum.OneToMany:
                    return this.AddRelationOneToMany(relationInfo, obj1Id, obj2Id);
                case DXRelationTypeEnum.ManyToOne:
                    return this.AddRelationManyToOne(relationInfo, obj1Id, obj2Id);
                case DXRelationTypeEnum.ZeroOneToMany:
                    return this.AddRelationOneToMany(relationInfo, obj1Id, obj2Id);
                case DXRelationTypeEnum.ManyToZeroOne:
                    return this.AddRelationManyToOne(relationInfo, obj1Id, obj2Id);
                case DXRelationTypeEnum.ZeroOneToZeroOne:
                    return this.AddRelationZeroOneToZeroOne(relationInfo, obj1Id, obj2Id);
                default:
                    throw new NotImplementedException($"Relation type '{relationInfo.RelationType}' is not supported.");
            }
        }

        public bool RemoveRelation(string obj1TypeName, Guid obj1Id, string relationToObj2Name, string obj2TypeName, Guid obj2Id)
        {
            var relationInfo = this.GetRelationInfo(obj1TypeName, relationToObj2Name);

            switch (relationInfo.RelationType)
            {
                case DXRelationTypeEnum.ManyToMany:
                    return this.RemoveRelationManyToMany(relationInfo, obj1Id, obj2Id);
                case DXRelationTypeEnum.OneToZeroOne:
                    throw new NotImplementedException("'1 to 0/1' relation couldn't be removed.");
                case DXRelationTypeEnum.ZeroOneToOne:
                    throw new NotImplementedException("'0/1 to 1' relation couldn't be removed.");
                case DXRelationTypeEnum.OneToMany:
                    throw new NotImplementedException("'1 to M' relation couldn't be removed.");
                case DXRelationTypeEnum.ManyToOne:
                    throw new NotImplementedException("'N to 1' relation couldn't be removed.");
                case DXRelationTypeEnum.ZeroOneToMany:
                    return this.RemoveRelationZeroOneToMany(relationInfo, obj1Id, obj2Id);
                case DXRelationTypeEnum.ManyToZeroOne:
                    return this.RemoveRelationManyToZeroOne(relationInfo, obj1Id, obj2Id);
                case DXRelationTypeEnum.ZeroOneToZeroOne:
                    return this.RemoveRelationZeroOneToZeroOne(relationInfo, obj1Id, obj2Id);
                default:
                    throw new NotImplementedException($"Relation type '{relationInfo.RelationType}' is not supported.");
            }
        }

        public Guid InsertSingleBlock(string esqlModelType, ESQLSingleItem esqlSingleBlock)
        {
            return this.InsertOrUpdateSingleBlockPrivate(esqlModelType, esqlSingleBlock, ProcessingType.Insert);
        }

        public Guid UpdateSingleBlock(string esqlModelType, ESQLSingleItem esqlSingleBlock)
        {
            return this.InsertOrUpdateSingleBlockPrivate(esqlModelType, esqlSingleBlock, ProcessingType.Update);
        }

        public Guid InsertOrUpdateSingleBlock(string esqlModelType, ESQLSingleItem esqlSingleBlock)
        {
            throw new NotImplementedException("InsertOrUpdateSingleBlock is not implemted yet.");
        }

        private Guid InsertOrUpdateSingleBlockPrivate(string esqlModelType, ESQLSingleItem esqlSingleBlock, ProcessingType processingType)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(esqlModelType);
            ArgumentNullException.ThrowIfNull(esqlSingleBlock);

            return this.RunRequestInTransaction((conn) =>
            {
                var dataSet = new DataSet(esqlSingleBlock.Name);

                var id = this.InsertOrUpdateESQLSingleItemToDataSet(
                    esqlSingleBlock,
                    esqlModelType,
                    esqlSingleBlock.Item.ObjectID.Value,
                    dataSet,
                    conn,
                    processingType);

                dataSet.AcceptChanges();

                return id;
            });
        }

        public bool DeleteSingleBlock(string typeName, Guid id)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(typeName);

            if (id == Guid.Empty)
                throw new ArgumentException("Id must be a non-empty GUID.", nameof(id));

            return this.RunRequestInTransaction((conn) =>
            {
                DataSet dataSet = new DataSet(typeName);

                var esqlModelAdapter = this.PopulateTableToDataSet(conn, dataSet, typeName, whereClause:
                    this._queryHelper.GetWhereExpressionForID(id));

                var esqlModelBuilder = this._queryHelper.GetDbCommandBuilder(esqlModelAdapter);

                esqlModelBuilder.GetDeleteCommand();

                DataTable dataTable = dataSet.Tables[typeName];

                var existingRow = dataTable.Rows.Cast<DataRow>().SingleOrDefault(x => ConvertHelper.ParseGuid(x["ID"]) == id);

                if (existingRow != null)
                {
                    existingRow.Delete();

                    esqlModelAdapter.Update(dataSet, typeName);

                    dataSet.AcceptChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        public ESQLSingleItem GetSingleBlock(ESQLBlockDefinition container, Guid id)
        {
            if (container == null)
                return null;

            ESQLSingleItem result = null;

            this.RunRequest((conn) =>
            {
                DataSet dataSet = new DataSet(container.Type);

                var whereClauseForID = this._queryHelper.GetWhereExpressionForID(id);

                this.PopulateTableToDataSet(conn, dataSet, container.Type,
                    columnNames: container.Select(x => x.ColumnDefinition.ESQLExpression),
                    whereClause: whereClauseForID);

                if (dataSet.Tables[container.Type].Rows.Count == 0)
                {
                    result = null;
                }
                else
                {
                    result = this.ConvertToESQLSingleItem(container);

                    this.PopulateESQLSingleItem(result, container, dataSet.Tables[result.Name].Rows.Cast<DataRow>()
                        .SingleOrDefault(y => ConvertHelper.ParseGuid(y[Constants.ID]) == id));
                }
            });

            return result;
        }

        // TODO: can be refactored using stored procedure
        private DXRelationDefinitionMainElement GetRelationInfo(string obj1Name, string relationToObj2Name)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var dataSet = new DataSet("DXRelationDefinitionUnit");

                this.PopulateTableToDataSet(conn, dataSet, "DXRelationDefinitionMainElement"
                    , whereClause:
                     this._queryHelper.GetWhereExpressionWithAnd(
                        new Dictionary<string, object>()
                        {
                            { "ObjectNameLeft", obj1Name },
                            { "RelationNameRight", relationToObj2Name }
                        })
                     );

                if (dataSet.Tables["DXRelationDefinitionMainElement"].Rows.Count == 0)
                {
                    throw new Exception($"Relation pair {obj1Name}-{relationToObj2Name} is not existing in system");
                }

                var row = dataSet.Tables["DXRelationDefinitionMainElement"].Rows[0];

                return new DXRelationDefinitionMainElement()
                {
                    RelationType = (DXRelationTypeEnum)ConvertHelper.ParseInt(row["RelationType"]),
                    RelationTable = ConvertHelper.ParseString(row["RelationTable"]),
                    ID = ConvertHelper.ParseGuid(row[Constants.ID]),
                    ObjectID = ConvertHelper.ParseGuid(row[Constants.ObjectID]),
                    ObjectNameLeft = ConvertHelper.ParseString(row["ObjectNameLeft"]),
                    ObjectNameRight = ConvertHelper.ParseString(row["ObjectNameRight"]),
                    RelationNameLeft = ConvertHelper.ParseString(row["RelationNameLeft"]),
                    RelationNameRight = ConvertHelper.ParseString(row["RelationNameRight"]),
                    RelationColumnNameLeft = ConvertHelper.ParseString(row["RelationColumnNameLeft"]),
                    RelationColumnNameRight = ConvertHelper.ParseString(row["RelationColumnNameRight"]),
                    RelationColumnTypeLeft = row["RelationColumnTypeLeft"] == DBNull.Value ? null : (DXColumnTypeEnum)ConvertHelper.ParseInt(row["RelationColumnTypeLeft"]),
                    RelationColumnTypeRight = row["RelationColumnTypeRight"] == DBNull.Value ? null : (DXColumnTypeEnum)ConvertHelper.ParseInt(row["RelationColumnTypeRight"])
                };
            });
        }

        private bool AddRelationManyToMany(DXRelationDefinitionMainElement relationInfo, Guid obj1Id, Guid obj2Id)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var dataSet = new DataSet(relationInfo.RelationTable);

                var adapter = this.PopulateTableToDataSet(conn, dataSet, relationInfo.RelationTable
                    , whereClause:
                    this._queryHelper.GetWhereExpressionWithAnd(
                        new Dictionary<string, object>()
                        {
                            { relationInfo.RelationNameLeft, obj1Id },
                            { relationInfo.RelationNameRight, obj2Id }
                        })
                    );

                DataRow dataRow;

                if (dataSet.Tables[relationInfo.RelationTable].Rows.Count == 0)
                {
                    var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
                    modelBuilder.GetInsertCommand();

                    dataRow = dataSet.Tables[relationInfo.RelationTable].NewRow();

                    dataSet.Tables[relationInfo.RelationTable].Rows.Add(dataRow);
                }
                else
                {
                    var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
                    modelBuilder.GetUpdateCommand();

                    dataRow = dataSet.Tables[relationInfo.RelationTable].Rows[0];
                }

                dataRow[relationInfo.RelationNameLeft] = obj1Id;
                dataRow[relationInfo.RelationNameRight] = obj2Id;

                adapter.Update(dataSet, relationInfo.RelationTable);
                dataSet.AcceptChanges();

                return true;
            });
        }

        private bool AddRelationManyToOne(DXRelationDefinitionMainElement relationInfo, Guid obj1Id, Guid obj2Id)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var tableName = relationInfo.ObjectNameLeft;

                var dataSet = new DataSet(tableName);

                var adapter = this.PopulateTableToDataSet(conn, dataSet, tableName
                    , whereClause: this._queryHelper.GetWhereExpressionForID(obj1Id));

                var rows = dataSet.Tables[tableName].Rows;

                if (rows.Count == 1)
                {
                    var row = rows[0];

                    row[relationInfo.RelationNameRight] = obj2Id;

                    var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
                    modelBuilder.GetUpdateCommand();

                    adapter.Update(dataSet, tableName);
                    dataSet.AcceptChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        private bool AddRelationOneToMany(DXRelationDefinitionMainElement relationInfo, Guid obj1Id, Guid obj2Id)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var tableName = relationInfo.ObjectNameRight;

                var dataSet = new DataSet(tableName);

                var adapter = this.PopulateTableToDataSet(conn, dataSet, tableName
                    , whereClause: this._queryHelper.GetWhereExpressionForID(obj2Id));

                var rows = dataSet.Tables[tableName].Rows;

                if (rows.Count == 1)
                {
                    var row = rows[0];

                    row[relationInfo.RelationNameLeft] = obj1Id;

                    var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
                    modelBuilder.GetUpdateCommand();

                    adapter.Update(dataSet, tableName);
                    dataSet.AcceptChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        private bool AddRelationZeroOneToZeroOne(DXRelationDefinitionMainElement relationInfo, Guid obj1Id, Guid obj2Id)
        {
            bool isRightTableContainsRelationID = relationInfo.RelationTable.Equals(relationInfo.ObjectNameRight);

            if (isRightTableContainsRelationID)
            {
                return this.AddRelationOneToMany(relationInfo, obj1Id, obj2Id);
            }
            else
            {
                return this.AddRelationManyToOne(relationInfo, obj1Id, obj2Id);
            }
        }

        private bool RemoveRelationManyToMany(DXRelationDefinitionMainElement relationInfo, Guid obj1Id, Guid obj2Id)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var dataSet = new DataSet(relationInfo.RelationTable);

                var adapter = this.PopulateTableToDataSet(conn, dataSet, relationInfo.RelationTable
                    , whereClause:
                    this._queryHelper.GetWhereExpressionWithAnd(
                        new Dictionary<string, object>()
                        {
                            { relationInfo.RelationNameLeft, obj1Id },
                            { relationInfo.RelationNameRight, obj2Id }
                        })
                    );

                if (dataSet.Tables[relationInfo.RelationTable].Rows.Count > 0)
                {
                    var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
                    modelBuilder.GetDeleteCommand();

                    for (int i = 0; i < dataSet.Tables[relationInfo.RelationTable].Rows.Count; i++)
                    {
                        var row = dataSet.Tables[relationInfo.RelationTable].Rows[i];
                        row.Delete();
                    }

                    adapter.Update(dataSet, relationInfo.RelationTable);
                    dataSet.AcceptChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        private bool RemoveRelationManyToZeroOne(DXRelationDefinitionMainElement relationInfo, Guid obj1Id, Guid obj2Id)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var tableName = relationInfo.ObjectNameLeft;

                var dataSet = new DataSet(tableName);

                var adapter = this.PopulateTableToDataSet(conn, dataSet, tableName
                    , whereClause:
                     this._queryHelper.GetWhereExpressionWithAnd(
                        new Dictionary<string, object>()
                        {
                            { "ID", obj1Id },
                            { relationInfo.RelationNameRight, obj2Id }
                        })
                     );

                var rows = dataSet.Tables[tableName].Rows;

                if (rows.Count == 1)
                {
                    var row = rows[0];

                    row[relationInfo.RelationNameRight] = DBNull.Value;

                    var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
                    modelBuilder.GetUpdateCommand();

                    adapter.Update(dataSet, tableName);
                    dataSet.AcceptChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        private bool RemoveRelationZeroOneToZeroOne(DXRelationDefinitionMainElement relationInfo, Guid obj1Id, Guid obj2Id)
        {
            bool isRightTableContainsRelationID = relationInfo.RelationTable.Equals(relationInfo.ObjectNameRight);

            if (isRightTableContainsRelationID)
            {
                return this.RemoveRelationZeroOneToMany(relationInfo, obj1Id, obj2Id);
            }
            else
            {
                return this.RemoveRelationManyToZeroOne(relationInfo, obj1Id, obj2Id);
            }
        }

        private bool RemoveRelationZeroOneToMany(DXRelationDefinitionMainElement relationInfo, Guid obj1Id, Guid obj2Id)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var tableName = relationInfo.ObjectNameRight;

                var dataSet = new DataSet(tableName);

                var adapter = this.PopulateTableToDataSet(conn, dataSet, tableName
                    , whereClause: this._queryHelper.GetWhereExpressionWithAnd(
                        new Dictionary<string, object>()
                        {
                            { "ID", obj2Id },
                            { relationInfo.RelationNameLeft, obj1Id}
                        })
                    );

                var rows = dataSet.Tables[tableName].Rows;

                if (rows.Count == 1)
                {
                    var row = rows[0];

                    row[relationInfo.RelationNameLeft] = DBNull.Value;

                    var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
                    modelBuilder.GetUpdateCommand();

                    adapter.Update(dataSet, tableName);
                    dataSet.AcceptChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        private IEnumerable<Guid> GetRelationsManyToMany(DXRelationDefinitionMainElement relationInfo, Guid obj1Id)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var dataSet = new DataSet(relationInfo.RelationTable);

                this.PopulateTableToDataSet(conn, dataSet, relationInfo.RelationTable
                    , whereClause: this._queryHelper.GetWhereExpressionWithAnd(
                        new Dictionary<string, object>()
                        {
                            { relationInfo.RelationNameLeft, obj1Id }
                        })
                    );

                return dataSet.Tables[relationInfo.RelationTable].Rows.Cast<DataRow>().Select(x =>
                {
                    var relatedId = ConvertHelper.ParseGuid(x[relationInfo.RelationNameRight]);

                    return relatedId;
                });
            });
        }

        private IEnumerable<Guid> GetRelationsManyToOne(DXRelationDefinitionMainElement relationInfo, Guid obj1Id)
        {
            IEnumerable<Guid> result = this.RunRequestInTransaction((conn) =>
            {
                var tableName = relationInfo.ObjectNameLeft;

                var dataSet = new DataSet(tableName);

                this.PopulateTableToDataSet(conn, dataSet, tableName
                    , whereClause: this._queryHelper.GetWhereExpressionForID(obj1Id));

                return dataSet.Tables[tableName].Rows.Cast<DataRow>()
                    .Where(x => x[relationInfo.RelationNameRight] != DBNull.Value)
                    .Select(x => ConvertHelper.ParseGuid(x[relationInfo.RelationNameRight]));
            });

            return result;
        }

        private IEnumerable<Guid> GetRelationsOneToMany(DXRelationDefinitionMainElement relationInfo, Guid obj1Id)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var tableName = relationInfo.ObjectNameRight;

                var dataSet = new DataSet(tableName);

                this.PopulateTableToDataSet(conn, dataSet, tableName
                    , whereClause: this._queryHelper.GetWhereExpressionWithAnd(
                        new Dictionary<string, object>()
                        {
                            { relationInfo.RelationNameLeft, obj1Id }
                        })
                    );

                return dataSet.Tables[tableName].Rows.Cast<DataRow>().Select(x => ConvertHelper.ParseGuid(x[Constants.ID]));
            });
        }

        private IEnumerable<Guid> GetRelationsZeroOneToZeroOne(DXRelationDefinitionMainElement relationInfo, Guid obj1Id)
        {
            bool isRightTableContainsRelationID = relationInfo.RelationTable.Equals(relationInfo.ObjectNameRight);

            return isRightTableContainsRelationID ? this.GetRelationsOneToMany(relationInfo, obj1Id) : this.GetRelationsManyToOne(relationInfo, obj1Id);
        }

        private DbDataAdapter PopulateTableToDataSet(
            DbConnection conn,
            DataSet dataSet,
            string tableName,
            IEnumerable<string> columnNames = null,
            string whereClause = null,
            IDictionary<string, string> orderBy = null,
            int? limit = null)
        {
            var adapter = this._queryHelper.GetDbDataAdapter(conn, this._queryHelper.GetSQLQuery(tableName, columnNames, whereClause, orderBy, limit));

            adapter.Fill(dataSet, tableName);

            return adapter;
        }

        private T RunRequestInTransaction<T>(Func<DbConnection, T> func)
        {
            using (DbConnection conn = this._queryHelper.GetDBConnection(this._connectionStr))
            {
                conn.Open();
                var transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted);

                try
                {
                    var result = func.Invoke(conn);
                    transaction.Commit();

                    return result;
                }
                catch (Exception exc)
                {
                    var exceptions = new List<Exception>() { exc };
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception exc2)
                    {
                        exceptions.Add(exc2);
                    }

                    throw new AggregateException(exceptions);
                }
            }
        }

        private void RunRequest(Action<DbConnection> action)
        {
            using (DbConnection conn = this._queryHelper.GetDBConnection(this._connectionStr))
            {
                conn.Open();

                action.Invoke(conn);
            }
        }

        public void DropDataBase()
        {
            this._queryHelper.DropDataBase(this._connectionStr);
        }

        public void CreateDataBase()
        {
            this._queryHelper.CreateDataBase(this._connectionStr);
        }

        private enum ProcessingType
        {
            Insert = 1,
            Update = 2,
            Delete = 3
        }
    }
}