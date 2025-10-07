using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;

namespace IV.DX.Contracts.Persistence.ExpressionTree
{
    internal class OrientedTree
    {
        public CoreNode CoreNode { get; private set; }

        public IEnumerable<PropertyNode> Leaves { get; private set; }

        public IEnumerable<BaseNode> AllNodes { get; private set; }

        public IEnumerable<BaseNode> AllNodesWithoutCoreAndLeaves { get; private set; }

        public bool IsValid { get; private set; }

        public IEnumerable<KeyValuePair<string, LogicOperation>> Expressions { get; private set; }

        private OrientedTree(CoreNode coreNode)
        {
            this.CoreNode = coreNode;
            this.Leaves = Enumerable.Empty<PropertyNode>();
            this.AllNodesWithoutCoreAndLeaves = Enumerable.Empty<BaseNode>();
            this.AllNodes = Enumerable.Empty<BaseNode>().Append(this.CoreNode);
            this.Expressions = Enumerable.Empty<KeyValuePair<string, LogicOperation>>();
        }

        public static OrientedTree CreateInstance(string type)
        {
            var coreNode = CoreNode.CreateInstance(type.Trim());

            var instance = new OrientedTree(coreNode);

            return instance;
        }

        public void Load(string fullExpression)
        {
            var expressions = fullExpression?.Trim().SplitAndKeep(new string[] { " and ", " or ", " AND ", " OR ", " And ", " Or " }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();

            int counter = 0;

            if (expressions != null && expressions.Any())
            {
                this.Load(expressions.First().Key, counter++, LogicOperation.AND);

                foreach (var expression in expressions.Skip(1))
                {
                    this.Load(expression.Key, counter++, this.ConvertToLogicOperation(expression.Value));
                }
            }
        }

        private LogicOperation ConvertToLogicOperation(string logicOpeationStr)
        {
            switch (logicOpeationStr)
            {
                case " AND ":
                case " And ":
                case " and ": return LogicOperation.AND;
                case " OR ":
                case " Or ":
                case " or ": return LogicOperation.OR;
                default:
                    throw new Exception($"Logic operation '{logicOpeationStr}' isn't supported yet.");
            }
        }

        public void Load(string expression, int expressionOrder, LogicOperation logicOpeation)
        {
            int level = 0;

            var loweredExpression = expression.Trim();

            var existingExpression = this.Expressions.SingleOrDefault(x => x.Key == expression);

            if (!existingExpression.Equals(default(KeyValuePair<string, LogicOperation>)))
            {
                if (existingExpression.Value != logicOpeation)
                {
                    throw new Exception($"Please check esql expression {expression}. It's duplicate and has wrong logic opeation {logicOpeation}.");
                }

                return;
            }
            else
            {
                this.Expressions = this.Expressions.Append(new KeyValuePair<string, LogicOperation>(loweredExpression, logicOpeation));
            }

            var expressionItems = loweredExpression.Split('.');

            var existingChilds = this.CoreNode.Childs;

            BaseNode lastExistingNode = null;

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
                    lastExistingNode = lastExistingNode.CreateEntityNodeInstanceChild(level, this.GetLastYByX(level) + 1, item);
                }
                else
                {
                    lastExistingNode = lastExistingNode.CreateBlockNodeInstanceChild(level, this.GetLastYByX(level) + 1, item);
                }

                level++;
                this.AllNodes = this.AllNodes.Append(lastExistingNode);
                this.AllNodesWithoutCoreAndLeaves = this.AllNodesWithoutCoreAndLeaves.Append(lastExistingNode);
            }

            lastExistingNode = lastExistingNode.CreatePropertyNodeInstanceChild(level, this.GetLastYByX(level) + 1, expressionItems.Last(), expressionOrder, logicOpeation);

            this.AllNodes = this.AllNodes.Append(lastExistingNode);
            this.Leaves = this.Leaves.Append(lastExistingNode as PropertyNode);
        }

        public int GetLastYByX(int x)
        {
            var allNodesByX = this.AllNodes.Where(node => node.X == x);

            if (allNodesByX.Count() == 0)
                return -1;

            var maxY = allNodesByX.Max(node => node.Y);

            return maxY;
        }

        public BaseNode GetNode(int x, int y)
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

                if (ProcessNodeAsEntityNode(item, relationInfos))
                    continue;

                if (ProcessNodeAsBlockNode(item, relationInfos))
                    continue;

                if (ProcessNodeAsPropertyNode(item, relationInfos))
                    continue;
            }
        }

        private bool ProcessNodeAsCoreNode(BaseNode node, IEnumerable<DXRelationDefinitionUnit> relationInfos)
        {
            var coreNode = node as CoreNode;

            if (coreNode == null)
            {
                return false;
            }

            return true;
        }

        private bool ProcessNodeAsEntityNode(BaseNode node, IEnumerable<DXRelationDefinitionUnit> relationInfos)
        {
            var entityNode = node as EntityNode;

            if (entityNode == null)
            {
                return false;
            }

            DXRelationDefinitionUnit relationInfo = null;

            var motherNodeAsEntityNode = entityNode.Mother as EntityNode;
            var motherNodeAsCoreNode = entityNode.Mother as CoreNode;

            if (motherNodeAsEntityNode != null)
            {
                relationInfo = relationInfos.SingleOrDefault(x =>
                    x.DXRelationDefinitionMainElement.ObjectNameLeft == motherNodeAsEntityNode.RelationInfo.DXRelationDefinitionMainElement.ObjectNameRight
                    && x.DXRelationDefinitionMainElement.RelationNameRight == entityNode.RelationName);
            }
            else if (motherNodeAsCoreNode != null)
            {
                relationInfo = relationInfos.SingleOrDefault(x =>
                    x.DXRelationDefinitionMainElement.ObjectNameLeft == motherNodeAsCoreNode.Value
                    && x.DXRelationDefinitionMainElement.RelationNameRight == entityNode.RelationName);
            }

            entityNode.RelationInfo = relationInfo;

            if (relationInfo == null)
            {
                throw new Exception($"There are not any relation info for {node.Value}");
            }

            return true;
        }

        private bool ProcessNodeAsBlockNode(BaseNode node, IEnumerable<DXRelationDefinitionUnit> relationInfos)
        {
            var blockNode = node as BlockNode;

            if (blockNode == null)
            {
                return false;
            }

            return true;
        }

        private bool ProcessNodeAsPropertyNode(BaseNode node, IEnumerable<DXRelationDefinitionUnit> relationInfos)
        {
            var propertyNode = node as PropertyNode;

            if (propertyNode == null)
            {
                return false;
            }

            return true;
        }
    }
}