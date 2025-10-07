using IV.DX.Contracts.Common.Enums;
using IV.DX.Contracts.Common.Models;

namespace IV.DX.Contracts.Persistence.ExpressionTree
{
    public class EntityNode : BaseNode
    {
        private IEnumerable<JoinedQueryInfo> _queryInfos;

        public IEnumerable<JoinedQueryInfo> QueryInfos
        {
            get
            {
                if (this._queryInfos == null)
                {
                    this._queryInfos = this.GetQueryInfos();
                }

                return this._queryInfos;
            }
        }

        private string _relationName;
        public string RelationName
        {
            get
            {
                if (string.IsNullOrEmpty(this._relationName))
                {
                    this._relationName = base.Value.Substring(2, base.Value.Length - 3);
                }

                return this._relationName;
            }
        }

        public DPRelationObject RelationInfo { get; set; }

        private string _tableNameAliasToJoin;

        public override string TableNameAliasToJoin
        {
            get
            {
                return this._tableNameAliasToJoin;
            }
        }

        private EntityNode(int x, int y, string value)
            : base(x, y, value)
        {

        }

        public static EntityNode CreateInstance(int x, int y, string value)
        {
            var instance = new EntityNode(x, y, value);

            return instance;
        }

        private IEnumerable<JoinedQueryInfo> GetQueryInfos()
        {
            var motherAsCoreNode = this.Mother as CoreNode;
            var motherAsEntityNode = this.Mother as EntityNode;

            switch (this.RelationInfo.DPRelationGenBlock.RelationType)
            {
                case DPRelationTypeEnum.OneToZeroOne:
                case DPRelationTypeEnum.OneToMany:
                case DPRelationTypeEnum.ZeroOneToMany:
                    {
                        JoinedQueryInfo queryInfo = new JoinedQueryInfo()
                        {
                            JoinedTableName = this.RelationInfo.DPRelationGenBlock.ObjectNameRight,
                            JoinedTableKey = this.RelationInfo.DPRelationGenBlock.RelationNameLeft,
                            MainTableKey = "ID",
                            JoinedTableAlias = this.TableNameAliasBase
                        };

                        if (motherAsCoreNode != null)
                        {
                            queryInfo.MainTableAlias = motherAsCoreNode.MainTableAlias;
                        }
                        else if (motherAsEntityNode != null)
                        {
                            queryInfo.MainTableAlias = motherAsEntityNode.QueryInfos.Last().JoinedTableAlias;
                        }

                        this._tableNameAliasToJoin = this.TableNameAliasBase;

                        return Enumerable.Empty<JoinedQueryInfo>().Append(queryInfo);
                    };
                case DPRelationTypeEnum.ZeroOneToOne:
                case DPRelationTypeEnum.ManyToOne:
                case DPRelationTypeEnum.ManyToZeroOne:
                    {
                        JoinedQueryInfo queryInfo = new JoinedQueryInfo()
                        {
                            JoinedTableName = this.RelationInfo.DPRelationGenBlock.ObjectNameRight,
                            JoinedTableKey = "ID",
                            MainTableKey = this.RelationInfo.DPRelationGenBlock.RelationNameRight,
                            JoinedTableAlias = this.TableNameAliasBase
                        };

                        if (motherAsCoreNode != null)
                        {
                            queryInfo.MainTableAlias = motherAsCoreNode.MainTableAlias;
                        }
                        else if (motherAsEntityNode != null)
                        {
                            queryInfo.MainTableAlias = motherAsEntityNode.QueryInfos.Last().JoinedTableAlias;
                        }

                        this._tableNameAliasToJoin = this.TableNameAliasBase;

                        return Enumerable.Empty<JoinedQueryInfo>().Append(queryInfo);
                    };
                case DPRelationTypeEnum.ZeroOneToZeroOne:
                    {
                        JoinedQueryInfo queryInfo = new JoinedQueryInfo()
                        {
                            JoinedTableAlias = this.TableNameAliasBase
                        };

                        if (this.RelationInfo.DPRelationGenBlock.RelationTable == this.RelationInfo.DPRelationGenBlock.ObjectNameLeft)
                        {
                            queryInfo.JoinedTableName = this.RelationInfo.DPRelationGenBlock.ObjectNameRight;
                            queryInfo.JoinedTableKey = "ID";
                            queryInfo.MainTableKey = this.RelationInfo.DPRelationGenBlock.RelationNameRight;
                        }
                        else
                        {
                            queryInfo.JoinedTableName = this.RelationInfo.DPRelationGenBlock.ObjectNameRight;
                            queryInfo.JoinedTableKey = this.RelationInfo.DPRelationGenBlock.RelationNameLeft;
                            queryInfo.MainTableKey = "ID";
                        }

                        if (motherAsCoreNode != null)
                        {
                            queryInfo.MainTableAlias = motherAsCoreNode.MainTableAlias;
                        }
                        else if (motherAsEntityNode != null)
                        {
                            queryInfo.MainTableAlias = motherAsEntityNode.QueryInfos.Last().JoinedTableAlias;
                        }

                        this._tableNameAliasToJoin = this.TableNameAliasBase;

                        return Enumerable.Empty<JoinedQueryInfo>().Append(queryInfo);
                    };
                case DPRelationTypeEnum.ManyToMany:
                    {
                        string motherTableName = null;

                        JoinedQueryInfo intermediate = new JoinedQueryInfo()
                        {
                            JoinedTableAlias = this.TableNameAliasBase + "_int",
                            JoinedTableName = this.RelationInfo.DPRelationGenBlock.RelationTable,
                            MainTableKey = "ID"
                        };

                        JoinedQueryInfo main = new JoinedQueryInfo()
                        {
                            JoinedTableAlias = this.TableNameAliasBase,
                            JoinedTableKey = "ID",
                            MainTableAlias = intermediate.JoinedTableAlias
                        };

                        if (motherAsCoreNode != null)
                        {
                            intermediate.MainTableAlias = motherAsCoreNode.MainTableAlias;
                            motherTableName = motherAsCoreNode.Value;
                        }
                        else if (motherAsEntityNode != null)
                        {
                            var motherLastQueryInfo = motherAsEntityNode.QueryInfos.Last();

                            intermediate.MainTableAlias = motherLastQueryInfo.JoinedTableAlias;
                            motherTableName = motherLastQueryInfo.JoinedTableName;
                        };

                        if (this.RelationInfo.DPRelationGenBlock.ObjectNameLeft == motherTableName)
                        {
                            intermediate.JoinedTableKey = this.RelationInfo.DPRelationGenBlock.RelationNameLeft;

                            main.MainTableKey = this.RelationInfo.DPRelationGenBlock.RelationNameRight;
                            main.JoinedTableName = this.RelationInfo.DPRelationGenBlock.ObjectNameRight;
                        }
                        else
                        {
                            intermediate.JoinedTableKey = this.RelationInfo.DPRelationGenBlock.RelationNameRight;

                            main.MainTableKey = this.RelationInfo.DPRelationGenBlock.RelationNameLeft;
                            main.JoinedTableName = this.RelationInfo.DPRelationGenBlock.ObjectNameLeft;
                        }

                        this._tableNameAliasToJoin = this.TableNameAliasBase;

                        return Enumerable.Empty<JoinedQueryInfo>().Append(intermediate).Append(main);
                    }
                default:
                    break;
            }

            return null;
        }
    }
}