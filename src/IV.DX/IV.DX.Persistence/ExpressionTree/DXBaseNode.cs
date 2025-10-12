namespace IV.DX.Contracts.Persistence.ExpressionTree
{
    internal abstract class DXBaseNode
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

        public IEnumerable<DXBaseNode> Childs { get; protected set; }
        public DXBaseNode Mother { get; protected set; }

        protected DXBaseNode(int x, int y, string value)
        {
            this.X = x;
            this.Y = y;
            this.Value = value;
            this.TableNameAliasBase = $"t_{x}_{y}";
            this.Childs = Enumerable.Empty<DXBaseNode>();
        }

        public DXElementNode CreateBlockNodeInstanceChild(int x, int y, string value)
        {
            var child = DXElementNode.CreateInstance(x, y, value);

            child.Mother = this;

            this.Childs = this.Childs.Append(child);

            return child;
        }

        public DXPropertyNode CreatePropertyNodeInstanceChild(int x, int y, string value, int expressiony, DXLogicOperation logicOperation)
        {
            var child = DXPropertyNode.CreateInstance(x, y, value, expressiony, logicOperation);

            child.Mother = this;

            this.Childs = this.Childs.Append(child);

            return child;
        }

        public DXUnitNode CreateEntityNodeInstanceChild(int x, int y, string value)
        {
            var leaf = DXUnitNode.CreateInstance(x, y, value);

            leaf.Mother = this;

            this.Childs = this.Childs.Append(leaf);

            return leaf;
        }

        public DXBaseNode FindChildByVaue(string value)
        {
            var nodeValueTrimed = value.Trim().ToLower();

            var existingChild = this.Childs.SingleOrDefault(x => x.Value == nodeValueTrimed);

            return existingChild;
        }
    }
}