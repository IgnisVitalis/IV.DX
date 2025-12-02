using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using System.Text;

namespace IV.DX.Kernel.Models.New
{
    public class DXNodeTree
    {
        HashSet<DXNode> DXNodes { get; }

        public DXNodeTree()
        {
            DXNodes = new HashSet<DXNode>();
        }

        public void Load(
            IEnumerable<DXRelationDefinitionUnit> dxRelations,
            IEnumerable<DXUnitDefinitionUnit> dxUnits,
            IEnumerable<DXElementDefinitionUnit> dxElements,
            IEnumerable<DXEnumDefinitionUnit> dxEnums)
        {
            foreach (var dxUnit in dxUnits)
            {
                var dxUnitNode = new DXNode(dxUnit.DXObjectDefinitionMainElement.Name);

                DXNodes.Add(dxUnitNode);

                foreach (var dxColumn in dxUnit.DXColumnDefinitionElement.Announced)
                {
                    var dxNodeRelated = new DXNode(dxColumn.Name);

                    var dxNodeRelation = new DXNodeRelation(dxColumn.Name, dxColumn.Name, null);

                    dxUnitNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }
            }

            foreach (var dxElement in dxElements)
            {
                var dxElementNode = new DXNode(dxElement.DXObjectDefinitionMainElement.Name);

                DXNodes.Add(dxElementNode);

                foreach (var dxColumn in dxElement.DXColumnDefinitionElement.Announced)
                {
                    var dxNodeRelated = new DXNode(dxColumn.Name);

                    var dxNodeRelation = new DXNodeRelation(dxColumn.Name, dxColumn.Name, null);

                    dxElementNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }
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
                            $"\"{dxElementName}\" AS \"{dxElementName}\" ON \"{dxElementName}\".\"DXUnitID\" = \"{dxUnitName}\".\"ID\"");

                    dxNode.AttachDXNode(dxNodeRelationToDXElement, dxNodeRelated);

                    var dxNodeRelationToDXUnit =
                        new DXNodeRelation(
                            dxUnitName,
                            $"U({dxUnitName})",
                            $"\"{dxUnitName}\" AS \"{dxUnitName}\" ON \"{dxUnitName}\".\"ID\" = {dxElementName}.\"DXUnitID\"");

                    dxNodeRelated.AttachDXNode(dxNodeRelationToDXUnit, dxNode);
                }

                foreach (var dxUnitRelationElement in dxUnit.DXUnitRelationElement.Announced)
                {
                    var dxUnitRelated = dxUnits.Single(x => x.ID == dxUnitRelationElement.TargetDXUnit);

                    var dxUnitNameRelated = dxUnitRelated.DXObjectDefinitionMainElement.Name;

                    var dxNodeRelated = this.Get(dxUnitNameRelated);

                    var dxRelation = dxRelations.Single(x =>
                        x.DXRelationDefinitionMainElement.ObjectNameLeft == dxUnitName
                        && x.DXRelationDefinitionMainElement.ObjectNameRight == dxUnitNameRelated);

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
                        dxNode.AttachDXNode(item.Key, item.Value);

                        var dxNodeRelationToDXUnit = new DXNodeRelation(
                            dxUnitName,
                            $"U({dxUnitName})",
                            $"\"{item.Value.Name}\" AS \"{item.Value.Name}\" ON \"{item.Value.Name}\".\"DXUnitID\" = \"{dxUnitName}\".\"ID\"");

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
                    var dxNodeRelated = new DXNode(dxColumn.Name);

                    var dxNodeRelation = new DXNodeRelation(dxColumn.Name, dxColumn.Name, null);

                    dxNode.AttachDXNode(dxNodeRelation, dxNodeRelated);
                }
            }
        }
        public string BuildSQLWhereExpression(string typeName, string dxFilter)
        {
            List<KeyValuePair<Guid, Guid>> idPairs = new List<KeyValuePair<Guid, Guid>>();

            var whereExpression = this.ProcessDXFilter(typeName, dxFilter, idPairs);
            this.LoadChainForDXColumns(typeName, dxFilter, idPairs);

            var fromExpression = GetFromExpression(typeName, idPairs);

            StringBuilder sb = new StringBuilder();
            sb.Append($"SELECT \"{typeName}\".\"ID\"\n");
            sb.Append($"FROM {fromExpression}");
            sb.Append($"WHERE {whereExpression}");

            return sb.ToString();
        }

        private void LoadChainForDXColumns(string typeName, string dxFilter, IList<KeyValuePair<Guid, Guid>> idPairs)
        {

        }

        private string ProcessDXFilter(string typeName, string dxFilter, IList<KeyValuePair<Guid, Guid>> idPairs)
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

                    var relatedNode = Get(baseDXNode, relationValue).Value;

                    KeyValuePair<Guid, Guid> idPair = new KeyValuePair<Guid, Guid>(baseDXNode.ID, relatedNode.ID);

                    if (!idPairs.Contains(idPair))
                    {
                        idPairs.Add(idPair);
                    }

                    baseDXNode = relatedNode;
                }

                var whereExpressionItem = route.Last();

                var propertyName = whereExpressionItem.Substring(0, whereExpressionItem.IndexOf(" "));
                var propertyValue = whereExpressionItem.Substring(propertyName.Length, whereExpressionItem.Length - propertyName.Length);

                whereExpressionItems.Add(new KeyValuePair<string, string>($"\"{baseDXNode.Name}\".\"{propertyName}\"{propertyValue}", expression.Value));
            }

            var whereExpression = String.Join(" ", whereExpressionItems.Select(x => $"{x.Value} {x.Key}")).Trim();

            return whereExpression;
        }

        private string GetFromExpression(string typeName, IList<KeyValuePair<Guid, Guid>> idPairs)
        {
            var fromExpression = new StringBuilder();

            fromExpression.Append($"\"{typeName}\" AS \"{typeName}\"\n");

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

        private string GetJoinForDXUnitRelation(DXRelationDefinitionUnit dxRelation)
        {
            var dxRelationData = dxRelation.DXRelationDefinitionMainElement;

            switch (dxRelationData.RelationType)
            {
                case DXRelationTypeEnum.ManyToMany:
                    return $"\"{dxRelationData.RelationTable}\" AS \"{dxRelationData.RelationTable}\" ON \"{dxRelationData.RelationTable}\".\"{dxRelationData.RelationNameLeft}\" = \"{dxRelationData.ObjectNameLeft}\".\"ID\"\nLEFT JOIN \"{dxRelationData.ObjectNameRight}\" AS \"{dxRelationData.ObjectNameRight}\" ON \"{dxRelationData.ObjectNameRight}\".\"ID\" = \"{dxRelationData.RelationTable}\".\"{dxRelationData.RelationNameRight}\"";
                case DXRelationTypeEnum.ManyToOne:
                case DXRelationTypeEnum.ManyToZeroOne:
                case DXRelationTypeEnum.OneToZeroOne:
                    return $"\"{dxRelationData.ObjectNameLeft}\" AS \"{dxRelationData.ObjectNameLeft}\" ON \"{dxRelationData.ObjectNameLeft}\".\"ID\" = \"{dxRelationData.ObjectNameRight}\".\"{dxRelationData.RelationNameLeft}\"";
                case DXRelationTypeEnum.OneToMany:
                case DXRelationTypeEnum.ZeroOneToMany:
                case DXRelationTypeEnum.ZeroOneToOne:
                case DXRelationTypeEnum.ZeroOneToZeroOne:
                    return $"\"{dxRelationData.ObjectNameLeft}\" AS \"{dxRelationData.ObjectNameLeft}\" ON \"{dxRelationData.ObjectNameLeft}\".\"{dxRelationData.RelationNameRight}\" = \"{dxRelationData.ObjectNameRight}\".\"ID\"";
                default: throw new Exception($"DXNode processing. There are no DXRelation type {dxRelationData.RelationType}");
            }
        }

        private DXNode Get(string name)
        {
            return DXNodes.Single(x => x.Name.Equals(name));
        }

        private DXNode Get(Guid id)
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
    }

    public class DXNode
    {
        public Guid ID { get; }
        public string Name { get; }
        public IDictionary<DXNodeRelation, DXNode> DXNodes { get; } = new Dictionary<DXNodeRelation, DXNode>();

        public DXNode(string name)
        {
            this.ID = Guid.NewGuid();
            this.Name = name;
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

    public struct DXNodeRelation
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