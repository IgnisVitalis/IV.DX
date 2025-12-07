using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using System.Data;
using System.Text;

namespace IV.DX.Persistence
{
    internal class SQLQueryBuilder(IDXStructureCache dxStructureCache) : ISQLQueryBuilder
    {
        private static IReadOnlyDictionary<string, DXNode> _nodesByName =
            new Dictionary<string, DXNode>(StringComparer.Ordinal);

        private static IReadOnlyDictionary<int, DXNode> _nodesById =
            new Dictionary<int, DXNode>();

        private static int _version = 0;

        public string BuildSQLExpression(
            string typeName,
            IDictionary<string, string>? columns = default,
            string? dxFilter = default)
        {
            BuildDXNodeTree();

            var joinPairs = new List<KeyValuePair<int, int>>();
            var joinPairSet = new HashSet<(int BaseId, int RelatedId)>();

            string whereExpression = string.Empty;
            bool hasFilter = !string.IsNullOrEmpty(dxFilter);

            if (hasFilter)
            {
                whereExpression = ProcessDXFilter(typeName, dxFilter!, joinPairs, joinPairSet);
            }

            var columnExpression = ProcessDXColumns(typeName, columns, joinPairs, joinPairSet);
            var fromExpression = GetFromExpression(typeName, joinPairs);

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
                .ToLookup(r => r.DXRelationDefinitionMainElement.ObjectNameLeft);

            var nodesById = new Dictionary<int, DXNode>(
                capacity: unitsList.Count + elementsList.Count + enumsList.Count);

            var nodesByName = new Dictionary<string, DXNode>(StringComparer.Ordinal);

            static void RegisterNode(
                DXNode node,
                IDictionary<int, DXNode> byId,
                IDictionary<string, DXNode> byName,
                bool registerByName)
            {
                byId[node.ID] = node;
                if (registerByName)
                {
                    byName[node.Name] = node;
                }
            }

            int counter = 0;

            // 1. Units
            foreach (var dxUnit in unitsList)
            {
                var node = new DXNode(counter++, dxUnit.DXObjectDefinitionMainElement.Name);
                RegisterNode(node, nodesById, nodesByName, registerByName: true);
            }

            // 2. Elements
            foreach (var dxElement in elementsList)
            {
                var node = new DXNode(counter++, dxElement.DXObjectDefinitionMainElement.Name);
                RegisterNode(node, nodesById, nodesByName, registerByName: true);
            }

            // 3. Enums
            foreach (var dxEnum in enumsList)
            {
                var node = new DXNode(counter++, dxEnum.DXObjectDefinitionMainElement.Name);
                RegisterNode(node, nodesById, nodesByName, registerByName: true);
            }

            DXNode GetNodeByName(string name) => nodesByName[name];

            // 4. Unit ↔ Element и Unit ↔ Unit (Relation)
            foreach (var dxUnit in unitsList)
            {
                var dxUnitName = dxUnit.DXObjectDefinitionMainElement.Name;
                var dxNode = GetNodeByName(dxUnitName);

                // Unit → Element
                foreach (var dxElementInUnit in dxUnit.DXElementInUnitDefinitionElement.Announced)
                {
                    var dxElement = elementsById[dxElementInUnit.DXElementDefinitionUnit];
                    var dxElementName = dxElement.DXObjectDefinitionMainElement.Name;
                    var dxNodeRelated = GetNodeByName(dxElementName);

                    var dxNodeRelationToDXElement =
                        new DXNodeRelation(
                            dxElementName,
                            dxElementName,
                            $"\"{dxElementName}\" AS \"{dxNodeRelated.TableAlias}\" ON \"{dxNodeRelated.TableAlias}\".\"DXUnitID\" = \"{dxNode.TableAlias}\".\"ID\"");

                    dxNode.AttachDXNode(dxNodeRelationToDXElement, dxNodeRelated);

                    var dxNodeRelationToDXUnit =
                        new DXNodeRelation(
                            dxUnitName,
                            $"U({dxUnitName})",
                            $"\"{dxUnitName}\" AS \"{dxNode.TableAlias}\" ON \"{dxNode.TableAlias}\".\"ID\" = {dxNodeRelated.TableAlias}.\"DXUnitID\"");

                    dxNodeRelated.AttachDXNode(dxNodeRelationToDXUnit, dxNode);
                }

                // Unit → Unit (DXRelation)
                foreach (var dxUnitRelationElement in dxUnit.DXUnitRelationElement.Announced)
                {
                    var dxUnitRelated = unitsById[dxUnitRelationElement.TargetDXUnit];
                    var dxUnitNameRelated = dxUnitRelated.DXObjectDefinitionMainElement.Name;
                    var dxNodeRelated = GetNodeByName(dxUnitNameRelated);

                    var candidates = relationsByLeft[dxUnitName]
                        .Where(r => r.DXRelationDefinitionMainElement.ObjectNameRight == dxUnitNameRelated)
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
                    var dxRelMain = dxRelation.DXRelationDefinitionMainElement;

                    var dxNodeRelation = new DXNodeRelation(
                        dxUnitNameRelated,
                        $"R({dxRelMain.RelationNameRight})",
                        GetJoinForDXUnitRelationInternal(dxRelation, GetNodeByName));

                    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }
            }

            // 5. Inheritance Units
            foreach (var dxUnit in unitsList)
            {
                if (dxUnit.DXUnitInheritanceElement is null)
                {
                    continue;
                }

                var dxUnitName = dxUnit.DXObjectDefinitionMainElement.Name;
                var dxNode = GetNodeByName(dxUnitName);

                var baseDXUnit = unitsById[dxUnit.DXUnitInheritanceElement.BaseDXUnit];
                var dxNodeForBaseDXUnit = GetNodeByName(baseDXUnit.DXObjectDefinitionMainElement.Name);

                foreach (var item in dxNodeForBaseDXUnit.DXNodes)
                {
                    var relation = new DXNodeRelation(
                        item.Key.TargetObjectName,
                        item.Key.RelationName,
                        item.Key.Join?.Replace(dxNodeForBaseDXUnit.TableAlias, dxNode.TableAlias));

                    dxNode.AttachDXNode(relation, item.Value);

                    var dxNodeRelationToDXUnit = new DXNodeRelation(
                        dxUnitName,
                        $"U({dxUnitName})",
                        $"\"{item.Value.Name}\" AS \"{item.Value.TableAlias}\" ON \"{item.Value.TableAlias}\".\"DXUnitID\" = \"{dxNode.TableAlias}\".\"ID\"");

                    item.Value.AttachDXNode(dxNodeRelationToDXUnit, dxNode);
                }
            }

            foreach (var dxUnit in unitsList)
            {
                var dxUnitName = dxUnit.DXObjectDefinitionMainElement.Name;
                var dxNode = GetNodeByName(dxUnitName);

                foreach (var dxColumn in dxUnit.DXColumnDefinitionElement.Announced)
                {
                    var dxNodeRelated = new DXNode(counter++, dxColumn.Name);
                    RegisterNode(dxNodeRelated, nodesById, nodesByName, registerByName: false);

                    var dxNodeRelation = new DXNodeRelation(dxColumn.Name, dxColumn.Name, null);
                    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }
            }

            foreach (var dxElement in elementsList)
            {
                var dxElementName = dxElement.DXObjectDefinitionMainElement.Name;
                var dxNode = GetNodeByName(dxElementName);

                foreach (var dxColumn in dxElement.DXColumnDefinitionElement.Announced)
                {
                    var dxNodeRelated = new DXNode(counter++, dxColumn.Name);
                    RegisterNode(dxNodeRelated, nodesById, nodesByName, registerByName: false);

                    var dxNodeRelation = new DXNodeRelation(dxColumn.Name, dxColumn.Name, null);
                    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }
            }

            _nodesById = nodesById;
            _nodesByName = nodesByName;
            _version = dxStructureCache.Version;

            static string GetJoinForDXUnitRelationInternal(
                DXRelationDefinitionUnit dxRelation,
                Func<string, DXNode> getNodeByName)
            {
                var dxRelationData = dxRelation.DXRelationDefinitionMainElement;

                var dxNode1 = getNodeByName(dxRelationData.ObjectNameRight);
                var dxNode2 = getNodeByName(dxRelationData.ObjectNameLeft);

                var query1 =
                    $"\"{dxRelationData.ObjectNameRight}\" AS \"{dxNode1.TableAlias}\" ON \"{dxNode1.TableAlias}\".\"ID\" = \"{dxNode2.TableAlias}\".\"{dxRelationData.RelationNameRight}\"";

                var query2 =
                    $"\"{dxRelationData.ObjectNameRight}\" AS \"{dxNode1.TableAlias}\" ON \"{dxNode1.TableAlias}\".\"{dxRelationData.RelationNameLeft}\" = \"{dxNode2.TableAlias}\".\"ID\"";

                return dxRelationData.RelationType switch
                {
                    DXRelationTypeEnum.ManyToMany =>
                        $"\"{dxRelationData.RelationTable}\" AS \"{dxRelationData.RelationTable}\" ON \"{dxRelationData.RelationTable}\".\"{dxRelationData.RelationNameLeft}\" = \"{dxNode2.TableAlias}\".\"ID\"\nLEFT JOIN \"{dxRelationData.ObjectNameRight}\" AS \"{dxNode1.TableAlias}\" ON \"{dxNode1.TableAlias}\".\"ID\" = \"{dxRelationData.RelationTable}\".\"{dxRelationData.RelationNameRight}\"",

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
        }

        private static DXNode Get(string name)
            => _nodesByName[name];

        private static DXNode Get(int id)
            => _nodesById[id];

        private KeyValuePair<DXNodeRelation, DXNode> Get(DXNode dxNode, string relationName)
        {
            return dxNode.TryGetRelation(relationName, out var pair)
                ? pair
                : default;
        }

        private DXNodeRelation GetRelation(DXNode baseDXNode, DXNode relatedDXNode)
        {
            return baseDXNode.GetRelationTo(relatedDXNode.ID);
        }

        private static void RegisterIdPair(
            DXNode baseDXNode,
            DXNode relatedDXNode,
            IList<KeyValuePair<int, int>> idPairs,
            ISet<(int BaseId, int RelatedId)> idPairSet)
        {
            var pair = (BaseId: baseDXNode.ID, RelatedId: relatedDXNode.ID);
            if (idPairSet.Add(pair))
            {
                idPairs.Add(new KeyValuePair<int, int>(pair.BaseId, pair.RelatedId));
            }
        }

        private string ProcessDXColumns(
            string typeName,
            IDictionary<string, string>? columns,
            IList<KeyValuePair<int, int>> idPairs,
            ISet<(int BaseId, int RelatedId)> idPairSet)
        {
            var coreDXNode = Get(typeName);

            var columnsExpressionItems = new List<string>
            {
                $"\"{coreDXNode.TableAlias}\".\"ID\" AS \"ID\""
            };

            if (columns is not null)
            {
                foreach (var column in columns)
                {
                    var alias = column.Key;
                    var expression = column.Value;

                    var route = expression.Split('.');
                    var baseDXNode = coreDXNode;

                    for (int i = 0; i < route.Length - 1; i++)
                    {
                        var relationValue = route[i];

                        var relatedPair = Get(baseDXNode, relationValue);
                        var relatedNode = relatedPair.Value;

                        RegisterIdPair(baseDXNode, relatedNode, idPairs, idPairSet);

                        baseDXNode = relatedNode;
                    }

                    columnsExpressionItems.Add(
                        $"\"{baseDXNode.TableAlias}\".\"{route[^1]}\" AS \"{alias}\"");
                }
            }

            return string.Join(",\n", columnsExpressionItems).Trim();
        }

        private string ProcessDXFilter(
            string typeName,
            string dxFilter,
            IList<KeyValuePair<int, int>> idPairs,
            ISet<(int BaseId, int RelatedId)> idPairSet)
        {
            var coreDXNode = Get(typeName);

            var expressions = dxFilter.Trim()
                .SplitAndKeep(
                    new[] { " and ", " or ", " AND ", " OR ", " And ", " Or " },
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            var whereExpressionItems = new List<KeyValuePair<string, string>>();

            foreach (var expression in expressions)
            {
                var route = expression.Key.Split('.');
                var baseDXNode = coreDXNode;

                for (int i = 0; i < route.Length - 1; i++)
                {
                    var relationValue = route[i];

                    var relatedPair = Get(baseDXNode, relationValue);
                    var relatedNode = relatedPair.Value;

                    RegisterIdPair(baseDXNode, relatedNode, idPairs, idPairSet);

                    baseDXNode = relatedNode;
                }

                var whereExpressionItem = route[^1];

                var spaceIndex = whereExpressionItem.IndexOf(' ');
                var propertyName = whereExpressionItem.Substring(0, spaceIndex);
                var propertyValue = whereExpressionItem.Substring(spaceIndex);

                var condition = $"\"{baseDXNode.TableAlias}\".\"{propertyName}\"{propertyValue}";

                whereExpressionItems.Add(
                    new KeyValuePair<string, string>(condition, expression.Value));
            }

            var whereExpression = string.Join(
                " ",
                whereExpressionItems.Select(x => $"{x.Value} {x.Key}")).Trim();

            return whereExpression;
        }

        private string GetFromExpression(
            string typeName,
            IList<KeyValuePair<int, int>> idPairs)
        {
            var fromExpression = new StringBuilder();

            var dxNode = Get(typeName);

            fromExpression.Append($"\"{typeName}\" AS \"{dxNode.TableAlias}\"\n");

            foreach (var idPair in idPairs)
            {
                var baseDXNode = Get(idPair.Key);
                var relatedDXNode = Get(idPair.Value);

                var relation = GetRelation(baseDXNode, relatedDXNode);

                fromExpression.Append("LEFT JOIN ")
                              .Append(relation.Join)
                              .Append('\n');
            }

            return fromExpression.ToString();
        }

        private sealed class DXNode
        {
            public int ID { get; }
            public string TableAlias { get; }
            public string Name { get; }

            public IDictionary<DXNodeRelation, DXNode> DXNodes { get; } =
                new Dictionary<DXNodeRelation, DXNode>();

            private readonly Dictionary<string, KeyValuePair<DXNodeRelation, DXNode>> _relationsByName =
                new(StringComparer.Ordinal);

            private readonly Dictionary<int, DXNodeRelation> _relationsByTargetId =
                new();

            public DXNode(int id, string name)
            {
                ID = id;
                Name = name;
                TableAlias = $"T_{id}";
            }

            public void AttachDXNode(DXNodeRelation dxRelation, DXNode dxNode)
            {
                DXNodes[dxRelation] = dxNode;
                _relationsByName[dxRelation.RelationName] =
                    new KeyValuePair<DXNodeRelation, DXNode>(dxRelation, dxNode);
                _relationsByTargetId[dxNode.ID] = dxRelation;
            }

            public bool TryGetRelation(
                string relationName,
                out KeyValuePair<DXNodeRelation, DXNode> relation)
            {
                return _relationsByName.TryGetValue(relationName, out relation);
            }

            public DXNodeRelation GetRelationTo(int targetId)
            {
                return _relationsByTargetId[targetId];
            }

            public override string ToString() => Name;
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
    }


}