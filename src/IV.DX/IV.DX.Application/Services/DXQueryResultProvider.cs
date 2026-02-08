using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Models;
using IV.DX.Application.Helpers;
using IV.DX.Application.PrivateModels.DXQueryUnit;
using IV.DX.Kernel;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IV.DX.Application.Services
{
    internal class DXQueryResultProvider(IDXUnitDataService dataService, IDXRawReader dxRawReader, IDXStructureCache dxStructureCache) : IDXQueryResultProvider
    {
        public async Task<JObject> GetAsync(Guid dxQueryID, Guid? dxFilterID, CancellationToken ct = default)
        {
            var dxQuery = await dataService.GetItemAsync<DXQueryUnit>(dxQueryID);

            if (dxQuery == null)
                return null;

            JObject jObject = new JObject();

            jObject.Add(new JProperty(Constants.SystemPropertyTypeName, dxQuery.DXUnitName));
            jObject.Add(this.GetDataDefintion(dxQuery));
            jObject.Add(this.GetContent(dxQuery));

            return jObject;
        }

        private JProperty GetDataDefintion(DXQueryUnit dxQuery)
        {
            List<DXQueryColumnElement> list = new List<DXQueryColumnElement>()
            {
                new DXQueryColumnElement()
                {
                    Name = Constants.ID,
                    Expression = Constants.ID,
                    Order = -1
                }
            };

            var orderedColumns = list.Concat(dxQuery.DXQueryColumnElement.Announced.OrderBy(x => x.Order));

            var propsToIgnore = new[] { Constants.ID, Constants.DXUnitID, Constants.TimeStamp };

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new IgnorePropertiesResolver(propsToIgnore)
            };

            var serializer = JsonSerializer.Create(settings);

            return new JProperty("QueryDefinition", JArray.FromObject(orderedColumns, serializer));
        }

        private JProperty GetContent(DXQueryUnit dxQuery)
        {
            var orderedColumns = dxQuery.DXQueryColumnElement.Announced.OrderBy(x => x.Order);

            var columns = orderedColumns.ToDictionary(x => x.Name, x => x.Expression);

            if (!columns.ContainsKey(Constants.ID))
            {
                columns.Add(Constants.ID, Constants.ID);
            }

            if (!columns.ContainsKey(Constants.TimeStamp))
            {
                columns.Add(Constants.TimeStamp, Constants.TimeStamp);
            }

            var block = dxRawReader.Get(dxQuery.DXUnitName, columns);
            MaskSensitiveColumns(block, dxQuery.DXUnitName, columns);

            return new JProperty("Content", JObject.FromObject(block));
        }

        public async Task<IEnumerable<DXDisplayValue>> GetDisplayValuesAsync(string typeName, CancellationToken ct = default)
        {
            DXObjectDefinitionUnit dxObjectInfo;

            dxObjectInfo = dxStructureCache.GetDXUnit(typeName);

            if (dxObjectInfo == null)
            {
                dxObjectInfo = dxStructureCache.GetDXElement(typeName);

                if (dxObjectInfo == null)
                {
                    dxObjectInfo = dxStructureCache.GetDXEnum(typeName);

                    throw new Exception($"There are no type '{typeName}' to provide display values");
                }
            }

            var displayValueExpression =
                string.IsNullOrEmpty(dxObjectInfo.DisplayValue) ?
                "ID" :
                dxObjectInfo.DisplayValue;

            var columns = new Dictionary<string, string>()
            {
                {Constants.ID, Constants.ID },
                {Constants.TimeStamp, Constants.TimeStamp },
                {"DisplayValue",  displayValueExpression }
            };

            var result = dxRawReader.Get(typeName, columns);
            var records = result.Data?.Items ?? new List<DXUnitRecord>();

            // Display values can point to arbitrary expressions; mask only when it maps to a sensitive column.
            MaskSensitiveColumns(result, typeName, columns);

            var displayValues = records.Select(x => new DXDisplayValue()
            {
                ID = x.ID,
                Type = typeName,
                DisplayValue = x.Fields != null && x.Fields.TryGetValue("DisplayValue", out var v)
                    ? v?.ToString()
                    : string.Empty
            }).ToList();

            return displayValues;
        }

        private void MaskSensitiveColumns(DXDataBlock<DXUnitRecord>? block, string? unitTypeName, IDictionary<string, string>? columns)
        {
            if (block == null || columns == null || columns.Count == 0)
                return;

            var sensitiveColumns = GetSensitiveColumnsForUnit(unitTypeName);
            if (sensitiveColumns.Count == 0)
                return;

            var toMask = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var kv in columns)
            {
                var alias = kv.Key;
                var expr = kv.Value;

                var identifier = TryGetSimpleIdentifier(expr);
                if (identifier != null && sensitiveColumns.Contains(identifier))
                {
                    toMask.Add(alias);
                }
            }

            if (toMask.Count == 0)
                return;

            var items = block.Data?.Items;
            if (items == null)
                return;

            foreach (var record in items)
            {
                if (record?.Fields == null) continue;

                foreach (var col in toMask)
                {
                    if (record.Fields.TryGetValue(col, out var token) && token != null && token.Type != JTokenType.Null)
                    {
                        record.Fields[col] = JToken.FromObject(string.Empty);
                    }
                }
            }
        }

        private HashSet<string> GetSensitiveColumnsForUnit(string? unitTypeName)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(unitTypeName))
                return result;

            var unit = dxStructureCache.GetDXUnit(unitTypeName);
            if (unit == null)
                return result;

            var hierarchy = dxStructureCache.GetDXUnitInheritance(unit);
            foreach (var item in hierarchy.Items)
            {
                var columns = item.DXUnit?.DXColumnDefinitionElement?.Announced;
                if (columns == null) continue;

                foreach (var c in columns)
                {
                    if (c == null) continue;
                    if (c.ColumnType == DXColumnTypeEnum.HashedString || c.ColumnType == DXColumnTypeEnum.EncryptedString)
                        result.Add(c.Name);
                }
            }

            return result;
        }

        private static readonly Regex _simpleIdentifierRegex = new Regex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        private static string? TryGetSimpleIdentifier(string? expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return null;

            var e = expression.Trim();

            if (e.Length >= 2)
            {
                if ((e[0] == '"' && e[^1] == '"') || (e[0] == '[' && e[^1] == ']') || (e[0] == '`' && e[^1] == '`'))
                {
                    e = e.Substring(1, e.Length - 2).Trim();
                }
            }

            return _simpleIdentifierRegex.IsMatch(e) ? e : null;
        }
    }
}

