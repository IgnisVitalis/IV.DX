using System.Collections.Generic;
using System.Linq;

namespace IV.DX.Contracts.Persistence.ExpressionTree
{
    internal abstract class BaseNode
    {
        public string Value { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        protected string TableNameAliasBase { get; private set; }

        public virtual string TableNameAliasToJoin
        {
            get
            {
                return this.TableNameAliasBase;
            }
        }

        public IEnumerable<BaseNode> Childs { get; protected set; }
        public BaseNode Mother { get; protected set; }

        protected BaseNode(int x, int y, string value)
        {
            this.X = x;
            this.Y = y;
            this.Value = value;
            this.TableNameAliasBase = $"t_{x}_{y}";
            this.Childs = Enumerable.Empty<BaseNode>();
        }

        public BlockNode CreateBlockNodeInstanceChild(int x, int y, string value)
        {
            var child = BlockNode.CreateInstance(x, y, value);

            child.Mother = this;

            this.Childs = this.Childs.Append(child);

            return child;
        }

        public PropertyNode CreatePropertyNodeInstanceChild(int x, int y, string value, int expressiony, LogicOperation logicOperation)
        {
            var child = PropertyNode.CreateInstance(x, y, value, expressiony, logicOperation);

            child.Mother = this;

            this.Childs = this.Childs.Append(child);

            return child;
        }

        public EntityNode CreateEntityNodeInstanceChild(int x, int y, string value)
        {
            var leaf = EntityNode.CreateInstance(x, y, value);

            leaf.Mother = this;

            this.Childs = this.Childs.Append(leaf);

            return leaf;
        }

        public BaseNode FindChildByVaue(string value)
        {
            var nodeValueTrimed = value.Trim().ToLower();

            var existingChild = this.Childs.SingleOrDefault(x => x.Value == nodeValueTrimed);

            return existingChild;
        }
    }
}