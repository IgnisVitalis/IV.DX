using IV.DX.Kernel;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXModelConverters;
using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Helpers.DXModelDefinitionHelpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Abstractions;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace IV.DX.Persistence
{
    internal partial class DXCoreRepository : IDXUnitCoreRepository, IDXStructureRepository, IDXEnumCoreRepository, IDXStructureRawReader, IDXElementCoreRepository, IDXRawReader
    {
        protected string _connectionStr;
        protected ISQLQueryDXHelper _queryHelper;
        IDXStructureCache _dxStructureCache;
        ISQLQueryBuilder _sqlQueryBuilder;

        public DXCoreRepository(
            DXDatabaseOptions options,
            IDXStructureCache dxStructureCache,
            ISQLQueryDXHelper queryHelper,
            ISQLQueryBuilder sqlQueryBuilder)
        {
            this._connectionStr = options.ConnectionString;
            this._queryHelper = queryHelper;
            this._dxStructureCache = dxStructureCache;
            this._sqlQueryBuilder = sqlQueryBuilder;
        }

        public bool Delete(string typeName, Guid id)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(typeName);

            if (id == Guid.Empty)
                throw new ArgumentException("Id must be a non-empty GUID.", nameof(id));

            var mainDXUnitInfo = this.GetDXUnitDefinition(typeName);

            var dxUnitHierarchy = this._dxStructureCache.GetDXUnitInheritance(mainDXUnitInfo);

            return this.RunRequestInTransaction((conn) =>
            {
                DataSet dataSet = new DataSet(typeName);

                foreach (var dxUnitHierarchyItem in dxUnitHierarchy.Items)
                {
                    // Delete related dxElements
                    foreach (var relatedDXElement in dxUnitHierarchyItem.AllDXElements)
                    {
                        this.DeleteDXElementsFromDataSet(relatedDXElement.Name, id, dataSet, conn);
                    }

                    // Delete dxUnit
                    this.DeleteDXUnitFromDataSet(dxUnitHierarchyItem.DXUnit.Name, id, dataSet, conn);
                }

                dataSet.AcceptChanges();

                return true;
            });
        }

        public DXModel? GetItem(DXDataSetDefinition container, Guid id, DXLoadingType typeOfLoading)
        {
            if (container == null)
                return null;

            return this.RunRequest((conn) =>
            {
                var dataSet = this.PopulateDataSetForTargetDXUnit(container, id, conn);
                var dataTable = dataSet.Tables[container.MainElement.Name];

                if (dataTable == null)
                    throw new Exception($"Table '{container.MainElement.Name}' wouldn't load");

                if (dataTable.Rows.Count == 0)
                {
                    return null;
                }
                else
                {
                    var dataRow = dataTable.Rows[0];
                    var record = this.ConvertToDXUnitRecord(dataSet, dataRow, container);

                    return BuildDXModel(container.MainElement.DXUnitType, record);
                }
            });
        }

        public DXDataBlock<DXUnitRecord>? GetItemRecord(DXDataSetDefinition container, Guid id, DXLoadingType typeOfLoading)
        {
            if (container == null)
                return null;

            return this.RunRequest((conn) =>
            {
                var dataSet = this.PopulateDataSetForTargetDXUnit(container, id, conn);
                var dataTable = dataSet.Tables[container.MainElement.Name];

                if (dataTable == null)
                    throw new Exception($"Table '{container.MainElement.Name}' wouldn't load");

                if (dataTable.Rows.Count == 0)
                {
                    return null;
                }
                else
                {
                    var dataRow = dataTable.Rows[0];
                    var record = this.ConvertToDXUnitRecord(dataSet, dataRow, container);

                    return new DXDataBlock<DXUnitRecord>
                    {
                        Meta = new DXMeta
                        {
                            Kind = "DXUnit",
                            Type = container.MainElement.DXUnitType,
                            Op = "Sync",
                            IsMulti = true
                        },
                        Data = new DXData<DXUnitRecord>
                        {
                            Upsert = new List<DXUnitRecord> { record }
                        }
                    };
                }
            });
        }


        public IEnumerable<DXModel> GetItems(DXDataSetDefinition container, IEnumerable<Guid> objIds, DXLoadingType typeOfLoading)
        {
            if (container == null)
                return null;

            if (objIds == null)
                return null;

            if (objIds.Count() == 0)
                return new List<DXModel>();

            IEnumerable<DXModel> resultItems = null;

            this.RunRequest((conn) =>
            {
                resultItems = this.GetItems(conn, container, objIds, typeOfLoading);
            });

            return resultItems;
        }

        public IEnumerable<DXModel> GetItems(DbConnection conn, DXDataSetDefinition container, IEnumerable<Guid> objIds, DXLoadingType typeOfLoading)
        {
            if (container == null)
                return null;

            if (objIds == null)
                return null;

            if (objIds.Count() == 0)
                return new List<DXModel>();

            IEnumerable<DXModel> resultItems = null;

            var dataSet = this.PopulateDataSetForTargetDXUnits(container, objIds, conn);
            var dataTable = dataSet.Tables[container.MainElement.DXUnitType];

            var items = dataTable.Rows;

            resultItems = items.Cast<DataRow>()
                .Select(x => this.ConvertToDXUnitRecord(dataSet, x, container))
                .Select(x => BuildDXModel(container.MainElement.DXUnitType, x))
                .ToList();

            dataSet.AcceptChanges();

            return resultItems;
        }

        public IEnumerable<DXModel> GetItems(string typeName)
        {
            var modelDefinition = this.GetModelDefinition(typeName);

            if (modelDefinition == null)
                return null;

            return this.GetItems(modelDefinition, DXLoadingType.Full);
        }

        public IEnumerable<DXModel> GetItems(string typeName, IEnumerable<Guid> ids)
        {
            var modelDefinition = this.GetModelDefinition(typeName);

            if (modelDefinition == null)
                return null;

            return this.GetItems(modelDefinition, ids, DXLoadingType.Full);
        }

        public IEnumerable<DXModel> GetItems(string typeName, string dxFilter)
        {
            var modelDefinition = this.GetModelDefinition(typeName);

            if (modelDefinition == null)
                return null;

            return this.GetItems(modelDefinition, dxFilter, DXLoadingType.Full);
        }

        // TODO: need to check what kind of data is loaded. Because this method should load only IDs
        public IEnumerable<Guid> GetItemIDs(string typeName, string? dxFilter = default)
        {
            string sqlQuery =
                  this._sqlQueryBuilder.BuildSQLExpression(typeName, SQLQueryBuilder.BaseColumns, dxFilter);

            return this.RunRequestInTransaction((conn) =>
            {
                var dataSet = new DataSet(typeName);

                var adapter = this._queryHelper.GetDbDataAdapter(conn, sqlQuery);

                adapter.Fill(dataSet, typeName);

                var table = dataSet.Tables[typeName];

                var ids = table.Rows.Cast<DataRow>().Select(x => ConvertHelper.ParseGuid(x[Constants.ID])).ToList();

                return ids;
            });
        }

        public DXModel GetItem(string typeName, Guid id)
        {
            var modelDefinition = this.GetModelDefinition(typeName);

            if (modelDefinition == null)
                return null;

            return this.GetItem(modelDefinition, id, DXLoadingType.Full);
        }

        public DXDataBlock<DXUnitRecord>? GetItemRecord(string typeName, Guid objectId)
        {
            var modelDefinition = this.GetModelDefinition(typeName);

            if (modelDefinition == null)
                return null;

            return this.GetItemRecord(modelDefinition, objectId, DXLoadingType.Full);
        }

        private DXDataSetDefinition GetModelDefinition(string type)
        {
            var mainDXUnit = this.GetDXUnitDefinition(type);

            if (mainDXUnit == null)
                throw new Exception($"There are no DXUnit with name '{mainDXUnit}'");

            var dxUnitHierarchy = this._dxStructureCache.GetDXUnitInheritance(mainDXUnit);

            var dxUnitDefinition = DXDataSetDefinitionConverter.ToDXModelDefinition(type, dxUnitHierarchy);

            return dxUnitDefinition;
        }

        public IEnumerable<DXModel> GetItems(DXDataSetDefinition container, DXLoadingType typeOfLoading)
        {
            return GetItems(container, string.Empty, typeOfLoading);
        }

        public IEnumerable<DXModel> GetItems(DXDataSetDefinition container, string dxFilter, DXLoadingType typeOfLoading)
        {
            string typeName = container.MainElement.DXUnitType;

            var ids = this.GetItemIDs(typeName, dxFilter);

            return this.GetItems(container, ids, typeOfLoading);
        }

        public DXDataBlock<DXUnitRecord> GetItemsRecord(string typeName)
        {
            var modelDefinition = this.GetModelDefinition(typeName);

            if (modelDefinition == null)
                return new DXDataBlock<DXUnitRecord>
                {
                    Meta = new DXMeta { Kind = "DXUnit", Type = typeName, Op = "Sync", IsMulti = true },
                    Data = new DXData<DXUnitRecord> { Upsert = new List<DXUnitRecord>() }
                };

            return GetItemsRecord(modelDefinition, DXLoadingType.Full);
        }

        public DXDataBlock<DXUnitRecord> GetItemsRecord(string typeName, IEnumerable<Guid> objectIds)
        {
            var modelDefinition = this.GetModelDefinition(typeName);

            if (modelDefinition == null)
                return new DXDataBlock<DXUnitRecord>
                {
                    Meta = new DXMeta { Kind = "DXUnit", Type = typeName, Op = "Sync", IsMulti = true },
                    Data = new DXData<DXUnitRecord> { Upsert = new List<DXUnitRecord>() }
                };

            return GetItemsRecord(modelDefinition, objectIds, DXLoadingType.Full);
        }

        public DXDataBlock<DXUnitRecord> GetItemsRecord(string typeName, string dxFilter)
        {
            var modelDefinition = this.GetModelDefinition(typeName);

            if (modelDefinition == null)
                return new DXDataBlock<DXUnitRecord>
                {
                    Meta = new DXMeta { Kind = "DXUnit", Type = typeName, Op = "Sync", IsMulti = true },
                    Data = new DXData<DXUnitRecord> { Upsert = new List<DXUnitRecord>() }
                };

            return GetItemsRecord(modelDefinition, dxFilter, DXLoadingType.Full);
        }

        public DXDataBlock<DXUnitRecord> GetItemsRecord(DXDataSetDefinition container, DXLoadingType typeOfLoading)
        {
            return GetItemsRecord(container, string.Empty, typeOfLoading);
        }

        public DXDataBlock<DXUnitRecord> GetItemsRecord(DXDataSetDefinition container, IEnumerable<Guid> objectIds, DXLoadingType typeOfLoading)
        {
            if (container == null || objectIds == null)
            {
                return new DXDataBlock<DXUnitRecord>
                {
                    Meta = new DXMeta { Kind = "DXUnit", Type = container?.MainElement?.DXUnitType, Op = "Sync", IsMulti = true },
                    Data = new DXData<DXUnitRecord> { Upsert = new List<DXUnitRecord>() }
                };
            }

            if (!objectIds.Any())
            {
                return new DXDataBlock<DXUnitRecord>
                {
                    Meta = new DXMeta { Kind = "DXUnit", Type = container.MainElement.DXUnitType, Op = "Sync", IsMulti = true },
                    Data = new DXData<DXUnitRecord> { Upsert = new List<DXUnitRecord>() }
                };
            }

            return this.RunRequest((conn) =>
            {
                var dataSet = this.PopulateDataSetForTargetDXUnits(container, objectIds, conn);
                var dataTable = dataSet.Tables[container.MainElement.DXUnitType];

                var items = dataTable.Rows.Cast<DataRow>()
                    .Select(x => this.ConvertToDXUnitRecord(dataSet, x, container))
                    .ToList();

                dataSet.AcceptChanges();

                return new DXDataBlock<DXUnitRecord>
                {
                    Meta = new DXMeta
                    {
                        Kind = "DXUnit",
                        Type = container.MainElement.DXUnitType,
                        Op = "Sync",
                        IsMulti = true
                    },
                    Data = new DXData<DXUnitRecord>
                    {
                        Upsert = items
                    }
                };
            });
        }

        public DXDataBlock<DXUnitRecord> GetItemsRecord(DXDataSetDefinition container, string dxFilter, DXLoadingType typeOfLoading)
        {
            string typeName = container.MainElement.DXUnitType;

            var ids = this.GetItemIDs(typeName, dxFilter);

            return this.GetItemsRecord(container, ids, typeOfLoading);
        }

        private DXUnitRecord ConvertToDXUnitRecord(DataSet dataSet, DataRow row, DXDataSetDefinition container)
        {
            var id = ConvertHelper.ParseGuid(row[Constants.ID]);
            var mainItem = this.GetDXItem(row, container.MainElement);

            var record = new DXUnitRecord
            {
                ID = id,
                TimeStamp = mainItem.TimeStamp,
                Fields = ConvertItemToFields(mainItem),
                DXElements = new Dictionary<string, DXDataBlock<DXElementRecord>>(StringComparer.OrdinalIgnoreCase)
            };

            foreach (var singleItem in container.SingleFragmentDefinitions)
            {
                var dataTable = dataSet.Tables[singleItem.Type];
                var dataRow = dataTable.Rows.Cast<DataRow>()
                    .SingleOrDefault(y => ConvertHelper.ParseGuid(y[Constants.DXUnitID]) == id);

                if (dataRow == null)
                    continue;

                var dxItem = this.GetDXItem(dataRow, singleItem);
                var elementRecord = new DXElementRecord
                {
                    ID = dxItem.ID,
                    TimeStamp = dxItem.TimeStamp,
                    DXUnitID = dxItem.DXUnitID,
                    Fields = ConvertItemToFields(dxItem)
                };

                record.DXElements[singleItem.Name] = new DXDataBlock<DXElementRecord>
                {
                    Meta = new DXMeta
                    {
                        Kind = "DXElement",
                        Type = singleItem.Name,
                        Op = "Patch",
                        IsMulti = false,
                        IsRequired = singleItem.IsRequired
                    },
                    Data = new DXData<DXElementRecord>
                    {
                        Upsert = new List<DXElementRecord> { elementRecord }
                    }
                };
            }

            foreach (var multiItem in container.MultiFragmentDefinitions)
            {
                var dataTable = dataSet.Tables[multiItem.Name];
                var announced = dataTable.Rows.Cast<DataRow>()
                    .Where(y => ConvertHelper.ParseGuid(y[Constants.DXUnitID]) == id)
                    .Select(x => this.GetDXItem(x, multiItem))
                    .Select(x => new DXElementRecord
                    {
                        ID = x.ID,
                        TimeStamp = x.TimeStamp,
                        DXUnitID = x.DXUnitID,
                        Fields = ConvertItemToFields(x)
                    })
                    .ToList();

                record.DXElements[multiItem.Name] = new DXDataBlock<DXElementRecord>
                {
                    Meta = new DXMeta
                    {
                        Kind = "DXElement",
                        Type = multiItem.Name,
                        Op = "Sync",
                        IsMulti = true,
                        IsRequired = multiItem.IsRequired
                    },
                    Data = new DXData<DXElementRecord>
                    {
                        Upsert = announced.Count == 0 ? null : announced
                    }
                };
            }

            return record;
        }

        private static Dictionary<string, JToken>? ConvertItemToFields(DXItem item)
        {
            if (item?.Content == null || item.Content.Count == 0)
                return null;

            var result = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in item.Content)
            {
                if (Constants.SystemProperties.Any(p => string.Equals(p, kvp.Key, StringComparison.OrdinalIgnoreCase)))
                    continue;

                result[kvp.Key] = kvp.Value == null ? JValue.CreateNull() : JToken.FromObject(kvp.Value);
            }

            return result.Count == 0 ? null : result;
        }

        private DataSet PopulateDataSetForTargetDXUnit(DXDataSetDefinition container, Guid id, DbConnection conn)
        {
            DataSet dataSet = new DataSet(container.MainElement.DXUnitType);

            this.PopulateTableToDataSet(conn, dataSet, container.MainElement.DXUnitType,
                columns: container.MainElement.GetColumns(),
                dxFilter: this.GetWhereExpressionForID(id), fillSchema: false);

            var whereClauseForDXUnitID = this.GetWhereExpressionForDXUnitID(id);

            foreach (var singleItem in container.SingleFragmentDefinitions)
            {
                this.PopulateTableToDataSet(conn, dataSet, singleItem.Type,
                    columns: singleItem.GetColumns(),
                    dxFilter: whereClauseForDXUnitID, fillSchema: false);
            }

            foreach (var multiItem in container.MultiFragmentDefinitions)
            {
                this.PopulateTableToDataSet(conn, dataSet, multiItem.Type,
                    columns: multiItem.GetColumns(),
                    dxFilter: whereClauseForDXUnitID, fillSchema: false);
            }

            return dataSet;
        }

        private DataSet PopulateDataSetForTargetDXUnits(DXDataSetDefinition container, IEnumerable<Guid> ids, DbConnection conn)
        {
            DataSet dataSet = new DataSet(container.MainElement.DXUnitType);

            if (ids != null && ids.Count() > 0)
            {
                this.PopulateTableToDataSet(
                    conn,
                    dataSet,
                    container.MainElement.DXUnitType,
                    columns: container.MainElement.GetColumns(),
                    dxFilter: this.GetWhereExpressionForID(ids),
                    fillSchema: false);

                var whereClauseForDXUnitIDs = this.GetWhereExpressionForDXUnitID(ids);

                foreach (var singleItem in container.SingleFragmentDefinitions)
                {
                    this.PopulateTableToDataSet(
                        conn,
                        dataSet,
                        singleItem.Type,
                        columns: singleItem.GetColumns(),
                        dxFilter: whereClauseForDXUnitIDs,
                        fillSchema: false);
                }

                foreach (var multiItem in container.MultiFragmentDefinitions)
                {
                    this.PopulateTableToDataSet(
                        conn,
                        dataSet,
                        multiItem.Type,
                        columns: multiItem.GetColumns(),
                        dxFilter: whereClauseForDXUnitIDs,
                        fillSchema: false);
                }
            }

            return dataSet;
        }

        private DXItem GetDXItem(string typeName, DataRow row, IDictionary<string, string> columns)
        {
            Dictionary<string, object> jObjectContainerCopy = new Dictionary<string, object>();

            jObjectContainerCopy[Constants.SystemPropertyTypeName] = typeName;

            foreach (DataColumn dataColumn in row.Table.Columns)
            {
                if (!columns.ContainsKey(dataColumn.ColumnName))
                    continue;

                if (row[dataColumn.ColumnName] != DBNull.Value)
                {
                    jObjectContainerCopy[dataColumn.ColumnName] = GetValueFromRow(row, dataColumn);
                }
                else
                {
                    jObjectContainerCopy[dataColumn.ColumnName] = null;
                }
            }

            var id = ConvertHelper.ParseGuid(row[Constants.ID]);
            var timeStamp = ConvertHelper.ParseDateTime(row[Constants.TimeStamp]);

            if (row.Table.Columns.Contains(Constants.DXUnitID))
            {
                var dxUnitID = ConvertHelper.ParseGuid(row[Constants.DXUnitID]);

                return new DXItem(typeName, id, dxUnitID, timeStamp, jObjectContainerCopy);
            }
            else
            {
                return new DXItem(typeName, id, id, timeStamp, jObjectContainerCopy);
            }
        }

        private DXItem GetDXItem(DataRow row, DXTableDefinition structure)
        {
            var typeName = structure.Type;
            var columns = structure.ToDictionary(x => x.ColumnAttribute.Name, x => x.ColumnAttribute.DXExpression);

            return this.GetDXItem(typeName, row, columns);
        }

        private DXItem GetDXItem(DataRow row, DXMainTableDefinition structure)
        {
            var typeName = structure.DXUnitType;
            var columns = structure.ToDictionary(x => x.ColumnAttribute.Name, x => x.ColumnAttribute.DXExpression);

            return this.GetDXItem(typeName, row, columns);
        }

        private object GetValueFromRow(DataRow dataRow, DataColumn dataColumn)
        {
            if (dataColumn.DataType == typeof(DateTime))
            {
                var dateTime = ConvertHelper.ParseDateTime(dataRow[dataColumn]);
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }

            if (dataColumn.DataType == typeof(byte[]))
            {
                var bytes = (byte[])dataRow[dataColumn];
                return bytes.Length == 0 ? string.Empty : Convert.ToBase64String(bytes);
            }

            return dataRow[dataColumn];
        }

        public Guid Insert(DXModel dxModel)
        {
            return this.InsertOrUpdate(dxModel, ProcessingType.Insert);
        }

        public Guid Update(DXModel dxModel)
        {
            return this.InsertOrUpdate(dxModel, ProcessingType.Update);
        }

        private Guid InsertOrUpdate(DXModel dxModel, ProcessingType processingType)
        {
            ArgumentNullException.ThrowIfNull(dxModel);

            var typeName = dxModel.DXMainElement.Attribute.Type;

            var mainDXUnitInfo = this.GetDXUnitDefinition(typeName);
            if (mainDXUnitInfo != null)
                return this.InsertOrUpdateDXUnit(mainDXUnitInfo, dxModel, processingType);

            throw new Exception($"Unit type '{dxModel.DXMainElement.Attribute.Type}' is not registered.");
        }

        public Guid InsertOrUpdate(DXModel dxModel)
        {
            var objId = dxModel.DXMainElement.Item.ID;
            var type = dxModel.DXMainElement.Attribute.Type;

            if (!string.IsNullOrEmpty(type)
                && this.IsItemExisting(type, objId))
            {
                return this.Update(dxModel);
            }
            else
            {
                return this.Insert(dxModel);
            }
        }

        public Guid InsertOrUpdate(DXDataBlock<DXUnitRecord> block)
        {
            ArgumentNullException.ThrowIfNull(block);

            var typeName = block.Meta?.Type;
            if (string.IsNullOrWhiteSpace(typeName))
                throw new InvalidOperationException("DXUnitRecord block Meta.Type is required.");

            if (block.Data?.Upsert == null || block.Data.Upsert.Count == 0)
                return Guid.Empty;

            Guid lastId = Guid.Empty;

            foreach (var record in block.Data.Upsert)
            {
                if (record == null) continue;

                var dxModel = BuildDXModel(typeName, record);
                lastId = this.InsertOrUpdate(dxModel);
            }

            return lastId;
        }

        private static DXModel BuildDXModel(string typeName, DXUnitRecord record)
        {
            var block = new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta
                {
                    Type = typeName,
                    Kind = "DXUnit",
                    Op = "Patch",
                    IsMulti = true
                },
                Data = new DXData<DXUnitRecord>
                {
                    Upsert = new List<DXUnitRecord> { record }
                }
            };

            return DXRecordModelConverter.ToDXModel(block, record);
        }

        public bool IsItemExisting(string type, Guid objectId)
        {
            DXDataSetDefinition dd = new DXDataSetDefinition(new DXMainTableDefinition(type, type));

            var item = this.GetItem(dd, objectId, DXLoadingType.Base);

            return item != null;
        }     

        private Guid InsertOrUpdateDXUnit(DXUnitDefinitionUnit mainDXUnitInfo, DXModel dxModel, ProcessingType processingType)
        {
            this.RunRequestInTransaction(conn =>
            {
                var dxUnitHierarchy = this._dxStructureCache.GetDXUnitInheritance(mainDXUnitInfo);

                var dxUnitDefinition = dxModel.ToDXModelDefinition(dxUnitHierarchy);

                foreach (var dxUnitHierarchyItem in dxUnitHierarchy.ItemsReverted)
                {
                    var dxUnitInfo = dxUnitHierarchyItem.DXUnit;

                    var dxUnitName = dxUnitInfo.Name;

                    // System provides columns for each base table from db because there are no information in dxModel and it is not possible to separate.
                    // As well during init of db the columns will be empty.
                    // For empty dict sql helper will provide * instead of list of columns.
                    // Need to find better solution later.
                    // var dxUnitColumns = dxUnitInfo.GetColumns();
                    var dxUnitColumns = SQLQueryBuilder.AllColumns;

                    var dataSet = new DataSet(dxUnitName);

                    var multiTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var relatedMM = this._dxStructureCache.GetRelatedDXElementDefinitions(dxUnitInfo, DXElementInUnitTypeEnum.MultiMandatory);
                    var relatedMO = this._dxStructureCache.GetRelatedDXElementDefinitions(dxUnitInfo, DXElementInUnitTypeEnum.MultiOptional);

                    var unitTable = dxUnitInfo.Name;
                    var objectId = dxModel.DXMainElement.Item.ID;

                    // 1) OWN
                    var adapter = PopulateTableToDataSet(
                        conn,
                        dataSet,
                        unitTable,
                        dxUnitColumns,
                        dxFilter: this.GetWhereExpressionForID(objectId));

                    UpsertOwnRow(dxModel, dataSet.Tables[unitTable], unitTable, processingType);

                    SaveTable(adapter, conn, dataSet, dataSet.Tables[unitTable], false);

                    // 2) SINGLE
                    var relatedSM = this._dxStructureCache.GetRelatedDXElementDefinitions(dxUnitInfo, DXElementInUnitTypeEnum.SingleMandatory);
                    var relatedSO = this._dxStructureCache.GetRelatedDXElementDefinitions(dxUnitInfo, DXElementInUnitTypeEnum.SingleOptional);
                    if (relatedSM != null) foreach (var el in relatedSM)
                        UpsertSingle(dxModel, dxUnitDefinition, unitTable, el.Name, dataSet, conn, processingType);
                    if (relatedSO != null) foreach (var el in relatedSO)
                        UpsertSingle(dxModel, dxUnitDefinition, unitTable, el.Name, dataSet, conn, processingType);

                    // 3) MULTI
                    if (relatedMM != null) foreach (var el in relatedMM)
                        UpsertMulti(dxModel, dxUnitDefinition, unitTable, el.Name, dataSet, conn, processingType);
                    if (relatedMO != null) foreach (var el in relatedMO)
                        UpsertMulti(dxModel, dxUnitDefinition, unitTable, el.Name, dataSet, conn, processingType);

                    dataSet.AcceptChanges();
                }

                return true;
            });

            return dxModel.DXMainElement.Item.ID;
        }

        private void SaveTable(DbDataAdapter adapter, DbConnection conn, DataSet dataSet, DataTable table, bool isMultiTable, int bulkThreshold = 500)
        {
            if (table == null || table.Rows.Count == 0) return;

            // bulk for multi-таблиц
            if (isMultiTable && _queryHelper is IDXBulkInsertCapable bulk && table.Rows.Count >= bulkThreshold)
            {
                bulk.BulkUpsert(conn, table, table.TableName);
                return;
            }

            _queryHelper.GetDbCommandBuilder(adapter);
            adapter.Update(table);
        }

        private void UpsertOwnRow(DXModel dxModel, DataTable table, string dxUnitType, ProcessingType processingType)
        {
            var id = dxModel.DXMainElement.Item.ID;

            var row = table.Rows.Find(id);

            if (row == null)
            {
                row = table.NewRow();
                MapdxItemToRow(dxModel.DXMainElement.Item, row, dxUnitType);
                table.Rows.Add(row);
            }
            else
            {
                MapdxItemToRow(dxModel.DXMainElement.Item, row, dxUnitType);
            }
        }

        private void UpsertSingle(
            DXModel dxModel,
            DXDataSetDefinition dxModelDefinition,
            string dxUnitType,
            string dxElementName,
            DataSet dataSet,
            DbConnection conn,
            ProcessingType processingType)
        {
            var dxElement = dxModel.DXSingleElements.SingleOrDefault(x => x.Name.Trim() == dxElementName);

            if (dxElement == null) return;

            var dxElementDefinition = dxModelDefinition.SingleFragmentDefinitions.SingleOrDefault(x => x.Type == dxElementName);

            if (dxElementDefinition == null) return;

            var adapter = PopulateTableToDataSet(conn, dataSet, dxElementName,
                dxElementDefinition.GetColumns(),
                dxFilter: this.GetWhereExpressionForID(dxElement.Item.ID));

            var table = dataSet.Tables[dxElementName];
            var id = dxElement.Item.ID;
            var row = id != Guid.Empty ? table.Rows.Find(id) : null;

            if (row == null)
            {
                row = table.NewRow();
                MapdxItemToRow(dxElement.Item, row, dxUnitType);
                table.Rows.Add(row);
            }
            else
            {
                MapdxItemToRow(dxElement.Item, row, dxUnitType);
            }

            SaveTable(adapter, conn, dataSet, table, false);
        }

        private void UpsertMulti(
            DXModel dxModel,
            DXDataSetDefinition dxModelDefinition,
            string dxUnitType,
            string dxElementName,
            DataSet dataSet,
            DbConnection conn,
            ProcessingType processingType)
        {
            var dxElement = dxModel.DXMultiElements.SingleOrDefault(x => x.Name.Trim() == dxElementName);

            if (dxElement == null)
                return;

            var parentId = dxModel.DXMainElement.Item.ID;

            var dxElementDefinition = dxModelDefinition.MultiFragmentDefinitions.SingleOrDefault(x => x.Type == dxElementName);

            // Because empty announced list is usefull to delete for full mode than need to load base structure to process elements.
            // If there are at least one element in Announced or Deleted list structure will be existing.
            var columns = dxElementDefinition == null ? SQLQueryBuilder.BaseColumns : dxElementDefinition.GetColumns();

            var adapter = this.PopulateTableToDataSet(
                conn,
                dataSet,
                dxElementName,
                columns,
                dxFilter: this.GetWhereExpressionForDXUnitID(parentId));

            var table = dataSet.Tables[dxElementName];

            if (table.PrimaryKey == null || table.PrimaryKey.Length == 0)
            {
                if (table.Columns.Contains("ID"))
                    table.PrimaryKey = new[] { table.Columns["ID"] };
            }

            foreach (var item in dxElement.Announced)
            {
                var id = item.ID;
                DataRow row = id != Guid.Empty ? table.Rows.Find(id) : null;

                if (row == null)
                {
                    row = table.NewRow();
                    MapdxItemToRow(item, row, dxUnitType);
                    table.Rows.Add(row);
                }
                else
                {
                    MapdxItemToRow(item, row, dxUnitType);
                }
            }

            // Full
            if (dxElement.Mode == MultiElementsMode.Full)
            {
                var announcedIds = new HashSet<Guid>(
                    dxElement.Announced.Select(a => a.ID));

                var toDelete = new List<DataRow>();
                foreach (DataRow r in table.Rows)
                {
                    var rid = r.Table.Columns.Contains("ID") ? ConvertHelper.ParseGuid(r["ID"]) : (Guid?)null;
                    if (!rid.HasValue || !announcedIds.Contains(rid.Value))
                        toDelete.Add(r);
                }
                foreach (var r in toDelete) r.Delete();
            }
            else if (dxElement.Mode == MultiElementsMode.Target)
            {
                ProcessAnnouncedItems(dxElement, table, dataSet.DataSetName);
                ProcessDeletedItems(dxElement, table);
            }

            this.SaveTable(adapter, conn, dataSet, table, true);
        }

        private void DeleteDXUnitFromDataSet(string dxUnitName, Guid id, DataSet dataSet, DbConnection conn)
        {
            var dxModelAdapter = this.PopulateTableToDataSet(conn, dataSet, dxUnitName,
                SQLQueryBuilder.BaseColumns,
                dxFilter: this.GetWhereExpressionForID(id));

            var dxModelBuilder = this._queryHelper.GetDbCommandBuilder(dxModelAdapter);

            dxModelBuilder.GetDeleteCommand();

            DataTable dataTable = dataSet.Tables[dxUnitName];

            foreach (DataRow row in dataTable.Rows)
            {
                row.Delete();
            }

            dxModelAdapter.Update(dataSet, dxUnitName);
        }

        private void DeleteDXElementsFromDataSet(string dxElementName, Guid objectID, DataSet dataSet, DbConnection conn)
        {
            var dxModelAdapter = this.PopulateTableToDataSet(
                conn,
                dataSet,
                dxElementName,
                SQLQueryBuilder.BaseColumns,
                dxFilter: this.GetWhereExpressionForDXUnitID(objectID));

            var dxModelBuilder = this._queryHelper.GetDbCommandBuilder(dxModelAdapter);

            dxModelBuilder.GetDeleteCommand();

            DataTable dataTable = dataSet.Tables[dxElementName];

            foreach (DataRow row in dataTable.Rows)
            {
                row.Delete();
            }

            dxModelAdapter.Update(dataSet, dxElementName);
        }

        private Guid
        InsertOrUpdatedxSingleItemToDataSet(
            DXSingleElement dxElement,
            string dxUnitType,
            Guid objectID,
            DataSet dataSet,
            DbConnection conn,
            ProcessingType processingType)
        {
            ArgumentNullException.ThrowIfNull(dxElement);

            var dxElementName = dxElement.Attribute.Type;

            var dxElementDefinition = dxElement.ToDXElementDefinition(dxUnitType);

            var dxModelAdapter = this.PopulateTableToDataSet(conn, dataSet, dxElementName,
                dxElementDefinition.GetColumns(),
                dxFilter: this.GetWhereExpressionForID(dxElement.Item.ID));

            var dxModelBuilder = this._queryHelper.GetDbCommandBuilder(dxModelAdapter);

            switch (processingType)
            {
                case ProcessingType.Insert:
                    dxModelBuilder.GetInsertCommand();
                    break;
                case ProcessingType.Update:
                    dxModelBuilder.GetUpdateCommand();
                    break;
            }

            DataTable dataTable = dataSet.Tables[dxElementName];

            if (dataTable.Rows.Count == 0)
            {
                var row = dataTable.NewRow();
                MapdxItemToRow(dxElement.Item, row, dxUnitType);
                dataTable.Rows.Add(row);
            }
            else
            {
                var row = dataTable.Rows[0];
                MapdxItemToRow(dxElement.Item, row, dxUnitType);
            }

            dxModelAdapter.Update(dataSet, dxElementName);

            return dxElement.Item.ID;
        }

        private void ProcessAnnouncedItems(DXMultiElement dxMultiItem, DataTable dataTable, string dxModelType)
        {
            foreach (var announcedItem in dxMultiItem.Announced)
            {
                var row = dataTable.Rows.Cast<DataRow>().SingleOrDefault(x => Guid.Parse(x[Constants.ID].ToString()) == announcedItem.ID);

                if (row == null)
                {
                    row = dataTable.NewRow();
                    MapdxItemToRow(announcedItem, row, dxModelType);
                    dataTable.Rows.Add(row);
                }
                else
                {
                    MapdxItemToRow(announcedItem, row, dxModelType);
                }
            }
        }

        private void ProcessDeletedItems(DXMultiElement dxMultiItem, DataTable dataTable)
        {
            var rowIDsToDelete = dxMultiItem.Deleted.Select(x => x.ID).ToList();

            var rowsToDelete = dataTable.Rows.Cast<DataRow>().Where(x => rowIDsToDelete.Contains(Guid.Parse(x[Constants.ID].ToString()))).ToList();

            foreach (var rowToDelete in rowsToDelete)
            {
                rowToDelete.Delete();
            }
        }

        private void MapdxItemToRow(DXItem dxItem, DataRow row, string dxModelType)
        {
            row[Constants.ID] = dxItem.ID;

            if (row.Table.Columns.Contains(Constants.DXUnitID))
            {
                row[Constants.DXUnitID] = dxItem.DXUnitID;
            }

            if (row.Table.Columns.Contains($"{dxModelType}ID"))
            {
                row[$"{dxModelType}ID"] = dxItem.DXUnitID;
            }

            if (row.Table.Columns.Contains(Constants.TimeStamp))
            {
                row[Constants.TimeStamp] = DateTime.UtcNow;
            }

            if (dxItem.Content == null)
                return;

            foreach (var column in row.Table.Columns.OfType<DataColumn>())
            {
                if (column.ColumnName == Constants.ID
                    || column.ColumnName == Constants.DXUnitID
                    || column.ColumnName == Constants.TimeStamp
                    || column.ColumnName == Constants.SystemPropertyTypeName
                    || column.ColumnName == $"{dxModelType}ID")
                {
                    continue;
                }

                var jProperty = dxItem.GetValue(column.ColumnName);

                if (!column.ReadOnly)
                {
                    if (jProperty != null)
                    {
                        if (this.IsNullOrEmpty(jProperty))
                        {
                            if (column.AllowDBNull)
                            {
                                this.SetNullValueToRowCell(row, column);
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

        private bool IsNullOrEmpty(object obj)
        {
            return obj == null || string.IsNullOrWhiteSpace(obj.ToString());
        }

        private void SetNullValueToRowCell(DataRow dataRow, DataColumn dataColumn)
        {
            dataRow[dataColumn] = DBNull.Value;
        }

        private void SetJPropertyValueToRowCell(DataRow dataRow, DataColumn dataColumn, object obj)
        {
            if (dataColumn.DataType == typeof(Guid))
            {
                dataRow[dataColumn] = obj;
            }
            else
                if (dataColumn.DataType == typeof(decimal))
                {
                    dataRow[dataColumn] = ConvertHelper.ParseDecimal(obj);
                }
                else
                    if (dataColumn.DataType == typeof(double))
                    {
                        dataRow[dataColumn] = ConvertHelper.ParseDouble(obj);
                    }
                    else
                        if (dataColumn.DataType == typeof(sbyte))
                        {
                            dataRow[dataColumn] = ConvertHelper.ParseSByte(obj);
                        }
                        else
                            if (dataColumn.DataType == typeof(int))
                            {
                                dataRow[dataColumn] = ConvertHelper.ParseInt(obj);
                            }
                            else
                                if (dataColumn.DataType == typeof(DateTime))
                                {
                                    var dt = ConvertHelper.ParseDateTime(obj);
                                    dataRow[dataColumn] = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                                }
                                else
                                    if (dataColumn.DataType == typeof(bool))
                                    {
                                        dataRow[dataColumn] = ConvertHelper.ParseBool(obj);
                                    }
                                    else
                                        if (dataColumn.DataType == typeof(byte[]))
                                        {
                                            dataRow[dataColumn] = ConvertToBytes(obj);
                                        }
                                        else if (dataColumn.DataType == typeof(TimeSpan))
                                        {
                                            dataRow[dataColumn] = ConvertHelper.ParseTimeSpan(obj);
                                        }
                                        else
                                        //if (dataColumn.DataType == typeof(string))
                                        {
                                            dataRow[dataColumn] = ConvertHelper.ParseString(obj);
                                        }
        }

        private static byte[] ConvertToBytes(object obj)
        {
            if (obj is null) return Array.Empty<byte>();

            if (obj is byte[] b) return b;

            // Newtonsoft
            if (obj is Newtonsoft.Json.Linq.JValue jv) obj = jv.Value!;
            if (obj is Newtonsoft.Json.Linq.JToken jt && jt.Type == Newtonsoft.Json.Linq.JTokenType.String) obj = jt.ToString();

            if (obj is string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return Array.Empty<byte>();

                var comma = s.IndexOf(',');
                if (s.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma >= 0)
                    s = s[(comma + 1)..];

                return Convert.FromBase64String(s);
            }

            throw new InvalidOperationException($"Unsupported blob value type: {obj.GetType().FullName}");
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
                                dataRow[dataColumn] = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
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

        // TODO: can be refactored using stored procedure
        private DXRelationDefinitionUnit GetRelationInfo(string obj1Name, string relationToObj2Name)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var dataSet = new DataSet("DXRelationDefinitionUnit");

                this.PopulateTableToDataSet(conn, dataSet, "DXRelationDefinitionUnit",
                    SQLQueryBuilder.AllColumns,
                    dxFilter: this.GetWhereExpressionWithAnd(
                        new Dictionary<string, object>()
                        {
                            { "ObjectNameLeft", obj1Name },
                            { "RelationNameRight", relationToObj2Name }
                        })
                     , fillSchema: false);

                var table = dataSet.Tables["DXRelationDefinitionUnit"];

                if (table.Rows.Count == 0)
                {
                    throw new Exception($"Relation pair {obj1Name}-{relationToObj2Name} is not existing in system");
                }

                var row = table.Rows[0];

                return new DXRelationDefinitionUnit()
                {
                    RelationType = (DXRelationTypeEnum)ConvertHelper.ParseInt(row["RelationType"]),
                    RelationTable = ConvertHelper.ParseString(row["RelationTable"]),
                    ID = ConvertHelper.ParseGuid(row[Constants.ID]),
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

        private bool AddRelationManyToMany(DXRelationDefinitionUnit relationInfo, Guid obj1Id, Guid obj2Id)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var dataSet = new DataSet(relationInfo.RelationTable);

                var adapter = this.PopulateTableToDataSet(
                    conn,
                    dataSet,
                    relationInfo.RelationTable,
                    SQLQueryBuilder.AllColumns,
                    dxFilter: this.GetWhereExpressionWithAnd(
                        new Dictionary<string, object>()
                        {
                            { relationInfo.RelationNameLeft, obj1Id },
                            { relationInfo.RelationNameRight, obj2Id }
                        })
                    , fillSchema: false);

                var table = dataSet.Tables[relationInfo.RelationTable];
                DataRow dataRow;

                if (table.Rows.Count == 0)
                {
                    var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
                    modelBuilder.GetInsertCommand();

                    dataRow = table.NewRow();

                    dataRow[relationInfo.RelationNameLeft] = obj1Id;
                    dataRow[relationInfo.RelationNameRight] = obj2Id;

                    table.Rows.Add(dataRow);
                }
                else
                {
                    var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
                    modelBuilder.GetUpdateCommand();

                    dataRow = table.Rows[0];

                    dataRow[relationInfo.RelationNameLeft] = obj1Id;
                    dataRow[relationInfo.RelationNameRight] = obj2Id;
                }

                SaveTable(adapter, conn, dataSet, table, true);
                dataSet.AcceptChanges();

                return true;
            });
        }

        private bool AddRelationManyToOne(DXRelationDefinitionUnit relationInfo, Guid obj1Id, Guid obj2Id)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var tableName = relationInfo.ObjectNameLeft;

                var dataSet = new DataSet(tableName);

                var adapter = this.PopulateTableToDataSet(conn, dataSet, tableName,
                    SQLQueryBuilder.AllColumns,
                    dxFilter: this.GetWhereExpressionForID(obj1Id));

                var table = dataSet.Tables[tableName];
                var rows = table.Rows;

                if (rows.Count == 1)
                {
                    var row = rows[0];

                    row[relationInfo.RelationNameRight] = obj2Id;

                    var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
                    modelBuilder.GetUpdateCommand();

                    SaveTable(adapter, conn, dataSet, table, true);
                    dataSet.AcceptChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        private bool AddRelationOneToMany(DXRelationDefinitionUnit relationInfo, Guid obj1Id, Guid obj2Id)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var tableName = relationInfo.ObjectNameRight;

                var dataSet = new DataSet(tableName);

                var adapter = this.PopulateTableToDataSet(conn, dataSet, tableName,
                    SQLQueryBuilder.AllColumns,
                    dxFilter: this.GetWhereExpressionForID(obj2Id));

                var table = dataSet.Tables[tableName];
                var rows = table.Rows;

                if (rows.Count == 1)
                {
                    var row = rows[0];

                    row[relationInfo.RelationNameLeft] = obj1Id;

                    var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
                    modelBuilder.GetUpdateCommand();

                    SaveTable(adapter, conn, dataSet, table, true);
                    dataSet.AcceptChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        private bool AddRelationZeroOneToZeroOne(DXRelationDefinitionUnit relationInfo, Guid obj1Id, Guid obj2Id)
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

        private bool RemoveRelationManyToMany(DXRelationDefinitionUnit relationInfo, Guid obj1Id, Guid obj2Id)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var dataSet = new DataSet(relationInfo.RelationTable);

                var adapter = this.PopulateTableToDataSet(conn, dataSet, relationInfo.RelationTable,
                    SQLQueryBuilder.AllColumns,
                    dxFilter:
                    this.GetWhereExpressionWithAnd(
                        new Dictionary<string, object>()
                        {
                            { relationInfo.RelationNameLeft, obj1Id },
                            { relationInfo.RelationNameRight, obj2Id }
                        })
                    , fillSchema: false);

                var table = dataSet.Tables[relationInfo.RelationTable];
                var rows = table.Rows;

                if (rows.Count > 0)
                {
                    var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
                    modelBuilder.GetDeleteCommand();

                    for (int i = 0; i < rows.Count; i++)
                    {
                        var row = rows[i];
                        row.Delete();
                    }

                    SaveTable(adapter, conn, dataSet, table, true);
                    dataSet.AcceptChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        private bool RemoveRelationManyToZeroOne(DXRelationDefinitionUnit relationInfo, Guid obj1Id, Guid obj2Id)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var tableName = relationInfo.ObjectNameLeft;

                var dataSet = new DataSet(tableName);

                var adapter = this.PopulateTableToDataSet(conn, dataSet, tableName,
                    SQLQueryBuilder.AllColumns,
                    dxFilter:
                     this.GetWhereExpressionWithAnd(
                        new Dictionary<string, object>()
                        {
                            { "ID", obj1Id },
                            { relationInfo.RelationNameRight, obj2Id }
                        })
                     );

                var table = dataSet.Tables[tableName];
                var rows = table.Rows;

                if (rows.Count == 1)
                {
                    var row = rows[0];

                    row[relationInfo.RelationNameRight] = DBNull.Value;

                    var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
                    modelBuilder.GetUpdateCommand();

                    SaveTable(adapter, conn, dataSet, table, true);
                    dataSet.AcceptChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        private bool RemoveRelationZeroOneToZeroOne(DXRelationDefinitionUnit relationInfo, Guid obj1Id, Guid obj2Id)
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

        private bool RemoveRelationZeroOneToMany(DXRelationDefinitionUnit relationInfo, Guid obj1Id, Guid obj2Id)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var tableName = relationInfo.ObjectNameRight;

                var dataSet = new DataSet(tableName);

                var adapter = this.PopulateTableToDataSet(conn, dataSet, tableName,
                    SQLQueryBuilder.AllColumns,
                    dxFilter: this.GetWhereExpressionWithAnd(
                        new Dictionary<string, object>()
                        {
                            { "ID", obj2Id },
                            { relationInfo.RelationNameLeft, obj1Id}
                        })
                    );

                var table = dataSet.Tables[tableName];
                var rows = table.Rows;

                if (rows.Count == 1)
                {
                    var row = rows[0];

                    row[relationInfo.RelationNameLeft] = DBNull.Value;

                    var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
                    modelBuilder.GetUpdateCommand();

                    SaveTable(adapter, conn, dataSet, table, true);
                    dataSet.AcceptChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        private IEnumerable<Guid> GetRelationsManyToMany(DXRelationDefinitionUnit relationInfo, Guid obj1Id)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var dataSet = new DataSet(relationInfo.RelationTable);

                this.PopulateTableToDataSet(conn, dataSet, relationInfo.RelationTable,
                    SQLQueryBuilder.AllColumns,
                    dxFilter: this.GetWhereExpressionWithAnd(
                        new Dictionary<string, object>()
                        {
                            { relationInfo.RelationNameLeft, obj1Id }
                        })
                    , fillSchema: false);

                var table = dataSet.Tables[relationInfo.RelationTable];
                var rows = table.Rows;

                return rows.Cast<DataRow>().Select(x =>
                {
                    var relatedId = ConvertHelper.ParseGuid(x[relationInfo.RelationNameRight]);

                    return relatedId;
                });
            });
        }

        private IEnumerable<Guid> GetRelationsManyToOne(DXRelationDefinitionUnit relationInfo, Guid obj1Id)
        {
            IEnumerable<Guid> result = this.RunRequestInTransaction((conn) =>
            {
                var tableName = relationInfo.ObjectNameLeft;

                var dataSet = new DataSet(tableName);

                this.PopulateTableToDataSet(conn, dataSet, tableName,
                    SQLQueryBuilder.AllColumns,
                    dxFilter: this.GetWhereExpressionForID(obj1Id), fillSchema: false);

                var table = dataSet.Tables[tableName];
                var rows = table.Rows;

                return rows.Cast<DataRow>()
                    .Where(x => x[relationInfo.RelationNameRight] != DBNull.Value)
                    .Select(x => ConvertHelper.ParseGuid(x[relationInfo.RelationNameRight]));
            });

            return result;
        }

        private IEnumerable<Guid> GetRelationsOneToMany(DXRelationDefinitionUnit relationInfo, Guid obj1Id)
        {
            return this.RunRequestInTransaction((conn) =>
            {
                var tableName = relationInfo.ObjectNameRight;

                var dataSet = new DataSet(tableName);

                this.PopulateTableToDataSet(conn, dataSet, tableName,
                    SQLQueryBuilder.AllColumns,
                    dxFilter: this.GetWhereExpressionWithAnd(
                        new Dictionary<string, object>()
                        {
                            { relationInfo.RelationNameLeft, obj1Id }
                        })
                    , fillSchema: false);

                var table = dataSet.Tables[tableName];

                var rows = table.Rows;

                return rows.Cast<DataRow>().Select(x => ConvertHelper.ParseGuid(x[Constants.ID]));
            });
        }

        private IEnumerable<Guid> GetRelationsZeroOneToZeroOne(DXRelationDefinitionUnit relationInfo, Guid obj1Id)
        {
            bool isRightTableContainsRelationID = relationInfo.RelationTable.Equals(relationInfo.ObjectNameRight);

            return isRightTableContainsRelationID ? this.GetRelationsOneToMany(relationInfo, obj1Id) : this.GetRelationsManyToOne(relationInfo, obj1Id);
        }

        private DbDataAdapter PopulateTableToDataSet(
            DbConnection conn,
            DataSet dataSet,
            string tableName,
            IDictionary<string, string> columns,
            string dxFilter = null,
            bool fillSchema = true)
        {
            var query = _sqlQueryBuilder.BuildSQLExpression(tableName, columns, dxFilter);

            var adapter = _queryHelper.GetDbDataAdapter(conn, query);

            if (fillSchema)
            {
                adapter.MissingSchemaAction = MissingSchemaAction.Add;
                adapter.MissingMappingAction = MissingMappingAction.Passthrough;

                adapter.FillSchema(dataSet, SchemaType.Mapped, tableName);

                var table = dataSet.Tables[tableName];

                foreach (DataColumn col in table.Columns)
                {
                    if (col.DataType == typeof(DateTime))
                        col.DateTimeMode = DataSetDateTime.Utc;
                }
            }

            var oldConstraints = dataSet.EnforceConstraints;
            dataSet.EnforceConstraints = false;

            adapter.Fill(dataSet, tableName);

            dataSet.EnforceConstraints = oldConstraints;
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

        private T RunRequest<T>(Func<DbConnection, T> func)
        {
            using (DbConnection conn = this._queryHelper.GetDBConnection(this._connectionStr))
            {
                conn.Open();

                return func.Invoke(conn);
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

        private string GetWhereExpressionForID(Guid id)
        {
            return $"ID = '{id}'";
        }

        private string GetWhereExpressionForDXUnitID(Guid id)
        {
            return $"DXUnitID = '{id}'";
        }

        private string GetWhereExpressionForID(IEnumerable<Guid> ids)
        {
            string idsString = String.Join(",", ids.Select(x => $"'{x}'"));

            return $"ID IN ({idsString})";
        }

        private string GetWhereExpressionForDXUnitID(IEnumerable<Guid> ids)
        {
            string idsString = String.Join(",", ids.Select(x => $"'{x}'"));

            return $"DXUnitID IN ({idsString})";
        }

        private string GetWhereExpressionWithAnd(IDictionary<string, object> values)
        {
            if (values == null)
                return null;

            return string.Join(" AND ", values.Select(x => $"{x.Key} = '{x.Value}'"));
        }
    }
}
