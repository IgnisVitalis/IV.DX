using IV.DX.Contracts.Persistence.ExpressionTree;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Abstractions;
using IV.DX.Persistence.Models;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;

namespace IV.DX.Persistence.SQLQueryHelpers
{
    internal class MySQLQueryHelper : ISQLQueryHelper
    {
        public MySQLQueryHelper()
        {
        }

        public string GetQuery(string typeName, string esqlWhereExpression, IEnumerable<DXRelationDefinitionUnit> relationInfos)
        {
            if (string.IsNullOrEmpty(esqlWhereExpression))
                return GetSQLQueryToSelectIDFromTable(typeName);

            var result = this.ConvertToQueryContainer(typeName, esqlWhereExpression, relationInfos);

            return result.Query;
        }

        public QueryContainer ConvertToQueryContainer(
           string entityType,
           string esqlWhereExpression,
           IEnumerable<DXRelationDefinitionUnit> relationInfos)
        {
            OrientedTree expressionTree = OrientedTree.CreateInstance(entityType);

            expressionTree.Load(esqlWhereExpression);

            expressionTree.LoadAdditionalInfosToNodes(relationInfos);

            QueryContainer result = new QueryContainer
            {
                SelectExpression = this.GetSelectQuery(expressionTree.CoreNode)
            };

            IEnumerable<string> leftJoins = Enumerable.Empty<string>();

            foreach (var item in expressionTree.AllNodesWithoutCoreAndLeaves)
            {
                var nodeAsEntityNode = item as EntityNode;
                var nodeAsBlockNode = item as BlockNode;

                if (nodeAsEntityNode != null)
                {
                    leftJoins = leftJoins.Concat(nodeAsEntityNode.QueryInfos.Select(x => this.GetLeftJoinQuery(x)));
                }
                else if (nodeAsBlockNode != null)
                {
                    leftJoins = leftJoins.Append(this.GetLeftJoinQuery(nodeAsBlockNode.QueryInfo));
                }
            }

            result.LeftJoinsExpression = string.Join(" ", leftJoins);

            result.WhereExpression = string.Join(" ", expressionTree.Leaves.OrderBy(x => x.ExpressionOrder).Select(x => this.GetWhereExpressionWithPropertyAndLogicOpeation(x)));

            return result;
        }

        private string GetLeftJoinQuery(JoinedQueryInfo queryInfo)
        {
            return $"LEFT JOIN {queryInfo.JoinedTableName} AS {queryInfo.JoinedTableAlias} ON {queryInfo.JoinedTableAlias}.{queryInfo.JoinedTableKey} = {queryInfo.MainTableAlias}.{queryInfo.MainTableKey}";
        }

        public string GetWhereExpressionWithPropertyAndLogicOpeation(PropertyNode propertyNode)
        {
            StringBuilder sb = new StringBuilder();

            if (propertyNode.ExpressionOrder > 0)
            {
                sb.Append(propertyNode.LogicOperation);
                sb.Append(" ");
            }

            sb.Append($"{propertyNode.Mother.TableNameAliasToJoin}.{propertyNode.Value}");

            return sb.ToString();
        }


        public string GetSelectQuery(CoreNode coreNode)
        {
            return $"SELECT {coreNode.MainTableAlias}.ID FROM {coreNode.Value} AS {coreNode.MainTableAlias}";
        }

        public string GetSQLQueryToCreateTable(DPObjectDescObject dataBlock)
        {
            // CREATE TABLE IF NOT EXISTS tasks (
            //     task_id INT AUTO_INCREMENT PRIMARY KEY,
            //     title VARCHAR(255) NOT NULL,
            //     start_date DATE,
            //     due_date DATE,
            //     status TINYINT NOT NULL,
            //     priority TINYINT NOT NULL,
            //     description TEXT,
            //     created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            // )  ENGINE=INNODB;

            StringBuilder sb = new StringBuilder();

            sb.Append($"CREATE TABLE IF NOT EXISTS {dataBlock.DXUnitDefinitionMainElement.Name}(");

            var clmDefList = dataBlock.DXColumnDefinitionElement.Announced.Select(x => this.GetSQLColumnDefinitionToAddInTable(x));

            var clmUniqueList = dataBlock.DXUniqueColumnsElement.Announced.Select(x => this.GetSQLColumnsUniqueToAddInTable(x));

            sb.Append(string.Join(",", clmDefList));

            if (clmUniqueList.Count() > 0)
            {
                sb.Append(",");
                sb.Append(string.Join(",", clmUniqueList));
            }

            sb.Append(")ENGINE=INNODB");

            return sb.ToString();
        }

        public string GetSQLColumnDefinitionToChangeInTable(
            DXColumnDefinitionElement DXColumnDefinitionElementNew,
            DXColumnDefinitionElement DXColumnDefinitionElementExisting)
        {
            // CHANGE COLUMN `name` `nameNew` DECIMAL NULL DEFAULT 10,

            var mySQLQueryToChangeColumn = $"CHANGE COLUMN `{DXColumnDefinitionElementExisting.Name}` {this.GetSQLColumnDefinitionToAddInTable(DXColumnDefinitionElementNew)}";

            return mySQLQueryToChangeColumn;
        }

        public string GetSQLQueryToDropTable(DPObjectDescObject dataBlock)
        {
            // TODO: need to find solution how to drop table by ObjectID
            return GetSQLQueryToDropTable(dataBlock.DXUnitDefinitionMainElement.Name);
        }

        public string GetSQLQueryToDropTable(string tableName)
        {
            // TODO: need to find solution how to drop table by ObjectID
            return $"DROP TABLE IF EXISTS {tableName}";
        }

        private string GetSQLColumnsUniqueToAddInTable(DXUniqueColumnsElement clmDesc)
        {
            var columns = clmDesc.Columns.Split(',').Select(x => x.Trim());

            var columnsWithBrackets = columns.Select(x => $"`{x}`");

            var uniqueKeyName = $"UC_{string.Join("_", columns)}";

            string result = $"CONSTRAINT {uniqueKeyName} UNIQUE({string.Join(",", columnsWithBrackets)})";

            return result;
        }

        public string GetSQLColumnDefinitionToAddInTable(DXColumnDefinitionElement clmDesc)
        {
            string mysqlClmDef = "";

            mysqlClmDef = $"`{clmDesc.Name}` {this.GetMySQLDataType(clmDesc.ColumnType)}";

            if (clmDesc.Length.HasValue)
            {
                mysqlClmDef += $"({clmDesc.Length.Value})";
            }

            if ((!clmDesc.AllowNull || clmDesc.Name == "ObjectID") && clmDesc.Name != "ID")
            {
                mysqlClmDef += $" NOT NULL";
            }

            if (!string.IsNullOrEmpty(clmDesc.DefaultValue)
            && clmDesc.Name != "ID"
            && clmDesc.Name != "ObjectID")
            {
                mysqlClmDef += $" DEFAULT {clmDesc.DefaultValue}";
            }

            if (clmDesc.Name == "ID")
            {
                mysqlClmDef += $" PRIMARY KEY UNIQUE";
            }

            return mysqlClmDef;
        }

        public string GetSQLQueryToAlterTable(
            DPObjectDescObject dataBlockNew,
            DPObjectDescObject dataBlockExisting)
        {
            // ALTER TABLE `new_table` 
            // DROP COLUMN `pwd`,
            // ADD COLUMN `newColumn` VARCHAR(45) NOT NULL DEFAULT 'default text',
            // CHANGE COLUMN `name` `nameNew` DECIMAL NULL DEFAULT 10,
            // RENAME TO `new_table_Updated` ;
            StringBuilder sb = new StringBuilder();

            var columnsToDrop = this.GetColumnDescBlocksToDrop(dataBlockNew, dataBlockExisting);
            var columnsToAdd = this.GetColumnDescBlocksToAdd(dataBlockNew, dataBlockExisting);
            var columnsToChange = this.GetColumnDescBlocksToChange(dataBlockNew, dataBlockExisting);

            var columnsToDropMySQLCommand = columnsToDrop.Select(x => $"DROP COLUMN `{x.Name}`");
            var columnsToAddMySQLCommand = columnsToAdd.Select(x => $"ADD COLUMN {this.GetSQLColumnDefinitionToAddInTable(x)}");
            var columnsToChangeMySQLCommand = columnsToChange.Select(x =>
                this.GetSQLColumnDefinitionToChangeInTable(
                    dataBlockNew.DXColumnDefinitionElement.Announced.Single(y => y.ID == x),
                    dataBlockExisting.DXColumnDefinitionElement.Announced.Single(y => y.ID == x)));

            sb.Append($"ALTER TABLE {dataBlockExisting.DXUnitDefinitionMainElement.Name} ");
            if (columnsToDropMySQLCommand != null && columnsToDropMySQLCommand.Count() > 0)
            {
                sb.Append($"{string.Join(",", columnsToDropMySQLCommand)},");
            }
            if (columnsToAddMySQLCommand != null && columnsToAddMySQLCommand.Count() > 0)
            {
                sb.Append($"{string.Join(",", columnsToAddMySQLCommand)},");
            }
            if (columnsToChangeMySQLCommand != null && columnsToChangeMySQLCommand.Count() > 0)
            {
                sb.Append($"{string.Join(",", columnsToChangeMySQLCommand)},");
            }
            sb.Append($"RENAME TO {dataBlockNew.DXUnitDefinitionMainElement.Name}");

            return sb.ToString();
        }

        public string GetSQLQueryToDeleteRelationZeroOneToOne(DXRelationDefinitionUnit obj)
        {
            var sb = new StringBuilder();

            sb.Append($"ALTER TABLE {obj.DPRelationGenBlock.ObjectNameLeft} ");
            sb.Append($"DROP FOREIGN KEY `FK_{obj.DPRelationGenBlock.ObjectNameLeft}_{obj.DPRelationGenBlock.RelationNameRight}`;");
            sb.Append($"ALTER TABLE {obj.DPRelationGenBlock.ObjectNameLeft} ");
            sb.Append($"DROP COLUMN `{obj.DPRelationGenBlock.RelationNameRight}`, ");
            sb.Append($"DROP INDEX `{obj.DPRelationGenBlock.RelationNameRight}`;");

            return sb.ToString();
        }

        public string GetSQLQueryToDeleteRelationOneToZeroOne(DXRelationDefinitionUnit obj)
        {
            var sb = new StringBuilder();

            sb.Append($"ALTER TABLE {obj.DPRelationGenBlock.ObjectNameRight} ");
            sb.Append($"DROP FOREIGN KEY `FK_{obj.DPRelationGenBlock.ObjectNameRight}_{obj.DPRelationGenBlock.RelationNameLeft}`;");
            sb.Append($"ALTER TABLE {obj.DPRelationGenBlock.ObjectNameRight} ");
            sb.Append($"DROP COLUMN `{obj.DPRelationGenBlock.RelationNameLeft}`, ");
            sb.Append($"DROP INDEX `{obj.DPRelationGenBlock.RelationNameLeft}`;");

            return sb.ToString();
        }

        public string GetSQLQueryToDropTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit block)
        {
            // ALTER TABLE `IV.DataProvider.TestDB`.`Table1` 
            // DROP FOREIGN KEY `fk_Table1_Table2_0000`;
            // ALTER TABLE `IV.DataProvider.TestDB`.`Table1` 
            // DROP INDEX `fk_Table1_Table2_0000_idx` ;
            // ALTER TABLE `IV.DataProvider.TestDB`.`Table1` 
            // DROP COLUMN Table2ID;

            StringBuilder sb = new StringBuilder();

            sb.Append($"ALTER TABLE {block.DXUnitDefinitionMainElement.Name} ");
            sb.Append($"DROP FOREIGN KEY `FK_{block.DXUnitDefinitionMainElement.Name}_{obj.DXUnitDefinitionMainElement.Name}_0000`; ");
            sb.Append($"ALTER TABLE {block.DXUnitDefinitionMainElement.Name} ");
            sb.Append($"DROP INDEX `FK_{block.DXUnitDefinitionMainElement.Name}_{obj.DXUnitDefinitionMainElement.Name}_0000_idx`;");
            sb.Append($"ALTER TABLE {block.DXUnitDefinitionMainElement.Name} ");
            sb.Append($"DROP COLUMN {obj.DXUnitDefinitionMainElement.Name}ID; ");

            return sb.ToString();
        }

        public string GetSQLQueryToCreateTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit block)
        {
            // ALTER TABLE `IV.DataProvider.TestDB`.`Table1` 
            // ADD COLUMN Table2ID CHAR(36) CHARACTER SET UTF8MB4; ;
            // ALTER TABLE `IV.DataProvider.TestDB`.`Table1` 
            // ADD INDEX `fk_Table1_Table2_0000_idx` (`Table2ID` ASC) VISIBLE;
            // ;
            // ALTER TABLE `IV.DataProvider.TestDB`.`Table1` 
            // ADD CONSTRAINT `fk_Table1_Table2_0000`
            //   FOREIGN KEY (`Table2ID`)
            //   REFERENCES `IV.DataProvider.TestDB`.`Table2` (`ID`)
            //   ON DELETE NO ACTION
            //   ON UPDATE NO ACTION;

            if (obj == null || block == null)
                return null;

            var blockInEntityInfo = obj.DPBlockInEntityDescGenBlock?.Announced.SingleOrDefault(x => x.DXElementDefinitionUnit == block.ID);

            if (blockInEntityInfo == null)
                return null;

            StringBuilder sb = new StringBuilder();

            sb.Append($"ALTER TABLE {block.DXUnitDefinitionMainElement.Name} ");
            sb.Append($"ADD COLUMN {obj.DXUnitDefinitionMainElement.Name}ID CHAR(36) CHARACTER SET UTF8MB4; ");

            if (blockInEntityInfo.RelationType == DPBlockInObjectTypeEnum.SingleOptional
            || blockInEntityInfo.RelationType == DPBlockInObjectTypeEnum.SingleMandatory
            )
            {
                sb.Append($"ALTER TABLE {block.DXUnitDefinitionMainElement.Name} ");
                sb.Append($"ADD CONSTRAINT {obj.DXUnitDefinitionMainElement.Name}ID_unique UNIQUE({obj.DXUnitDefinitionMainElement.Name}ID); ");
            }

            sb.Append($"ALTER TABLE {block.DXUnitDefinitionMainElement.Name} ");
            sb.Append($"ADD INDEX `FK_{block.DXUnitDefinitionMainElement.Name}_{obj.DXUnitDefinitionMainElement.Name}_0000_idx` (`{obj.DXUnitDefinitionMainElement.Name}ID` ASC) VISIBLE; ");
            sb.Append($"ALTER TABLE {block.DXUnitDefinitionMainElement.Name} ");
            sb.Append($"ADD CONSTRAINT `FK_{block.DXUnitDefinitionMainElement.Name}_{obj.DXUnitDefinitionMainElement.Name}_0000` ");
            sb.Append($"FOREIGN KEY (`{obj.DXUnitDefinitionMainElement.Name}ID`) ");
            sb.Append($"REFERENCES `{obj.DXUnitDefinitionMainElement.Name}` (`ID`) ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION;");

            return sb.ToString();
        }

        public string GetSQLQueryToDeleteRelationManyToOne(DXRelationDefinitionUnit obj)
        {
            var sb = new StringBuilder();

            sb.Append($"ALTER TABLE {obj.DPRelationGenBlock.ObjectNameLeft} ");
            sb.Append($"DROP FOREIGN KEY `FK_{obj.DPRelationGenBlock.ObjectNameLeft}_{obj.DPRelationGenBlock.RelationNameRight}`;");
            sb.Append($"ALTER TABLE {obj.DPRelationGenBlock.ObjectNameLeft} ");
            sb.Append($"DROP COLUMN `{obj.DPRelationGenBlock.RelationNameRight}`, ");
            sb.Append($"DROP INDEX `FK_{obj.DPRelationGenBlock.ObjectNameLeft}_{obj.DPRelationGenBlock.RelationNameRight}`;");

            return sb.ToString();
        }


        public string GetSQLQueryToDeleteRelationOneToMany(DXRelationDefinitionUnit obj)
        {
            var sb = new StringBuilder();

            sb.Append($"ALTER TABLE {obj.DPRelationGenBlock.ObjectNameRight} ");
            sb.Append($"DROP FOREIGN KEY `FK_{obj.DPRelationGenBlock.ObjectNameRight}_{obj.DPRelationGenBlock.RelationNameLeft}`;");
            sb.Append($"ALTER TABLE {obj.DPRelationGenBlock.ObjectNameRight} ");
            sb.Append($"DROP COLUMN `{obj.DPRelationGenBlock.RelationNameLeft}`, ");
            sb.Append($"DROP INDEX `FK_{obj.DPRelationGenBlock.ObjectNameRight}_{obj.DPRelationGenBlock.RelationNameLeft}`;");

            return sb.ToString();
        }

        public string GetSQLQueryToCreateRelationManyTo(DXRelationDefinitionUnit obj, bool isNullable, bool isUnique)
        {
            StringBuilder sb = new StringBuilder();

            var nullValue = isNullable ? "NULL" : "NOT NULL";
            var uniqueValue = isUnique ? "UNIQUE" : "";

            var rightColumnName = obj.DPRelationGenBlock.RelationColumnNameRight;
            var rightColumnType = this.GetMySQLDataType(obj.DPRelationGenBlock.RelationColumnTypeRight.Value);

            sb.Append($"ALTER TABLE {obj.DPRelationGenBlock.ObjectNameLeft} ");
            sb.Append($"ADD COLUMN {obj.DPRelationGenBlock.RelationNameRight} {rightColumnType} {nullValue} {uniqueValue} AFTER `TimeStamp`;");

            sb.Append($"ALTER TABLE {obj.DPRelationGenBlock.ObjectNameLeft} ");
            sb.Append($"ADD CONSTRAINT `FK_{obj.DPRelationGenBlock.ObjectNameLeft}_{obj.DPRelationGenBlock.RelationNameRight}` ");
            sb.Append($"FOREIGN KEY(`{obj.DPRelationGenBlock.RelationNameRight}`) ");
            sb.Append($"REFERENCES `{obj.DPRelationGenBlock.ObjectNameRight}` (`{rightColumnName}`) ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION; ");

            return sb.ToString();
        }

        public string GetSQLQueryToCreateRelationToMany(DXRelationDefinitionUnit obj, bool isNullable, bool isUnique)
        {
            StringBuilder sb = new StringBuilder();

            var nullValue = isNullable ? "NULL" : "NOT NULL";
            var uniqueValue = isUnique ? "UNIQUE" : "";

            var leftColumnName = obj.DPRelationGenBlock.RelationColumnNameLeft;
            var leftColumnType = this.GetMySQLDataType(obj.DPRelationGenBlock.RelationColumnTypeLeft.Value);

            sb.Append($"ALTER TABLE {obj.DPRelationGenBlock.ObjectNameRight} ");
            sb.Append($"ADD COLUMN {obj.DPRelationGenBlock.RelationNameLeft} {leftColumnType} {nullValue} {uniqueValue} AFTER `TimeStamp`;");

            sb.Append($"ALTER TABLE {obj.DPRelationGenBlock.ObjectNameRight} ");
            sb.Append($"ADD CONSTRAINT `FK_{obj.DPRelationGenBlock.ObjectNameRight}_{obj.DPRelationGenBlock.RelationNameLeft}` ");
            sb.Append($"FOREIGN KEY(`{obj.DPRelationGenBlock.RelationNameLeft}`) ");
            sb.Append($"REFERENCES `{obj.DPRelationGenBlock.ObjectNameLeft}` (`{leftColumnName}`) ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION; ");

            return sb.ToString();
        }

        public string GetSQLQueryToCreateRelationManyToMany(DXRelationDefinitionUnit obj, string connectionStr)
        {
            StringBuilder sb = new StringBuilder();

            var numberOfTable = this.GetNumberOfIntermediateTable(obj, connectionStr);

            var intermediateTableName = $"Relation_{obj.DPRelationGenBlock.ObjectNameLeft}_{obj.DPRelationGenBlock.ObjectNameRight}_{numberOfTable}";

            var leftColumnName = obj.DPRelationGenBlock.RelationColumnNameLeft;
            var leftColumnType = this.GetMySQLDataType(obj.DPRelationGenBlock.RelationColumnTypeLeft.Value);
            var rightColumnName = obj.DPRelationGenBlock.RelationColumnNameRight;
            var rightColumnType = this.GetMySQLDataType(obj.DPRelationGenBlock.RelationColumnTypeRight.Value);

            sb.Append($"CREATE TABLE IF NOT EXISTS {intermediateTableName}(");
            sb.Append($"{obj.DPRelationGenBlock.RelationNameLeft} {leftColumnType},");
            sb.Append($"{obj.DPRelationGenBlock.RelationNameRight} {rightColumnType}, ");
            sb.Append($"PRIMARY KEY({obj.DPRelationGenBlock.RelationNameLeft}, {obj.DPRelationGenBlock.RelationNameRight})");
            sb.Append(")ENGINE=INNODB;");

            sb.Append($"ALTER TABLE {intermediateTableName} ");
            sb.Append($"ADD CONSTRAINT `FK_{intermediateTableName}_{obj.DPRelationGenBlock.ObjectNameLeft}` ");
            sb.Append($"FOREIGN KEY (`{obj.DPRelationGenBlock.RelationNameLeft}`) ");
            sb.Append($"REFERENCES `{obj.DPRelationGenBlock.ObjectNameLeft}` (`{leftColumnName}`) ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION;");

            sb.Append($"ALTER TABLE {intermediateTableName} ");
            sb.Append($"ADD CONSTRAINT `FK_{intermediateTableName}_{obj.DPRelationGenBlock.ObjectNameRight}` ");
            sb.Append($"FOREIGN KEY (`{obj.DPRelationGenBlock.RelationNameRight}`) ");
            sb.Append($"REFERENCES `{obj.DPRelationGenBlock.ObjectNameRight}` (`{rightColumnName}`) ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION;");

            obj.DPRelationGenBlock.RelationTable = intermediateTableName;

            return sb.ToString();
        }

        private int GetNumberOfIntermediateTable(DXRelationDefinitionUnit obj, string connectionStr)
        {
            var intermediateTableBaseName = $"Relation_{obj.DPRelationGenBlock.ObjectNameLeft}_{obj.DPRelationGenBlock.ObjectNameRight}";

            DataSet dataSet = new DataSet();

            using (MySqlConnection conn = new MySqlConnection(connectionStr))
            {
                conn.Open();

                this.PopulateTableToDataSet(
                    conn,
                    dataSet,
                    "information_schema.tables",
                    new List<string> { "table_name" },
                    $"table_name LIKE '{intermediateTableBaseName}%' AND TABLE_SCHEMA = 'IV.DataProvider.TestDB'",
                    new Dictionary<string, string>() { { "CREATE_TIME", "DESC" } },
                    1);
            }

            DataTable dataTable = dataSet.Tables["information_schema.tables"];

            if (dataTable.Rows.Count == 0)
            {
                return 0;
            }
            else
            {
                var lastTableName = dataTable.Rows[0][0].ToString();

                var number = Regex.Match(lastTableName, @"\d+$").Value;

                return int.Parse(number);
            }
        }

        public MySqlDataAdapter PopulateTableToDataSet(
            MySqlConnection conn,
            DataSet dataSet,
            string tableName,
            IEnumerable<string> columnNames = null,
            string whereClause = null,
            IDictionary<string, string> orderBy = null,
            int? limit = null)
        {
            StringBuilder sb = new StringBuilder();

            string columnNamesString = columnNames == null ? "*" : string.Join(",", ProtectReservedMySQLNames(columnNames));

            sb.Append($"SELECT {columnNamesString} FROM {tableName}");

            if (!string.IsNullOrEmpty(whereClause))
            {
                sb.Append($" WHERE {whereClause}");
            }

            if (orderBy != null && orderBy.Count() > 0)
            {
                string orderByString = string.Join(",", orderBy.Select(x => $"{x.Key} {x.Value}"));

                sb.Append($" ORDER BY {orderByString}");
            }

            if (limit.HasValue)
            {
                sb.Append($" LIMIT {limit.Value}");
            }

            sb.Append(";");

            var adapter = new MySqlDataAdapter(sb.ToString(), conn);

            adapter.Fill(dataSet, tableName);

            return adapter;
        }

        private IEnumerable<string> ProtectReservedMySQLNames(IEnumerable<string> income)
        {
            var reserevedMySQLNames = new List<string>()
            {
                "Precision"
            };

            return income.Select(x =>
            {
                if (reserevedMySQLNames.Contains(x))
                {
                    return $"`{x}`";
                }
                else
                {
                    return x;
                }

            }).ToList();
        }

        private IEnumerable<DXColumnDefinitionElement> GetColumnDescBlocksToDrop(
           DPObjectDescObject dataBlockNew,
           DPObjectDescObject dataBlockExisting
           )
        {
            var columnDescBlockNewIds = dataBlockNew.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.ID);
            var columnDescBlockExistingIds = dataBlockExisting.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.ID);

            var idsToRemove = columnDescBlockExistingIds.Where(x => !columnDescBlockNewIds.Contains(x));

            return dataBlockExisting.DXColumnDefinitionElement.Announced.Where(x => idsToRemove.Contains(x.ID)).ToList();
        }

        private IEnumerable<DXColumnDefinitionElement> GetColumnDescBlocksToAdd(
            DPObjectDescObject dataBlockNew,
            DPObjectDescObject dataBlockExisting)
        {
            var columnDescBlockNewIds = dataBlockNew.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.ID);
            var columnDescBlockExistingIds = dataBlockExisting.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.ID);

            var idsToAdd = columnDescBlockNewIds.Where(x => !columnDescBlockExistingIds.Contains(x));

            return dataBlockNew.DXColumnDefinitionElement.Announced.Where(x => idsToAdd.Contains(x.ID)).ToList();
        }

        private bool FilterForNonSystemColumns(string columnName)
        {
            return columnName != "ID" && columnName != "ObjectID" && columnName != "TimeStamp";
        }

        private IEnumerable<Guid> GetColumnDescBlocksToChange(
            DPObjectDescObject dataBlockNew,
            DPObjectDescObject dataBlockExisting)
        {
            var columnDescBlockNewIds = dataBlockNew.DXColumnDefinitionElement.Announced.Where(x => x.Name != "ID" && x.Name != "ObjectID").Select(x => x.ID);
            var columnDescBlockExistingIds = dataBlockExisting.DXColumnDefinitionElement.Announced.Where(x => x.Name != "ID" && x.Name != "ObjectID").Select(x => x.ID);

            var idsToChange = columnDescBlockNewIds.Intersect(columnDescBlockExistingIds).Where(x =>
            {
                var DXColumnDefinitionElementNew = dataBlockNew.DXColumnDefinitionElement.Announced.Single(y => y.ID == x);
                var DXColumnDefinitionElementExisting = dataBlockExisting.DXColumnDefinitionElement.Announced.Single(y => y.ID == x);

                var result = !(DXColumnDefinitionElementNew.AllowNull == DXColumnDefinitionElementExisting.AllowNull
                && this.AreEqual(DXColumnDefinitionElementNew.DefaultValue, DXColumnDefinitionElementExisting.DefaultValue)
                && DXColumnDefinitionElementNew.ColumnType == DXColumnDefinitionElementExisting.ColumnType
                && DXColumnDefinitionElementNew.Length == DXColumnDefinitionElementExisting.Length
                && DXColumnDefinitionElementNew.Name == DXColumnDefinitionElementExisting.Name);

                return result;
            });

            //return dataBlockNew.DXColumnDefinitionElement.Where(x => idsToChange.Contains(x.ID)).ToList();
            return idsToChange;
        }

        private string GetMySQLDataType(DXColumnTypeEnum clmType)
        {
            string mysqlDataType = null;

            switch (clmType)
            {
                case DXColumnTypeEnum.Bool:
                    mysqlDataType = "TINYINT";
                    break;
                case DXColumnTypeEnum.DateTime:
                    mysqlDataType = "DATETIME";
                    break;
                case DXColumnTypeEnum.Decimal:
                    mysqlDataType = "DECIMAL";
                    break;
                case DXColumnTypeEnum.GUID:
                    mysqlDataType = "CHAR(36) CHARACTER SET UTF8MB4";
                    break;
                case DXColumnTypeEnum.Int:
                    mysqlDataType = "INT";
                    break;
                case DXColumnTypeEnum.String:
                    mysqlDataType = "NVARCHAR";
                    break;
                case DXColumnTypeEnum.TimeStamp:
                    mysqlDataType = "TIMESTAMP";
                    break;
                case DXColumnTypeEnum.Text:
                    mysqlDataType = "LONGTEXT";
                    break;
                case DXColumnTypeEnum.Short:
                    mysqlDataType = "SMALLINT";
                    break;
                case DXColumnTypeEnum.Long:
                    mysqlDataType = "BIGINT";
                    break;
                case DXColumnTypeEnum.Float:
                    mysqlDataType = "FLOAT";
                    break;
                case DXColumnTypeEnum.Currency:
                    mysqlDataType = "DECIMAL(13,4)";
                    break;
                case DXColumnTypeEnum.Blob:
                    mysqlDataType = "BLOB";
                    break;
            }

            return mysqlDataType;
        }

        private bool AreEqual(string a, string b)
        {
            if (string.IsNullOrEmpty(a))
            {
                return string.IsNullOrEmpty(b);
            }
            else
            {
                return string.Equals(a, b);
            }
        }

        public DbDataAdapter GetDbDataAdapter(DbConnection dbconnection, string query)
        {
            var conn = dbconnection as MySqlConnection;

            return new MySqlDataAdapter(query, conn);
        }

        public DbCommandBuilder GetDbCommandBuilder(DbDataAdapter dataAdapter)
        {
            DbCommandBuilder commandBuilder = new MySqlCommandBuilder
            {
                DataAdapter = dataAdapter as MySqlDataAdapter
            };

            return commandBuilder;
        }

        public string GetSQLQuery(string tableName, IEnumerable<string> columnNames = null, string whereClause = null, IDictionary<string, string> orderBy = null, int? limit = null)
        {
            StringBuilder sb = new StringBuilder();

            string columnNamesString = columnNames == null ? "*" : string.Join(",", ProtectReservedMySQLNames(columnNames));

            sb.Append($"SELECT {columnNamesString} FROM {tableName}");

            if (!string.IsNullOrEmpty(whereClause))
            {
                sb.Append($" WHERE {whereClause}");
            }

            if (orderBy != null && orderBy.Count() > 0)
            {
                string orderByString = string.Join(",", orderBy.Select(x => $"{x.Key} {x.Value}"));

                sb.Append($" ORDER BY {orderByString}");
            }

            if (limit.HasValue)
            {
                sb.Append($" LIMIT {limit.Value}");
            }

            sb.Append(";");

            return sb.ToString();
        }

        public DbConnection GetDBConnection(string connectionStr)
        {
            return new MySqlConnection(connectionStr);
        }

        public void RunSQLQuery(string connectionString, string query)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted);

                try
                {
                    MySqlCommand mysqlCommand = new MySqlCommand(query, conn);
                    mysqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception exc)
                {
                    var exceptions = new List<Exception>() { exc };
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception exc2)
                    {
                        exceptions.Add(exc2);
                    }

                    throw new AggregateException(exceptions);
                }
            }
        }

        public string GetSQLQueryToSelectIDFromTable(string tableName)
        {
            return $"SELECT ID FROM {tableName}";
        }

        public void DropDataBase(string connectionString)
        {
            var args = this.GetParametersToCreateOrDeleteBD(connectionString);

            if (args == null)
                return;

            this.RunSQLQuery(args.Item2, $"DROP SCHEMA IF EXISTS `{args.Item1}`");
        }

        public void CreateDataBase(string connectionString)
        {
            var args = this.GetParametersToCreateOrDeleteBD(connectionString);

            if (args == null)
                return;

            this.RunSQLQuery(args.Item2, $"CREATE SCHEMA IF NOT EXISTS `{args.Item1}`");
        }

        private Tuple<string, string> GetParametersToCreateOrDeleteBD(string connectionString)
        {
            var parameters = connectionString.Split(';').Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim());

            var dbNameParameter = parameters.SingleOrDefault(x => x.Length > 8 && x.ToLower().Substring(0, 8) == "database");

            if (dbNameParameter == null)
                return null;

            var parametersWithoutDB = parameters.Where(x => x.Length < 8 || x.ToLower().Substring(0, 8) != "database");

            var connectionStringWithoutDatabase = string.Join(';', parametersWithoutDB);

            var dbName = dbNameParameter.Substring(dbNameParameter.IndexOf("=") + 1, dbNameParameter.Length - dbNameParameter.IndexOf("=") - 1).Trim();

            return new Tuple<string, string>(dbName, connectionStringWithoutDatabase);
        }

        public string GetQueryToSetEntityInheritance(string childEntity, string baseEntity)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"ALTER TABLE {childEntity} ");
            sb.Append($"ADD CONSTRAINT `FK_{childEntity}_{baseEntity}_Base` ");
            sb.Append($"FOREIGN KEY (`ID`) ");
            sb.Append($"REFERENCES `{baseEntity}` (`ID`) ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION;");

            return sb.ToString();
        }

        public string GetWhereExpressionForID(Guid id)
        {
            return $"ID = '{id}'";
        }

        public string GetWhereExpressionForObjectID(Guid id)
        {
            return $"ObjectID = '{id}'";
        }

        public string GetWhereExpressionWithAnd(IDictionary<string, object> values)
        {
            if (values == null)
                return null;

            return string.Join(" AND ", values.Select(x => $"{x.Key} = '{x.Value}'"));
        }

        public string GetWhereExpressionForID(IEnumerable<Guid> ids)
        {
            string idsString = String.Join(",", ids.Select(x => $"'{x}'"));

            return $"ID IN ({idsString})";
        }

        public string GetWhereExpressionForObjectID(IEnumerable<Guid> ids)
        {
            string idsString = String.Join(",", ids.Select(x => $"'{x}'"));

            return $"ObjectID IN ({idsString})";
        }
    }
}