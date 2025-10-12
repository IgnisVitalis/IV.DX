using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Contracts.Persistence.ExpressionTree
{
    internal class DXUnitNode : DXBaseNode
    {
        private IEnumerable<DXJoinedQueryInfo> _queryInfos;

        public IEnumerable<DXJoinedQueryInfo> QueryInfos
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

        public DXRelationDefinitionUnit RelationInfo { get; set; }

        private string _tableNameAliasToJoin;

        public override string TableNameAliasToJoin
        {
            get
            {
                return this._tableNameAliasToJoin;
            }
        }

        private DXUnitNode(int x, int y, string value)
            : base(x, y, value)
        {

        }

        public static DXUnitNode CreateInstance(int x, int y, string value)
        {
            var instance = new DXUnitNode(x, y, value);

            return instance;
        }

        private IEnumerable<DXJoinedQueryInfo> GetQueryInfos()
        {
            var motherAsCoreNode = this.Mother as DXCoreNode;
            var motherAsEntityNode = this.Mother as DXUnitNode;

            switch (this.RelationInfo.DXRelationDefinitionMainElement.RelationType)
            {
                case DXRelationTypeEnum.OneToZeroOne:
                case DXRelationTypeEnum.OneToMany:
                case DXRelationTypeEnum.ZeroOneToMany:
                    {
                        DXJoinedQueryInfo queryInfo = new DXJoinedQueryInfo()
                        {
                            JoinedTableName = this.RelationInfo.DXRelationDefinitionMainElement.ObjectNameRight,
                            JoinedTableKey = this.RelationInfo.DXRelationDefinitionMainElement.RelationNameLeft,
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

                        return Enumerable.Empty<DXJoinedQueryInfo>().Append(queryInfo);
                    };
                case DXRelationTypeEnum.ZeroOneToOne:
                case DXRelationTypeEnum.ManyToOne:
                case DXRelationTypeEnum.ManyToZeroOne:
                    {
                        DXJoinedQueryInfo queryInfo = new DXJoinedQueryInfo()
                        {
                            JoinedTableName = this.RelationInfo.DXRelationDefinitionMainElement.ObjectNameRight,
                            JoinedTableKey = "ID",
                            MainTableKey = this.RelationInfo.DXRelationDefinitionMainElement.RelationNameRight,
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

                        return Enumerable.Empty<DXJoinedQueryInfo>().Append(queryInfo);
                    };
                case DXRelationTypeEnum.ZeroOneToZeroOne:
                    {
                        DXJoinedQueryInfo queryInfo = new DXJoinedQueryInfo()
                        {
                            JoinedTableAlias = this.TableNameAliasBase
                        };

                        if (this.RelationInfo.DXRelationDefinitionMainElement.RelationTable == this.RelationInfo.DXRelationDefinitionMainElement.ObjectNameLeft)
                        {
                            queryInfo.JoinedTableName = this.RelationInfo.DXRelationDefinitionMainElement.ObjectNameRight;
                            queryInfo.JoinedTableKey = "ID";
                            queryInfo.MainTableKey = this.RelationInfo.DXRelationDefinitionMainElement.RelationNameRight;
                        }
                        else
                        {
                            queryInfo.JoinedTableName = this.RelationInfo.DXRelationDefinitionMainElement.ObjectNameRight;
                            queryInfo.JoinedTableKey = this.RelationInfo.DXRelationDefinitionMainElement.RelationNameLeft;
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

                        return Enumerable.Empty<DXJoinedQueryInfo>().Append(queryInfo);
                    };
                case DXRelationTypeEnum.ManyToMany:
                    {
                        string motherTableName = null;

                        DXJoinedQueryInfo intermediate = new DXJoinedQueryInfo()
                        {
                            JoinedTableAlias = this.TableNameAliasBase + "_int",
                            JoinedTableName = this.RelationInfo.DXRelationDefinitionMainElement.RelationTable,
                            MainTableKey = "ID"
                        };

                        DXJoinedQueryInfo main = new DXJoinedQueryInfo()
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

                        if (this.RelationInfo.DXRelationDefinitionMainElement.ObjectNameLeft == motherTableName)
                        {
                            intermediate.JoinedTableKey = this.RelationInfo.DXRelationDefinitionMainElement.RelationNameLeft;

                            main.MainTableKey = this.RelationInfo.DXRelationDefinitionMainElement.RelationNameRight;
                            main.JoinedTableName = this.RelationInfo.DXRelationDefinitionMainElement.ObjectNameRight;
                        }
                        else
                        {
                            intermediate.JoinedTableKey = this.RelationInfo.DXRelationDefinitionMainElement.RelationNameRight;

                            main.MainTableKey = this.RelationInfo.DXRelationDefinitionMainElement.RelationNameLeft;
                            main.JoinedTableName = this.RelationInfo.DXRelationDefinitionMainElement.ObjectNameLeft;
                        }

                        this._tableNameAliasToJoin = this.TableNameAliasBase;

                        return Enumerable.Empty<DXJoinedQueryInfo>().Append(intermediate).Append(main);
                    }
                default:
                    break;
            }

            return null;
        }
    }
}