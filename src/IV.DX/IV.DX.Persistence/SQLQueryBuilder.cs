using IV.DX.Kernel;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using System.Data;
using System.Text;

namespace IV.DX.Persistence
{
    internal class SQLQueryBuilder(
        IDXStructureCache dxStructureCache,
        ISQLDialect sqlDialect) : ISQLQueryBuilder
    {
        private readonly ISQLDialect _sqlHelper = sqlDialect;

        public static IDictionary<string, string> AllColumns { get; } = new Dictionary<string, string>();
        public static IDictionary<string, string> BaseColumns { get; } = new Dictionary<string, string>()
        {
            {"Id","Id" },
            {"TimeStamp", "TimeStamp" }
        };

        private string FormatTableAlias(string tableName, string alias)
            => _sqlHelper.FormatTableAlias(tableName, alias);

        private string FormatColumnReference(string tableAlias, string columnName)
            => _sqlHelper.FormatColumnReference(tableAlias, columnName);

        private string FormatColumnAlias(string columnExpression, string alias)
            => _sqlHelper.FormatColumnAlias(columnExpression, alias);

        private static IReadOnlyDictionary<string, DXNode> _nodesByName =
            new Dictionary<string, DXNode>(StringComparer.Ordinal);

        private static int _version = 0;

        private static WeakReference<IDXStructureCache>? _cacheRef;

        private static readonly object _schemaLock = new();

        public string BuildSQLExpression(
            string typeName,
            IDictionary<string, string> columns,
            string? dxFilter = default)
        {
            BuildDXNodeTree();

            var queryContext = new QueryContext();

            string whereExpression = string.Empty;
            bool hasFilter = !string.IsNullOrEmpty(dxFilter);

            if (hasFilter)
            {
                whereExpression = ProcessDXFilter(typeName, dxFilter!, queryContext);
            }

            var columnExpression = ProcessDXColumns(typeName, columns, queryContext);
            var fromExpression = GetFromExpression(typeName, queryContext);

            var sb = new StringBuilder();
            sb.Append("SELECT\n")
              .Append(columnExpression)
              .Append("\nFROM\n")
              .Append(fromExpression);

            if (hasFilter)
            {
                sb.Append("WHERE\n")
                  .Append(whereExpression);
            }

            return sb.ToString();
        }

        private sealed class QueryContext
        {
            private readonly Dictionary<string, string> _aliasByPath =
                new(StringComparer.Ordinal);

            private readonly HashSet<string> _usedAliases =
                new(StringComparer.Ordinal);

            private readonly Dictionary<string, int> _nextDisambiguatorByAlias =
                new(StringComparer.Ordinal);

            private readonly HashSet<(string BasePath, string RelatedPath)> _joinSet =
                new();

            public List<JoinInstance> Joins { get; } =
                new();

            public string GetOrCreateAlias(string pathKey, DXNode schemaNode)
            {
                ArgumentNullException.ThrowIfNull(schemaNode);

                if (_aliasByPath.TryGetValue(pathKey, out var existing))
                {
                    return existing;
                }

                var candidate = schemaNode.TableAlias;
                var alias = candidate;

                if (!_usedAliases.Add(alias))
                {
                    if (!_nextDisambiguatorByAlias.TryGetValue(candidate, out var disambiguator))
                    {
                        disambiguator = 1;
                    }

                    while (true)
                    {
                        alias = $"{candidate}_{disambiguator}";
                        disambiguator++;

                        if (_usedAliases.Add(alias))
                        {
                            _nextDisambiguatorByAlias[candidate] = disambiguator;
                            break;
                        }
                    }
                }

                _aliasByPath[pathKey] = alias;
                return alias;
            }

            public void RegisterJoin(
                string basePathKey,
                DXNode baseSchemaNode,
                string baseAlias,
                string relatedPathKey,
                DXNode relatedSchemaNode,
                string relatedAlias,
                JoinSpec? joinSpec = null)
            {
                if (_joinSet.Add((basePathKey, relatedPathKey)))
                {
                    Joins.Add(new JoinInstance(
                        basePathKey,
                        baseSchemaNode,
                        baseAlias,
                        relatedPathKey,
                        relatedSchemaNode,
                        relatedAlias,
                        joinSpec));
                }
            }
        }

        private readonly record struct JoinInstance(
            string BasePathKey,
            DXNode BaseSchemaNode,
            string BaseAlias,
            string RelatedPathKey,
            DXNode RelatedSchemaNode,
            string RelatedAlias,
            JoinSpec? JoinSpec);

        private static string FormatAvailableRelations(DXNode node, int maxPerGroup = 20)
        {
            static string FormatList(IReadOnlyList<string> items, int max)
            {
                if (items.Count == 0)
                {
                    return "<none>";
                }

                if (items.Count <= max)
                {
                    return string.Join(", ", items);
                }

                return $"{string.Join(", ", items.Take(max))}, ... (+{items.Count - max} more)";
            }

            var relations = node.GetRelations()
                .Select(x => new { Name = x.RelationName, TargetKind = x.TargetNode.Kind })
                .ToList();

            if (relations.Count == 0)
            {
                return "<none>";
            }

            var directProperties = relations
                .Where(x => x.TargetKind == DXNodeKind.DXProperty)
                .Select(x => x.Name)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();

            var unitElements = relations
                .Where(x => x.TargetKind == DXNodeKind.DXElement
                    && !x.Name.StartsWith("U2E(", StringComparison.Ordinal))
                .Select(x => x.Name)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();

            var relatedUnitsU2U = relations
                .Where(x => x.Name.StartsWith("U2U(", StringComparison.Ordinal))
                .Select(x => x.Name)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();

            var relatedElementsU2E = relations
                .Where(x => x.Name.StartsWith("U2E(", StringComparison.Ordinal))
                .Select(x => x.Name)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();

            var included = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in directProperties) included.Add(item);
            foreach (var item in unitElements) included.Add(item);
            foreach (var item in relatedUnitsU2U) included.Add(item);
            foreach (var item in relatedElementsU2E) included.Add(item);

            var other = relations
                .Select(x => x.Name)
                .Where(x => !included.Contains(x))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();

            var lines = new List<string>(capacity: 5)
            {
                $"Own properties: {FormatList(directProperties, maxPerGroup)}",
                $"Related DXElements: {FormatList(unitElements, maxPerGroup)}",
                $"Related DXUnits (U2U): {FormatList(relatedUnitsU2U, maxPerGroup)}",
                $"Related DXElements (U2E): {FormatList(relatedElementsU2E, maxPerGroup)}"
            };

            if (other.Count > 0)
            {
                lines.Add($"Other: {FormatList(other, maxPerGroup)}");
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static DXNode GetRelatedNodeOrThrow(
            DXNode startSchemaNode,
            string relationValue,
            string typeName,
            string expression,
            string expressionAlias,
            int segmentIndex,
            int segmentCount)
        {
            if (startSchemaNode.TryGetRelation(relationValue, out var relatedPair) && relatedPair.Value is not null)
            {
                return relatedPair.Value;
            }

            throw new InvalidOperationException(
                $"Invalid DX expression '{expression}' (alias '{expressionAlias}') for type '{typeName}': " +
                $"cannot resolve relation '{relationValue}' from '{startSchemaNode.Name}' at segment {segmentIndex}/{segmentCount}. " +
                $"{Environment.NewLine}Available relations:{Environment.NewLine}{FormatAvailableRelations(startSchemaNode)}");
        }

        private void BuildDXNodeTree()
            => BuildDXNodeTree(force: false);

        private void BuildDXNodeTree(bool force)
        {
            lock (_schemaLock)
            {
                if (dxStructureCache.DXUnits.Count == 0
                    && dxStructureCache.DXElements.Count == 0
                    && dxStructureCache.DXEnums.Count == 0)
                {
                    dxStructureCache.RefreshAsync().GetAwaiter().GetResult();
                }

                var cacheChanged =
                    _cacheRef == null
                    || !_cacheRef.TryGetTarget(out var cached)
                    || !ReferenceEquals(cached, dxStructureCache);

                if (!force && !cacheChanged && dxStructureCache.Version <= _version)
                    return;

                Load(
                    dxStructureCache.DXRelations,
                    dxStructureCache.DXUnits,
                    dxStructureCache.DXElements,
                    dxStructureCache.DXEnums);

                _cacheRef = new WeakReference<IDXStructureCache>(dxStructureCache);
            }
        }

        private DXNode GetNode(string name)
        {
            if (_nodesByName.TryGetValue(name, out var node))
                return node;

            BuildDXNodeTree(force: true);

            if (_nodesByName.TryGetValue(name, out node))
                return node;

            dxStructureCache.RefreshAsync().GetAwaiter().GetResult();
            BuildDXNodeTree(force: true);

            if (_nodesByName.TryGetValue(name, out node))
                return node;

            throw new KeyNotFoundException($"The given key '{name}' was not present in the dictionary.");
        }

        private void Load(
            IEnumerable<DXRelationDefinitionUnit> dxRelations,
            IEnumerable<DXUnitDefinitionUnit> dxUnits,
            IEnumerable<DXElementDefinitionUnit> dxElements,
            IEnumerable<DXEnumDefinitionUnit> dxEnums)
        {
            var unitsList = dxUnits as IList<DXUnitDefinitionUnit> ?? dxUnits.ToList();
            var elementsList = dxElements as IList<DXElementDefinitionUnit> ?? dxElements.ToList();
            var enumsList = dxEnums as IList<DXEnumDefinitionUnit> ?? dxEnums.ToList();
            var relationsList = dxRelations as IList<DXRelationDefinitionUnit> ?? dxRelations.ToList();

            var unitsById = unitsList.ToDictionary(x => x.Id);
            var elementsById = elementsList.ToDictionary(x => x.Id);
            var enumsById = enumsList.ToDictionary(x => x.Id);

            var commonElementNames = elementsList
                .Where(x => x.IsCommon)
                .Select(x => x.Name)
                .ToHashSet(StringComparer.Ordinal);

            var relationsByLeft = relationsList
                .ToLookup(r => r.ObjectNameLeft);

            var nodesByName = new Dictionary<string, DXNode>(StringComparer.Ordinal);

            static void RegisterNode(
                DXNode node,
                IDictionary<string, DXNode> byName,
                bool registerByName)
            {
                if (registerByName)
                {
                    byName[node.Name] = node;
                }
            }

            int counter = 0;

            // 1. Units
            foreach (var dxUnit in unitsList)
            {
                var node = new DXNode(new DXNodeKey(counter++), dxUnit.Name, DXNodeKind.DXUnit);
                RegisterNode(node, nodesByName, registerByName: true);
            }

            // 2. Elements
            foreach (var dxElement in elementsList)
            {
                var node = new DXNode(new DXNodeKey(counter++), dxElement.Name, DXNodeKind.DXElement);
                RegisterNode(node, nodesByName, registerByName: true);
            }

            // 3. Enums
            foreach (var dxEnum in enumsList)
            {
                var node = new DXNode(new DXNodeKey(counter++), dxEnum.Name, DXNodeKind.DXElement);
                RegisterNode(node, nodesByName, registerByName: true);
            }

            // 4.1. Register DXUnits columns as DXNodes
            foreach (var dxUnit in unitsList)
            {
                var dxUnitName = dxUnit.Name;
                var dxNode = GetNodeByName(dxUnitName);

                foreach (var dxColumn in dxUnit.DXColumnDefinitionElement.Announced)
                {
                    var dxNodeRelated = new DXNode(new DXNodeKey(counter++), dxColumn.Name, DXNodeKind.DXProperty);
                    RegisterNode(dxNodeRelated, nodesByName, registerByName: false);

                    var dxNodeRelation = new DXNodeRelation(dxColumn.Name, dxColumn.Name, null);
                    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }

                // DXColumnDefinitionElement already provide all columns of all kind of relations.
                //foreach (var dxEnumColumn in dxUnit.DXObjectEnumElement.Announced)
                //{
                //    var dxNodeRelated = new DXNode(new DXNodeKey(counter++), dxEnumColumn.Name, DXNodeKind.DXProperty);
                //    RegisterNode(dxNodeRelated, nodesByName, registerByName: false);

                //    var dxNodeRelation = new DXNodeRelation(dxEnumColumn.Name, dxEnumColumn.Name, null);
                //    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                //}
            }

            // 4.2. Register DXElements columns as DXNodes
            foreach (var dxElement in elementsList)
            {
                var dxElementName = dxElement.Name;
                var dxNode = GetNodeByName(dxElementName);

                foreach (var dxColumn in dxElement.DXColumnDefinitionElement.Announced)
                {
                    var dxNodeRelated = new DXNode(new DXNodeKey(counter++), dxColumn.Name, DXNodeKind.DXProperty);
                    RegisterNode(dxNodeRelated, nodesByName, registerByName: false);

                    var dxNodeRelation = new DXNodeRelation(dxColumn.Name, dxColumn.Name, null);
                    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }

                // DXColumnDefinitionElement already provide all columns of all kind of relations.
                //foreach (var dxEnumColumn in dxElement.DXObjectEnumElement.Announced)
                //{
                //    var dxNodeRelated = new DXNode(new DXNodeKey(counter++), dxEnumColumn.Name, DXNodeKind.DXProperty);
                //    RegisterNode(dxNodeRelated, nodesByName, registerByName: false);

                //    var dxNodeRelation = new DXNodeRelation(dxEnumColumn.Name, dxEnumColumn.Name, null);
                //    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                //}
            }

            // 4.3. Register DXEnums columns as DXNodes
            foreach (var dxEnum in enumsList)
            {
                var dxElementName = dxEnum.Name;
                var dxNode = GetNodeByName(dxElementName);

                foreach (var dxColumn in dxEnum.DXColumnDefinitionElement.Announced)
                {
                    var dxNodeRelated = new DXNode(new DXNodeKey(counter++), dxColumn.Name, DXNodeKind.DXProperty);
                    RegisterNode(dxNodeRelated, nodesByName, registerByName: false);

                    var dxNodeRelation = new DXNodeRelation(dxColumn.Name, dxColumn.Name, null);
                    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }

                // DXColumnDefinitionElement already provide all columns of all kind of relations.
                //foreach (var dxEnumColumn in dxEnum.DXObjectEnumElement.Announced)
                //{
                //    var dxNodeRelated = new DXNode(new DXNodeKey(counter++), dxEnumColumn.Name, DXNodeKind.DXProperty);
                //    RegisterNode(dxNodeRelated, nodesByName, registerByName: false);

                //    var dxNodeRelation = new DXNodeRelation(dxEnumColumn.Name, dxEnumColumn.Name, null);
                //    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                //}
            }

            // 4.4. Register custom properties that not defined (Relation between DX Elements for example.)
            // There are only 2 case in core data structure.
            // Need to find solution
            var customProps = new Dictionary<string, string[]>()
            {
                {"DXObjectEnumElement", new[]{  "EnumKey", "EnumType" } },
                {"DXElementInUnitDefinitionElement", new[] { "DXElementDefinitionUnit" } },
                {"DXUnitToUnitRelationElement", new[]{ "TargetDXUnit" } }
            };

            foreach (var item in customProps)
            {
                if (!nodesByName.TryGetValue(item.Key, out var dxNode))
                {
                    continue;
                }

                foreach (var column in item.Value)
                {
                    var dxNodeRelated = new DXNode(new DXNodeKey(counter++), column, DXNodeKind.DXProperty);
                    RegisterNode(dxNodeRelated, nodesByName, registerByName: false);
                    var dxNodeRelation = new DXNodeRelation(column, column, null);
                    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }
            }

            DXNode GetNodeByName(string name) => nodesByName[name];

            // 5. Unit ↔ Element (Containment) and Unit ↔ Unit (Relation)
            foreach (var dxUnit in unitsList)
            {
                var dxUnitName = dxUnit.Name;
                var dxNode = GetNodeByName(dxUnitName);

                // Unit → Element (Containment)
                foreach (var dxElementInUnit in dxUnit.DXElementInUnitDefinitionElement.Announced)
                {
                    var dxElement = elementsById[dxElementInUnit.DXElementDefinitionUnit];
                    var dxElementName = dxElement.Name;
                    var dxNodeRelated = GetNodeByName(dxElementName);

                    Guid? unitTypeFilter = dxElement.IsCommon ? dxUnit.Id : null;

                    var dxNodeRelationToDXElement =
                        new DXNodeRelation(
                            dxElementName,
                            dxElementName,
                            new JoinSpec
                            {
                                TargetTable      = dxElementName,
                                SourceColumn     = Constants.Id,
                                TargetColumn     = Constants.DXUnitId,
                                DXUnitTypeFilter = unitTypeFilter
                            });

                    dxNode.AttachDXNode(dxNodeRelationToDXElement, dxNodeRelated);

                    var dxNodeRelationToDXUnit =
                        new DXNodeRelation(
                            dxUnitName,
                            $"E2UIn({dxUnitName})",
                            new JoinSpec
                            {
                                TargetTable              = dxUnitName,
                                SourceColumn             = Constants.DXUnitId,
                                TargetColumn             = Constants.Id,
                                DXUnitTypeFilter         = unitTypeFilter,
                                DXUnitTypeFilterOnSource = true
                            });

                    dxNodeRelated.AttachDXNode(dxNodeRelationToDXUnit, dxNode);
                }

                // Unit → Other Unit (DXRelation)
                foreach (var DXUnitToUnitRelationElement in dxUnit.DXUnitToUnitRelationElement.Announced.Where(x => x.TargetDXUnit != x.DXUnitId))
                {
                    var dxUnitRelated = unitsById[DXUnitToUnitRelationElement.TargetDXUnit];
                    var dxUnitNameRelated = dxUnitRelated.Name;

                    var dxNodeRelated = GetNodeByName(dxUnitNameRelated);

                    var dxRelation = relationsByLeft[dxUnitName]
                        .FirstOrDefault(r => r.ObjectNameRight == dxUnitNameRelated
                            && r.RelationNameLeft == DXUnitToUnitRelationElement.OwnRelationName
                            && r.RelationNameRight == DXUnitToUnitRelationElement.TargetRelationName);

                    if (dxRelation == null)
                    {
                        continue;
                    }
                    var dxNodeRelation = new DXNodeRelation(
                        dxUnitNameRelated,
                        $"U2U({dxRelation.RelationNameRight})",
                        GetJoinSpecForDXUnitRelation(dxRelation));

                    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }

                // Unit → Element (Equal Relation)
                if (dxUnit.DXUnitToElementRelationElement != null)
                {
                    foreach (var dxUnitToElementRelationElement in dxUnit.DXUnitToElementRelationElement.Announced)
                    {
                        var dxElementRelated = elementsById[dxUnitToElementRelationElement.TargetDXElement];
                        var dxElementNameRelated = dxElementRelated.Name;

                        var dxNodeRelated = GetNodeByName(dxElementNameRelated);

                        var candidates = relationsByLeft[dxUnitName]
                            .Where(r => r.ObjectNameRight == dxElementNameRelated
                                && r.RelationNameLeft == dxUnitToElementRelationElement.OwnRelationName
                                && r.RelationNameRight == dxUnitToElementRelationElement.TargetRelationName)
                            .ToList();

                        if (candidates.Count == 0)
                        {
                            continue;
                        }

                        if (candidates.Count > 1)
                        {
                            throw new InvalidOperationException(
                                $"Sequence contains more than one matching element for pair ({dxUnitName}, {dxElementNameRelated}).");
                        }

                        var dxRelation = candidates[0];

                        var dxNodeRelation = new DXNodeRelation(
                            dxElementNameRelated,
                            $"U2E({dxRelation.RelationNameRight})",
                            GetJoinSpecForDXUnitRelation(dxRelation));

                        dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                    }
                }
            }

            // 5.1 Element → Unit (Equal Relation)
            foreach (var dxElement in elementsList)
            {
                if (dxElement.DXElementToUnitRelationElement == null)
                {
                    continue;
                }

                var dxElementName = dxElement.Name;
                var dxNode = GetNodeByName(dxElementName);

                foreach (var dxElementToUnitRelationElement in dxElement.DXElementToUnitRelationElement.Announced)
                {
                    var dxUnitRelated = unitsById[dxElementToUnitRelationElement.TargetDXUnit];
                    var dxUnitNameRelated = dxUnitRelated.Name;

                    var dxNodeRelated = GetNodeByName(dxUnitNameRelated);

                    var candidates = relationsByLeft[dxElementName]
                        .Where(r => r.ObjectNameRight == dxUnitNameRelated
                            && r.RelationNameLeft == dxElementToUnitRelationElement.OwnRelationName
                            && r.RelationNameRight == dxElementToUnitRelationElement.TargetRelationName)
                        .ToList();

                    if (candidates.Count == 0)
                    {
                        continue;
                    }

                    if (candidates.Count > 1)
                    {
                        throw new InvalidOperationException(
                            $"Sequence contains more than one matching element for pair ({dxElementName}, {dxUnitNameRelated}).");
                    }

                    var dxRelation = candidates[0];

                    var dxNodeRelation = new DXNodeRelation(
                        dxUnitNameRelated,
                        $"E2U({dxRelation.RelationNameRight})",
                        GetJoinSpecForDXUnitRelation(dxRelation));

                    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }
            }

            // 6. Inheritance Units
            foreach (var dxUnit in unitsList)
            {
                if (!dxUnit.BaseDXUnit.HasValue)
                {
                    continue;
                }

                var dxUnitName = dxUnit.Name;
                var dxNode = GetNodeByName(dxUnitName);

                var baseDXUnit = unitsById[dxUnit.BaseDXUnit.Value];
                var dxNodeForBaseDXUnit = GetNodeByName(baseDXUnit.Name);

                // Use a dedicated base node instance per derived unit to avoid alias collisions when the same
                // base table is joined through different traversal paths in a single query.
                var baseDXNodeForThisDerived = dxNodeForBaseDXUnit.CloneWithNewKey(new DXNodeKey(counter++));
                RegisterNode(baseDXNodeForThisDerived, nodesByName, registerByName: false);

                dxNode.SetBaseDXNode(baseDXNodeForThisDerived);

                foreach (var item in dxNodeForBaseDXUnit.DXNodes.Where(x =>
                    x.Value.Kind == DXNodeKind.DXElement &&
                    !x.Key.RelationName.StartsWith("U2E(", StringComparison.Ordinal)))
                {
                    // JoinSpec contains no aliases — copy directly, no substitution needed
                    var relation = new DXNodeRelation(
                        item.Key.TargetObjectName,
                        item.Key.RelationName,
                        item.Key.JoinSpec);

                    dxNode.AttachDXNode(relation, item.Value);

                    if (item.Value.Kind == DXNodeKind.DXElement)
                    {
                        // E2UIn: get DXUnitTypeFilter from the copied JoinSpec (no regex needed)
                        var dxNodeRelationToDXUnit = new DXNodeRelation(
                            dxUnitName,
                            $"E2UIn({dxUnitName})",
                            new JoinSpec
                            {
                                TargetTable              = dxUnitName,
                                SourceColumn             = Constants.DXUnitId,
                                TargetColumn             = Constants.Id,
                                DXUnitTypeFilter         = item.Key.JoinSpec?.DXUnitTypeFilter,
                                DXUnitTypeFilterOnSource = true
                            });

                        item.Value.AttachDXNode(dxNodeRelationToDXUnit, dxNode);
                    }
                }
            }

            // 7. Process self related DXUnits
            foreach (var dxUnit in unitsList)
            {
                var dxUnitName = dxUnit.Name;
                var dxNode = GetNodeByName(dxUnitName);

                var selfRelatedRelations = dxUnit.DXUnitToUnitRelationElement.Announced.Where(x => x.TargetDXUnit == x.DXUnitId);

                if (selfRelatedRelations.Count() > 0)
                {
                    var dxRelationsByLeft = relationsByLeft[dxUnitName]
                        .Where(r => r.ObjectNameRight == dxUnitName);

                    var clones = dxNode.Clone(dxRelationsByLeft);

                    foreach (var clone in clones)
                    {
                        RegisterNode(clone, nodesByName, registerByName: false);
                    }
                }
            }

            // 8. Register nodes which started from Relation table in Many To Many relation.
            foreach (var item in dxRelations.Where(x => x.RelationType == DXRelationTypeEnum.ManyToMany))
            {
                var dxNode = new DXNode(new DXNodeKey(counter++), item.RelationTable!, DXNodeKind.DXManyToManyTable);
                RegisterNode(dxNode, nodesByName, registerByName: true);
            }

            _nodesByName = nodesByName;
            _version = dxStructureCache.Version;
        }

        private DXNodeRelation GetRelation(DXNode baseDXNode, DXNode relatedDXNode)
        {
            return baseDXNode.GetRelationTo(relatedDXNode.Key);
        }

        private string ProcessDXColumns(
            string typeName,
            IDictionary<string, string> columns,
            QueryContext queryContext)
        {
            var coreDXNode = GetNode(typeName);
            var corePathKey = typeName;
            var coreAlias = queryContext.GetOrCreateAlias(corePathKey, coreDXNode);

            var columnsExpressionItems = new List<string>();

            if (columns.Count() == 0)
                return $"{_sqlHelper.QuoteIdentifier(coreAlias)}.*";

            if (columns is not null)
            {
                foreach (var column in columns)
                {
                    var alias = column.Key;
                    var expression = column.Value;
                    if (string.IsNullOrWhiteSpace(expression))
                    {
                        throw new InvalidOperationException(
                            $"Column '{alias}' has an empty DX expression for type '{typeName}'.");
                    }

                    var route = expression.Split('.');
                    var startSchemaNode = coreDXNode;
                    var startPathKey = corePathKey;
                    var startAlias = coreAlias;

                    for (int i = 0; i < route.Length - 1; i++)
                    {
                        var relationValue = route[i];

                        while (!startSchemaNode.TryGetRelation(relationValue, out _))
                        {
                            var baseNode = startSchemaNode.BaseDXNode;
                            if (baseNode == null)
                                break;
                            var basePathKey = $"{startPathKey}.__base({baseNode.Name})";
                            var baseAlias = queryContext.GetOrCreateAlias(basePathKey, baseNode);
                            queryContext.RegisterJoin(startPathKey, startSchemaNode, startAlias, basePathKey, baseNode, baseAlias);
                            startSchemaNode = baseNode;
                            startPathKey = basePathKey;
                            startAlias = baseAlias;
                        }

                        var relatedNode = GetRelatedNodeOrThrow(
                            startSchemaNode,
                            relationValue,
                            typeName,
                            expression,
                            alias,
                            segmentIndex: i + 1,
                            segmentCount: route.Length - 1);

                        startSchemaNode.TryGetRelation(relationValue, out var relatedNodeRelation);

                        var relatedPathKey = $"{startPathKey}.{relationValue}";
                        var relatedAlias = queryContext.GetOrCreateAlias(relatedPathKey, relatedNode);

                        queryContext.RegisterJoin(
                            startPathKey,
                            startSchemaNode,
                            startAlias,
                            relatedPathKey,
                            relatedNode,
                            relatedAlias,
                            relatedNodeRelation.Key.JoinSpec);

                        startSchemaNode = relatedNode;
                        startPathKey = relatedPathKey;
                        startAlias = relatedAlias;
                    }

                    string columnExpressionItem;

                    var columnName = route[^1];
                    if (string.IsNullOrWhiteSpace(columnName))
                    {
                        throw new InvalidOperationException(
                            $"Invalid DX expression '{expression}' (alias '{alias}') for type '{typeName}': missing column name.");
                    }

                    if (startSchemaNode.ContainsProperty(columnName))
                    {
                        columnExpressionItem = FormatColumnAlias(
                            FormatColumnReference(startAlias, columnName),
                            alias);
                    }
                    else
                    {
                        var baseDXNode = startSchemaNode.GetBaseDXNodeWithProperty(columnName);
                        if (baseDXNode is null)
                        {
                            throw new InvalidOperationException(
                                $"Property '{columnName}' not found for type '{typeName}'.");
                        }

                        var basePathKey = $"{startPathKey}.__base({baseDXNode.Name})";
                        var baseAlias = queryContext.GetOrCreateAlias(basePathKey, baseDXNode);

                        queryContext.RegisterJoin(
                            startPathKey,
                            startSchemaNode,
                            startAlias,
                            basePathKey,
                            baseDXNode,
                            baseAlias);

                        columnExpressionItem = FormatColumnAlias(
                            FormatColumnReference(baseAlias, columnName),
                            alias);
                    }

                    columnsExpressionItems.Add(columnExpressionItem);
                }
            }

            return string.Join(",\n", columnsExpressionItems).Trim();
        }

        private string ProcessDXFilter(
            string typeName,
            string dxFilter,
            QueryContext queryContext)
        {
            var coreDXNode = GetNode(typeName);
            var corePathKey = typeName;
            var coreAlias = queryContext.GetOrCreateAlias(corePathKey, coreDXNode);

            var expressions = dxFilter.Trim()
                .SplitAndKeep(
                    new[] { " and ", " or ", " AND ", " OR ", " And ", " Or " },
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            var whereExpressionItems = new List<KeyValuePair<string, string>>();

            foreach (var expression in expressions)
            {
                var route = expression.Key.Split('.');
                var startSchemaNode = coreDXNode;
                var startPathKey = corePathKey;
                var startAlias = coreAlias;

                for (int i = 0; i < route.Length - 1; i++)
                {
                    var relationValue = route[i];

                    while (!startSchemaNode.TryGetRelation(relationValue, out _))
                    {
                        var baseNode = startSchemaNode.BaseDXNode;
                        if (baseNode == null)
                            break;
                        var basePathKey = $"{startPathKey}.__base({baseNode.Name})";
                        var baseAlias = queryContext.GetOrCreateAlias(basePathKey, baseNode);
                        queryContext.RegisterJoin(startPathKey, startSchemaNode, startAlias, basePathKey, baseNode, baseAlias);
                        startSchemaNode = baseNode;
                        startPathKey = basePathKey;
                        startAlias = baseAlias;
                    }

                    var relatedNode = GetRelatedNodeOrThrow(
                        startSchemaNode,
                        relationValue,
                        typeName,
                        expression.Key,
                        expressionAlias: "<filter>",
                        segmentIndex: i + 1,
                        segmentCount: route.Length - 1);

                    startSchemaNode.TryGetRelation(relationValue, out var relatedNodeRelation);

                    var relatedPathKey = $"{startPathKey}.{relationValue}";
                    var relatedAlias = queryContext.GetOrCreateAlias(relatedPathKey, relatedNode);

                    queryContext.RegisterJoin(
                        startPathKey,
                        startSchemaNode,
                        startAlias,
                        relatedPathKey,
                        relatedNode,
                        relatedAlias,
                        relatedNodeRelation.Key.JoinSpec);

                    startSchemaNode = relatedNode;
                    startPathKey = relatedPathKey;
                    startAlias = relatedAlias;
                }

                var whereExpressionItem = route[^1];
                if (string.IsNullOrWhiteSpace(whereExpressionItem))
                {
                    throw new InvalidOperationException(
                        $"Invalid DX filter expression '{expression.Key}' for type '{typeName}': missing condition.");
                }

                var spaceIndex = whereExpressionItem.IndexOf(' ');
                if (spaceIndex < 0)
                {
                    throw new InvalidOperationException(
                        $"Invalid DX filter expression '{expression.Key}' for type '{typeName}': missing operator/value after '{whereExpressionItem}'.");
                }
                var propertyName = whereExpressionItem.Substring(0, spaceIndex);
                var propertyValue = whereExpressionItem.Substring(spaceIndex);

                string condition;

                if (startSchemaNode.ContainsProperty(propertyName))
                {
                    condition = $"{FormatColumnReference(startAlias, propertyName)}{propertyValue}";
                }
                else
                {
                    var baseDXNode = startSchemaNode.GetBaseDXNodeWithProperty(propertyName);
                    if (baseDXNode is null)
                    {
                        throw new InvalidOperationException(
                            $"Property '{propertyName}' not found for type '{typeName}'.");
                    }

                    var basePathKey = $"{startPathKey}.__base({baseDXNode.Name})";
                    var baseAlias = queryContext.GetOrCreateAlias(basePathKey, baseDXNode);

                    queryContext.RegisterJoin(
                        startPathKey,
                        startSchemaNode,
                        startAlias,
                        basePathKey,
                        baseDXNode,
                        baseAlias);

                    condition = $"{FormatColumnReference(baseAlias, propertyName)}{propertyValue}";
                }

                whereExpressionItems.Add(
                    new KeyValuePair<string, string>(condition, expression.Value!));
            }

            var whereExpression = string.Join(
                " ",
                whereExpressionItems.Select(x => $"{x.Value} {x.Key}")).Trim();

            return whereExpression;
        }

        static JoinSpec GetJoinSpecForDXUnitRelation(DXRelationDefinitionUnit r) => r.RelationType switch
        {
            DXRelationTypeEnum.ManyToMany =>
                new JoinSpec
                {
                    TargetTable  = r.ObjectNameRight,
                    SourceColumn = Constants.Id,
                    TargetColumn = Constants.Id,
                    ViaTable     = new ManyToManySpec
                    {
                        TableName    = r.RelationTable!,
                        SourceColumn = r.RelationNameLeft,
                        TargetColumn = r.RelationNameRight
                    }
                },

            DXRelationTypeEnum.ManyToOne
                or DXRelationTypeEnum.ManyToZeroOne
                or DXRelationTypeEnum.ZeroOneToOne =>
                new JoinSpec { TargetTable = r.ObjectNameRight, SourceColumn = r.RelationNameRight, TargetColumn = Constants.Id },

            DXRelationTypeEnum.OneToMany
                or DXRelationTypeEnum.ZeroOneToMany
                or DXRelationTypeEnum.OneToZeroOne =>
                new JoinSpec { TargetTable = r.ObjectNameRight, SourceColumn = Constants.Id, TargetColumn = r.RelationNameLeft },

            DXRelationTypeEnum.ZeroOneToZeroOne =>
                r.RelationColumnNameRight == "Id"
                    ? new JoinSpec { TargetTable = r.ObjectNameRight, SourceColumn = r.RelationNameRight, TargetColumn = Constants.Id }
                    : new JoinSpec { TargetTable = r.ObjectNameRight, SourceColumn = Constants.Id,        TargetColumn = r.RelationNameLeft },

            _ => throw new Exception($"DXNode processing. There are no DXRelation type {r.RelationType}")
        };

        private string GetFromExpression(
            string typeName,
            QueryContext queryContext)
        {
            var fromExpression = new StringBuilder();

            var coreSchemaNode = GetNode(typeName);
            var corePathKey = typeName;
            var coreAlias = queryContext.GetOrCreateAlias(corePathKey, coreSchemaNode);

            fromExpression.Append($"{FormatTableAlias(typeName, coreAlias)}\n");

            foreach (var join in queryContext.Joins)
            {
                var baseSchemaNode    = join.BaseSchemaNode;
                var relatedSchemaNode = join.RelatedSchemaNode;

                var spec = join.JoinSpec
                    ?? GetRelation(baseSchemaNode, relatedSchemaNode).JoinSpec
                    ?? throw new InvalidOperationException(
                        $"Missing JOIN for '{baseSchemaNode.Name}' → '{relatedSchemaNode.Name}'.");

                if (spec.ViaTable is { } via)
                {
                    // ManyToMany: join via-table first, then target table
                    fromExpression
                        .Append("LEFT JOIN ").Append(_sqlHelper.FormatTableAlias(via.TableName, via.TableName))
                        .Append(" ON ").Append(_sqlHelper.FormatColumnReference(via.TableName, via.SourceColumn))
                        .Append(" = ").Append(_sqlHelper.FormatColumnReference(join.BaseAlias, spec.SourceColumn)).Append('\n')
                        .Append("LEFT JOIN ").Append(_sqlHelper.FormatTableAlias(spec.TargetTable, join.RelatedAlias))
                        .Append(" ON ").Append(_sqlHelper.FormatColumnReference(join.RelatedAlias, spec.TargetColumn))
                        .Append(" = ").Append(_sqlHelper.FormatColumnReference(via.TableName, via.TargetColumn)).Append('\n');
                }
                else
                {
                    fromExpression
                        .Append("LEFT JOIN ").Append(_sqlHelper.FormatTableAlias(spec.TargetTable, join.RelatedAlias))
                        .Append(" ON ").Append(_sqlHelper.FormatColumnReference(join.RelatedAlias, spec.TargetColumn))
                        .Append(" = ").Append(_sqlHelper.FormatColumnReference(join.BaseAlias, spec.SourceColumn));

                    if (spec.DXUnitTypeFilter is { } filter)
                    {
                        var filterAlias = spec.DXUnitTypeFilterOnSource ? join.BaseAlias : join.RelatedAlias;
                        fromExpression
                            .Append(" AND ").Append(_sqlHelper.FormatColumnReference(filterAlias, Constants.DXUnitType))
                            .Append($" = '{filter}'");
                    }

                    fromExpression.Append('\n');
                }
            }

            return fromExpression.ToString();
        }

        private sealed class DXNode
        {
            public DXNodeKey Key { get; }
            public string TableAlias { get; }
            public string Name { get; }
            public DXNode? OriginalNode { get; private set; }
            public DXNodeKind Kind { get; }

            public DXNode? BaseDXNode { get; set; }

            public IDictionary<DXNodeRelation, DXNode> DXNodes { get; } =
                new Dictionary<DXNodeRelation, DXNode>();

            private readonly Dictionary<string, KeyValuePair<DXNodeRelation, DXNode>> _relationsByName =
                new(StringComparer.Ordinal);

            private readonly Dictionary<DXNodeKey, DXNodeRelation> _relationsByTargetId =
                new();

            public DXNode(DXNodeKey key, string name, DXNodeKind kind)
            {
                this.Key = key;
                this.Name = name;
                this.Kind = kind;
                this.TableAlias = $"T_{Key.Id}_{Key.SubId}";
                this.OriginalNode = null;
            }

            public void AttachDXNode(DXNodeRelation dxRelation, DXNode dxNode)
            {
                DXNodes[dxRelation] = dxNode;
                _relationsByName[dxRelation.RelationName] =
                    new KeyValuePair<DXNodeRelation, DXNode>(dxRelation, dxNode);
                _relationsByTargetId[dxNode.Key] = dxRelation;
            }

            public bool TryGetRelation(
                string relationName,
                out KeyValuePair<DXNodeRelation, DXNode> relation)
            {
                return _relationsByName.TryGetValue(relationName, out relation);
            }

            public IEnumerable<string> GetRelationNames()
            {
                return _relationsByName.Keys;
            }

            public IEnumerable<(string RelationName, DXNodeRelation Relation, DXNode TargetNode)> GetRelations()
            {
                foreach (var item in _relationsByName)
                {
                    yield return (item.Key, item.Value.Key, item.Value.Value);
                }
            }

            public DXNodeRelation GetRelationTo(DXNodeKey targetId)
            {
                return _relationsByTargetId[targetId];
            }

            public void SetBaseDXNode(DXNode baseDXNode)
            {
                this.BaseDXNode = baseDXNode;

                // derived → base: derived.Id = base.Id
                var dxNodeReltionToBaseDXUnit =
                   new DXNodeRelation(
                       this.Name,
                       this.Name,
                       new JoinSpec { TargetTable = baseDXNode.Name, SourceColumn = Constants.Id, TargetColumn = Constants.Id });

                this.AttachDXNode(dxNodeReltionToBaseDXUnit, baseDXNode);

                // base → derived: derived.Id = base.Id
                var dxNodeReltionToInheritedDXUnit =
                    new DXNodeRelation(
                        baseDXNode.Name,
                        baseDXNode.Name,
                        new JoinSpec { TargetTable = this.Name, SourceColumn = Constants.Id, TargetColumn = Constants.Id });

                baseDXNode.AttachDXNode(dxNodeReltionToInheritedDXUnit, this);
            }

            public IEnumerable<DXNode> Clone(IEnumerable<DXRelationDefinitionUnit> dxRelations)
            {
                int counter = 1;

                List<DXNode> clones = new List<DXNode>();

                foreach (var dxRelation in dxRelations)
                {
                    var clone = new DXNode(new DXNodeKey(this.Key.Id, counter++), this.Name, this.Kind);

                    clone.OriginalNode = this;
                    clone.BaseDXNode = this.BaseDXNode;

                    // JoinSpec contains no aliases — copy directly
                    foreach (var item in DXNodes)
                    {
                        var dxNodeRelation = new DXNodeRelation(
                            item.Key.TargetObjectName,
                            item.Key.RelationName,
                            item.Key.JoinSpec);

                        clone.AttachDXNode(dxNodeRelation, item.Value);
                    }

                    clones.Add(clone);

                    var dxNodeRelation2 = new DXNodeRelation(
                        dxRelation.ObjectNameLeft,
                        $"U2U({dxRelation.RelationNameRight})",
                        GetJoinSpecForDXUnitRelation(dxRelation));

                    this.AttachDXNode(dxNodeRelation2, clone);
                }

                return clones;
            }

            public DXNode CloneWithNewKey(DXNodeKey key)
            {
                var clone = new DXNode(key, this.Name, this.Kind);

                clone.OriginalNode = this.OriginalNode ?? this;
                clone.BaseDXNode = this.BaseDXNode;

                // JoinSpec contains no aliases — copy directly
                foreach (var item in DXNodes)
                {
                    var dxNodeRelation = new DXNodeRelation(
                        item.Key.TargetObjectName,
                        item.Key.RelationName,
                        item.Key.JoinSpec);

                    clone.AttachDXNode(dxNodeRelation, item.Value);
                }

                return clone;
            }

            public override string ToString()
            {
                return $"{this.Kind} {this.Name}";
            }

            public bool ContainsProperty(string propertyName)
            {
                if (propertyName == Constants.Id || propertyName == Constants.TimeStamp)
                    return true;

                if (this.Kind == DXNodeKind.DXElement && propertyName == Constants.DXUnitId)
                    return true;

                if (this.Kind == DXNodeKind.DXElement && propertyName == Constants.DXUnitType)
                    return true;

                // Need to define columns for table for N to M relation
                if (this.Kind == DXNodeKind.DXManyToManyTable)
                    return true;

                return this.DXNodes.Any(x =>
                x.Key.RelationName == propertyName
                && x.Key.TargetObjectName == propertyName
                && x.Value.Kind == DXNodeKind.DXProperty);
            }

            public DXNode? GetBaseDXNodeWithProperty(string propertyName)
            {
                if (this.BaseDXNode == null)
                    return null;

                if (this.BaseDXNode.ContainsProperty(propertyName))
                {
                    return this.BaseDXNode;
                }
                else
                {
                    return this.BaseDXNode.GetBaseDXNodeWithProperty(propertyName);
                }
            }
        }

        private sealed class JoinSpec
        {
            public required string TargetTable      { get; init; }
            public required string SourceColumn     { get; init; }
            public required string TargetColumn     { get; init; }
            public Guid?           DXUnitTypeFilter { get; init; }
            public bool            DXUnitTypeFilterOnSource { get; init; }
            public ManyToManySpec? ViaTable         { get; init; }
        }

        private sealed class ManyToManySpec
        {
            public required string TableName     { get; init; }
            public required string SourceColumn  { get; init; }
            public required string TargetColumn  { get; init; }
        }

        private struct DXNodeRelation
        {
            public string   TargetObjectName { get; }
            public string   RelationName     { get; }
            public JoinSpec? JoinSpec        { get; }

            public DXNodeRelation(string targetObjectName, string relationName, JoinSpec? joinSpec)
            {
                TargetObjectName = targetObjectName;
                RelationName     = relationName;
                JoinSpec         = joinSpec;
            }
        }

        private struct DXNodeKey
        {
            public int Id { get; }
            public int SubId { get; }

            public DXNodeKey(int id)
            {
                this.Id = id;
                this.SubId = 0;
            }

            public DXNodeKey(int id, int subId)
            {
                this.Id = id;
                this.SubId = subId;
            }
        }

        private enum DXNodeKind
        {
            DXProperty,
            DXEnum,
            DXElement,
            DXUnit,
            DXBaseUnit,
            DXManyToManyTable
        }
    }
}
