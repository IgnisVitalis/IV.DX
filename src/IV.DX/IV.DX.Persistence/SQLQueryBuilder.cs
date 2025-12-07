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
        static HashSet<DXNode> DXNodes { get; set; }
        static int version = 0;

        public string BuildSQLExpression(string typeName, IDictionary<string, string>? columns = default, string? dxFilter = default)
        {
            this.BuildDXNodeTree();

            List<KeyValuePair<int, int>> idPairs = new List<KeyValuePair<int, int>>();

            string whereExpression = string.Empty;

            if (!string.IsNullOrEmpty(dxFilter))
            {
                whereExpression = this.ProcessDXFilter(typeName, dxFilter, idPairs);
            }

            var columnExpression = this.ProcessDXColumns(typeName, columns, idPairs);

            var fromExpression = GetFromExpression(typeName, idPairs);

            StringBuilder sb = new StringBuilder();
            sb.Append($"SELECT\n{columnExpression}\n");
            sb.Append($"FROM\n{fromExpression}");

            if (!string.IsNullOrEmpty(dxFilter))
            {
                sb.Append($"WHERE\n{whereExpression}");
            }

            return sb.ToString();
        }

        private void BuildDXNodeTree()
        {
            if (dxStructureCache.Version > version)
            {
                this.Load(dxStructureCache.DXRelations, dxStructureCache.DXUnits, dxStructureCache.DXElements, dxStructureCache.DXEnums);
            }
        }

        private void Load(
            IEnumerable<DXRelationDefinitionUnit> dxRelations,
            IEnumerable<DXUnitDefinitionUnit> dxUnits,
            IEnumerable<DXElementDefinitionUnit> dxElements,
            IEnumerable<DXEnumDefinitionUnit> dxEnums)
        {
            DXNodes = new HashSet<DXNode>();

            int counter = 0;

            foreach (var dxUnit in dxUnits)
            {
                var dxUnitNode = new DXNode(counter++, dxUnit.DXObjectDefinitionMainElement.Name);

                DXNodes.Add(dxUnitNode);

            }

            foreach (var dxElement in dxElements)
            {
                var dxElementNode = new DXNode(counter++, dxElement.DXObjectDefinitionMainElement.Name);

                DXNodes.Add(dxElementNode);
            }

            foreach (var dxEnum in dxEnums)
            {
                var dxEnumNode = new DXNode(counter++, dxEnum.DXObjectDefinitionMainElement.Name);

                DXNodes.Add(dxEnumNode);
            }

            foreach (var dxUnit in dxUnits)
            {
                var dxUnitName = dxUnit.DXObjectDefinitionMainElement.Name;
                var dxNode = this.Get(dxUnitName);

                foreach (var dxElementInUnit in dxUnit.DXElementInUnitDefinitionElement.Announced)
                {
                    var dxElement = dxElements.Single(x => x.ID == dxElementInUnit.DXElementDefinitionUnit);

                    var dxElementName = dxElement.DXObjectDefinitionMainElement.Name;

                    var dxNodeRelated = this.Get(dxElementName);

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

                foreach (var dxUnitRelationElement in dxUnit.DXUnitRelationElement.Announced)
                {
                    var dxUnitRelated = dxUnits.Single(x => x.ID == dxUnitRelationElement.TargetDXUnit);

                    var dxUnitNameRelated = dxUnitRelated.DXObjectDefinitionMainElement.Name;

                    var dxNodeRelated = this.Get(dxUnitNameRelated);

                    var dxRelation = dxRelations.SingleOrDefault(x =>
                        x.DXRelationDefinitionMainElement.ObjectNameLeft == dxUnitName
                        && x.DXRelationDefinitionMainElement.ObjectNameRight == dxUnitNameRelated);

                    if (dxRelation == null)
                        continue;

                    var dxNodeRelation = new DXNodeRelation(
                        dxUnitNameRelated,
                        $"R({dxRelation.DXRelationDefinitionMainElement.RelationNameRight})",
                        this.GetJoinForDXUnitRelation(dxRelation));

                    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }
            }

            foreach (var dxUnit in dxUnits)
            {
                if (dxUnit.DXUnitInheritanceElement != null)
                {
                    var dxUnitName = dxUnit.DXObjectDefinitionMainElement.Name;

                    var dxNode = this.Get(dxUnitName);

                    var baseDXUnit = dxUnits.Single(x => x.ID == dxUnit.DXUnitInheritanceElement.BaseDXUnit);

                    var dxNodeForBaseDXUnit = this.Get(baseDXUnit.DXObjectDefinitionMainElement.Name);

                    foreach (var item in dxNodeForBaseDXUnit.DXNodes)
                    {
                        var relation = new DXNodeRelation(
                            item.Key.TargetObjectName,
                            item.Key.RelationName,
                            item.Key.Join.Replace(dxNodeForBaseDXUnit.TableAlias, dxNode.TableAlias));

                        dxNode.AttachDXNode(relation, item.Value);

                        var dxNodeRelationToDXUnit = new DXNodeRelation(
                            dxUnitName,
                            $"U({dxUnitName})",
                            $"\"{item.Value.Name}\" AS \"{item.Value.TableAlias}\" ON \"{item.Value.TableAlias}\".\"DXUnitID\" = \"{dxNode.TableAlias}\".\"ID\"");

                        item.Value.AttachDXNode(dxNodeRelationToDXUnit, dxNode);
                    }
                }
            }

            foreach (var dxUnit in dxUnits)
            {
                var dxUnitName = dxUnit.DXObjectDefinitionMainElement.Name;
                var dxNode = this.Get(dxUnitName);

                foreach (var dxColumn in dxUnit.DXColumnDefinitionElement.Announced)
                {
                    var dxNodeRelated = new DXNode(counter++, dxColumn.Name);

                    DXNodes.Add(dxNodeRelated);

                    var dxNodeRelation = new DXNodeRelation(dxColumn.Name, dxColumn.Name, null);

                    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }
            }

            foreach (var dxElement in dxElements)
            {
                var dxElementName = dxElement.DXObjectDefinitionMainElement.Name;
                var dxNode = this.Get(dxElementName);

                foreach (var dxColumn in dxElement.DXColumnDefinitionElement.Announced)
                {
                    var dxNodeRelated = new DXNode(counter++, dxColumn.Name);

                    DXNodes.Add(dxNodeRelated);

                    var dxNodeRelation = new DXNodeRelation(dxColumn.Name, dxColumn.Name, null);

                    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }
            }

            version = dxStructureCache.Version;
        }

        private string GetJoinForDXUnitRelation(DXRelationDefinitionUnit dxRelation)
        {
            var dxRelationData = dxRelation.DXRelationDefinitionMainElement;

            var dxNode1 = this.Get(dxRelationData.ObjectNameRight);
            var dxNode2 = this.Get(dxRelationData.ObjectNameLeft);

            var query1 = $"\"{dxRelationData.ObjectNameRight}\" AS \"{dxNode1.TableAlias}\" ON \"{dxNode1.TableAlias}\".\"ID\" = \"{dxNode2.TableAlias}\".\"{dxRelationData.RelationNameRight}\"";
            var query2 = $"\"{dxRelationData.ObjectNameRight}\" AS \"{dxNode1.TableAlias}\" ON \"{dxNode1.TableAlias}\".\"{dxRelationData.RelationNameLeft}\" = \"{dxNode2.TableAlias}\".\"ID\"";

            switch (dxRelationData.RelationType)
            {
                case DXRelationTypeEnum.ManyToMany:
                    return $"\"{dxRelationData.RelationTable}\" AS \"{dxRelationData.RelationTable}\" ON \"{dxRelationData.RelationTable}\".\"{dxRelationData.RelationNameLeft}\" = \"{dxNode2.TableAlias}\".\"ID\"\nLEFT JOIN \"{dxRelationData.ObjectNameRight}\" AS \"{dxNode1.TableAlias}\" ON \"{dxNode1.TableAlias}\".\"ID\" = \"{dxRelationData.RelationTable}\".\"{dxRelationData.RelationNameRight}\"";
                case DXRelationTypeEnum.ManyToOne:
                case DXRelationTypeEnum.ManyToZeroOne:
                case DXRelationTypeEnum.ZeroOneToOne:
                    return query1;
                case DXRelationTypeEnum.OneToMany:
                case DXRelationTypeEnum.ZeroOneToMany:
                case DXRelationTypeEnum.OneToZeroOne:
                    return query2;
                case DXRelationTypeEnum.ZeroOneToZeroOne:
                    return dxRelationData.RelationColumnNameRight == "ID" ? query1 : query2;
                default: throw new Exception($"DXNode processing. There are no DXRelation type {dxRelationData.RelationType}");
            }
        }


        private DXNode Get(string name)
        {
            return DXNodes.Single(x => x.Name.Equals(name));
        }

        private DXNode Get(int id)
        {
            return DXNodes.Single(x => x.ID == id);
        }

        private KeyValuePair<DXNodeRelation, DXNode> Get(DXNode dxNode, string relationName)
        {
            var result = dxNode.DXNodes.SingleOrDefault(x => x.Key.RelationName.Equals(relationName));

            return result;
        }

        private DXNodeRelation GetRelation(DXNode baseDXNode, DXNode relatedDXNode)
        {
            return baseDXNode.DXNodes.Single(x => x.Value.ID == relatedDXNode.ID).Key;
        }


        private string ProcessDXColumns(string typeName, IDictionary<string, string>? columns, IList<KeyValuePair<int, int>> idPairs)
        {
            var coreDXNode = this.Get(typeName);

            List<string> columnsExpressionItems = new List<string>();

            columnsExpressionItems.Add($"\"{coreDXNode.TableAlias}\".\"ID\" AS \"ID\"");

            if (columns != null && columns != default)
            {
                foreach (var column in columns)
                {
                    var alias = column.Key;
                    var expression = column.Value;

                    var route = expression.Split(".");

                    var baseDXNode = coreDXNode;

                    for (int i = 0; i < route.Length - 1; i++)
                    {
                        var relationValue = route[i];

                        var relatedNode = this.Get(baseDXNode, relationValue).Value;

                        KeyValuePair<int, int> idPair = new KeyValuePair<int, int>(baseDXNode.ID, relatedNode.ID);

                        if (!idPairs.Contains(idPair))
                        {
                            idPairs.Add(idPair);
                        }

                        baseDXNode = relatedNode;
                    }

                    columnsExpressionItems.Add($"\"{baseDXNode.TableAlias}\".\"{route.Last()}\" AS \"{alias}\"");
                }
            }

            var columnsExpression = String.Join(",\n", columnsExpressionItems).Trim();

            return columnsExpression;
        }

        private string ProcessDXFilter(string typeName, string dxFilter, IList<KeyValuePair<int, int>> idPairs)
        {
            var coreDXNode = this.Get(typeName);

            var expressions =
                dxFilter?.Trim()
                .SplitAndKeep(new string[] { " and ", " or ", " AND ", " OR ", " And ", " Or " }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();

            List<KeyValuePair<string, string>> whereExpressionItems = new List<KeyValuePair<string, string>>();

            foreach (var expression in expressions)
            {
                var route = expression.Key.Split(".");

                var baseDXNode = coreDXNode;

                for (int i = 0; i < route.Length - 1; i++)
                {
                    var relationValue = route[i];

                    var relatedNode = this.Get(baseDXNode, relationValue).Value;

                    KeyValuePair<int, int> idPair = new KeyValuePair<int, int>(baseDXNode.ID, relatedNode.ID);

                    if (!idPairs.Contains(idPair))
                    {
                        idPairs.Add(idPair);
                    }

                    baseDXNode = relatedNode;
                }

                var whereExpressionItem = route.Last();

                var propertyName = whereExpressionItem.Substring(0, whereExpressionItem.IndexOf(" "));
                var propertyValue = whereExpressionItem.Substring(propertyName.Length, whereExpressionItem.Length - propertyName.Length);

                whereExpressionItems.Add(new KeyValuePair<string, string>($"\"{baseDXNode.TableAlias}\".\"{propertyName}\"{propertyValue}", expression.Value));
            }

            var whereExpression = String.Join(" ", whereExpressionItems.Select(x => $"{x.Value} {x.Key}")).Trim();

            return whereExpression;
        }

        private string GetFromExpression(string typeName, IList<KeyValuePair<int, int>> idPairs)
        {
            var fromExpression = new StringBuilder();

            var dxNode = this.Get(typeName);

            fromExpression.Append($"\"{typeName}\" AS \"{dxNode.TableAlias}\"\n");

            foreach (var idPair in idPairs)
            {
                var baseDXNodeID = idPair.Key;
                var relatedDXNodeID = idPair.Value;

                var baseDXNode = this.Get(baseDXNodeID);
                var relatedDXNode = this.Get(relatedDXNodeID);

                var relation = this.GetRelation(baseDXNode, relatedDXNode);

                fromExpression.Append("LEFT JOIN ");

                fromExpression.Append(relation.Join);
                fromExpression.Append("\n");
            }

            return fromExpression.ToString();
        }

        private class DXNode
        {
            public int ID { get; }
            public string TableAlias { get; }
            public string Name { get; }
            public IDictionary<DXNodeRelation, DXNode> DXNodes { get; } = new Dictionary<DXNodeRelation, DXNode>();

            public DXNode(int id, string name)
            {
                this.ID = id;
                this.Name = name;
                this.TableAlias = $"T_{id}";
            }

            public void AttachDXNode(DXNodeRelation dxRelation, DXNode dxNode)
            {
                DXNodes[dxRelation] = dxNode;
            }

            public override string ToString()
            {
                return $"{Name}";
            }
        }

        private struct DXNodeRelation
        {
            public string TargetObjectName { get; }
            public string RelationName { get; }
            public string Join { get; }

            public DXNodeRelation(string targetObjectName, string relationName, string join)
            {
                this.TargetObjectName = targetObjectName;
                this.RelationName = relationName;
                this.Join = join;
            }
        }
    }
}