using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Models;
using IV.DX.Application.Helpers;
using IV.DX.Kernel;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace IV.DX.Application.Services
{
    internal class DXQueryResultProvider(
        IDXUnitDataReader dataReader,
        IDXRawReader dxRawReader,
        IDXStructureCache dxStructureCache,
        IDXUnitTypeAccessChecker unitTypeAccessChecker,
        IDXUnitGenericRepository genericRepo,
        IDXExecutionContextAccessor executionContextAccessor,
        ILogger<DXQueryResultProvider> logger) : IDXQueryResultProvider
    {
        public async Task<JObject?> GetAsync(Guid dxQueryID, CancellationToken ct = default)
        {
            DXQueryUnit? dxQuery;

            if (executionContextAccessor.Current == null)
            {
                using var _ = executionContextAccessor.BeginScope(new DXExecutionContext
                {
                    SubjectId = "system:query-definition-read",
                    IsSystem = true
                });

                dxQuery = await dataReader.GetItemAsync<DXQueryUnit>(dxQueryID);
            }
            else
            {
                unitTypeAccessChecker.EnsureAccess(nameof(DXQueryUnit), DXUnitTypeAccessOperation.Read);
                dxQuery = await dataReader.GetItemAsync<DXQueryUnit>(dxQueryID);
            }

            if (dxQuery == null)
            {
                logger.LogWarning("DX query {QueryId} was not found.", dxQueryID);
                return null;
            }

            JObject jObject = new JObject();
        
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
            var unitName = dxStructureCache.DXUnits.FirstOrDefault(x => x.ID == dxQuery.DXUnitDefinition)?.Name;

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

            var block = GetScopedRawBlock(unitName!, columns, dxQuery.FilterExpression);
            MaskSensitiveColumns(block, unitName!, columns);

            return new JProperty("Content", JObject.FromObject(block));
        }

        public Task<IEnumerable<DXTitleExpression>> GetDXTitleExpressionsAsync(string typeName, CancellationToken ct = default)
        {
            DXObjectDefinitionUnit? dxObjectInfo;

            dxObjectInfo = dxStructureCache.GetDXUnit(typeName);

            if (dxObjectInfo == null)
                dxObjectInfo = dxStructureCache.GetDXElement(typeName);

            if (dxObjectInfo == null)
                dxObjectInfo = dxStructureCache.GetDXEnum(typeName);

            if (dxObjectInfo == null)
            {
                logger.LogError("Display values requested for unknown DX type {TypeName}.", typeName);
                throw new Exception($"There are no type '{typeName}' to provide display values");
            }

            var DXTitleExpressionExpression =
                string.IsNullOrEmpty(dxObjectInfo.DXTitleExpression) ?
                "ID" :
                dxObjectInfo.DXTitleExpression;

            var columns = new Dictionary<string, string>()
            {
                {Constants.ID, Constants.ID },
                {Constants.TimeStamp, Constants.TimeStamp },
                {"DXTitleExpression",  DXTitleExpressionExpression }
            };

            var result = GetScopedRawBlock(typeName, columns);
            var records = result.Data?.Items ?? new List<DXUnitRecord>();

            // Display values can point to arbitrary expressions; mask only when it maps to a sensitive column.
            MaskSensitiveColumns(result, typeName, columns);

            var DXTitleExpressions = records.Select(x => new DXTitleExpression()
            {
                ID = x.ID,
                Type = typeName,
                Expression = x.Fields != null && x.Fields.TryGetValue("DXTitleExpression", out var v)
                    ? v?.ToString() ?? string.Empty
                    : string.Empty
            }).ToList();

            return Task.FromResult<IEnumerable<DXTitleExpression>>(DXTitleExpressions);
        }

        private DXDataBlock<DXUnitRecord> GetScopedRawBlock(string typeName, IDictionary<string, string> columns, string? dxFilter = null)
        {
            var decision = unitTypeAccessChecker.CheckAccess(typeName, DXUnitTypeAccessOperation.Read);
            var publicFallback = ShouldApplyAnonymousPublicFallback(decision);

            if (decision == DXAccessDecision.Denied && !publicFallback)
                ThrowDenied(typeName, DXUnitTypeAccessOperation.Read);

            if (decision == DXAccessDecision.Allowed)
                return dxRawReader.Get(typeName, columns, dxFilter);

            var scopedIds = CollectReadableIds(typeName, executionContextAccessor.Current);
            if (scopedIds.Count == 0)
                return BuildEmptyBlock(typeName);

            var scopedFilter = BuildIdInFilter(scopedIds, dxFilter);

            using var _ = executionContextAccessor.BeginScope(new DXExecutionContext
            {
                SubjectId = "system:query-scoped-read",
                IsSystem = true
            });

            return dxRawReader.Get(typeName, columns, scopedFilter);
        }

        private HashSet<Guid> CollectReadableIds(string typeName, DXExecutionContext? context)
        {
            var result = new HashSet<Guid>();
            var unitDef = dxStructureCache.GetDXUnit(typeName);

            if (unitDef == null)
                return result;

            if (unitDef.SupportsOwnership && context?.IdentityID.HasValue == true)
            {
                var identityOwned = genericRepo.GetDXUnits<DXIdentityOwnershipUnit>(
                    $"Identity = '{context.IdentityID.Value}' AND DXUnitDefinition = '{unitDef.ID}'");

                foreach (var o in identityOwned)
                    result.Add(o.OwnedDXUnitID);
            }

            if (unitDef.SupportsOwnership && context?.ActiveGroupIDs != null)
            {
                foreach (var groupId in context.ActiveGroupIDs)
                {
                    var groupOwned = genericRepo.GetDXUnits<DXGroupOwnershipUnit>(
                        $"Group = '{groupId}' AND DXUnitDefinition = '{unitDef.ID}'");

                    foreach (var o in groupOwned)
                        result.Add(o.OwnedDXUnitID);
                }
            }

            var publicAccess = genericRepo.GetDXUnits<DXPublicAccessUnit>(
                $"DXUnitDefinition = '{unitDef.ID}'");

            foreach (var access in publicAccess)
            {
                if (access.PublicDXUnitID != Guid.Empty)
                    result.Add(access.PublicDXUnitID);
            }

            return result;
        }

        private static DXDataBlock<DXUnitRecord> BuildEmptyBlock(string typeName)
        {
            return new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXUnit",
                    Type = typeName,
                    Op = "Sync",
                    IsMulti = true
                },
                Data = new DXData<DXUnitRecord>
                {
                    Items = new List<DXUnitRecord>()
                }
            };
        }

        private static string BuildIdInFilter(IReadOnlyCollection<Guid> ids, string? originalFilter)
        {
            var inList = string.Join(",", ids.Select(x => $"'{x}'"));
            var idIn = $"ID IN ({inList})";
            return string.IsNullOrWhiteSpace(originalFilter)
                ? idIn
                : $"({idIn}) AND ({originalFilter})";
        }

        private bool ShouldApplyAnonymousPublicFallback(DXAccessDecision decision)
        {
            return decision == DXAccessDecision.Denied && executionContextAccessor.Current == null;
        }

        private void ThrowDenied(string typeName, DXUnitTypeAccessOperation operation)
        {
            var subject = GetCurrentSubject();
            logger.LogWarning(
                "Query access denied for subject {Subject} to DX type {TypeName} and operation {Operation}.",
                subject,
                typeName,
                operation);

            throw new UnauthorizedAccessException($"Access denied for '{subject}' to '{typeName}' ({operation}).");
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

        private string GetCurrentSubject()
        {
            var context = executionContextAccessor.Current;
            return context == null || string.IsNullOrWhiteSpace(context.SubjectId)
                ? "anonymous"
                : context.SubjectId;
        }
    }
}

