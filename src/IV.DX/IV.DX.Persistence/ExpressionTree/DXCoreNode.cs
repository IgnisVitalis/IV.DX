namespace IV.DX.Contracts.Persistence.ExpressionTree
{
    internal class DXCoreNode : DXBaseNode
    {
        public string MainTableAlias
        {
            get
            {
                return base.TableNameAliasBase;
            }
        }

        private DXCoreNode(string value)
            : base(0, 0, value)
        {

        }

        public static DXCoreNode CreateInstance(string value)
        {
            return new DXCoreNode(value);
        }
    }
}