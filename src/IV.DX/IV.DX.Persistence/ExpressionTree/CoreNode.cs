namespace IV.DX.Contracts.Persistence.ExpressionTree
{
    internal class CoreNode : BaseNode
    {
        public string MainTableAlias
        {
            get
            {
                return base.TableNameAliasBase;
            }
        }

        private CoreNode(string value)
            : base(0, 0, value)
        {

        }

        public static CoreNode CreateInstance(string value)
        {
            return new CoreNode(value);
        }
    }
}