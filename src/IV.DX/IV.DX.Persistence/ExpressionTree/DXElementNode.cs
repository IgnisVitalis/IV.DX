namespace IV.DX.Contracts.Persistence.ExpressionTree
{
    internal class DXElementNode : DXBaseNode
    {
        private DXJoinedQueryInfo _queryInfo;

        public DXJoinedQueryInfo QueryInfo
        {
            get
            {
                if (this._queryInfo == null)
                {
                    this._queryInfo = this.GetQueryInfo();
                }

                return this._queryInfo;
            }
        }

        private DXElementNode(int x, int y, string value)
            : base(x, y, value)
        {

        }

        public static DXElementNode CreateInstance(int x, int y, string value)
        {
            var instance = new DXElementNode(x, y, value);

            return instance;
        }

        private DXJoinedQueryInfo GetQueryInfo()
        {
            var result = new DXJoinedQueryInfo()
            {
                JoinedTableName = this.Value,
                JoinedTableAlias = base.TableNameAliasBase,
                JoinedTableKey = "ObjectID",
                MainTableKey = "ID"
            };

            var motherAsCoreNode = this.Mother as DXCoreNode;

            if (motherAsCoreNode != null)
            {
                result.MainTableAlias = motherAsCoreNode.MainTableAlias;

                return result;
            }

            var motherAsDXUnitNode = this.Mother as DXUnitNode;

            if (motherAsDXUnitNode != null)
            {
                result.MainTableAlias = motherAsDXUnitNode.TableNameAliasToJoin;

                return result;
            }

            throw new Exception("DXElementNode can have only CoreNode or DXUnitNode as mother node. Please check DX query.");
        }
    }
}