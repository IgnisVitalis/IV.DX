using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Contracts.Persistence.ExpressionTree
{
    internal class EntityNode : BaseNode
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

        public DXRelationDefinitionUnit RelationInfo { get; set; }

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

            switch (this.RelationInfo.DXRelationDefinitionMainElement.RelationType)
            {
                case DXRelationTypeEnum.OneToZeroOne:
                case DXRelationTypeEnum.OneToMany:
                case DXRelationTypeEnum.ZeroOneToMany:
                    {
                        JoinedQueryInfo queryInfo = new JoinedQueryInfo()
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

                        return Enumerable.Empty<JoinedQueryInfo>().Append(queryInfo);
                    };
                case DXRelationTypeEnum.ZeroOneToOne:
                case DXRelationTypeEnum.ManyToOne:
                case DXRelationTypeEnum.ManyToZeroOne:
                    {
                        JoinedQueryInfo queryInfo = new JoinedQueryInfo()
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

                        return Enumerable.Empty<JoinedQueryInfo>().Append(queryInfo);
                    };
                case DXRelationTypeEnum.ZeroOneToZeroOne:
                    {
                        JoinedQueryInfo queryInfo = new JoinedQueryInfo()
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

                        return Enumerable.Empty<JoinedQueryInfo>().Append(queryInfo);
                    };
                case DXRelationTypeEnum.ManyToMany:
                    {
                        string motherTableName = null;

                        JoinedQueryInfo intermediate = new JoinedQueryInfo()
                        {
                            JoinedTableAlias = this.TableNameAliasBase + "_int",
                            JoinedTableName = this.RelationInfo.DXRelationDefinitionMainElement.RelationTable,
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

                        return Enumerable.Empty<JoinedQueryInfo>().Append(intermediate).Append(main);
                    }
                default:
                    break;
            }

            return null;
        }
    }
}