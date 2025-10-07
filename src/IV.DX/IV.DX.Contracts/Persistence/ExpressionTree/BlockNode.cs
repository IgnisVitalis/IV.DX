using System;

namespace IV.DX.Contracts.Persistence.ExpressionTree
{
    public class BlockNode : BaseNode
    {
        private JoinedQueryInfo _queryInfo;

        public JoinedQueryInfo QueryInfo
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

        private BlockNode(int x, int y, string value)
            : base(x, y, value)
        {

        }

        public static BlockNode CreateInstance(int x, int y, string value)
        {
            var instance = new BlockNode(x, y, value);

            return instance;
        }

        private JoinedQueryInfo GetQueryInfo()
        {
            var result = new JoinedQueryInfo()
            {
                JoinedTableName = this.Value,
                JoinedTableAlias = base.TableNameAliasBase,
                JoinedTableKey = "ObjectID",
                MainTableKey = "ID"
            };

            var motherAsCoreNode = this.Mother as CoreNode;

            if (motherAsCoreNode != null)
            {
                result.MainTableAlias = motherAsCoreNode.MainTableAlias;

                return result;
            }

            var motherAsEntityNode = this.Mother as EntityNode;

            if (motherAsEntityNode != null)
            {
                result.MainTableAlias = motherAsEntityNode.TableNameAliasToJoin;

                return result;
            }

            throw new Exception("BlockNode can have only CoreNode or EntityNode as mother node. Please check ESQL query.");
        }
    }
}