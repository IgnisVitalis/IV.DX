using IV.DX.Kernel.Attributes;
using System;
using System.Collections.Generic;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;
using System.Data;

namespace IV.DX.Persistence
{
    internal partial class DXCoreRepository : IDXUnitCoreRepository, IDXStructureRepository, IDXEnumCoreRepository, IDXStructureRawReader, IDXElementCoreRepository, IDXRawReader
    {
        public Guid Insert(string dxModelType, DXSingleElement dxSingleDXElement)
        {
            return this.InsertOrUpdateSingleDXElementPrivate(dxModelType, dxSingleDXElement, ProcessingType.Insert);
        }

        public Guid Update(string dxModelType, DXSingleElement dxSingleDXElement)
        {
            return this.InsertOrUpdateSingleDXElementPrivate(dxModelType, dxSingleDXElement, ProcessingType.Update);
        }

        public Guid InsertOrUpdate(string dxModelType, DXSingleElement dxSingleDXElement)
        {
            return this.InsertOrUpdateSingleDXElementPrivate(dxModelType, dxSingleDXElement, ProcessingType.Update);
        }

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

                var element = BuildSingleElement(elementTypeName, record, block.Meta?.IsRequired ?? false);
                lastId = this.InsertOrUpdate(dxUnitTypeName, element);
            }

            return lastId;
        }

        private Guid InsertOrUpdateSingleDXElementPrivate(string dxModelType, DXSingleElement dxSingleDXElement, ProcessingType processingType)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(dxModelType);
            ArgumentNullException.ThrowIfNull(dxSingleDXElement);

            return this.RunRequestInTransaction((conn) =>
            {
                var dataSet = new DataSet(dxSingleDXElement.Name);

                var id = this.InsertOrUpdatedxSingleItemToDataSet(
                    dxSingleDXElement,
                    dxModelType,
                    dxSingleDXElement.Item.DXUnitID,
                    dataSet,
                    conn,
                    processingType);

                dataSet.AcceptChanges();

                return id;
            });
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

                var dxModelBuilder = this._queryHelper.GetDbCommandBuilder(dxModelAdapter);

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

        public DXSingleElement? GetItem(DXTableDefinition container, Guid id)
        {
            var dxFilter = $"ID = '{id}'";

            var result = GetItems(container, dxFilter);

            return result?.SingleOrDefault();
        }

        public IEnumerable<DXSingleElement> GetItems(DXTableDefinition container, string dxFilter)
        {
            if (container == null)
                return null;

            string typeName = container.Type;
            var ids = this.GetItemIDs(typeName, dxFilter);

            if (ids.Count() == 0)
                return Enumerable.Empty<DXSingleElement>();

            var sqlWhereClause = this.GetWhereExpressionForID(ids);

            var result = this.RunRequest((conn) =>
            {
                DataSet dataSet = new DataSet(container.Type);

                this.PopulateTableToDataSet(conn, dataSet, container.Type,
                    columns: container.GetColumns(),
                    dxFilter: sqlWhereClause, fillSchema: false);

                var dataTable = dataSet.Tables[container.Type];

                var result = dataSet.Tables[container.Type].Rows.Cast<DataRow>().Select(dataRow => new DXSingleElement(
                            container.Type,
                            new DXElementAttribute(container.Type),
                            this.GetDXItem(dataRow, container),
                            container.IsRequired)).ToList();

                return result;
            });

            return result;
        }

        private static DXSingleElement BuildSingleElement(string elementTypeName, DXElementRecord record, bool isRequired)
        {
            if (string.IsNullOrWhiteSpace(elementTypeName))
                throw new ArgumentException("Element type name is required.", nameof(elementTypeName));

            var dxUnitId = record.DXUnitID;
            if (dxUnitId == Guid.Empty)
                throw new ArgumentException("DXUnitID is required for DXElementRecord.", nameof(record));

            var content = ConvertFields(record.Fields);
            var item = new DXItem(elementTypeName, record.ID, dxUnitId, record.TimeStamp, content);

            return new DXSingleElement(elementTypeName, new DXElementAttribute(elementTypeName), item, isRequired);
        }

        private static Dictionary<string, object> ConvertFields(IDictionary<string, JToken>? fields)
        {
            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (fields == null || fields.Count == 0)
                return result;

            foreach (var kvp in fields)
            {
                result[kvp.Key] = ConvertTokenToObject(kvp.Value);
            }

            return result;
        }

        private static object? ConvertTokenToObject(JToken? token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;

            return token.ToObject<object>();
        }
    }
}
