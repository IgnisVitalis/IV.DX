using IV.DX.Contracts.Persistence.ExpressionTree;
using IV.DX.Kernel;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Abstractions;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.Common;

namespace IV.DX.Persistence
{
    //internal partial class DXCoreRepository : IDXCoreRepository, IDXStructureRepository, IDXEnumCoreRepository, IDXStructureRawReader
    //{
    //    protected string _connectionStr;
    //    protected ISQLQueryDXHelper _queryHelper;
    //    IDXStructureCache _dxStructureCache;

    //    public DXCoreRepository(
    //        DXDatabaseOptions options,
    //        IDXStructureCache dxStructureCache,
    //        ISQLQueryDXHelper queryHelper)
    //    {
    //        _connectionStr = options.ConnectionString;
    //        _queryHelper = queryHelper;
    //        _dxStructureCache = dxStructureCache;
    //    }

    //    public bool Delete(string typeName, Guid id)
    //    {
    //        ArgumentNullException.ThrowIfNullOrEmpty(typeName);

    //        if (id == Guid.Empty)
    //            throw new ArgumentException("Id must be a non-empty GUID.", nameof(id));

    //        var mainDXUnitInfo = this.GetDXUnitDefinition(typeName);

    //        var dxUnitHierarchy = this.GetHierarchyChainOfBaseEntitiesFromDerivedToBase(mainDXUnitInfo);

    //        return this.RunRequestInTransaction((conn) =>
    //        {
    //            DataSet dataSet = new DataSet(typeName);

    //            foreach (var dxUnitInfo in dxUnitHierarchy)
    //            {
    //                var relatedDXElements = this.GetRelatedDXElementDefinitions(dxUnitInfo);

    //                if (relatedDXElements != null)
    //                {
    //                    // Delete related dxElements
    //                    foreach (var relatedDXElement in relatedDXElements)
    //                    {
    //                        this.DeleteDXElementsFromDataSet(relatedDXElement.DXObjectDefinitionMainElement.Name, id, dataSet, conn);
    //                    }
    //                }

    //                // Delete dxUnit
    //                this.DeleteDXUnitFromDataSet(dxUnitInfo.DXObjectDefinitionMainElement.Name, id, dataSet, conn);
    //            }

    //            dataSet.AcceptChanges();

    //            return true;
    //        });
    //    }

    //    public DXModel GetItem(DXModelDefinition container, Guid id, DXLoadingType typeOfLoading)
    //    {
    //        if (container == null)
    //            return null;

    //        DXModel result = this.GetDXModel(container, id);

    //        this.RunRequest((conn) =>
    //        {
    //            var dataSet = this.PopulateDataSetForTargetDXUnit(container, id, conn);
    //            var dataTable = dataSet.Tables[result.OwnSingleItem.ObjectInfo.ObjectName];

    //            if (dataTable.Rows.Count == 0)
    //            {
    //                result = null;
    //            }
    //            else
    //            {
    //                var dataRow = dataTable.Rows[0];

    //                result = this.ConvertToDXModel(dataSet, dataRow, container);
    //            }
    //        });

    //        return result;
    //    }

    //    private DXModel GetDXModel(DXModelDefinition container, Guid id)
    //    {
    //        DXModel result = new DXModel(new DXMainItem(new DXUnitAttribute(container.OwnSingleItem.Name))
    //        {
    //            Item = new DXItem()
    //            {
    //                ID = id
    //            }
    //        });

    //        return result;
    //    }

    //    public IEnumerable<DXModel> GetItems(DXModelDefinition container, IEnumerable<Guid> objIds, DXLoadingType typeOfLoading)
    //    {
    //        if (container == null)
    //            return null;

    //        if (objIds == null)
    //            return null;

    //        if (objIds.Count() == 0)
    //            return new List<DXModel>();

    //        IEnumerable<DXModel> resultItems = null;

    //        this.RunRequest((conn) =>
    //        {
    //            resultItems = this.GetItems(conn, container, objIds, typeOfLoading);
    //        });

    //        return resultItems;
    //    }

    //    public IEnumerable<DXModel> GetItems(DbConnection conn, DXModelDefinition container, IEnumerable<Guid> objIds, DXLoadingType typeOfLoading)
    //    {
    //        if (container == null)
    //            return null;

    //        if (objIds == null)
    //            return null;

    //        if (objIds.Count() == 0)
    //            return new List<DXModel>();

    //        IEnumerable<DXModel> resultItems = null;

    //        var dataSet = this.PopulateDataSetForTargetDXUnits(container, objIds, conn);
    //        var dataTable = dataSet.Tables[container.OwnSingleItem.Type];

    //        var items = dataTable.Rows;

    //        resultItems = items.Cast<DataRow>().Select(x => this.ConvertToDXModel(dataSet, x, container)).ToList();

    //        dataSet.AcceptChanges();

    //        return resultItems;
    //    }

    //    private DXModel ConvertToDXModel(DataSet dataSet, DataRow x, DXModelDefinition container)
    //    {
    //        DXModel resultItem = this.GetDXModel(container, ConvertHelper.ParseGuid(x[Constants.ID]));

    //        // Process DX model
    //        this.PopulateDXMainItem(resultItem.OwnSingleItem, container.OwnSingleItem, x);

    //        // Process DX single items
    //        resultItem.SingleItems = container.SingleFragmentDefinitions.Select(item =>
    //        {
    //            DXSingleElement singleItem = this.ConvertTodxSingleItem(item);

    //            var dataTable = dataSet.Tables[singleItem.Name];

    //            this.PopulatedxSingleItem(singleItem, item, dataTable.Rows.Cast<DataRow>()
    //                .SingleOrDefault(y => ConvertHelper.ParseGuid(y[Constants.ObjectID]) == resultItem.OwnSingleItem.Item.ID));

    //            return singleItem;
    //        }).ToList();

    //        // Process DX multi items          
    //        resultItem.MultiItems = container.MultiFragmentDefinitions.Select(item =>
    //        {
    //            DXMultiElement multiItem = this.ConvertToDXMultiItem(item);

    //            var dataTable = dataSet.Tables[multiItem.Name];

    //            var rows =
    //                dataTable.Rows.Cast<DataRow>()
    //                .Where(y => ConvertHelper.ParseGuid(y[Constants.ObjectID]) == resultItem.OwnSingleItem.Item.ID).ToList();

    //            this.PopulateDXMultiItem(multiItem, item, rows);

    //            return multiItem;
    //        }).ToList();

    //        return resultItem;
    //    }

    //    private DXSingleElement ConvertTodxSingleItem(DXElementDefinition item)
    //    {
    //        return new DXSingleElement()
    //        {
    //            Name = item.Name,
    //            ElementInfo = new DXElementAttribute(item.Name)
    //        };
    //    }

    //    private DXMultiElement ConvertToDXMultiItem(DXElementDefinition item)
    //    {
    //        return new DXMultiElement()
    //        {
    //            Name = item.Name,
    //            DXElementInfo = new DXElementAttribute(item.Name)
    //        };
    //    }

    //    public IEnumerable<DXModel> GetItems(string typeName)
    //    {
    //        var modelDefinition = this.GetModelDefinition(typeName);

    //        if (modelDefinition == null)
    //            return null;

    //        return this.GetItems(modelDefinition, DXLoadingType.Full);
    //    }

    //    public IEnumerable<DXModel> GetItems(string typeName, IEnumerable<Guid> ids)
    //    {
    //        var modelDefinition = this.GetModelDefinition(typeName);

    //        if (modelDefinition == null)
    //            return null;

    //        return this.GetItems(modelDefinition, ids, DXLoadingType.Full);
    //    }

    //    public IEnumerable<DXModel> GetItems(string typeName, string dxsqlWhereExpression)
    //    {
    //        var modelDefinition = this.GetModelDefinition(typeName);

    //        if (modelDefinition == null)
    //            return null;

    //        return this.GetItems(modelDefinition, dxsqlWhereExpression, DXLoadingType.Full);
    //    }

    //    // TODO: need to check what kind of data is loaded. Because this method should load only IDs
    //    public IEnumerable<Guid> GetItemIDs(string typeName, string? dxsqlWhereExpression = default)
    //    {
    //        string sqlQuery = this._queryHelper.GetQuery(typeName, dxsqlWhereExpression, this._dxStructureCache.DXRelations);

    //        return this.RunRequestInTransaction((conn) =>
    //        {
    //            var dataSet = new DataSet(typeName);

    //            var adapter = this._queryHelper.GetDbDataAdapter(conn, sqlQuery);

    //            adapter.Fill(dataSet, typeName);

    //            var table = dataSet.Tables[typeName];

    //            var ids = table.Rows.Cast<DataRow>().Select(x => ConvertHelper.ParseGuid(x[Constants.ID])).ToList();

    //            return ids;
    //        });
    //    }

    //    public DXModel GetItem(string typeName, Guid id)
    //    {
    //        var modelDefinition = this.GetModelDefinition(typeName);

    //        if (modelDefinition == null)
    //            return null;

    //        return this.GetItem(modelDefinition, id, DXLoadingType.Full);
    //    }

    //    private DXModelDefinition GetModelDefinition(string type)
    //    {
    //        var mainDXUnit = this.GetDXUnitDefinition(type);

    //        if (mainDXUnit == null)
    //            return null;

    //        var entities = this.GetHierarchyChainOfBaseEntitiesFromBaseToDerived(mainDXUnit);

    //        List<DXElementDefinitionUnit> singleMandatoryDXElements = new List<DXElementDefinitionUnit>();
    //        List<DXElementDefinitionUnit> singleOptionalDXElements = new List<DXElementDefinitionUnit>();
    //        List<DXElementDefinitionUnit> multiMandatoryDXElements = new List<DXElementDefinitionUnit>();
    //        List<DXElementDefinitionUnit> multiOptionalDXElements = new List<DXElementDefinitionUnit>();

    //        foreach (var dxUnit in entities)
    //        {
    //            var singleMandatoryDXElementsTemp = this.GetRelatedDXElementDefinitions(dxUnit, DXElementInUnitTypeEnum.SingleMandatory);

    //            if (singleMandatoryDXElementsTemp != null)
    //            {
    //                singleMandatoryDXElements.AddRange(singleMandatoryDXElementsTemp);
    //            }

    //            var singleOptionalDXElementsTemp = this.GetRelatedDXElementDefinitions(dxUnit, DXElementInUnitTypeEnum.SingleOptional);

    //            if (singleOptionalDXElementsTemp != null)
    //            {
    //                singleOptionalDXElements.AddRange(singleOptionalDXElementsTemp);
    //            }

    //            var multiMandatoryDXElementsTemp = this.GetRelatedDXElementDefinitions(dxUnit, DXElementInUnitTypeEnum.MultiMandatory);

    //            if (multiMandatoryDXElementsTemp != null)
    //            {
    //                multiMandatoryDXElements.AddRange(multiMandatoryDXElementsTemp);
    //            }

    //            var multiOptionalDXElementsTemp = this.GetRelatedDXElementDefinitions(dxUnit, DXElementInUnitTypeEnum.MultiOptional);

    //            if (multiOptionalDXElementsTemp != null)
    //            {
    //                multiOptionalDXElements.AddRange(multiOptionalDXElementsTemp);
    //            }
    //        }

    //        var modelDefinition = DXModelDefinition.BuildModelDefinition(
    //            mainDXUnit,
    //            singleMandatoryDXElements,
    //            singleOptionalDXElements,
    //            multiMandatoryDXElements,
    //            multiOptionalDXElements);

    //        return modelDefinition;
    //    }

    //    public IEnumerable<DXModel> GetItems(DXModelDefinition container, DXLoadingType typeOfLoading)
    //    {
    //        return GetItems(container, string.Empty, typeOfLoading);
    //    }

    //    public IEnumerable<DXModel> GetItems(DXModelDefinition container, string dxsqlWhereExpression, DXLoadingType typeOfLoading)
    //    {
    //        string typeName = container.OwnSingleItem.Type;
    //        string sqlQuery = this._queryHelper.GetQuery(typeName, dxsqlWhereExpression, this._dxStructureCache.DXRelations);

    //        return this.RunRequestInTransaction((conn) =>
    //        {
    //            var dataSet = new DataSet(typeName);

    //            var adapter = this._queryHelper.GetDbDataAdapter(conn, sqlQuery);

    //            adapter.Fill(dataSet, typeName);

    //            var table = dataSet.Tables[typeName];

    //            var ids = table.Rows.Cast<DataRow>().Select(x => ConvertHelper.ParseGuid(x[Constants.ID]));

    //            return this.GetItems(conn, container, ids, typeOfLoading);
    //        });
    //    }

    //    private DataSet PopulateDataSetForTargetDXUnit(DXModelDefinition container, Guid id, DbConnection conn)
    //    {
    //        DataSet dataSet = new DataSet(container.OwnSingleItem.Type);

    //        this.PopulateTableToDataSet(conn, dataSet, container.OwnSingleItem.Type,
    //            whereClause: this._queryHelper.GetWhereExpressionForID(id), fillSchema: false);

    //        var whereClauseForObjectID = this._queryHelper.GetWhereExpressionForObjectID(id);

    //        foreach (var singleItem in container.SingleFragmentDefinitions)
    //        {
    //            this.PopulateTableToDataSet(conn, dataSet, singleItem.Type,
    //                columnNames: singleItem.Select(x => x.ColumnDefinition.DXExpression),
    //                whereClause: whereClauseForObjectID, fillSchema: false);
    //        }

    //        foreach (var multiItem in container.MultiFragmentDefinitions)
    //        {
    //            this.PopulateTableToDataSet(conn, dataSet, multiItem.Type,
    //                multiItem.Select(x => x.ColumnDefinition.DXExpression),
    //                whereClause: whereClauseForObjectID, fillSchema: false);
    //        }

    //        return dataSet;
    //    }

    //    private DataSet PopulateDataSetForTargetDXUnits(DXModelDefinition container, IEnumerable<Guid> ids, DbConnection conn)
    //    {
    //        DataSet dataSet = new DataSet(container.OwnSingleItem.Type);

    //        if (ids != null && ids.Count() > 0)
    //        {
    //            this.PopulateTableToDataSet(conn, dataSet, container.OwnSingleItem.Type, whereClause: this._queryHelper.GetWhereExpressionForID(ids), fillSchema: false);

    //            var whereClauseForObjectIDs = this._queryHelper.GetWhereExpressionForObjectID(ids);

    //            foreach (var singleItem in container.SingleFragmentDefinitions)
    //            {
    //                this.PopulateTableToDataSet(conn, dataSet, singleItem.Type, whereClause: whereClauseForObjectIDs, fillSchema: false);
    //            }

    //            foreach (var multiItem in container.MultiFragmentDefinitions)
    //            {
    //                this.PopulateTableToDataSet(conn, dataSet, multiItem.Type, whereClause: whereClauseForObjectIDs, fillSchema: false);
    //            }
    //        }

    //        return dataSet;
    //    }

    //    private void PopulateDXMainItem(
    //        DXMainItem ownItem,
    //        DXElementDefinition fragmentDefinition,
    //        DataRow dataRow)
    //    {
    //        if (dataRow == null || ownItem == null)
    //            return;

    //        ownItem.Item =
    //            this.GetdxItem(dataRow, fragmentDefinition);
    //    }

    //    private void PopulatedxSingleItem(
    //        DXSingleElement singleItem,
    //        DXElementDefinition fragmentDefinition,
    //        DataRow dataRow)
    //    {
    //        if (dataRow == null || singleItem == null)
    //            return;

    //        singleItem.Item =
    //            this.GetdxItem(dataRow, fragmentDefinition);
    //    }

    //    private void PopulateDXMultiItem(
    //        DXMultiElement multiItem,
    //        DXElementDefinition fragmentDefinition,
    //        IEnumerable<DataRow> rows)
    //    {
    //        multiItem.Announced = rows.OfType<DataRow>().Select(x => this.GetdxItem(x, fragmentDefinition)).ToList();
    //        multiItem.Mode = MultiElementsMode.Full;
    //    }

    //    private DXItem GetdxItem(DataRow row, DXElementDefinition structure)
    //    {
    //        JObject jObjectContainerCopy = new JObject();

    //        foreach (DataColumn column in row.Table.Columns)
    //        {
    //            var property = structure.SingleOrDefault(x => x.ColumnDefinition.Name == column.ColumnName);

    //            if (property == null)
    //                continue;

    //            if (row[column] != DBNull.Value)
    //            {
    //                jObjectContainerCopy[property.ColumnDefinition.Name] = GetValueFromRow(row, column);
    //            }
    //            else
    //            {
    //                jObjectContainerCopy[property.ColumnDefinition.Name] = null;
    //            }
    //        }

    //        var dxItem = new DXItem()
    //        {
    //            ID = ConvertHelper.ParseGuid(row[Constants.ID]),
    //            Content = jObjectContainerCopy
    //        };

    //        if (row.Table.Columns.Contains(Constants.ObjectID))
    //        {
    //            dxItem.ObjectID = ConvertHelper.ParseGuid(row[Constants.ObjectID]);
    //        }
    //        else
    //        {
    //            dxItem.ObjectID = dxItem.ID;
    //        }

    //        return dxItem;
    //    }

    //    private JValue GetValueFromRow(DataRow dataRow, DataColumn dataColumn)
    //    {
    //        JValue value = null;

    //        if (dataColumn.DataType == typeof(DateTime))
    //        {
    //            var dateTime = ConvertHelper.ParseDateTime(dataRow[dataColumn]);
    //            dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

    //            value = new JValue(dateTime);
    //        }
    //        else
    //        {
    //            value = new JValue(dataRow[dataColumn]);
    //        }

    //        return value;
    //    }

    //    public Guid Insert(DXModel dxModel)
    //    {
    //        return this.InsertOrUpdate(dxModel, ProcessingType.Insert);
    //    }

    //    public Guid Update(DXModel dxModel)
    //    {
    //        return this.InsertOrUpdate(dxModel, ProcessingType.Update);
    //    }

    //    public Guid InsertOrUpdate(DXModel dxModel)
    //    {
    //        var objId = dxModel.OwnSingleItem.Item.ID;
    //        var type = dxModel.OwnSingleItem.ObjectInfo.ObjectName;

    //        if (objId.HasValue
    //            && !string.IsNullOrEmpty(type)
    //            && this.IsItemExisting(type, objId.Value))
    //        {
    //            return this.Update(dxModel);
    //        }
    //        else
    //        {
    //            return this.Insert(dxModel);
    //        }
    //    }

    //    public bool IsItemExisting(string type, Guid objectId)
    //    {
    //        DXModelDefinition dd = new DXModelDefinition(new DXElementDefinition(type, type));

    //        var item = this.GetItem(dd, objectId, DXLoadingType.Base);

    //        return item != null;
    //    }

    //    private Guid InsertOrUpdate(DXModel dxModel, ProcessingType processingType)
    //    {
    //        ArgumentNullException.ThrowIfNull(dxModel);

    //        if (!dxModel.OwnSingleItem.Item.ID.HasValue)
    //        {
    //            dxModel.OwnSingleItem.Item.ID = Guid.NewGuid();
    //        }

    //        var typeName = dxModel.OwnSingleItem.ObjectInfo.ObjectName;

    //        var mainDXUnitInfo = this.GetDXUnitDefinition(typeName);

    //        if (mainDXUnitInfo != null)
    //        {
    //            var dxUnitHierarchy = this.GetHierarchyChainOfBaseEntitiesFromBaseToDerived(mainDXUnitInfo);

    //            this.ProcessDXModelAsDXDXUnit(typeName, dxModel, dxUnitHierarchy, processingType);
    //        }
    //        else
    //        {
    //            var enumInfo = this.GetDXEnumDefinition(typeName);

    //            if (enumInfo != null)
    //            {
    //                this.ProcessDXModelAsDXEnum(typeName, enumInfo, dxModel, processingType);
    //            }
    //            else
    //            {
    //                throw new Exception($"Type '{dxModel.OwnSingleItem.ObjectInfo.ObjectName}' is not registered.");
    //            }
    //        }

    //        return dxModel.OwnSingleItem.Item.ID.Value;
    //    }

    //    private void ProcessDXModelAsDXDXUnit(string typeName, DXModel dxModel, IEnumerable<DXUnitDefinitionUnit> dxUnitHierarchy, ProcessingType processingType)
    //    {
    //        this.RunRequestInTransaction((conn) =>
    //        {
    //            DataSet dataSet = new DataSet(typeName);

    //            foreach (var dxUnitInfo in dxUnitHierarchy)
    //            {
    //                this.InsertOrUpdateDXOwnItemToDataSet(dxModel, dxUnitInfo.DXObjectDefinitionMainElement.Name, dataSet, conn, processingType);

    //                var relatedDXElementsSM = this.GetRelatedDXElementDefinitions(dxUnitInfo, DXElementInUnitTypeEnum.SingleMandatory);
    //                var relatedDXElementsSO = this.GetRelatedDXElementDefinitions(dxUnitInfo, DXElementInUnitTypeEnum.SingleOptional);
    //                var relatedDXElementsMM = this.GetRelatedDXElementDefinitions(dxUnitInfo, DXElementInUnitTypeEnum.MultiMandatory);
    //                var relatedDXElementsMO = this.GetRelatedDXElementDefinitions(dxUnitInfo, DXElementInUnitTypeEnum.MultiOptional);

    //                var objectID = dxModel.OwnSingleItem.Item.ID.Value;
    //                var dxUnitType = dxUnitInfo.DXObjectDefinitionMainElement.Name;
    //                // Process DX single items
    //                if (relatedDXElementsSM != null)
    //                {
    //                    foreach (var singleItem in relatedDXElementsSM)
    //                    {
    //                        var dxElementName = singleItem.DXObjectDefinitionMainElement.Name.Trim();
    //                        var dxElement = dxModel.SingleItems.SingleOrDefault(x => x.Name.Trim() == dxElementName);

    //                        if (dxElement == null)
    //                            continue;

    //                        this.InsertOrUpdatedxSingleItemToDataSet(dxElement, dxUnitType, objectID, dataSet, conn, processingType);
    //                    }
    //                }

    //                if (relatedDXElementsSO != null)
    //                {
    //                    foreach (var singleItem in relatedDXElementsSO)
    //                    {
    //                        var dxElementName = singleItem.DXObjectDefinitionMainElement.Name.Trim();
    //                        var dxElement = dxModel.SingleItems.SingleOrDefault(x => x.Name.Trim() == dxElementName);

    //                        if (dxElement == null)
    //                            continue;

    //                        this.InsertOrUpdatedxSingleItemToDataSet(dxElement, dxUnitType, objectID, dataSet, conn, processingType);
    //                    }
    //                }

    //                // Process DX mutli items
    //                if (relatedDXElementsMM != null)
    //                {
    //                    foreach (var multiItem in relatedDXElementsMM)
    //                    {
    //                        this.InsertOrUpdateDXMultiItemToDataSet(dxModel, dxUnitInfo, multiItem, dataSet, conn, processingType);
    //                    }
    //                }

    //                if (relatedDXElementsMO != null)
    //                {
    //                    foreach (var multiItem in relatedDXElementsMO)
    //                    {
    //                        this.InsertOrUpdateDXMultiItemToDataSet(dxModel, dxUnitInfo, multiItem, dataSet, conn, processingType);
    //                    }
    //                }
    //            }

    //            dataSet.AcceptChanges();

    //            return true;
    //        });
    //    }

    //    private void ProcessDXModelAsDXEnum(string typeName, DXEnumDefinitionUnit dxUnit, DXModel dxModel, ProcessingType processingType)
    //    {
    //        this.RunRequestInTransaction((conn) =>
    //        {
    //            DataSet dataSet = new DataSet(typeName);

    //            this.InsertOrUpdateDXOwnItemToDataSet(dxModel, dxUnit.DXObjectDefinitionMainElement.Name, dataSet, conn, processingType);

    //            dataSet.AcceptChanges();

    //            return true;
    //        });
    //    }

    //    private IEnumerable<DXUnitDefinitionUnit> GetHierarchyChainOfBaseEntitiesFromDerivedToBase(DXUnitDefinitionUnit derivedDXUnit)
    //    {
    //        var result = new List<DXUnitDefinitionUnit>() { derivedDXUnit };

    //        if (derivedDXUnit.DXUnitInheritanceElement?.BaseDXUnit == null)
    //            return result;

    //        var derivedDXUnitInfo = derivedDXUnit;

    //        while (true)
    //        {
    //            var baseClass = this.GetBaseDXUnit(derivedDXUnitInfo);

    //            result.Add(baseClass);

    //            if (baseClass.DXUnitInheritanceElement != null)
    //            {
    //                derivedDXUnitInfo = baseClass;
    //            }
    //            else
    //            {
    //                break;
    //            }
    //        }

    //        return result.ToList();
    //    }

    //    private IEnumerable<DXUnitDefinitionUnit> GetHierarchyChainOfBaseEntitiesFromBaseToDerived(DXUnitDefinitionUnit derivedDXUnit)
    //    {
    //        return this.GetHierarchyChainOfBaseEntitiesFromDerivedToBase(derivedDXUnit).Reverse();
    //    }

    //    private void DeleteDXUnitFromDataSet(string dxUnitName, Guid id, DataSet dataSet, DbConnection conn)
    //    {
    //        var dxModelAdapter = this.PopulateTableToDataSet(conn, dataSet, dxUnitName,
    //            whereClause: this._queryHelper.GetWhereExpressionForID(id));

    //        var dxModelBuilder = this._queryHelper.GetDbCommandBuilder(dxModelAdapter);

    //        dxModelBuilder.GetDeleteCommand();

    //        DataTable dataTable = dataSet.Tables[dxUnitName];

    //        foreach (DataRow row in dataTable.Rows)
    //        {
    //            row.Delete();
    //        }

    //        dxModelAdapter.Update(dataSet, dxUnitName);
    //    }

    //    private void DeleteDXElementsFromDataSet(string dxElementName, Guid objectID, DataSet dataSet, DbConnection conn)
    //    {
    //        var dxModelAdapter = this.PopulateTableToDataSet(conn, dataSet, dxElementName, whereClause:
    //            this._queryHelper.GetWhereExpressionForObjectID(objectID));

    //        var dxModelBuilder = this._queryHelper.GetDbCommandBuilder(dxModelAdapter);

    //        dxModelBuilder.GetDeleteCommand();

    //        DataTable dataTable = dataSet.Tables[dxElementName];

    //        foreach (DataRow row in dataTable.Rows)
    //        {
    //            row.Delete();
    //        }

    //        dxModelAdapter.Update(dataSet, dxElementName);
    //    }

    //    private void InsertOrUpdateDXOwnItemToDataSet(DXModel dxModel, string dxUnitType, DataSet dataSet, DbConnection conn, ProcessingType processingType)
    //    {
    //        var objectID = dxModel.OwnSingleItem.Item.ID;

    //        var dxModelAdapter = this.PopulateTableToDataSet(conn, dataSet, dxUnitType,
    //            whereClause: this._queryHelper.GetWhereExpressionForID(objectID.Value));

    //        DataTable dataTable = dataSet.Tables[dxUnitType];

    //        var dxModelBuilder = this._queryHelper.GetDbCommandBuilder(dxModelAdapter);

    //        switch (processingType)
    //        {
    //            case ProcessingType.Insert:
    //                dxModelBuilder.GetInsertCommand();
    //                break;
    //            case ProcessingType.Update:
    //                dxModelBuilder.GetUpdateCommand();
    //                break;
    //            case ProcessingType.Delete:
    //            default:
    //                return;
    //        }

    //        if (dataTable.Rows.Count == 0)
    //        {
    //            var row = dataTable.NewRow();
    //            MapdxItemToRow(dxModel.OwnSingleItem.Item, row, dxUnitType);
    //            dataTable.Rows.Add(row);
    //        }
    //        else
    //        {
    //            var row = dataTable.Rows[0];
    //            MapdxItemToRow(dxModel.OwnSingleItem.Item, row, dxUnitType);
    //        }

    //        dxModelAdapter.Update(dataSet, dxUnitType);
    //    }

    //    //private DataTable GetDataTable(DataSet dataset, string tableName)
    //    //{
    //    //    DataTable dataTable = dataset.Tables[tableName];

    //    //    if (dataTable.Rows.Count > 0)
    //    //        return dataTable;

    //    //    foreach (DataColumn column in dataTable.Columns)
    //    //    {
    //    //        if (column.DataType == typeof(DateTime))
    //    //        {
    //    //            column.DateTimeMode = DataSetDateTime.Utc;
    //    //        }
    //    //    }

    //    //    return dataTable;
    //    //}

    //    private Guid InsertOrUpdatedxSingleItemToDataSet(
    //        DXSingleElement dxElement,
    //        string dxUnitType,
    //        Guid objectID,
    //        DataSet dataSet,
    //        DbConnection conn,
    //        ProcessingType processingType)
    //    {
    //        ArgumentNullException.ThrowIfNull(dxElement);

    //        if (!dxElement.Item.ID.HasValue)
    //        {
    //            dxElement.Item.ID = Guid.NewGuid();
    //        }

    //        var dxElementName = dxElement.ElementInfo.Name;

    //        var dxModelAdapter = this.PopulateTableToDataSet(conn, dataSet, dxElementName,
    //            whereClause: this._queryHelper.GetWhereExpressionForID(dxElement.Item.ID.Value));

    //        var dxModelBuilder = this._queryHelper.GetDbCommandBuilder(dxModelAdapter);

    //        switch (processingType)
    //        {
    //            case ProcessingType.Insert:
    //                dxModelBuilder.GetInsertCommand();
    //                break;
    //            case ProcessingType.Update:
    //                dxModelBuilder.GetUpdateCommand();
    //                break;
    //        }

    //        DataTable dataTable = dataSet.Tables[dxElementName];

    //        if (dataTable.Rows.Count == 0)
    //        {
    //            var row = dataTable.NewRow();
    //            MapdxItemToRow(dxElement.Item, row, dxUnitType);
    //            dataTable.Rows.Add(row);
    //        }
    //        else
    //        {
    //            var row = dataTable.Rows[0];
    //            MapdxItemToRow(dxElement.Item, row, dxUnitType);
    //        }

    //        dxModelAdapter.Update(dataSet, dxElementName);

    //        return dxElement.Item.ID.Value;
    //    }

    //    private void InsertOrUpdateDXMultiItemToDataSet(DXModel dxModel, DXUnitDefinitionUnit dxUnitInfo, DXElementDefinitionUnit dxElementInfo, DataSet dataSet, DbConnection conn, ProcessingType processingType)
    //    {
    //        var dxElementName = dxElementInfo.DXObjectDefinitionMainElement.Name.Trim();
    //        var objectID = dxModel.OwnSingleItem.Item.ID;
    //        var dxUnitType = dxUnitInfo.DXObjectDefinitionMainElement.Name;

    //        var dxElement = dxModel.MultiItems.SingleOrDefault(x => x.Name.Trim() == dxElementName);

    //        if (dxElement == null)
    //            return;

    //        var dxModelAdapter = this.PopulateTableToDataSet(conn, dataSet, dxElementName, whereClause:
    //            this._queryHelper.GetWhereExpressionForObjectID(objectID.Value));

    //        var dxModelBuilder = this._queryHelper.GetDbCommandBuilder(dxModelAdapter);

    //        if (processingType == ProcessingType.Insert)
    //        {
    //            dxModelBuilder.GetInsertCommand();
    //        }
    //        else if (processingType == ProcessingType.Update)
    //        {
    //            dxModelBuilder.GetUpdateCommand();
    //        }

    //        DataTable dataTable = dataSet.Tables[dxElementName];

    //        if (dxElement.Mode == MultiElementsMode.Full)
    //        {
    //            this.ProcessAnnouncedItems(dxElement, dataTable, dataSet.DataSetName);

    //            var rowsToDelete = dataTable.Rows.Cast<DataRow>()
    //                .Where(row =>
    //                {
    //                    var id = Guid.Parse(Convert.ToString(row[Constants.ID]));
    //                    return !dxElement.Announced.Any(x => x.ID == id);
    //                })
    //                .ToList();

    //            foreach (var row in rowsToDelete)
    //            {
    //                row.Delete();
    //            }

    //        }
    //        else if (dxElement.Mode == MultiElementsMode.Target)
    //        {
    //            this.ProcessAnnouncedItems(dxElement, dataTable, dataSet.DataSetName);
    //            this.ProcessDeletedItems(dxElement, dataTable);
    //        }

    //        dxModelAdapter.Update(dataSet, dxElement.DXElementInfo.Name);
    //    }

    //    private void ProcessAnnouncedItems(DXMultiElement dxMultiItem, DataTable dataTable, string dxModelType)
    //    {
    //        foreach (var announcedItem in dxMultiItem.Announced)
    //        {
    //            var row = dataTable.Rows.Cast<DataRow>().SingleOrDefault(x => Guid.Parse(x[Constants.ID].ToString()) == announcedItem.ID);

    //            if (row == null)
    //            {
    //                row = dataTable.NewRow();
    //                MapdxItemToRow(announcedItem, row, dxModelType);
    //                dataTable.Rows.Add(row);
    //            }
    //            else
    //            {
    //                MapdxItemToRow(announcedItem, row, dxModelType);
    //            }
    //        }
    //    }

    //    private void ProcessDeletedItems(DXMultiElement dxMultiItem, DataTable dataTable)
    //    {
    //        var rowIDsToDelete = dxMultiItem.Deleted.Select(x => x.ID).ToList();

    //        var rowsToDelete = dataTable.Rows.Cast<DataRow>().Where(x => rowIDsToDelete.Contains(Guid.Parse(x[Constants.ID].ToString()))).ToList();

    //        foreach (var rowToDelete in rowsToDelete)
    //        {
    //            rowToDelete.Delete();
    //        }
    //    }

    //    private void MapdxItemToRow(DXItem dxItem, DataRow row, string dxModelType)
    //    {
    //        row[Constants.ID] = dxItem.ID;

    //        if (row.Table.Columns.Contains(Constants.ObjectID))
    //        {
    //            row[Constants.ObjectID] = dxItem.ObjectID;
    //        }

    //        if (row.Table.Columns.Contains($"{dxModelType}ID"))
    //        {
    //            row[$"{dxModelType}ID"] = dxItem.ObjectID;
    //        }

    //        if (row.Table.Columns.Contains("TimeStamp"))
    //        {
    //            row["TimeStamp"] = DateTime.UtcNow;
    //        }

    //        if (dxItem.Content == null)
    //            return;

    //        var properties = dxItem.Content.Children().Select(x => x as JProperty).Where(x => x != null);

    //        foreach (var column in row.Table.Columns.OfType<DataColumn>())
    //        {
    //            var jProperty = properties.SingleOrDefault(x => x.Name == column.ColumnName);

    //            if (column.ColumnName == "ID"
    //                || column.ColumnName == "ObjectID"
    //                || column.ColumnName == "TimeStamp"
    //                || column.ColumnName == $"{dxModelType}ID")
    //            {
    //                continue;
    //            }

    //            if (!column.ReadOnly)
    //            {
    //                if (jProperty != null)
    //                {
    //                    if (this.IsNullOrEmpty(jProperty.Value as JValue))
    //                    {
    //                        if (column.AllowDBNull)
    //                        {
    //                            this.SetNullValueToRowCell(row, column, jProperty);
    //                        }
    //                        else
    //                        {
    //                            this.SetNotNullValueToRowCell(row, column);
    //                        }
    //                    }
    //                    else
    //                    {
    //                        this.SetJPropertyValueToRowCell(row, column, jProperty);
    //                    }
    //                }
    //                else if (
    //                    row[column] == DBNull.Value
    //                    && !column.AllowDBNull
    //                   )
    //                {
    //                    this.SetNotNullValueToRowCell(row, column);
    //                }
    //            }
    //        }

    //    }

    //    private bool IsNullOrEmpty(JValue jValue)
    //    {
    //        return jValue == null || string.IsNullOrWhiteSpace(jValue.ToString());
    //    }

    //    private void SetNullValueToRowCell(DataRow dataRow, DataColumn dataColumn, JProperty jProperty)
    //    {
    //        JValue jValue = jProperty.Value as JValue;

    //        dataRow[dataColumn] = DBNull.Value;
    //    }

    //    private void SetJPropertyValueToRowCell(DataRow dataRow, DataColumn dataColumn, JProperty jProperty)
    //    {
    //        JValue jValue = jProperty.Value as JValue;

    //        if (dataColumn.DataType == typeof(Guid))
    //        {
    //            dataRow[dataColumn] = jValue.Value;
    //        }
    //        else
    //        if (dataColumn.DataType == typeof(decimal))
    //        {
    //            dataRow[dataColumn] = ConvertHelper.ParseDecimal(jValue.Value);
    //        }
    //        else
    //        if (dataColumn.DataType == typeof(double))
    //        {
    //            dataRow[dataColumn] = ConvertHelper.ParseDouble(jValue.Value);
    //        }
    //        else
    //        if (dataColumn.DataType == typeof(sbyte))
    //        {
    //            dataRow[dataColumn] = ConvertHelper.ParseSByte(jValue.Value);
    //        }
    //        else
    //        if (dataColumn.DataType == typeof(int))
    //        {
    //            dataRow[dataColumn] = ConvertHelper.ParseInt(jValue.Value);
    //        }
    //        else
    //        if (dataColumn.DataType == typeof(DateTime))
    //        {
    //            dataRow[dataColumn] = ConvertHelper.ParseDateTime(jValue.Value);
    //        }
    //        else
    //        if (dataColumn.DataType == typeof(bool))
    //        {
    //            dataRow[dataColumn] = ConvertHelper.ParseBool(jValue.Value);
    //        }
    //        else
    //        if (dataColumn.DataType == typeof(byte[]))
    //        {
    //            dataRow[dataColumn] = (byte[])jValue.Value;
    //        }
    //        else if (dataColumn.DataType == typeof(TimeSpan))
    //        {
    //            dataRow[dataColumn] = ConvertHelper.ParseTimeSpan(jValue.Value);
    //        }
    //        else
    //        //if (dataColumn.DataType == typeof(string))
    //        {
    //            dataRow[dataColumn] = ConvertHelper.ParseString(jValue.Value);
    //        }
    //    }

    //    private void SetNotNullValueToRowCell(DataRow dataRow, DataColumn dataColumn)
    //    {
    //        if (dataColumn.DataType == typeof(Guid))
    //        {
    //            dataRow[dataColumn] = new Guid();
    //        }
    //        else
    //        if (dataColumn.DataType == typeof(decimal))
    //        {
    //            dataRow[dataColumn] = 0;
    //        }
    //        else
    //        if (dataColumn.DataType == typeof(double))
    //        {
    //            dataRow[dataColumn] = 0;
    //        }
    //        else
    //        if (dataColumn.DataType == typeof(int))
    //        {
    //            dataRow[dataColumn] = 0;
    //        }
    //        else
    //        if (dataColumn.DataType == typeof(DateTime))
    //        {
    //            dataRow[dataColumn] = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    //        }
    //        else
    //        if (dataColumn.DataType == typeof(bool))
    //        {
    //            dataRow[dataColumn] = false;
    //        }
    //        else
    //        if (dataColumn.DataType == typeof(byte[]))
    //        {
    //            dataRow[dataColumn] = new byte[0];
    //        }
    //        else
    //        //if (dataColumn.DataType == typeof(string))
    //        {
    //            dataRow[dataColumn] = "";
    //        }
    //    }

    //    public IEnumerable<Guid> GetRelations(string ObjectTypeNameLeft, Guid obj1Id, string relationToObj2Name)
    //    {
    //        var relationInfo = this.GetRelationInfo(ObjectTypeNameLeft, relationToObj2Name);

    //        IEnumerable<Guid> result = null;

    //        switch (relationInfo.RelationType)
    //        {
    //            case DXRelationTypeEnum.ManyToMany:
    //                result = this.GetRelationsManyToMany(relationInfo, obj1Id);
    //                break;
    //            case DXRelationTypeEnum.OneToZeroOne:
    //                result = this.GetRelationsOneToMany(relationInfo, obj1Id);
    //                break;
    //            case DXRelationTypeEnum.ZeroOneToOne:
    //                result = this.GetRelationsManyToOne(relationInfo, obj1Id);
    //                break;
    //            case DXRelationTypeEnum.OneToMany:
    //                result = this.GetRelationsOneToMany(relationInfo, obj1Id);
    //                break;
    //            case DXRelationTypeEnum.ManyToOne:
    //                result = this.GetRelationsManyToOne(relationInfo, obj1Id);
    //                break;
    //            case DXRelationTypeEnum.ZeroOneToMany:
    //                result = this.GetRelationsOneToMany(relationInfo, obj1Id);
    //                break;
    //            case DXRelationTypeEnum.ManyToZeroOne:
    //                result = this.GetRelationsManyToOne(relationInfo, obj1Id);
    //                break;
    //            case DXRelationTypeEnum.ZeroOneToZeroOne:
    //                result = this.GetRelationsZeroOneToZeroOne(relationInfo, obj1Id);
    //                break;
    //        }

    //        return result;
    //    }

    //    public Guid? GetRelation(string obj1TypeName, Guid obj1Id, string relationToObj2Name)
    //    {
    //        var relationInfo = this.GetRelationInfo(obj1TypeName, relationToObj2Name);

    //        IEnumerable<Guid> ids = null;

    //        Guid? result = null;

    //        switch (relationInfo.RelationType)
    //        {
    //            case DXRelationTypeEnum.ManyToMany:
    //                ids = this.GetRelationsManyToMany(relationInfo, obj1Id);
    //                break;
    //            case DXRelationTypeEnum.OneToZeroOne:
    //                ids = this.GetRelationsOneToMany(relationInfo, obj1Id);
    //                break;
    //            case DXRelationTypeEnum.ZeroOneToOne:
    //                ids = this.GetRelationsManyToOne(relationInfo, obj1Id);
    //                break;
    //            case DXRelationTypeEnum.OneToMany:
    //                ids = this.GetRelationsOneToMany(relationInfo, obj1Id);
    //                break;
    //            case DXRelationTypeEnum.ManyToOne:
    //                ids = this.GetRelationsManyToOne(relationInfo, obj1Id);
    //                break;
    //            case DXRelationTypeEnum.ZeroOneToMany:
    //                ids = this.GetRelationsOneToMany(relationInfo, obj1Id);
    //                break;
    //            case DXRelationTypeEnum.ManyToZeroOne:
    //                ids = this.GetRelationsManyToOne(relationInfo, obj1Id);
    //                break;
    //            case DXRelationTypeEnum.ZeroOneToZeroOne:
    //                ids = this.GetRelationsZeroOneToZeroOne(relationInfo, obj1Id);
    //                break;
    //        }

    //        if (ids != null && ids.Count() > 1)
    //        {
    //            throw new Exception($"Object '{obj1TypeName}'('{obj1Id}') for '{relationToObj2Name}' ralation has more than one related entries. Please use 'GetRelations' method instead.");
    //        }

    //        if (ids != null && ids.Any())
    //        {
    //            result = ids.First();
    //        }

    //        return result;
    //    }

    //    public bool AddRelation(string obj1TypeName, Guid obj1Id, string relationToObj2Name, string obj2TypeName, Guid obj2Id)
    //    {
    //        var relationInfo = this.GetRelationInfo(obj1TypeName, relationToObj2Name);

    //        switch (relationInfo.RelationType)
    //        {
    //            case DXRelationTypeEnum.ManyToMany:
    //                return this.AddRelationManyToMany(relationInfo, obj1Id, obj2Id);
    //            case DXRelationTypeEnum.OneToZeroOne:
    //                return this.AddRelationOneToMany(relationInfo, obj1Id, obj2Id);
    //            case DXRelationTypeEnum.ZeroOneToOne:
    //                return this.AddRelationManyToOne(relationInfo, obj1Id, obj2Id);
    //            case DXRelationTypeEnum.OneToMany:
    //                return this.AddRelationOneToMany(relationInfo, obj1Id, obj2Id);
    //            case DXRelationTypeEnum.ManyToOne:
    //                return this.AddRelationManyToOne(relationInfo, obj1Id, obj2Id);
    //            case DXRelationTypeEnum.ZeroOneToMany:
    //                return this.AddRelationOneToMany(relationInfo, obj1Id, obj2Id);
    //            case DXRelationTypeEnum.ManyToZeroOne:
    //                return this.AddRelationManyToOne(relationInfo, obj1Id, obj2Id);
    //            case DXRelationTypeEnum.ZeroOneToZeroOne:
    //                return this.AddRelationZeroOneToZeroOne(relationInfo, obj1Id, obj2Id);
    //            default:
    //                throw new NotImplementedException($"Relation type '{relationInfo.RelationType}' is not supported.");
    //        }
    //    }

    //    public bool RemoveRelation(string obj1TypeName, Guid obj1Id, string relationToObj2Name, string obj2TypeName, Guid obj2Id)
    //    {
    //        var relationInfo = this.GetRelationInfo(obj1TypeName, relationToObj2Name);

    //        switch (relationInfo.RelationType)
    //        {
    //            case DXRelationTypeEnum.ManyToMany:
    //                return this.RemoveRelationManyToMany(relationInfo, obj1Id, obj2Id);
    //            case DXRelationTypeEnum.OneToZeroOne:
    //                throw new NotImplementedException("'1 to 0/1' relation couldn't be removed.");
    //            case DXRelationTypeEnum.ZeroOneToOne:
    //                throw new NotImplementedException("'0/1 to 1' relation couldn't be removed.");
    //            case DXRelationTypeEnum.OneToMany:
    //                throw new NotImplementedException("'1 to M' relation couldn't be removed.");
    //            case DXRelationTypeEnum.ManyToOne:
    //                throw new NotImplementedException("'N to 1' relation couldn't be removed.");
    //            case DXRelationTypeEnum.ZeroOneToMany:
    //                return this.RemoveRelationZeroOneToMany(relationInfo, obj1Id, obj2Id);
    //            case DXRelationTypeEnum.ManyToZeroOne:
    //                return this.RemoveRelationManyToZeroOne(relationInfo, obj1Id, obj2Id);
    //            case DXRelationTypeEnum.ZeroOneToZeroOne:
    //                return this.RemoveRelationZeroOneToZeroOne(relationInfo, obj1Id, obj2Id);
    //            default:
    //                throw new NotImplementedException($"Relation type '{relationInfo.RelationType}' is not supported.");
    //        }
    //    }

    //    public Guid InsertSingleDXElement(string dxModelType, DXSingleElement dxSingleDXElement)
    //    {
    //        return this.InsertOrUpdateSingleDXElementPrivate(dxModelType, dxSingleDXElement, ProcessingType.Insert);
    //    }

    //    public Guid UpdateSingleDXElement(string dxModelType, DXSingleElement dxSingleDXElement)
    //    {
    //        return this.InsertOrUpdateSingleDXElementPrivate(dxModelType, dxSingleDXElement, ProcessingType.Update);
    //    }

    //    public Guid InsertOrUpdateSingleDXElement(string dxModelType, DXSingleElement dxSingleDXElement)
    //    {
    //        throw new NotImplementedException("InsertOrUpdateSingleDXElement is not implemted yet.");
    //    }

    //    private Guid InsertOrUpdateSingleDXElementPrivate(string dxModelType, DXSingleElement dxSingleDXElement, ProcessingType processingType)
    //    {
    //        ArgumentNullException.ThrowIfNullOrEmpty(dxModelType);
    //        ArgumentNullException.ThrowIfNull(dxSingleDXElement);

    //        return this.RunRequestInTransaction((conn) =>
    //        {
    //            var dataSet = new DataSet(dxSingleDXElement.Name);

    //            var id = this.InsertOrUpdatedxSingleItemToDataSet(
    //                dxSingleDXElement,
    //                dxModelType,
    //                dxSingleDXElement.Item.ObjectID.Value,
    //                dataSet,
    //                conn,
    //                processingType);

    //            dataSet.AcceptChanges();

    //            return id;
    //        });
    //    }

    //    public bool DeleteSingleDXElement(string typeName, Guid id)
    //    {
    //        ArgumentNullException.ThrowIfNullOrEmpty(typeName);

    //        if (id == Guid.Empty)
    //            throw new ArgumentException("Id must be a non-empty GUID.", nameof(id));

    //        return this.RunRequestInTransaction((conn) =>
    //        {
    //            DataSet dataSet = new DataSet(typeName);

    //            var dxModelAdapter = this.PopulateTableToDataSet(conn, dataSet, typeName, whereClause:
    //                this._queryHelper.GetWhereExpressionForID(id));

    //            var dxModelBuilder = this._queryHelper.GetDbCommandBuilder(dxModelAdapter);

    //            dxModelBuilder.GetDeleteCommand();

    //            DataTable dataTable = dataSet.Tables[typeName];

    //            var existingRow = dataTable.Rows.Cast<DataRow>().SingleOrDefault(x => ConvertHelper.ParseGuid(x["ID"]) == id);

    //            if (existingRow != null)
    //            {
    //                existingRow.Delete();

    //                dxModelAdapter.Update(dataSet, typeName);

    //                dataSet.AcceptChanges();

    //                return true;
    //            }
    //            else
    //            {
    //                return false;
    //            }
    //        });
    //    }

    //    public DXSingleElement GetSingleDXElement(DXElementDefinition container, Guid id)
    //    {
    //        if (container == null)
    //            return null;

    //        DXSingleElement result = null;

    //        this.RunRequest((conn) =>
    //        {
    //            DataSet dataSet = new DataSet(container.Type);

    //            var whereClauseForID = this._queryHelper.GetWhereExpressionForID(id);

    //            this.PopulateTableToDataSet(conn, dataSet, container.Type,
    //                columnNames: container.Select(x => x.ColumnDefinition.DXExpression),
    //                whereClause: whereClauseForID, fillSchema: false);

    //            var dataTable = dataSet.Tables[container.Type];

    //            if (dataTable.Rows.Count == 0)
    //            {
    //                result = null;
    //            }
    //            else
    //            {
    //                result = this.ConvertTodxSingleItem(container);

    //                this.PopulatedxSingleItem(result, container, dataSet.Tables[result.Name].Rows.Cast<DataRow>()
    //                    .SingleOrDefault(y => ConvertHelper.ParseGuid(y[Constants.ID]) == id));
    //            }
    //        });

    //        return result;
    //    }

    //    // TODO: can be refactored using stored procedure
    //    private DXRelationDefinitionMainElement GetRelationInfo(string obj1Name, string relationToObj2Name)
    //    {
    //        return this.RunRequestInTransaction((conn) =>
    //        {
    //            var dataSet = new DataSet("DXRelationDefinitionUnit");

    //            this.PopulateTableToDataSet(conn, dataSet, "DXRelationDefinitionMainElement"
    //                , whereClause:
    //                 this._queryHelper.GetWhereExpressionWithAnd(
    //                    new Dictionary<string, object>()
    //                    {
    //                        { "ObjectNameLeft", obj1Name },
    //                        { "RelationNameRight", relationToObj2Name }
    //                    })
    //                 , fillSchema: false);

    //            var table = dataSet.Tables["DXRelationDefinitionMainElement"];

    //            if (table.Rows.Count == 0)
    //            {
    //                throw new Exception($"Relation pair {obj1Name}-{relationToObj2Name} is not existing in system");
    //            }

    //            var row = table.Rows[0];

    //            return new DXRelationDefinitionMainElement()
    //            {
    //                RelationType = (DXRelationTypeEnum)ConvertHelper.ParseInt(row["RelationType"]),
    //                RelationTable = ConvertHelper.ParseString(row["RelationTable"]),
    //                ID = ConvertHelper.ParseGuid(row[Constants.ID]),
    //                ObjectID = ConvertHelper.ParseGuid(row[Constants.ObjectID]),
    //                ObjectNameLeft = ConvertHelper.ParseString(row["ObjectNameLeft"]),
    //                ObjectNameRight = ConvertHelper.ParseString(row["ObjectNameRight"]),
    //                RelationNameLeft = ConvertHelper.ParseString(row["RelationNameLeft"]),
    //                RelationNameRight = ConvertHelper.ParseString(row["RelationNameRight"]),
    //                RelationColumnNameLeft = ConvertHelper.ParseString(row["RelationColumnNameLeft"]),
    //                RelationColumnNameRight = ConvertHelper.ParseString(row["RelationColumnNameRight"]),
    //                RelationColumnTypeLeft = row["RelationColumnTypeLeft"] == DBNull.Value ? null : (DXColumnTypeEnum)ConvertHelper.ParseInt(row["RelationColumnTypeLeft"]),
    //                RelationColumnTypeRight = row["RelationColumnTypeRight"] == DBNull.Value ? null : (DXColumnTypeEnum)ConvertHelper.ParseInt(row["RelationColumnTypeRight"])
    //            };
    //        });
    //    }

    //    private bool AddRelationManyToMany(DXRelationDefinitionMainElement relationInfo, Guid obj1Id, Guid obj2Id)
    //    {
    //        return this.RunRequestInTransaction((conn) =>
    //        {
    //            var dataSet = new DataSet(relationInfo.RelationTable);

    //            var adapter = this.PopulateTableToDataSet(conn, dataSet, relationInfo.RelationTable
    //                , whereClause:
    //                this._queryHelper.GetWhereExpressionWithAnd(
    //                    new Dictionary<string, object>()
    //                    {
    //                        { relationInfo.RelationNameLeft, obj1Id },
    //                        { relationInfo.RelationNameRight, obj2Id }
    //                    })
    //                , fillSchema: false);

    //            var table = dataSet.Tables[relationInfo.RelationTable];
    //            DataRow dataRow;

    //            if (table.Rows.Count == 0)
    //            {
    //                var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
    //                modelBuilder.GetInsertCommand();

    //                dataRow = table.NewRow();

    //                dataRow[relationInfo.RelationNameLeft] = obj1Id;
    //                dataRow[relationInfo.RelationNameRight] = obj2Id;

    //                table.Rows.Add(dataRow);
    //            }
    //            else
    //            {
    //                var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
    //                modelBuilder.GetUpdateCommand();

    //                dataRow = table.Rows[0];

    //                dataRow[relationInfo.RelationNameLeft] = obj1Id;
    //                dataRow[relationInfo.RelationNameRight] = obj2Id;
    //            }

    //            adapter.Update(dataSet, relationInfo.RelationTable);
    //            dataSet.AcceptChanges();

    //            return true;
    //        });
    //    }

    //    private bool AddRelationManyToOne(DXRelationDefinitionMainElement relationInfo, Guid obj1Id, Guid obj2Id)
    //    {
    //        return this.RunRequestInTransaction((conn) =>
    //        {
    //            var tableName = relationInfo.ObjectNameLeft;

    //            var dataSet = new DataSet(tableName);

    //            var adapter = this.PopulateTableToDataSet(conn, dataSet, tableName
    //                , whereClause: this._queryHelper.GetWhereExpressionForID(obj1Id));

    //            var table = dataSet.Tables[tableName];
    //            var rows = table.Rows;

    //            if (rows.Count == 1)
    //            {
    //                var row = rows[0];

    //                row[relationInfo.RelationNameRight] = obj2Id;

    //                var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
    //                modelBuilder.GetUpdateCommand();

    //                adapter.Update(dataSet, tableName);
    //                dataSet.AcceptChanges();

    //                return true;
    //            }
    //            else
    //            {
    //                return false;
    //            }
    //        });
    //    }

    //    private bool AddRelationOneToMany(DXRelationDefinitionMainElement relationInfo, Guid obj1Id, Guid obj2Id)
    //    {
    //        return this.RunRequestInTransaction((conn) =>
    //        {
    //            var tableName = relationInfo.ObjectNameRight;

    //            var dataSet = new DataSet(tableName);

    //            var adapter = this.PopulateTableToDataSet(conn, dataSet, tableName
    //                , whereClause: this._queryHelper.GetWhereExpressionForID(obj2Id));

    //            var table = dataSet.Tables[tableName];
    //            var rows = table.Rows;

    //            if (rows.Count == 1)
    //            {
    //                var row = rows[0];

    //                row[relationInfo.RelationNameLeft] = obj1Id;

    //                var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
    //                modelBuilder.GetUpdateCommand();

    //                adapter.Update(dataSet, tableName);
    //                dataSet.AcceptChanges();

    //                return true;
    //            }
    //            else
    //            {
    //                return false;
    //            }
    //        });
    //    }

    //    private bool AddRelationZeroOneToZeroOne(DXRelationDefinitionMainElement relationInfo, Guid obj1Id, Guid obj2Id)
    //    {
    //        bool isRightTableContainsRelationID = relationInfo.RelationTable.Equals(relationInfo.ObjectNameRight);

    //        if (isRightTableContainsRelationID)
    //        {
    //            return this.AddRelationOneToMany(relationInfo, obj1Id, obj2Id);
    //        }
    //        else
    //        {
    //            return this.AddRelationManyToOne(relationInfo, obj1Id, obj2Id);
    //        }
    //    }

    //    private bool RemoveRelationManyToMany(DXRelationDefinitionMainElement relationInfo, Guid obj1Id, Guid obj2Id)
    //    {
    //        return this.RunRequestInTransaction((conn) =>
    //        {
    //            var dataSet = new DataSet(relationInfo.RelationTable);

    //            var adapter = this.PopulateTableToDataSet(conn, dataSet, relationInfo.RelationTable
    //                , whereClause:
    //                this._queryHelper.GetWhereExpressionWithAnd(
    //                    new Dictionary<string, object>()
    //                    {
    //                        { relationInfo.RelationNameLeft, obj1Id },
    //                        { relationInfo.RelationNameRight, obj2Id }
    //                    })
    //                , fillSchema: false);

    //            var table = dataSet.Tables[relationInfo.RelationTable];
    //            var rows = table.Rows;

    //            if (rows.Count > 0)
    //            {
    //                var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
    //                modelBuilder.GetDeleteCommand();

    //                for (int i = 0; i < rows.Count; i++)
    //                {
    //                    var row = rows[i];
    //                    row.Delete();
    //                }

    //                adapter.Update(dataSet, relationInfo.RelationTable);
    //                dataSet.AcceptChanges();

    //                return true;
    //            }
    //            else
    //            {
    //                return false;
    //            }
    //        });
    //    }

    //    private bool RemoveRelationManyToZeroOne(DXRelationDefinitionMainElement relationInfo, Guid obj1Id, Guid obj2Id)
    //    {
    //        return this.RunRequestInTransaction((conn) =>
    //        {
    //            var tableName = relationInfo.ObjectNameLeft;

    //            var dataSet = new DataSet(tableName);

    //            var adapter = this.PopulateTableToDataSet(conn, dataSet, tableName
    //                , whereClause:
    //                 this._queryHelper.GetWhereExpressionWithAnd(
    //                    new Dictionary<string, object>()
    //                    {
    //                        { "ID", obj1Id },
    //                        { relationInfo.RelationNameRight, obj2Id }
    //                    })
    //                 );

    //            var table = dataSet.Tables[tableName];
    //            var rows = table.Rows;

    //            if (rows.Count == 1)
    //            {
    //                var row = rows[0];

    //                row[relationInfo.RelationNameRight] = DBNull.Value;

    //                var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
    //                modelBuilder.GetUpdateCommand();

    //                adapter.Update(dataSet, tableName);
    //                dataSet.AcceptChanges();

    //                return true;
    //            }
    //            else
    //            {
    //                return false;
    //            }
    //        });
    //    }

    //    private bool RemoveRelationZeroOneToZeroOne(DXRelationDefinitionMainElement relationInfo, Guid obj1Id, Guid obj2Id)
    //    {
    //        bool isRightTableContainsRelationID = relationInfo.RelationTable.Equals(relationInfo.ObjectNameRight);

    //        if (isRightTableContainsRelationID)
    //        {
    //            return this.RemoveRelationZeroOneToMany(relationInfo, obj1Id, obj2Id);
    //        }
    //        else
    //        {
    //            return this.RemoveRelationManyToZeroOne(relationInfo, obj1Id, obj2Id);
    //        }
    //    }

    //    private bool RemoveRelationZeroOneToMany(DXRelationDefinitionMainElement relationInfo, Guid obj1Id, Guid obj2Id)
    //    {
    //        return this.RunRequestInTransaction((conn) =>
    //        {
    //            var tableName = relationInfo.ObjectNameRight;

    //            var dataSet = new DataSet(tableName);

    //            var adapter = this.PopulateTableToDataSet(conn, dataSet, tableName
    //                , whereClause: this._queryHelper.GetWhereExpressionWithAnd(
    //                    new Dictionary<string, object>()
    //                    {
    //                        { "ID", obj2Id },
    //                        { relationInfo.RelationNameLeft, obj1Id}
    //                    })
    //                );

    //            var table = dataSet.Tables[tableName];
    //            var rows = table.Rows;

    //            if (rows.Count == 1)
    //            {
    //                var row = rows[0];

    //                row[relationInfo.RelationNameLeft] = DBNull.Value;

    //                var modelBuilder = this._queryHelper.GetDbCommandBuilder(adapter);
    //                modelBuilder.GetUpdateCommand();

    //                adapter.Update(dataSet, tableName);
    //                dataSet.AcceptChanges();

    //                return true;
    //            }
    //            else
    //            {
    //                return false;
    //            }
    //        });
    //    }

    //    private IEnumerable<Guid> GetRelationsManyToMany(DXRelationDefinitionMainElement relationInfo, Guid obj1Id)
    //    {
    //        return this.RunRequestInTransaction((conn) =>
    //        {
    //            var dataSet = new DataSet(relationInfo.RelationTable);

    //            this.PopulateTableToDataSet(conn, dataSet, relationInfo.RelationTable
    //                , whereClause: this._queryHelper.GetWhereExpressionWithAnd(
    //                    new Dictionary<string, object>()
    //                    {
    //                        { relationInfo.RelationNameLeft, obj1Id }
    //                    })
    //                , fillSchema: false);

    //            var table = dataSet.Tables[relationInfo.RelationTable];
    //            var rows = table.Rows;

    //            return rows.Cast<DataRow>().Select(x =>
    //            {
    //                var relatedId = ConvertHelper.ParseGuid(x[relationInfo.RelationNameRight]);

    //                return relatedId;
    //            });
    //        });
    //    }

    //    private IEnumerable<Guid> GetRelationsManyToOne(DXRelationDefinitionMainElement relationInfo, Guid obj1Id)
    //    {
    //        IEnumerable<Guid> result = this.RunRequestInTransaction((conn) =>
    //        {
    //            var tableName = relationInfo.ObjectNameLeft;

    //            var dataSet = new DataSet(tableName);

    //            this.PopulateTableToDataSet(conn, dataSet, tableName
    //                , whereClause: this._queryHelper.GetWhereExpressionForID(obj1Id), fillSchema: false);

    //            var table = dataSet.Tables[tableName];
    //            var rows = table.Rows;

    //            return rows.Cast<DataRow>()
    //                .Where(x => x[relationInfo.RelationNameRight] != DBNull.Value)
    //                .Select(x => ConvertHelper.ParseGuid(x[relationInfo.RelationNameRight]));
    //        });

    //        return result;
    //    }

    //    private IEnumerable<Guid> GetRelationsOneToMany(DXRelationDefinitionMainElement relationInfo, Guid obj1Id)
    //    {
    //        return this.RunRequestInTransaction((conn) =>
    //        {
    //            var tableName = relationInfo.ObjectNameRight;

    //            var dataSet = new DataSet(tableName);

    //            this.PopulateTableToDataSet(conn, dataSet, tableName
    //                , whereClause: this._queryHelper.GetWhereExpressionWithAnd(
    //                    new Dictionary<string, object>()
    //                    {
    //                        { relationInfo.RelationNameLeft, obj1Id }
    //                    })
    //                , fillSchema: false);

    //            var table = dataSet.Tables[tableName];

    //            var rows = table.Rows;

    //            return rows.Cast<DataRow>().Select(x => ConvertHelper.ParseGuid(x[Constants.ID]));
    //        });
    //    }

    //    private IEnumerable<Guid> GetRelationsZeroOneToZeroOne(DXRelationDefinitionMainElement relationInfo, Guid obj1Id)
    //    {
    //        bool isRightTableContainsRelationID = relationInfo.RelationTable.Equals(relationInfo.ObjectNameRight);

    //        return isRightTableContainsRelationID ? this.GetRelationsOneToMany(relationInfo, obj1Id) : this.GetRelationsManyToOne(relationInfo, obj1Id);
    //    }

    //    private DbDataAdapter PopulateTableToDataSet(
    //        DbConnection conn,
    //        DataSet dataSet,
    //        string tableName,
    //        IEnumerable<string> columnNames = null,
    //        string whereClause = null,
    //        IDictionary<string, string> orderBy = null,
    //        int? limit = null,
    //        bool fillSchema = true)
    //    {
    //        var adapter = this._queryHelper.GetDbDataAdapter(conn, this._queryHelper.GetSQLQuery(tableName, columnNames, whereClause, orderBy, limit));

    //        if (fillSchema)
    //        {
    //            adapter.FillSchema(dataSet, SchemaType.Source, tableName);

    //            foreach (DataColumn col in dataSet.Tables[tableName].Columns)
    //            {
    //                if (col.DataType == typeof(DateTime))
    //                    col.DateTimeMode = DataSetDateTime.Utc;
    //            }
    //        }

    //        adapter.Fill(dataSet, tableName);

    //        return adapter;
    //    }

    //    private T RunRequestInTransaction<T>(Func<DbConnection, T> func)
    //    {
    //        using (DbConnection conn = this._queryHelper.GetDBConnection(this._connectionStr))
    //        {
    //            conn.Open();
    //            var transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted);

    //            try
    //            {
    //                var result = func.Invoke(conn);
    //                transaction.Commit();

    //                return result;
    //            }
    //            catch (Exception exc)
    //            {
    //                var exceptions = new List<Exception>() { exc };
    //                try
    //                {
    //                    transaction.Rollback();
    //                }
    //                catch (Exception exc2)
    //                {
    //                    exceptions.Add(exc2);
    //                }

    //                throw new AggregateException(exceptions);
    //            }
    //        }
    //    }

    //    private void RunRequest(Action<DbConnection> action)
    //    {
    //        using (DbConnection conn = this._queryHelper.GetDBConnection(this._connectionStr))
    //        {
    //            conn.Open();

    //            action.Invoke(conn);
    //        }
    //    }

    //    public void DropDataBase()
    //    {
    //        this._queryHelper.DropDataBase(this._connectionStr);
    //    }

    //    public void CreateDataBase()
    //    {
    //        this._queryHelper.CreateDataBase(this._connectionStr);
    //    }

    //    private enum ProcessingType
    //    {
    //        Insert = 1,
    //        Update = 2,
    //        Delete = 3
    //    }
    //}
}