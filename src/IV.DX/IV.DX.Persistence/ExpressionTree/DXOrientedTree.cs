using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;

namespace IV.DX.Contracts.Persistence.ExpressionTree
{
    internal class DXOrientedTree
    {
        public DXCoreNode CoreNode { get; private set; }

        public IEnumerable<DXPropertyNode> Leaves { get; private set; }

        public IEnumerable<DXBaseNode> AllNodes { get; private set; }

        public IEnumerable<DXBaseNode> AllNodesWithoutCoreAndLeaves { get; private set; }

        public bool IsValid { get; private set; }

        public IEnumerable<KeyValuePair<string, DXLogicOperation>> Expressions { get; private set; }

        private DXOrientedTree(DXCoreNode coreNode)
        {
            this.CoreNode = coreNode;
            this.Leaves = Enumerable.Empty<DXPropertyNode>();
            this.AllNodesWithoutCoreAndLeaves = Enumerable.Empty<DXBaseNode>();
            this.AllNodes = Enumerable.Empty<DXBaseNode>().Append(this.CoreNode);
            this.Expressions = Enumerable.Empty<KeyValuePair<string, DXLogicOperation>>();
        }

        public static DXOrientedTree CreateInstance(string type)
        {
            var coreNode = DXCoreNode.CreateInstance(type.Trim());

            var instance = new DXOrientedTree(coreNode);

            return instance;
        }

        public void Load(string fullExpression)
        {
            var expressions = fullExpression?.Trim().SplitAndKeep(new string[] { " and ", " or ", " AND ", " OR ", " And ", " Or " }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();

            int counter = 0;

            if (expressions != null && expressions.Any())
            {
                this.Load(expressions.First().Key, counter++, DXLogicOperation.AND);

                foreach (var expression in expressions.Skip(1))
                {
                    this.Load(expression.Key, counter++, this.ConvertToLogicOperation(expression.Value));
                }
            }
        }

        private DXLogicOperation ConvertToLogicOperation(string logicOpeationStr)
        {
            switch (logicOpeationStr)
            {
                case " AND ":
                case " And ":
                case " and ": return DXLogicOperation.AND;
                case " OR ":
                case " Or ":
                case " or ": return DXLogicOperation.OR;
                default:
                    throw new Exception($"Logic operation '{logicOpeationStr}' isn't supported yet.");
            }
        }

        public void Load(string expression, int expressionOrder, DXLogicOperation logicOpeation)
        {
            int level = 0;

            var loweredExpression = expression.Trim();

            var existingExpression = this.Expressions.SingleOrDefault(x => x.Key == expression);

            if (!existingExpression.Equals(default(KeyValuePair<string, DXLogicOperation>)))
            {
                if (existingExpression.Value != logicOpeation)
                {
                    throw new Exception($"Please check dx expression {expression}. It's duplicate and has wrong logic opeation {logicOpeation}.");
                }

                return;
            }
            else
            {
                this.Expressions = this.Expressions.Append(new KeyValuePair<string, DXLogicOperation>(loweredExpression, logicOpeation));
            }

            var expressionItems = loweredExpression.Split('.');

            var existingChilds = this.CoreNode.Childs;

            DXBaseNode lastExistingNode = null;

            var levelCounter = 0;

            foreach (var item in expressionItems.ToList())
            {
                var existingNode = existingChilds.SingleOrDefault(x => x.Value == item);

                if (existingNode != null)
                {
                    lastExistingNode = existingNode;
                    existingChilds = lastExistingNode.Childs;
                    levelCounter++;
                    continue;
                }
                else
                {
                    break;
                }
            }

            if (lastExistingNode == null)
            {
                lastExistingNode = this.CoreNode;
            }

            level = levelCounter + 1;

            foreach (var item in expressionItems.Skip(levelCounter).SkipLast(1).ToList())
            {
                if ((item.StartsWith("r(") || item.StartsWith("R(")) && item.EndsWith(')'))
                {
                    lastExistingNode = lastExistingNode.CreateDXUnitNodeInstanceChild(level, this.GetLastYByX(level) + 1, item);
                }
                else
                {
                    lastExistingNode = lastExistingNode.CreateDXElementNodeInstanceChild(level, this.GetLastYByX(level) + 1, item);
                }

                level++;
                this.AllNodes = this.AllNodes.Append(lastExistingNode);
                this.AllNodesWithoutCoreAndLeaves = this.AllNodesWithoutCoreAndLeaves.Append(lastExistingNode);
            }

            lastExistingNode = lastExistingNode.CreatePropertyNodeInstanceChild(level, this.GetLastYByX(level) + 1, expressionItems.Last(), expressionOrder, logicOpeation);

            this.AllNodes = this.AllNodes.Append(lastExistingNode);
            this.Leaves = this.Leaves.Append(lastExistingNode as DXPropertyNode);
        }

        public int GetLastYByX(int x)
        {
            var allNodesByX = this.AllNodes.Where(node => node.X == x);

            if (allNodesByX.Count() == 0)
                return -1;

            var maxY = allNodesByX.Max(node => node.Y);

            return maxY;
        }

        public DXBaseNode GetNode(int x, int y)
        {
            var result = this.AllNodes.SingleOrDefault(node => node.X == x && node.Y == y);

            return result;
        }

        public int GetLastX()
        {
            var result = this.AllNodes.Max(node => node.X);

            return result;
        }

        public void LoadAdditionalInfosToNodes(IEnumerable<DXRelationDefinitionUnit> relationInfos)
        {
            foreach (var item in this.AllNodes)
            {
                if (ProcessNodeAsCoreNode(item, relationInfos))
                    continue;

                if (ProcessNodeAsDXUnitNode(item, relationInfos))
                    continue;

                if (ProcessNodeAsDXElementNode(item, relationInfos))
                    continue;

                if (ProcessNodeAsPropertyNode(item, relationInfos))
                    continue;
            }
        }

        private bool ProcessNodeAsCoreNode(DXBaseNode node, IEnumerable<DXRelationDefinitionUnit> relationInfos)
        {
            var coreNode = node as DXCoreNode;

            if (coreNode == null)
            {
                return false;
            }

            return true;
        }

        private bool ProcessNodeAsDXUnitNode(DXBaseNode node, IEnumerable<DXRelationDefinitionUnit> relationInfos)
        {
            var dxUnitNode = node as DXUnitNode;

            if (dxUnitNode == null)
            {
                return false;
            }

            DXRelationDefinitionUnit relationInfo = null;

            var motherNodeAsDXUnitNode = dxUnitNode.Mother as DXUnitNode;
            var motherNodeAsCoreNode = dxUnitNode.Mother as DXCoreNode;

            if (motherNodeAsDXUnitNode != null)
            {
                relationInfo = relationInfos.SingleOrDefault(x =>
                    x.DXRelationDefinitionMainElement.ObjectNameLeft == motherNodeAsDXUnitNode.RelationInfo.DXRelationDefinitionMainElement.ObjectNameRight
                    && x.DXRelationDefinitionMainElement.RelationNameRight == dxUnitNode.RelationName);
            }
            else if (motherNodeAsCoreNode != null)
            {
                relationInfo = relationInfos.SingleOrDefault(x =>
                    x.DXRelationDefinitionMainElement.ObjectNameLeft == motherNodeAsCoreNode.Value
                    && x.DXRelationDefinitionMainElement.RelationNameRight == dxUnitNode.RelationName);
            }

            dxUnitNode.RelationInfo = relationInfo;

            if (relationInfo == null)
            {
                throw new Exception($"There are not any relation info for {node.Value}");
            }

            return true;
        }

        private bool ProcessNodeAsDXElementNode(DXBaseNode node, IEnumerable<DXRelationDefinitionUnit> relationInfos)
        {
            var dxElementNode = node as DXElementNode;

            if (dxElementNode == null)
            {
                return false;
            }

            return true;
        }

        private bool ProcessNodeAsPropertyNode(DXBaseNode node, IEnumerable<DXRelationDefinitionUnit> relationInfos)
        {
            var propertyNode = node as DXPropertyNode;

            if (propertyNode == null)
            {
                return false;
            }

            return true;
        }
    }
}