using IV.DX.Kernel;
using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Helpers.DXObjectHelpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

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

            if (block.Data?.Items == null || block.Data.Items.Count == 0)
                return Guid.Empty;

            // One transaction for the whole block, so a block of elements either lands or does not.
            // Per-record transactions made a failure halfway through leave the earlier ones written.
            return this.RunRequestInTransaction(conn =>
            {
                Guid lastId = Guid.Empty;

                foreach (var record in block.Data.Items)
                {
                    if (record == null) continue;

                    record.DXUnitId = DXObjectHelper.GetDeclaredDXUnitId(record, dxUnitTypeName);
                    lastId = InsertOrUpdateElementRecord(dxUnitTypeName, elementTypeName, record, block.Meta?.IsRequired ?? false, conn);
                }

                return lastId;
            });
        }

        bool IDXElementCoreRepository.Delete(string typeName, IEnumerable<Guid> ids)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(typeName);
            ArgumentNullException.ThrowIfNull(ids);

            var targets = ids.Distinct().ToList();

            if (targets.Count == 0)
                return false;

            if (targets.Any(id => id == Guid.Empty))
                throw new ArgumentException("Id must be a non-empty GUID.", nameof(ids));

            // One transaction for the whole set: a caller deleting several elements is making one
            // decision, and half of it landing is worse than none of it.
            return this.RunRequestInTransaction((conn) =>
            {
                DataSet dataSet = new DataSet(typeName);

                var dxModelAdapter = this.PopulateTableToDataSet(conn, dataSet, typeName, SQLQueryBuilder.BaseColumns, dxFilter:
                    this.GetWhereExpressionForId(targets));

                var dxModelBuilder = this._dbProvider.GetDbCommandBuilder(dxModelAdapter);

                dxModelBuilder.GetDeleteCommand();

                DataTable dataTable = dataSet.Tables[typeName]!;

                var existingRows = dataTable.Rows.Cast<DataRow>()
                    .Where(x => targets.Contains(ConvertHelper.ParseGuid(x["Id"])))
                    .ToList();

                if (existingRows.Count == 0)
                    return false;

                foreach (var row in existingRows)
                    row.Delete();

                dxModelAdapter.Update(dataSet, typeName);

                dataSet.AcceptChanges();

                return true;
            });
        }

        /// <summary>
        /// The unit a stored element belongs to, or <see cref="Guid.Empty"/> when the element is not
        /// there. Every write and delete resolves the owner this way instead of believing the one in
        /// the request: a caller who names someone else's element alongside their own unit would
        /// otherwise pass the access check on their unit and then rewrite the other one.
        /// </summary>
        public Guid GetOwnerDXUnitId(string dxUnitTypeName, string elementTypeName, Guid id)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(dxUnitTypeName);
            ArgumentNullException.ThrowIfNullOrEmpty(elementTypeName);

            if (id == Guid.Empty)
                return Guid.Empty;

            var elementDef = _dxStructureCache.GetDXElement(elementTypeName);
            if (elementDef == null)
                throw new Exception($"There are no DXElement with name '{elementTypeName}'");

            var container = elementDef.ToDXTableDefinition(dxUnitTypeName, _dxStructureCache.DXRelations, false);

            return this.GetItemRecord(container, id)?.DXUnitId ?? Guid.Empty;
        }

        public DXElementRecord? GetItemRecord(DXTableDefinition container, Guid id)
        {
            if (container == null)
                return null;

            var sqlWhereClause = this.GetWhereExpressionForId(id);

            return this.RunRequest((conn) =>
            {
                DataSet dataSet = new DataSet(container.Type);

                var columns = EnsureDxUnitIdColumn(container.GetColumns());
                this.PopulateTableToDataSet(conn, dataSet, container.Type,
                    columns: columns,
                    dxFilter: sqlWhereClause, fillSchema: false);

                return dataSet.Tables[container.Type]!.Rows.Cast<DataRow>()
                    .Select(dataRow => BuildElementRecordFromRow(dataRow, columns, container.DXUnitType))
                    .FirstOrDefault();
            });
        }

        public IEnumerable<DXElementRecord> GetItemsRecordByUnits(DXTableDefinition container, IEnumerable<Guid> dxUnitIds)
        {
            if (container == null || dxUnitIds == null)
                return Enumerable.Empty<DXElementRecord>();

            var owners = dxUnitIds.Where(x => x != Guid.Empty).Distinct().ToList();

            if (owners.Count == 0)
                return Enumerable.Empty<DXElementRecord>();

            return ReadElementRows(container, this.GetWhereExpressionForDXElementRows(container.DXUnitType, container.Type, owners));
        }

        public IEnumerable<DXElementRecord> GetItemsRecord(DXTableDefinition container, string dxFilter)
        {
            if (container == null)
                return Enumerable.Empty<DXElementRecord>();

            // The filter selects units; the element rows follow from whichever units it matched.
            var unitIds = this.GetItemIDs(container.DXUnitType, dxFilter);

            if (!unitIds.Any())
                return Enumerable.Empty<DXElementRecord>();

            return ReadElementRows(container, this.GetWhereExpressionForDXElementRows(container.DXUnitType, container.Type, unitIds));
        }

        private IEnumerable<DXElementRecord> ReadElementRows(DXTableDefinition container, string sqlWhereClause)
        {
            var result = this.RunRequest((conn) =>
            {
                DataSet dataSet = new DataSet(container.Type);

                var columns = EnsureDxUnitIdColumn(container.GetColumns());
                this.PopulateTableToDataSet(conn, dataSet, container.Type,
                    columns: columns,
                    dxFilter: sqlWhereClause, fillSchema: false);

                var dataTable = dataSet.Tables[container.Type];

                var records = dataSet.Tables[container.Type]!.Rows.Cast<DataRow>()
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
            bool isRequired,
            DbConnection conn)
        {
            if (string.IsNullOrWhiteSpace(elementTypeName))
                throw new ArgumentException("Element type name is required.", nameof(elementTypeName));
            if (record.DXUnitId == Guid.Empty)
                throw new ArgumentException("DXUnitId is required for DXElementRecord.", nameof(record));

            var dataSet = new DataSet(elementTypeName);
            var elementDef = _dxStructureCache.GetDXElement(elementTypeName);
            if (elementDef == null)
                throw new Exception($"There are no DXElement with name '{elementTypeName}'");

            var tableDef = elementDef.ToDXTableDefinition(dxUnitTypeName, _dxStructureCache.DXRelations, isRequired);
            var columns = EnsureDxUnitIdColumn(tableDef.GetColumns());
            var adapter = this.PopulateTableToDataSet(conn, dataSet, elementTypeName,
                columns: columns,
                dxFilter: this.GetWhereExpressionForId(record.Id));

            var table = dataSet.Tables[elementTypeName]!;
            if (table.PrimaryKey == null || table.PrimaryKey.Length == 0)
            {
                if (table.Columns.Contains("Id"))
                    table.PrimaryKey = new[] { table.Columns["Id"]! };
            }

            var item = BuildRowItemFromElementRecord(record, elementTypeName, record.DXUnitId);
            var row = table.Rows.Find(item.Id);

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

            return item.Id;
        }

        private DXElementRecord BuildElementRecordFromRow(DataRow row, IDictionary<string, string> columns, string dxUnitType)
        {
            var id = ConvertHelper.ParseGuid(row[Constants.Id]);
            var timeStamp = ConvertHelper.ParseDateTime(row[Constants.TimeStamp]);
            var dxUnitId = ResolveDxUnitId(row, dxUnitType, id);

            var fields = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase);
            foreach (var column in row.Table.Columns.OfType<DataColumn>())
            {
                if (!columns.ContainsKey(column.ColumnName))
                    continue;

                if (Constants.SystemProperties.Any(p => string.Equals(p, column.ColumnName, StringComparison.OrdinalIgnoreCase))
                    || string.Equals(column.ColumnName, $"{dxUnitType}Id", StringComparison.OrdinalIgnoreCase))
                    continue;

                var value = row[column] == DBNull.Value ? null : GetValueFromRow(row, column);
                fields[column.ColumnName] = value == null ? JValue.CreateNull() : JToken.FromObject(value);
            }

            return new DXElementRecord
            {
                Id = id,
                TimeStamp = timeStamp,
                DXUnitId = dxUnitId,
                Fields = fields.Count == 0 ? null : fields
            };
        }


    }
}

