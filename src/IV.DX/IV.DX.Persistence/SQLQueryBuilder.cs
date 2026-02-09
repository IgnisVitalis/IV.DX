using IV.DX.Kernel;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Abstractions;
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
            {"ID","ID" },
            {"TimeStamp", "TimeStamp" }
        };

        private string FormatTableAlias(string tableName, string alias)
            => _sqlHelper.FormatTableAlias(tableName, alias);

        private string FormatColumnReference(string tableAlias, string columnName)
            => _sqlHelper.FormatColumnReference(tableAlias, columnName);

        private string FormatColumnAlias(string columnExpression, string alias)
            => _sqlHelper.FormatColumnAlias(columnExpression, alias);

        private string FormatJoin(
            string tableName,
            string tableAlias,
            string leftAlias,
            string leftColumn,
            string rightAlias,
            string rightColumn)
        {
            var tablePart = FormatTableAlias(tableName, tableAlias);
            var left = FormatColumnReference(leftAlias, leftColumn);
            var right = FormatColumnReference(rightAlias, rightColumn);

            return $"{tablePart} ON {left} = {right}";
        }

        private static IReadOnlyDictionary<string, DXNode> _nodesByName =
            new Dictionary<string, DXNode>(StringComparer.Ordinal);

        private static IReadOnlyDictionary<DXNodeKey, DXNode> _nodesById =
            new Dictionary<DXNodeKey, DXNode>();

        private static int _version = 0;

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
                string relatedAlias)
            {
                if (_joinSet.Add((basePathKey, relatedPathKey)))
                {
                    Joins.Add(new JoinInstance(
                        basePathKey,
                        baseSchemaNode,
                        baseAlias,
                        relatedPathKey,
                        relatedSchemaNode,
                        relatedAlias));
                }
            }
        }

        private readonly record struct JoinInstance(
            string BasePathKey,
            DXNode BaseSchemaNode,
            string BaseAlias,
            string RelatedPathKey,
            DXNode RelatedSchemaNode,
            string RelatedAlias);

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
        {
            if (dxStructureCache.Version > _version)
            {
                Load(
                    dxStructureCache.DXRelations,
                    dxStructureCache.DXUnits,
                    dxStructureCache.DXElements,
                    dxStructureCache.DXEnums);
            }
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

            var unitsById = unitsList.ToDictionary(x => x.ID);
            var elementsById = elementsList.ToDictionary(x => x.ID);
            var enumsById = enumsList.ToDictionary(x => x.ID);

            var relationsByLeft = relationsList
                .ToLookup(r => r.ObjectNameLeft);

            var nodesById = new Dictionary<DXNodeKey, DXNode>();

            var nodesByName = new Dictionary<string, DXNode>(StringComparer.Ordinal);

            static void RegisterNode(
                DXNode node,
                IDictionary<DXNodeKey, DXNode> byId,
                IDictionary<string, DXNode> byName,
                bool registerByName)
            {
                byId[node.Key] = node;
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
                RegisterNode(node, nodesById, nodesByName, registerByName: true);
            }

            // 2. Elements
            foreach (var dxElement in elementsList)
            {
                var node = new DXNode(new DXNodeKey(counter++), dxElement.Name, DXNodeKind.DXElement);
                RegisterNode(node, nodesById, nodesByName, registerByName: true);
            }

            // 3. Enums
            foreach (var dxEnum in enumsList)
            {
                var node = new DXNode(new DXNodeKey(counter++), dxEnum.Name, DXNodeKind.DXElement);
                RegisterNode(node, nodesById, nodesByName, registerByName: true);
            }

            // 4.1. Register DXUnits columns as DXNodes
            foreach (var dxUnit in unitsList)
            {
                var dxUnitName = dxUnit.Name;
                var dxNode = GetNodeByName(dxUnitName);

                foreach (var dxColumn in dxUnit.DXColumnDefinitionElement.Announced)
                {
                    var dxNodeRelated = new DXNode(new DXNodeKey(counter++), dxColumn.Name, DXNodeKind.DXProperty);
                    RegisterNode(dxNodeRelated, nodesById, nodesByName, registerByName: false);

                    var dxNodeRelation = new DXNodeRelation(dxColumn.Name, dxColumn.Name, null);
                    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }

                // DXColumnDefinitionElement already provide all columns of all kind of relations.
                //foreach (var dxEnumColumn in dxUnit.DXObjectEnumElement.Announced)
                //{
                //    var dxNodeRelated = new DXNode(new DXNodeKey(counter++), dxEnumColumn.Name, DXNodeKind.DXProperty);
                //    RegisterNode(dxNodeRelated, nodesById, nodesByName, registerByName: false);

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
                    RegisterNode(dxNodeRelated, nodesById, nodesByName, registerByName: false);

                    var dxNodeRelation = new DXNodeRelation(dxColumn.Name, dxColumn.Name, null);
                    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }

                // DXColumnDefinitionElement already provide all columns of all kind of relations.
                //foreach (var dxEnumColumn in dxElement.DXObjectEnumElement.Announced)
                //{
                //    var dxNodeRelated = new DXNode(new DXNodeKey(counter++), dxEnumColumn.Name, DXNodeKind.DXProperty);
                //    RegisterNode(dxNodeRelated, nodesById, nodesByName, registerByName: false);

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
                    RegisterNode(dxNodeRelated, nodesById, nodesByName, registerByName: false);

                    var dxNodeRelation = new DXNodeRelation(dxColumn.Name, dxColumn.Name, null);
                    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }

                // DXColumnDefinitionElement already provide all columns of all kind of relations.
                //foreach (var dxEnumColumn in dxEnum.DXObjectEnumElement.Announced)
                //{
                //    var dxNodeRelated = new DXNode(new DXNodeKey(counter++), dxEnumColumn.Name, DXNodeKind.DXProperty);
                //    RegisterNode(dxNodeRelated, nodesById, nodesByName, registerByName: false);

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
                var dxNode = GetNodeByName(item.Key);

                foreach (var column in item.Value)
                {
                    var dxNodeRelated = new DXNode(new DXNodeKey(counter++), column, DXNodeKind.DXProperty);
                    RegisterNode(dxNodeRelated, nodesById, nodesByName, registerByName: false);
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

                    var dxNodeRelationToDXElement =
                        new DXNodeRelation(
                            dxElementName,
                            dxElementName,
                            FormatJoin(
                                dxElementName,
                                dxNodeRelated.TableAlias,
                                dxNodeRelated.TableAlias,
                                Constants.DXUnitID,
                                dxNode.TableAlias,
                                Constants.ID));

                    dxNode.AttachDXNode(dxNodeRelationToDXElement, dxNodeRelated);

                    var dxNodeRelationToDXUnit =
                        new DXNodeRelation(
                            dxUnitName,
                            $"E2UIn({dxUnitName})",
                            FormatJoin(
                                dxUnitName,
                                dxNode.TableAlias,
                                dxNode.TableAlias,
                                Constants.ID,
                                dxNodeRelated.TableAlias,
                                Constants.DXUnitID));

                    dxNodeRelated.AttachDXNode(dxNodeRelationToDXUnit, dxNode);
                }

                // Unit → Other Unit (DXRelation)
                foreach (var DXUnitToUnitRelationElement in dxUnit.DXUnitToUnitRelationElement.Announced.Where(x => x.TargetDXUnit != x.DXUnitID))
                {
                    var dxUnitRelated = unitsById[DXUnitToUnitRelationElement.TargetDXUnit];
                    var dxUnitNameRelated = dxUnitRelated.Name;

                    var dxNodeRelated = GetNodeByName(dxUnitNameRelated);

                    var candidates = relationsByLeft[dxUnitName]
                        .Where(r => r.ObjectNameRight == dxUnitNameRelated)
                        .ToList();

                    if (candidates.Count == 0)
                    {
                        continue;
                    }

                    if (candidates.Count > 1)
                    {
                        throw new InvalidOperationException(
                            $"Sequence contains more than one matching element for pair ({dxUnitName}, {dxUnitNameRelated}).");
                    }

                    var dxRelation = candidates[0];
                    var dxRelMain = dxRelation;

                    var dxNodeFrom = GetNodeByName(dxRelation.ObjectNameRight);
                    var dxNodeTo = GetNodeByName(dxRelation.ObjectNameLeft);

                    var dxNodeJoin = GetJoinForDXUnitRelationInternal(_sqlHelper, dxRelation, dxNodeFrom, dxNodeTo);

                    var dxNodeRelation = new DXNodeRelation(
                        dxUnitNameRelated,
                        $"U2U({dxRelMain.RelationNameRight})",
                        dxNodeJoin);

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

                        var dxNodeFrom = GetNodeByName(dxRelation.ObjectNameRight);
                        var dxNodeTo = GetNodeByName(dxRelation.ObjectNameLeft);

                        var dxNodeJoin = GetJoinForDXUnitRelationInternal(_sqlHelper, dxRelation, dxNodeFrom, dxNodeTo);

                        var dxNodeRelation = new DXNodeRelation(
                            dxElementNameRelated,
                            $"U2E({dxRelation.RelationNameRight})",
                            dxNodeJoin);

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

                    var dxNodeFrom = GetNodeByName(dxRelation.ObjectNameRight);
                    var dxNodeTo = GetNodeByName(dxRelation.ObjectNameLeft);

                    var dxNodeJoin = GetJoinForDXUnitRelationInternal(_sqlHelper, dxRelation, dxNodeFrom, dxNodeTo);

                    var dxNodeRelation = new DXNodeRelation(
                        dxUnitNameRelated,
                        $"E2U({dxRelation.RelationNameRight})",
                        dxNodeJoin);

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
                RegisterNode(baseDXNodeForThisDerived, nodesById, nodesByName, registerByName: false);

                dxNode.SetBaseDXNode(baseDXNodeForThisDerived, _sqlHelper);

                foreach (var item in dxNodeForBaseDXUnit.DXNodes.Where(x => x.Value.Kind != DXNodeKind.DXProperty))
                {
                    var relation = new DXNodeRelation(
                        item.Key.TargetObjectName,
                        item.Key.RelationName,
                        item.Key.Join?.Replace(dxNodeForBaseDXUnit.TableAlias, dxNode.TableAlias));

                    dxNode.AttachDXNode(relation, item.Value);

                    if (item.Value.Kind == DXNodeKind.DXElement)
                    {
                        var dxNodeRelationToDXUnit = new DXNodeRelation(
                            dxUnitName,
                            $"E2UIn({dxUnitName})",
                            FormatJoin(
                                item.Value.Name,
                                item.Value.TableAlias,
                                item.Value.TableAlias,
                                Constants.DXUnitID,
                                dxNode.TableAlias,
                                Constants.ID));

                        item.Value.AttachDXNode(dxNodeRelationToDXUnit, dxNode);
                    }
                }
            }

            // 7. Process self related DXUnits
            foreach (var dxUnit in unitsList)
            {
                var dxUnitName = dxUnit.Name;
                var dxNode = GetNodeByName(dxUnitName);

                var selfRelatedRelations = dxUnit.DXUnitToUnitRelationElement.Announced.Where(x => x.TargetDXUnit == x.DXUnitID);

                if (selfRelatedRelations.Count() > 0)
                {
                    var dxRelationsByLeft = relationsByLeft[dxUnitName];

                    var clones = dxNode.Clone(dxRelationsByLeft, _sqlHelper);

                    foreach (var clone in clones)
                    {
                        RegisterNode(clone, nodesById, nodesByName, registerByName: false);
                    }
                }
            }

            // 8. Register nodes which started from Relation table in Many To Many relation.
            foreach (var item in dxRelations.Where(x => x.RelationType == DXRelationTypeEnum.ManyToMany))
            {
                var dxNode = new DXNode(new DXNodeKey(counter++), item.RelationTable, DXNodeKind.DXManyToManyTable);
                RegisterNode(dxNode, nodesById, nodesByName, registerByName: true);
            }

            _nodesById = nodesById;
            _nodesByName = nodesByName;
            _version = dxStructureCache.Version;
        }

        private static DXNode Get(string name)
            => _nodesByName[name];

        private static DXNode Get(DXNodeKey id)
            => _nodesById[id];

        private KeyValuePair<DXNodeRelation, DXNode> Get(DXNode dxNode, string relationName)
        {
            return dxNode.TryGetRelation(relationName, out var pair)
                ? pair
                : default;
        }

        private DXNodeRelation GetRelation(DXNode baseDXNode, DXNode relatedDXNode)
        {
            return baseDXNode.GetRelationTo(relatedDXNode.Key);
        }

        private static void RegisterIdPair(
            DXNode baseDXNode,
            DXNode relatedDXNode,
            IList<KeyValuePair<DXNodeKey, DXNodeKey>> idPairs,
            ISet<(DXNodeKey BaseId, DXNodeKey RelatedId)> idPairSet)
        {
            var pair = (BaseId: baseDXNode.Key, RelatedId: relatedDXNode.Key);
            if (idPairSet.Add(pair))
            {
                idPairs.Add(new KeyValuePair<DXNodeKey, DXNodeKey>(pair.BaseId, pair.RelatedId));
            }
        }

        private string ProcessDXColumns(
            string typeName,
            IDictionary<string, string> columns,
            QueryContext queryContext)
        {
            var coreDXNode = Get(typeName);
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

                        var relatedNode = GetRelatedNodeOrThrow(
                            startSchemaNode,
                            relationValue,
                            typeName,
                            expression,
                            alias,
                            segmentIndex: i + 1,
                            segmentCount: route.Length - 1);

                        var relatedPathKey = $"{startPathKey}.{relationValue}";
                        var relatedAlias = queryContext.GetOrCreateAlias(relatedPathKey, relatedNode);

                        queryContext.RegisterJoin(
                            startPathKey,
                            startSchemaNode,
                            startAlias,
                            relatedPathKey,
                            relatedNode,
                            relatedAlias);

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
            var coreDXNode = Get(typeName);
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

                    var relatedNode = GetRelatedNodeOrThrow(
                        startSchemaNode,
                        relationValue,
                        typeName,
                        expression.Key,
                        expressionAlias: "<filter>",
                        segmentIndex: i + 1,
                        segmentCount: route.Length - 1);

                    var relatedPathKey = $"{startPathKey}.{relationValue}";
                    var relatedAlias = queryContext.GetOrCreateAlias(relatedPathKey, relatedNode);

                    queryContext.RegisterJoin(
                        startPathKey,
                        startSchemaNode,
                        startAlias,
                        relatedPathKey,
                        relatedNode,
                        relatedAlias);

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
                    new KeyValuePair<string, string>(condition, expression.Value));
            }

            var whereExpression = string.Join(
                " ",
                whereExpressionItems.Select(x => $"{x.Value} {x.Key}")).Trim();

            return whereExpression;
        }

        static string GetJoinForDXUnitRelationInternal(
                ISQLDialect sqlHelper,
                DXRelationDefinitionUnit dxRelation,
                DXNode dxNodeFrom,
                DXNode dxNodeTo)
        {
            var dxRelationData = dxRelation;

            var dxNode1 = dxNodeTo;
            var dxNode2 = dxNodeFrom;

            var query1 =
                $"{sqlHelper.FormatTableAlias(dxRelationData.ObjectNameRight, dxNode2.TableAlias)} ON " +
                $"{sqlHelper.FormatColumnReference(dxNode2.TableAlias, Constants.ID)} = " +
                $"{sqlHelper.FormatColumnReference(dxNode1.TableAlias, dxRelationData.RelationNameRight)}";

            var query2 =
                $"{sqlHelper.FormatTableAlias(dxRelationData.ObjectNameRight, dxNode2.TableAlias)} ON " +
                $"{sqlHelper.FormatColumnReference(dxNode2.TableAlias, dxRelationData.RelationNameLeft)} = " +
                $"{sqlHelper.FormatColumnReference(dxNode1.TableAlias, Constants.ID)}";

            return dxRelationData.RelationType switch
            {
                DXRelationTypeEnum.ManyToMany =>
                    $"{sqlHelper.FormatTableAlias(dxRelationData.RelationTable, dxRelationData.RelationTable)} ON " +
                    $"{sqlHelper.FormatColumnReference(dxRelationData.RelationTable, dxRelationData.RelationNameLeft)} = " +
                    $"{sqlHelper.FormatColumnReference(dxNode1.TableAlias, Constants.ID)}\n" +
                    $"LEFT JOIN {sqlHelper.FormatTableAlias(dxRelationData.ObjectNameRight, dxNode2.TableAlias)} ON " +
                    $"{sqlHelper.FormatColumnReference(dxNode2.TableAlias, Constants.ID)} = " +
                    $"{sqlHelper.FormatColumnReference(dxRelationData.RelationTable, dxRelationData.RelationNameRight)}",

                DXRelationTypeEnum.ManyToOne
                    or DXRelationTypeEnum.ManyToZeroOne
                    or DXRelationTypeEnum.ZeroOneToOne
                    => query1,

                DXRelationTypeEnum.OneToMany
                    or DXRelationTypeEnum.ZeroOneToMany
                    or DXRelationTypeEnum.OneToZeroOne
                    => query2,

                DXRelationTypeEnum.ZeroOneToZeroOne =>
                    dxRelationData.RelationColumnNameRight == "ID" ? query1 : query2,

                _ => throw new Exception(
                    $"DXNode processing. There are no DXRelation type {dxRelationData.RelationType}")
            };
        }

        private string ReplaceQuotedAlias(string sql, string fromAlias, string toAlias)
            => sql.Replace(_sqlHelper.QuoteIdentifier(fromAlias), _sqlHelper.QuoteIdentifier(toAlias));

        private string GetFromExpression(
            string typeName,
            QueryContext queryContext)
        {
            var fromExpression = new StringBuilder();

            var coreSchemaNode = Get(typeName);
            var corePathKey = typeName;
            var coreAlias = queryContext.GetOrCreateAlias(corePathKey, coreSchemaNode);

            fromExpression.Append($"{FormatTableAlias(typeName, coreAlias)}\n");

            foreach (var join in queryContext.Joins)
            {
                var baseSchemaNode = join.BaseSchemaNode;
                var relatedSchemaNode = join.RelatedSchemaNode;

                var relation = GetRelation(baseSchemaNode, relatedSchemaNode);
                if (relation.Join is null)
                {
                    throw new InvalidOperationException(
                        $"Missing JOIN expression for relation from '{baseSchemaNode.Name}' to '{relatedSchemaNode.Name}'.");
                }

                fromExpression.Append("LEFT JOIN ")
                              .Append(ReplaceQuotedAlias(
                                  ReplaceQuotedAlias(relation.Join, baseSchemaNode.TableAlias, join.BaseAlias),
                                  relatedSchemaNode.TableAlias,
                                  join.RelatedAlias))
                              .Append('\n');
            }

            return fromExpression.ToString();
        }

        private sealed class DXNode
        {
            public DXNodeKey Key { get; }
            public string TableAlias { get; }
            public string Name { get; }
            public DXNode OriginalNode { get; private set; }
            public DXNodeKind Kind { get; }

            public DXNode BaseDXNode { get; set; }

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
                this.TableAlias = $"T_{Key.ID}_{Key.SubID}";
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

            public void SetBaseDXNode(DXNode baseDXNode, ISQLDialect sqlHelper)
            {
                this.BaseDXNode = baseDXNode;

                var dxNodeReltionToBaseDXUnit =
                   new DXNodeRelation(
                       this.Name,
                       this.Name,
                       $"{sqlHelper.FormatTableAlias(baseDXNode.Name, baseDXNode.TableAlias)} ON " +
                       $"{sqlHelper.FormatColumnReference(baseDXNode.TableAlias, Constants.ID)} = " +
                       $"{sqlHelper.FormatColumnReference(this.TableAlias, Constants.ID)}");

                this.AttachDXNode(dxNodeReltionToBaseDXUnit, baseDXNode);

                var dxNodeReltionToInheritedDXUnit =
                    new DXNodeRelation(
                        baseDXNode.Name,
                        baseDXNode.Name,
                        $"{sqlHelper.FormatTableAlias(this.Name, this.TableAlias)} ON " +
                        $"{sqlHelper.FormatColumnReference(this.TableAlias, Constants.ID)} = " +
                        $"{sqlHelper.FormatColumnReference(baseDXNode.TableAlias, Constants.ID)}");

                baseDXNode.AttachDXNode(dxNodeReltionToInheritedDXUnit, this);
            }

            public IEnumerable<DXNode> Clone(
                IEnumerable<DXRelationDefinitionUnit> dxRelations,
                ISQLDialect sqlHelper)
            {
                int counter = 1;

                List<DXNode> clones = new List<DXNode>();

                foreach (var dxRelation in dxRelations)
                {
                    var clone = new DXNode(new DXNodeKey(this.Key.ID, counter++), this.Name, this.Kind);

                    clone.OriginalNode = this;
                    clone.BaseDXNode = this.BaseDXNode;

                    foreach (var item in DXNodes)
                    {
                        var dxNodeRelation = new DXNodeRelation(
                            item.Key.TargetObjectName,
                            item.Key.RelationName,
                            item.Key.Join?.Replace($"{this.TableAlias}", $"{clone.TableAlias}"));

                        clone.AttachDXNode(dxNodeRelation, item.Value);
                    }

                    clones.Add(clone);

                    var dxJoin = GetJoinForDXUnitRelationInternal(sqlHelper, dxRelation, clone, this);

                    var dxNodeRelation2 = new DXNodeRelation(
                        dxRelation.ObjectNameLeft,
                        $"U2U({dxRelation.RelationNameRight})",
                        dxJoin);

                    this.AttachDXNode(dxNodeRelation2, clone);
                }

                return clones;
            }

            public DXNode CloneWithNewKey(DXNodeKey key)
            {
                var clone = new DXNode(key, this.Name, this.Kind);

                clone.OriginalNode = this.OriginalNode ?? this;
                clone.BaseDXNode = this.BaseDXNode;

                foreach (var item in DXNodes)
                {
                    var dxNodeRelation = new DXNodeRelation(
                        item.Key.TargetObjectName,
                        item.Key.RelationName,
                        item.Key.Join?.Replace($"{this.TableAlias}", $"{clone.TableAlias}"));

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
                if (propertyName == Constants.ID || propertyName == Constants.TimeStamp)
                    return true;

                if (this.Kind == DXNodeKind.DXElement && propertyName == Constants.DXUnitID)
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

        private struct DXNodeRelation
        {
            public string TargetObjectName { get; }
            public string RelationName { get; }
            public string? Join { get; }

            public DXNodeRelation(string targetObjectName, string relationName, string? join)
            {
                TargetObjectName = targetObjectName;
                RelationName = relationName;
                Join = join;
            }
        }

        private struct DXNodeKey
        {
            public int ID { get; }
            public int SubID { get; }

            public DXNodeKey(int id)
            {
                this.ID = id;
                this.SubID = 0;
            }

            public DXNodeKey(int id, int subID)
            {
                this.ID = id;
                this.SubID = subID;
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
