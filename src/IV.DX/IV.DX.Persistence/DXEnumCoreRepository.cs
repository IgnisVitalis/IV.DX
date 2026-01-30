using System.Data;
using System;
using System.Collections.Generic;
using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers.DXModelDefinitionHelpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;

namespace IV.DX.Persistence
{
    internal partial class DXCoreRepository : IDXUnitCoreRepository, IDXStructureRepository, IDXEnumCoreRepository, IDXStructureRawReader, IDXElementCoreRepository, IDXRawReader
    {
        IEnumerable<DXModel> IDXEnumCoreRepository.GetItems(string enumType)
        {
            var modelDefinition = this.GetEnumModelDefinition(enumType);

            if (modelDefinition == null)
                return null;

            return this.GetItems(modelDefinition, DXLoadingType.Full);
        }

        bool IDXEnumCoreRepository.IsItemExisting(string typeName, Guid objectId)
        {
            DXDataSetDefinition dd = new DXDataSetDefinition(new DXMainTableDefinition(typeName, typeName));

            var item = this.GetItem(dd, objectId, DXLoadingType.Base);

            return item != null;
        }

        private DXDataSetDefinition GetEnumModelDefinition(string type)
        {
            var mainDXUnit = this.GetDXEnumDefinition(type);

            if (mainDXUnit == null)
                return null;

            var modelDefinition = DXModelDefinitionHelper.BuildModelDefinition(mainDXUnit, _dxStructureCache.DXRelations);

            return modelDefinition;
        }

        DXModel? IDXEnumCoreRepository.GetItem(string typeName, Guid objectId)
        {
            var modelDefinition = this.GetEnumModelDefinition(typeName);

            if (modelDefinition == null)
                return null;

            return this.GetItem(modelDefinition, objectId, DXLoadingType.Full);
        }

        Guid IDXEnumCoreRepository.Insert(DXModel dxModel)
        {
            var typeName = dxModel.DXMainElement.Attribute.Type;
            var enumInfo = this.GetDXEnumDefinition(typeName);

            return this.InsertOrUpdateDXEnum(enumInfo, dxModel, ProcessingType.Insert);

            throw new Exception($"Enum type '{dxModel.DXMainElement.Attribute.Type}' is not registered.");
        }

        Guid IDXEnumCoreRepository.Update(DXModel dxModel)
        {
            var typeName = dxModel.DXMainElement.Attribute.Type;
            var enumInfo = this.GetDXEnumDefinition(typeName);

            return this.InsertOrUpdateDXEnum(enumInfo, dxModel, ProcessingType.Update);

            throw new Exception($"Enum type '{dxModel.DXMainElement.Attribute.Type}' is not registered.");
        }

        Guid IDXEnumCoreRepository.InsertOrUpdate(DXModel dxModel)
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

        Guid IDXEnumCoreRepository.InsertOrUpdate(DXDataBlock<DXEnumRecord> block)
        {
            ArgumentNullException.ThrowIfNull(block);

            if (block.Data?.Upsert == null || block.Data.Upsert.Count == 0)
                return Guid.Empty;

            Guid lastId = Guid.Empty;
            foreach (var record in block.Data.Upsert)
            {
                if (record == null) continue;

                var typeName = GetEnumType(block.Meta?.Type, record);
                var enumInfo = this.GetDXEnumDefinition(typeName);

                if (enumInfo == null)
                    throw new Exception($"There are no DXEnum with name '{typeName}'");

                lastId = InsertOrUpdateDXEnumRecord(enumInfo, record);
            }

            return lastId;
        }

        bool IDXEnumCoreRepository.Delete(string typeName, Guid id)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(typeName);

            if (id == Guid.Empty)
                throw new ArgumentException("Id must be a non-empty GUID.", nameof(id));

            var mainDXUnitInfo = this.GetDXEnumDefinition(typeName);

            return this.RunRequestInTransaction((conn) =>
            {
                DataSet dataSet = new DataSet(typeName);

                this.DeleteDXUnitFromDataSet(typeName, id, dataSet, conn);

                dataSet.AcceptChanges();

                return true;
            });
        }

        private Guid InsertOrUpdateDXEnum(DXEnumDefinitionUnit enumInfo, DXModel dxModel, ProcessingType processingType)
        {
            this.RunRequestInTransaction(conn =>
            {
                var dataSet = new DataSet(enumInfo.Name);
                var enumTable = enumInfo.Name;

                var dxModelDefinition = dxModel.ToDXModelDefinition(enumInfo);

                var adapter = this.PopulateTableToDataSet(
                    conn,
                    dataSet,
                    enumTable,
                    dxFilter:
                    this.GetWhereExpressionForID(dxModel.DXMainElement.Item.ID),
                    columns: dxModelDefinition.MainElement.GetColumns());

                UpsertOwnRow(dxModel, dataSet.Tables[enumTable], enumTable, processingType);

                SaveTable(adapter, conn, dataSet, dataSet.Tables[enumTable], false);

                dataSet.AcceptChanges();
                return true;
            });

            return dxModel.DXMainElement.Item.ID;
        }

        private static string GetEnumType(string? enumTypeName, DXEnumRecord record)
        {
            var typeName = string.IsNullOrWhiteSpace(enumTypeName) ? record.Type : enumTypeName;
            if (string.IsNullOrWhiteSpace(typeName))
                throw new InvalidOperationException("Enum type name is required.");

            return typeName;
        }

        private static object? ConvertEnumTokenToObject(JToken? token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;

            return token.ToObject<object>();
        }

        private Guid InsertOrUpdateDXEnumRecord(DXEnumDefinitionUnit enumInfo, DXEnumRecord record)
        {
            return this.RunRequestInTransaction(conn =>
            {
                var dataSet = new DataSet(enumInfo.Name);
                var enumTable = enumInfo.Name;

                var modelDefinition = this.GetEnumModelDefinition(enumInfo.Name);
                if (modelDefinition == null)
                    throw new Exception($"There are no DXEnum with name '{enumInfo.Name}'");

                var adapter = this.PopulateTableToDataSet(
                    conn,
                    dataSet,
                    enumTable,
                    dxFilter: this.GetWhereExpressionForID(record.ID),
                    columns: modelDefinition.MainElement.GetColumns());

                var table = dataSet.Tables[enumTable];

                if (table.PrimaryKey == null || table.PrimaryKey.Length == 0)
                {
                    if (table.Columns.Contains("ID"))
                        table.PrimaryKey = new[] { table.Columns["ID"] };
                }

                var item = BuildEnumItem(enumInfo.Name, record);
                var row = table.Rows.Find(item.ID);

                if (row == null)
                {
                    row = table.NewRow();
                    MapdxItemToRow(item, row, enumInfo.Name);
                    table.Rows.Add(row);
                }
                else
                {
                    MapdxItemToRow(item, row, enumInfo.Name);
                }

                SaveTable(adapter, conn, dataSet, table, false);

                dataSet.AcceptChanges();
                return item.ID;
            });
        }

        private static DXItem BuildEnumItem(string enumTypeName, DXEnumRecord record)
        {
            var content = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (record.Fields != null)
            {
                foreach (var kvp in record.Fields)
                {
                    content[kvp.Key] = ConvertEnumTokenToObject(kvp.Value);
                }
            }

            if (record.Key != null)
                content["Key"] = ConvertEnumTokenToObject(record.Key);
            if (record.Value != null)
                content["Value"] = ConvertEnumTokenToObject(record.Value);

            return new DXItem(enumTypeName, record.ID, record.ID, record.TimeStamp, content);
        }
    }
}
