using IV.DX.Kernel;
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
        public static IDictionary<string, string> AllColumns { get; } = new Dictionary<string, string>();
        public static IDictionary<string, string> BaseColumns { get; } = new Dictionary<string, string>()
        {
            {"ID","ID" },
            {"TimeStamp", "TimeStamp" }
        };

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

            var joinPairs = new List<KeyValuePair<DXNodeKey, DXNodeKey>>();
            var joinPairSet = new HashSet<(DXNodeKey BaseId, DXNodeKey RelatedId)>();

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

            // 5. Unit ↔ Element и Unit ↔ Unit (Relation)
            foreach (var dxUnit in unitsList)
            {
                var dxUnitName = dxUnit.Name;
                var dxNode = GetNodeByName(dxUnitName);

                // Unit → Element
                foreach (var dxElementInUnit in dxUnit.DXElementInUnitDefinitionElement.Announced)
                {
                    var dxElement = elementsById[dxElementInUnit.DXElementDefinitionUnit];
                    var dxElementName = dxElement.Name;
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

                    var dxNodeJoin = GetJoinForDXUnitRelationInternal(dxRelation, dxNodeFrom, dxNodeTo);

                    var dxNodeRelation = new DXNodeRelation(
                        dxUnitNameRelated,
                        $"R({dxRelMain.RelationNameRight})",
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

                dxNode.SetBaseDXNode(dxNodeForBaseDXUnit);

                foreach (var item in dxNodeForBaseDXUnit.DXNodes.Where(x => x.Value.Kind != DXNodeKind.DXProperty))
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

            // 7. Process self related DXUnits
            foreach (var dxUnit in unitsList)
            {
                var dxUnitName = dxUnit.Name;
                var dxNode = GetNodeByName(dxUnitName);

                var selfRelatedRelations = dxUnit.DXUnitToUnitRelationElement.Announced.Where(x => x.TargetDXUnit == x.DXUnitID);

                if (selfRelatedRelations.Count() > 0)
                {
                    var dxRelationsByLeft = relationsByLeft[dxUnitName];

                    var clones = dxNode.Clone(dxRelationsByLeft);

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
            IList<KeyValuePair<DXNodeKey, DXNodeKey>> idPairs,
            ISet<(DXNodeKey BaseId, DXNodeKey RelatedId)> idPairSet)
        {
            var coreDXNode = Get(typeName);

            var columnsExpressionItems = new List<string>();

            if (columns.Count() == 0)
                return $"\"{coreDXNode.TableAlias}\".*";

            if (columns is not null)
            {
                foreach (var column in columns)
                {
                    var alias = column.Key;
                    var expression = column.Value;

                    var route = expression.Split('.');
                    var startDXNode = coreDXNode;

                    for (int i = 0; i < route.Length - 1; i++)
                    {
                        var relationValue = route[i];

                        var relatedPair = Get(startDXNode, relationValue);
                        var relatedNode = relatedPair.Value;

                        RegisterIdPair(startDXNode, relatedNode, idPairs, idPairSet);

                        startDXNode = relatedNode;
                    }

                    string columnExpressionItem;

                    var columnName = route[^1];

                    if (startDXNode.ContainsProperty(columnName))
                    {
                        columnExpressionItem = $"\"{startDXNode.TableAlias}\".\"{columnName}\" AS \"{alias}\"";
                    }
                    else
                    {
                        var baseDXNode = startDXNode.GetBaseDXNodeWithProperty(columnName);

                        RegisterIdPair(startDXNode, baseDXNode, idPairs, idPairSet);

                        columnExpressionItem = $"\"{baseDXNode.TableAlias}\".\"{columnName}\" AS \"{alias}\"";
                    }

                    columnsExpressionItems.Add(columnExpressionItem);
                }
            }

            return string.Join(",\n", columnsExpressionItems).Trim();
        }

        private string ProcessDXFilter(
            string typeName,
            string dxFilter,
            IList<KeyValuePair<DXNodeKey, DXNodeKey>> idPairs,
            ISet<(DXNodeKey BaseId, DXNodeKey RelatedId)> idPairSet)
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
                var startDXNode = coreDXNode;

                for (int i = 0; i < route.Length - 1; i++)
                {
                    var relationValue = route[i];

                    var relatedPair = Get(startDXNode, relationValue);
                    var relatedNode = relatedPair.Value;

                    RegisterIdPair(startDXNode, relatedNode, idPairs, idPairSet);

                    startDXNode = relatedNode;
                }

                var whereExpressionItem = route[^1];

                var spaceIndex = whereExpressionItem.IndexOf(' ');
                var propertyName = whereExpressionItem.Substring(0, spaceIndex);
                var propertyValue = whereExpressionItem.Substring(spaceIndex);

                string condition;

                if (startDXNode.ContainsProperty(propertyName))
                {
                    condition = $"\"{startDXNode.TableAlias}\".\"{propertyName}\"{propertyValue}";
                }
                else
                {
                    var baseDXNode = startDXNode.GetBaseDXNodeWithProperty(propertyName);

                    RegisterIdPair(startDXNode, baseDXNode, idPairs, idPairSet);

                    condition = $"\"{baseDXNode.TableAlias}\".\"{propertyName}\"{propertyValue}";
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
                DXRelationDefinitionUnit dxRelation,
                DXNode dxNodeFrom,
                DXNode dxNodeTo)
        {
            var dxRelationData = dxRelation;

            var dxNode1 = dxNodeTo;
            var dxNode2 = dxNodeFrom;

            var query1 =
                $"\"{dxRelationData.ObjectNameRight}\" AS \"{dxNode2.TableAlias}\" ON \"{dxNode2.TableAlias}\".\"ID\" = \"{dxNode1.TableAlias}\".\"{dxRelationData.RelationNameRight}\"";

            var query2 =
                $"\"{dxRelationData.ObjectNameRight}\" AS \"{dxNode2.TableAlias}\" ON \"{dxNode2.TableAlias}\".\"{dxRelationData.RelationNameLeft}\" = \"{dxNode1.TableAlias}\".\"ID\"";

            return dxRelationData.RelationType switch
            {
                DXRelationTypeEnum.ManyToMany =>
                    $"\"{dxRelationData.RelationTable}\" AS \"{dxRelationData.RelationTable}\" ON \"{dxRelationData.RelationTable}\".\"{dxRelationData.RelationNameLeft}\" = \"{dxNode1.TableAlias}\".\"ID\"\nLEFT JOIN \"{dxRelationData.ObjectNameRight}\" AS \"{dxNode2.TableAlias}\" ON \"{dxNode2.TableAlias}\".\"ID\" = \"{dxRelationData.RelationTable}\".\"{dxRelationData.RelationNameRight}\"",

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


        private string GetFromExpression(
            string typeName,
            IList<KeyValuePair<DXNodeKey, DXNodeKey>> idPairs)
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

            public DXNodeRelation GetRelationTo(DXNodeKey targetId)
            {
                return _relationsByTargetId[targetId];
            }

            public void SetBaseDXNode(DXNode baseDXNode)
            {
                this.BaseDXNode = baseDXNode;

                var dxNodeReltionToBaseDXUnit =
                   new DXNodeRelation(
                       this.Name,
                       this.Name,
                       $"\"{baseDXNode.Name}\" AS \"{baseDXNode.TableAlias}\" ON \"{baseDXNode.TableAlias}\".\"ID\" = \"{this.TableAlias}\".\"ID\"");

                this.AttachDXNode(dxNodeReltionToBaseDXUnit, baseDXNode);

                var dxNodeReltionToInheritedDXUnit =
                    new DXNodeRelation(
                        baseDXNode.Name,
                        baseDXNode.Name,
                        $"\"{this.Name}\" AS \"{this.TableAlias}\" ON \"{this.TableAlias}\".\"ID\" = \"{baseDXNode.TableAlias}\".\"ID\"");

                baseDXNode.AttachDXNode(dxNodeReltionToInheritedDXUnit, this);
            }

            public IEnumerable<DXNode> Clone(IEnumerable<DXRelationDefinitionUnit> dxRelations)
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

                    var dxJoin = GetJoinForDXUnitRelationInternal(dxRelation, clone, this);

                    var dxNodeRelation2 = new DXNodeRelation(
                        dxRelation.ObjectNameLeft,
                        $"R({dxRelation.RelationNameRight})",
                        dxJoin);

                    this.AttachDXNode(dxNodeRelation2, clone);
                }

                return clones;
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