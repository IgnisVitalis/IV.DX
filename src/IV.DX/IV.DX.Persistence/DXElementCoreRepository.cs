using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;
using IV.DX.Kernel;

namespace IV.DX.Persistence
{
    internal partial class DXCoreRepository : IDXUnitCoreRepository, IDXStructureRepository, IDXEnumCoreRepository, IDXStructureRawReader, IDXElementCoreRepository, IDXRawReader
    {
        public Guid InsertOrUpdate(DXDataBlock<DXElementRecord> block)
        {
            ArgumentNullException.ThrowIfNull(block);

            var dxUnitTypeName = block.Meta?.DXUnitContext;
            var elementTypeName = block.Meta?.Type;

            if (string.IsNullOrWhiteSpace(dxUnitTypeName))
                throw new InvalidOperationException("DXElement block Meta.DXUnitContext is required.");
            if (string.IsNullOrWhiteSpace(elementTypeName))
                throw new InvalidOperationException("DXElement block Meta.Type is required.");

            if (block.Data?.Upsert == null || block.Data.Upsert.Count == 0)
                return Guid.Empty;

            Guid lastId = Guid.Empty;
            foreach (var record in block.Data.Upsert)
            {
                if (record == null) continue;

                var normalizedRecord = EnsureDxUnitId(record, dxUnitTypeName);
                lastId = InsertOrUpdateElementRecord(dxUnitTypeName, elementTypeName, normalizedRecord, block.Meta?.IsRequired ?? false);
            }

            return lastId;
        }

        bool IDXElementCoreRepository.Delete(string typeName, Guid id)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(typeName);

            if (id == Guid.Empty)
                throw new ArgumentException("Id must be a non-empty GUID.", nameof(id));

            return this.RunRequestInTransaction((conn) =>
            {
                DataSet dataSet = new DataSet(typeName);

                var dxModelAdapter = this.PopulateTableToDataSet(conn, dataSet, typeName, SQLQueryBuilder.BaseColumns, dxFilter:
                    this.GetWhereExpressionForID(id));

                var dxModelBuilder = this._dbProvider.GetDbCommandBuilder(dxModelAdapter);

                dxModelBuilder.GetDeleteCommand();

                DataTable dataTable = dataSet.Tables[typeName];

                var existingRow = dataTable.Rows.Cast<DataRow>().SingleOrDefault(x => ConvertHelper.ParseGuid(x["ID"]) == id);

                if (existingRow != null)
                {
                    existingRow.Delete();

                    dxModelAdapter.Update(dataSet, typeName);

                    dataSet.AcceptChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        public DXElementRecord? GetItemRecord(DXTableDefinition container, Guid id)
        {
            var dxFilter = $"ID = '{id}'";

            var result = GetItemsRecord(container, dxFilter);

            return result?.SingleOrDefault();
        }

        public IEnumerable<DXElementRecord> GetItemsRecord(DXTableDefinition container, string dxFilter)
        {
            if (container == null)
                return Enumerable.Empty<DXElementRecord>();

            string typeName = container.Type;
            var ids = this.GetItemIDs(typeName, dxFilter);

            if (ids.Count() == 0)
                return Enumerable.Empty<DXElementRecord>();

            var sqlWhereClause = this.GetWhereExpressionForID(ids);

            var result = this.RunRequest((conn) =>
            {
                DataSet dataSet = new DataSet(container.Type);

                var columns = EnsureDxUnitIdColumn(container.GetColumns());
                this.PopulateTableToDataSet(conn, dataSet, container.Type,
                    columns: columns,
                    dxFilter: sqlWhereClause, fillSchema: false);

                var dataTable = dataSet.Tables[container.Type];

                var records = dataSet.Tables[container.Type].Rows.Cast<DataRow>()
                    .Select(dataRow => BuildElementRecordFromRow(dataRow, columns, container.DXUnitType))
                    .ToList();

                return records;
            });

            return result;
        }

        private Guid InsertOrUpdateElementRecord(
            string dxUnitTypeName,
            string elementTypeName,
            DXElementRecord record,
            bool isRequired)
        {
            if (string.IsNullOrWhiteSpace(elementTypeName))
                throw new ArgumentException("Element type name is required.", nameof(elementTypeName));
            if (record.DXUnitID == Guid.Empty)
                throw new ArgumentException("DXUnitID is required for DXElementRecord.", nameof(record));

            return this.RunRequestInTransaction(conn =>
            {
                var dataSet = new DataSet(elementTypeName);
                var elementDef = _dxStructureCache.GetDXElement(elementTypeName);
                if (elementDef == null)
                    throw new Exception($"There are no DXElement with name '{elementTypeName}'");

                var tableDef = elementDef.ToDXTableDefinition(dxUnitTypeName, _dxStructureCache.DXRelations, isRequired);
                var columns = EnsureDxUnitIdColumn(tableDef.GetColumns());
                var adapter = this.PopulateTableToDataSet(conn, dataSet, elementTypeName,
                    columns: columns,
                    dxFilter: this.GetWhereExpressionForID(record.ID));

                var table = dataSet.Tables[elementTypeName];
                if (table.PrimaryKey == null || table.PrimaryKey.Length == 0)
                {
                    if (table.Columns.Contains("ID"))
                        table.PrimaryKey = new[] { table.Columns["ID"] };
                }

                var item = BuildRowItemFromElementRecord(record, elementTypeName, record.DXUnitID);
                var row = table.Rows.Find(item.ID);

                if (row == null)
                {
                    row = table.NewRow();
                    MapRowItemToRow(item, row, dxUnitTypeName);
                    table.Rows.Add(row);
                }
                else
                {
                    MapRowItemToRow(item, row, dxUnitTypeName);
                }

                SaveTable(adapter, conn, dataSet, table, false);
                dataSet.AcceptChanges();

                return item.ID;
            });
        }

        private DXElementRecord BuildElementRecordFromRow(DataRow row, IDictionary<string, string> columns, string dxUnitType)
        {
            var id = ConvertHelper.ParseGuid(row[Constants.ID]);
            var timeStamp = ConvertHelper.ParseDateTime(row[Constants.TimeStamp]);
            var dxUnitId = ResolveDxUnitId(row, dxUnitType, id);

            var fields = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase);
            foreach (var column in row.Table.Columns.OfType<DataColumn>())
            {
                if (!columns.ContainsKey(column.ColumnName))
                    continue;

                if (Constants.SystemProperties.Any(p => string.Equals(p, column.ColumnName, StringComparison.OrdinalIgnoreCase))
                    || string.Equals(column.ColumnName, $"{dxUnitType}ID", StringComparison.OrdinalIgnoreCase))
                    continue;

                var value = row[column] == DBNull.Value ? null : GetValueFromRow(row, column);
                fields[column.ColumnName] = value == null ? JValue.CreateNull() : JToken.FromObject(value);
            }

            return new DXElementRecord
            {
                ID = id,
                TimeStamp = timeStamp,
                DXUnitID = dxUnitId,
                Fields = fields.Count == 0 ? null : fields
            };
        }


        private static DXElementRecord EnsureDxUnitId(DXElementRecord record, string dxUnitTypeName)
        {
            if (record.DXUnitID != Guid.Empty)
                return record;

            if (record.Fields != null)
            {
                if (TryReadGuid(record.Fields, Constants.DXUnitID, out var value))
                {
                    record.DXUnitID = value;
                    return record;
                }

                var customKey = $"{dxUnitTypeName}ID";
                if (TryReadGuid(record.Fields, customKey, out value))
                {
                    record.DXUnitID = value;
                    return record;
                }
            }

            return record;
        }

        private static bool TryReadGuid(IDictionary<string, JToken> fields, string key, out Guid value)
        {
            value = Guid.Empty;
            if (!fields.TryGetValue(key, out var token))
            {
                foreach (var kvp in fields)
                {
                    if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                    {
                        token = kvp.Value;
                        break;
                    }
                }
            }

            if (token == null || token.Type == JTokenType.Null)
                return false;

            if (token.Type == JTokenType.Guid)
            {
                value = token.ToObject<Guid>();
                return value != Guid.Empty;
            }

            var str = token.ToString();
            return Guid.TryParse(str, out value) && value != Guid.Empty;
        }

    }
}
