using System.Data;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers.DXModelDefinitionHelpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;

namespace IV.DX.Persistence
{
    internal partial class DXCoreRepository : IDXUnitCoreRepository, IDXStructureRepository, IDXEnumCoreRepository, IDXStructureRawReader, IDXElementCoreRepository, IDXRawReader
    {
        DXDataBlock<DXEnumRecord> IDXEnumCoreRepository.GetItemsRecord(string enumType)
        {
            var modelDefinition = GetEnumModelDefinition(enumType);
            if (modelDefinition == null)
                throw new Exception($"There are no DXEnum with name '{enumType}'");

            var unitBlock = this.GetItemsRecord(modelDefinition, DXLoadingType.Full);
            return MapUnitBlockToEnumBlock(enumType, unitBlock);
        }

        bool IDXEnumCoreRepository.IsItemExisting(string typeName, Guid objectId)
        {
            var item = ((IDXEnumCoreRepository)this).GetItemRecord(typeName, objectId);
            return item != null && item.Data?.Upsert?.Count > 0;
        }

        private DXDataSetDefinition GetEnumModelDefinition(string type)
        {
            var mainDXUnit = this.GetDXEnumDefinition(type);

            if (mainDXUnit == null)
                return null;

            var modelDefinition = DXModelDefinitionHelper.BuildModelDefinition(mainDXUnit, _dxStructureCache.DXRelations);

            return modelDefinition;
        }

        DXDataBlock<DXEnumRecord>? IDXEnumCoreRepository.GetItemRecord(string typeName, Guid objectId)
        {
            var modelDefinition = GetEnumModelDefinition(typeName);
            if (modelDefinition == null)
                return null;

            var unitBlock = this.GetItemRecord(modelDefinition, objectId, DXLoadingType.Full);
            if (unitBlock == null)
                return null;

            return MapUnitBlockToEnumBlock(typeName, unitBlock);
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
                    MapRowItemToRow(item, row, enumInfo.Name);
                    table.Rows.Add(row);
                }
                else
                {
                    MapRowItemToRow(item, row, enumInfo.Name);
                }

                SaveTable(adapter, conn, dataSet, table, false);

                dataSet.AcceptChanges();
                return item.ID;
            });
        }

        private static RowItem BuildEnumItem(string enumTypeName, DXEnumRecord record)
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

            return new RowItem(enumTypeName, record.ID, record.ID, record.TimeStamp, content);
        }

        private static DXDataBlock<DXEnumRecord> MapUnitBlockToEnumBlock(string enumTypeName, DXDataBlock<DXUnitRecord> unitBlock)
        {
            var records = new List<DXEnumRecord>();

            if (unitBlock.Data?.Upsert != null)
            {
                foreach (var record in unitBlock.Data.Upsert)
                {
                    if (record == null) continue;
                    records.Add(MapUnitRecordToEnumRecord(record, enumTypeName));
                }
            }

            return new DXDataBlock<DXEnumRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXEnum",
                    Type = enumTypeName
                },
                Data = new DXData<DXEnumRecord>
                {
                    Upsert = records
                }
            };
        }

        private static DXEnumRecord MapUnitRecordToEnumRecord(DXUnitRecord record, string enumTypeName)
        {
            var result = new DXEnumRecord
            {
                ID = record.ID,
                TimeStamp = record.TimeStamp,
                Type = enumTypeName
            };

            if (record.Fields != null)
            {
                var fields = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase);
                foreach (var kvp in record.Fields)
                {
                    if (string.Equals(kvp.Key, "Key", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Key = kvp.Value;
                        continue;
                    }

                    if (string.Equals(kvp.Key, "Value", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Value = kvp.Value;
                        continue;
                    }

                    fields[kvp.Key] = kvp.Value;
                }

                result.Fields = fields.Count == 0 ? null : fields;
            }

            return result;
        }
    }
}
