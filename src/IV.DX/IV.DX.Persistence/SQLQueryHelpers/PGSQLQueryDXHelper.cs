using IV.DX.Contracts.Persistence.ExpressionTree;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Abstractions;
using IV.DX.Persistence.Models;
using Npgsql;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;

namespace IV.DX.Persistence.SQLQueryHelpers
{
    internal class PGSQLQueryDXHelper : ISQLQueryDXHelper
    {
        private readonly string closeSessionToDatabaseQuery = "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE pid <> pg_backend_pid() AND datname = '{0}';";

        public QueryContainer ConvertToQueryContainer(string entityType, string esqlWhereExpression, IEnumerable<DXRelationDefinitionUnit> relationInfos)
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
            return $"LEFT JOIN \"{queryInfo.JoinedTableName}\" AS \"{queryInfo.JoinedTableAlias}\" ON \"{queryInfo.JoinedTableAlias}\".\"{queryInfo.JoinedTableKey}\" = \"{queryInfo.MainTableAlias}\".\"{queryInfo.MainTableKey}\"";
        }

        public string GetWhereExpressionWithPropertyAndLogicOpeation(PropertyNode propertyNode)
        {
            StringBuilder sb = new StringBuilder();

            if (propertyNode.ExpressionOrder > 0)
            {
                sb.Append(propertyNode.LogicOperation);
                sb.Append(" ");
            }

            sb.Append($"\"{propertyNode.Mother.TableNameAliasToJoin}\".\"{propertyNode.LeftValue}\" {propertyNode.Operator} {propertyNode.RightValue}");

            return sb.ToString();
        }

        public void CreateDataBase(string connectionString)
        {
            var args = this.GetParametersToCreateOrDeleteBD(connectionString);

            if (args == null)
                return;

            if (!this.IsDatabaseExisting(args.Item2, args.Item1))
            {
                this.RunSQLQuery(args.Item2, string.Format(closeSessionToDatabaseQuery, "template1"));
                this.RunSQLQueryWithoutTransactionBlock(args.Item2, $"CREATE DATABASE \"{args.Item1}\";");
            }
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

        public void DropDataBase(string connectionString)
        {
            var args = this.GetParametersToCreateOrDeleteBD(connectionString);

            if (args == null)
                return;

            this.RunSQLQuery(args.Item2, string.Format(closeSessionToDatabaseQuery, args.Item1));
            this.RunSQLQueryWithoutTransactionBlock(args.Item2, $"DROP DATABASE IF EXISTS \"{args.Item1}\" WITH (FORCE);");
        }

        public DbCommandBuilder GetDbCommandBuilder(DbDataAdapter dataAdapter)
        {
            DbCommandBuilder commandBuilder = new NpgsqlCommandBuilder
            {
                DataAdapter = dataAdapter as NpgsqlDataAdapter
            };

            return commandBuilder;
        }

        public DbConnection GetDBConnection(string connectionStr)
        {
            return new NpgsqlConnection(connectionStr);
        }

        public DbDataAdapter GetDbDataAdapter(DbConnection dbconnection, string query)
        {
            var conn = dbconnection as NpgsqlConnection;

            return new NpgsqlDataAdapter(query, conn);
        }

        public string GetQuery(string typeName, string esqlWhereExpression, IEnumerable<DXRelationDefinitionUnit> relationInfos)
        {
            if (string.IsNullOrEmpty(esqlWhereExpression))
                return GetSQLQueryToSelectIDFromTable(typeName);

            var result = this.ConvertToQueryContainer(typeName, esqlWhereExpression, relationInfos);

            return result.Query;
        }

        public string GetQueryToSetEntityInheritance(string childEntity, string baseEntity)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"ALTER TABLE \"{childEntity}\" ");
            sb.Append($"ADD CONSTRAINT \"FK_{childEntity}_{baseEntity}_Base\" ");
            sb.Append($"FOREIGN KEY (\"ID\") ");
            sb.Append($"REFERENCES \"{baseEntity}\" (\"ID\") ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION;");

            return sb.ToString();
        }

        public string GetSelectQuery(CoreNode coreNode)
        {
            return $"SELECT \"{coreNode.MainTableAlias}\".\"ID\" FROM \"{coreNode.Value}\" AS \"{coreNode.MainTableAlias}\"";
        }

        public string GetSQLColumnDefinitionToAddInTable(DXColumnDefinitionElement clmDesc)
        {
            string sqlClmDef = "";

            sqlClmDef = $"\"{clmDesc.Name}\" {this.GetPostgreSQLDataType(clmDesc.ColumnType)}";

            if (clmDesc.Length.HasValue)
            {
                sqlClmDef += $"({clmDesc.Length.Value})";
            }

            if ((!clmDesc.AllowNull || clmDesc.Name == "ObjectID") && clmDesc.Name != "ID")
            {
                sqlClmDef += $" NOT NULL";
            }

            if (!string.IsNullOrEmpty(clmDesc.DefaultValue)
            && clmDesc.Name != "ID"
            && clmDesc.Name != "ObjectID")
            {
                if (clmDesc.ColumnType == DXColumnTypeEnum.Bool)
                {
                    sqlClmDef += $" DEFAULT '{clmDesc.DefaultValue}'";
                }
                else
                {
                    sqlClmDef += $" DEFAULT {clmDesc.DefaultValue}";
                }
            }

            if (clmDesc.Name == "ID")
            {
                sqlClmDef += $" PRIMARY KEY";
            }

            return sqlClmDef;
        }

        private string GetSQLColumnsUniqueToAddInTable(string tableName, DXUniqueColumnsElement clmDesc)
        {
            var columns = clmDesc.Columns.Split(',').Select(x => x.Trim());

            var columnsWithBrackets = columns.Select(x => $"\"{x}\"");

            var uniqueKeyName = $"UC_{tableName}_{string.Join("_", columns)}";

            string result = $"CONSTRAINT \"{uniqueKeyName}\" UNIQUE({string.Join(",", columnsWithBrackets)})";

            return result;
        }

        //public string GetSQLColumnDefinitionToChangeInTable(DXColumnDefinitionElement DXColumnDefinitionElementNew, DXColumnDefinitionElement DXColumnDefinitionElementExisting)
        //{
        //    // RENAME COLUMN column_name TO new_column_name;

        //    var mySQLQueryToChangeColumn = $"RENAME COLUMN \"{DXColumnDefinitionElementExisting.Name}\" {this.GetSQLColumnDefinitionToAddInTable(DXColumnDefinitionElementNew)}";

        //    return mySQLQueryToChangeColumn;
        //}

        private string GetSQLColumnDefinitionToAlterColumnSetType(DXColumnDefinitionElement DXColumnDefinitionElementNew, DXColumnDefinitionElement DXColumnDefinitionElementExisting)
        {
            // RENAME COLUMN column_name TO new_column_name;

            var mySQLQueryToChangeColumn = $"ALTER COLUMN \"{DXColumnDefinitionElementExisting.Name}\" TYPE {this.GetPostgreSQLDataType(DXColumnDefinitionElementNew.ColumnType)}";

            if (DXColumnDefinitionElementNew.Length.HasValue)
            {
                mySQLQueryToChangeColumn += $"({DXColumnDefinitionElementNew.Length.Value})";
            }

            return mySQLQueryToChangeColumn;
        }

        private string GetSQLColumnDefinitionToAlterColumnSetAllowNull(DXColumnDefinitionElement DXColumnDefinitionElementNew, DXColumnDefinitionElement DXColumnDefinitionElementExisting)
        {
            var mySQLQueryToChangeColumn = $"ALTER COLUMN \"{DXColumnDefinitionElementExisting.Name}\"";

            if (DXColumnDefinitionElementNew.AllowNull)
            {
                mySQLQueryToChangeColumn += " SET NULL";
            }
            else
            {
                mySQLQueryToChangeColumn += " SET NOT NULL";
            }

            return mySQLQueryToChangeColumn;
        }

        private string GetSQLColumnDefinitionToAlterColumnSetDefaultValue(DXColumnDefinitionElement DXColumnDefinitionElementNew, DXColumnDefinitionElement DXColumnDefinitionElementExisting)
        {
            var mySQLQueryToChangeColumn = $"ALTER COLUMN \"{DXColumnDefinitionElementExisting.Name}\"";

            if (DXColumnDefinitionElementNew.DefaultValue == null)
            {
                mySQLQueryToChangeColumn += $" SET DEFAULT NULL";
            }
            else
            {
                if (DXColumnDefinitionElementNew.ColumnType == DXColumnTypeEnum.Bool)
                {
                    mySQLQueryToChangeColumn += $" SET DEFAULT '{DXColumnDefinitionElementNew.DefaultValue}'";
                }
                else
                {
                    mySQLQueryToChangeColumn += $" SET DEFAULT {DXColumnDefinitionElementNew.DefaultValue}";
                }
            }


            return mySQLQueryToChangeColumn;
        }

        private string GetSQLColumnDefinitionToChangeColumnNames(DXColumnDefinitionElement DXColumnDefinitionElementNew, DXColumnDefinitionElement DXColumnDefinitionElementExisting)
        {
            // RENAME COLUMN column_name TO new_column_name;

            var mySQLQueryToChangeColumn = $"RENAME COLUMN \"{DXColumnDefinitionElementExisting.Name}\" TO \"{DXColumnDefinitionElementNew.Name}\"";

            return mySQLQueryToChangeColumn;
        }

        public string GetSQLQuery(string tableName, IEnumerable<string> columnNames = null, string whereClause = null, IDictionary<string, string> orderBy = null, int? limit = null)
        {
            StringBuilder sb = new StringBuilder();

            string columnNamesString = columnNames == null ? "*" : string.Join(",", columnNames.Select(x => $"\"{x}\""));

            sb.Append($"SELECT {columnNamesString} FROM \"{tableName}\"");

            if (!string.IsNullOrEmpty(whereClause))
            {
                sb.Append($" WHERE {whereClause}");
            }

            if (orderBy != null && orderBy.Count() > 0)
            {
                string orderByString = string.Join(",", orderBy.Select(x => $"\"{x.Key}\" {x.Value}"));

                sb.Append($" ORDER BY {orderByString}");
            }

            if (limit.HasValue)
            {
                sb.Append($" LIMIT {limit.Value}");
            }

            sb.Append(";");

            return sb.ToString();
        }

        public string GetSQLQueryToAlterTable(DXObjectDefinitionUnit dataBlockNew, DXObjectDefinitionUnit dataBlockExisting)
        {
            StringBuilder sb = new StringBuilder();

            var columnsToAdd = this.GetColumnDescBlocksToAdd(dataBlockNew, dataBlockExisting);
            var columnsToDrop = this.GetColumnDescBlocksToDrop(dataBlockNew, dataBlockExisting);
            var columnIDsToChange = this.GetColumnDescBlockIDsToChange(dataBlockNew, dataBlockExisting);

            var columnsToDropMySQLCommand = columnsToDrop.Select(x => $"DROP COLUMN \"{x.Name}\"");
            var columnsToAddMySQLCommand = columnsToAdd.Select(x => $"ADD COLUMN {this.GetSQLColumnDefinitionToAddInTable(x)}");
            var columnsToAlterColumnSetTypeCommand = columnIDsToChange.Select(x =>
                this.GetSQLColumnDefinitionToAlterColumnSetType(
                    dataBlockNew.DXColumnDefinitionElement.Announced.Single(y => y.ID == x),
                    dataBlockExisting.DXColumnDefinitionElement.Announced.Single(y => y.ID == x)));

            var columnsToAlterColumnSetAllowNullCommand = columnIDsToChange.Select(x =>
            this.GetSQLColumnDefinitionToAlterColumnSetAllowNull(
                dataBlockNew.DXColumnDefinitionElement.Announced.Single(y => y.ID == x),
                dataBlockExisting.DXColumnDefinitionElement.Announced.Single(y => y.ID == x)));

            var columnsToAlterColumnSetDefaultValueCommand = columnIDsToChange.Select(x =>
            this.GetSQLColumnDefinitionToAlterColumnSetDefaultValue(
                dataBlockNew.DXColumnDefinitionElement.Announced.Single(y => y.ID == x),
                dataBlockExisting.DXColumnDefinitionElement.Announced.Single(y => y.ID == x)));

            var columnsToChangeNamesCommand = columnIDsToChange.Select(x =>
              this.GetSQLColumnDefinitionToChangeColumnNames(
                  dataBlockNew.DXColumnDefinitionElement.Announced.Single(y => y.ID == x),
                  dataBlockExisting.DXColumnDefinitionElement.Announced.Single(y => y.ID == x)));

            if (columnsToDropMySQLCommand != null && columnsToDropMySQLCommand.Count() > 0)
            {
                sb.Append($"ALTER TABLE \"{dataBlockExisting.DXUnitDefinitionMainElement.Name}\" ");
                sb.Append($"{string.Join(",", columnsToDropMySQLCommand)}");
                sb.Append(";");
            }

            if (columnsToAddMySQLCommand != null && columnsToAddMySQLCommand.Count() > 0)
            {
                sb.Append($"ALTER TABLE \"{dataBlockExisting.DXUnitDefinitionMainElement.Name}\" ");
                sb.Append($"{string.Join(",", columnsToAddMySQLCommand)}");
                sb.Append(";");
            }

            if (columnsToAlterColumnSetTypeCommand != null && columnsToAlterColumnSetTypeCommand.Count() > 0)
            {
                sb.Append($"ALTER TABLE \"{dataBlockExisting.DXUnitDefinitionMainElement.Name}\" ");
                sb.Append($"{string.Join(",", columnsToAlterColumnSetTypeCommand)}");
                sb.Append(";");
            }

            if (columnsToAlterColumnSetAllowNullCommand != null && columnsToAlterColumnSetAllowNullCommand.Count() > 0)
            {
                sb.Append($"ALTER TABLE \"{dataBlockExisting.DXUnitDefinitionMainElement.Name}\" ");
                sb.Append($"{string.Join(",", columnsToAlterColumnSetAllowNullCommand)}");
                sb.Append(";");
            }

            if (columnsToAlterColumnSetDefaultValueCommand != null && columnsToAlterColumnSetDefaultValueCommand.Count() > 0)
            {
                sb.Append($"ALTER TABLE \"{dataBlockExisting.DXUnitDefinitionMainElement.Name}\" ");
                sb.Append($"{string.Join(",", columnsToAlterColumnSetDefaultValueCommand)}");
                sb.Append(";");
            }

            if (columnsToChangeNamesCommand != null && columnsToChangeNamesCommand.Count() > 0)
            {
                foreach (var item in columnsToChangeNamesCommand)
                {
                    sb.Append($"ALTER TABLE \"{dataBlockExisting.DXUnitDefinitionMainElement.Name}\" ");
                    sb.Append(item);
                    sb.Append(";");
                }
            }

            if (!dataBlockExisting.DXUnitDefinitionMainElement.Name.Equals(dataBlockNew.DXUnitDefinitionMainElement.Name))
            {
                sb.Append($"ALTER TABLE \"{dataBlockExisting.DXUnitDefinitionMainElement.Name}\" ");
                sb.Append($"RENAME TO \"{dataBlockNew.DXUnitDefinitionMainElement.Name}\";");
            }

            return sb.ToString();
        }

        private IEnumerable<Guid> GetColumnDescBlockIDsToChange(
            DXObjectDefinitionUnit dataBlockNew,
            DXObjectDefinitionUnit dataBlockExisting)
        {
            if (dataBlockNew.DXColumnDefinitionElement.Mode == MultiElementsMode.Target)
            {
                var columnDescBlockExistingIds = dataBlockExisting.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.ID);

                return dataBlockNew.DXColumnDefinitionElement.Announced.Where(x => columnDescBlockExistingIds.Contains(x.ID)).Select(x => x.ID).ToList();
            }
            else
            {
                var columnDescBlockNewIds = dataBlockNew.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.ID);
                var columnDescBlockExistingIds = dataBlockExisting.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.ID);

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

                return idsToChange;
            }
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

        private IEnumerable<DXColumnDefinitionElement> GetColumnDescBlocksToDrop(
            DXObjectDefinitionUnit dataBlockNew,
            DXObjectDefinitionUnit dataBlockExisting)
        {
            if (dataBlockNew.DXColumnDefinitionElement.Mode == MultiElementsMode.Target)
            {
                var columnDescBlockExistingIds = dataBlockExisting.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.ID);

                return dataBlockNew.DXColumnDefinitionElement.Deleted.Where(x => columnDescBlockExistingIds.Contains(x.ID)).ToList();
            }
            else
            {
                var columnDescBlockNewIds = dataBlockNew.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.ID);
                var columnDescBlockExistingIds = dataBlockExisting.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.ID);

                var idsToRemove = columnDescBlockExistingIds.Where(x => !columnDescBlockNewIds.Contains(x));

                return dataBlockExisting.DXColumnDefinitionElement.Announced.Where(x => idsToRemove.Contains(x.ID)).ToList();
            }
        }

        private IEnumerable<DXColumnDefinitionElement> GetColumnDescBlocksToAdd(
            DXObjectDefinitionUnit dataBlockNew,
            DXObjectDefinitionUnit dataBlockExisting)
        {
            if (dataBlockNew.DXColumnDefinitionElement.Mode == MultiElementsMode.Target)
            {
                var columnDescBlockExistingIds = dataBlockExisting.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.ID);

                return dataBlockNew.DXColumnDefinitionElement.Announced.Where(x => !columnDescBlockExistingIds.Contains(x.ID)).ToList();
            }
            else
            {
                var columnDescBlockNewIds = dataBlockNew.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.ID);
                var columnDescBlockExistingIds = dataBlockExisting.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.ID);

                var idsToAdd = columnDescBlockNewIds.Where(x => !columnDescBlockExistingIds.Contains(x));

                return dataBlockNew.DXColumnDefinitionElement.Announced.Where(x => idsToAdd.Contains(x.ID)).ToList();
            }
        }

        private bool FilterForNonSystemColumns(string columnName)
        {
            return columnName != "ID" && columnName != "ObjectID" && columnName != "TimeStamp";
        }

        public string GetSQLQueryToCreateRelationManyTo(DXRelationDefinitionUnit obj, bool isNullable, bool isUnique)
        {
            StringBuilder sb = new StringBuilder();

            var nullValue = isNullable ? "NULL" : "NOT NULL";
            var uniqueValue = isUnique ? "UNIQUE" : "";

            var rightColumnName = obj.DXRelationDefinitionMainElement.RelationColumnNameRight;
            var rightColumnType = this.GetPostgreSQLDataType(obj.DXRelationDefinitionMainElement.RelationColumnTypeRight.Value);

            sb.Append($"ALTER TABLE \"{obj.DXRelationDefinitionMainElement.ObjectNameLeft}\" ");
            sb.Append($"ADD COLUMN \"{obj.DXRelationDefinitionMainElement.RelationNameRight}\" {rightColumnType} {nullValue} {uniqueValue};");

            sb.Append($"ALTER TABLE \"{obj.DXRelationDefinitionMainElement.ObjectNameLeft}\" ");
            sb.Append($"ADD CONSTRAINT \"FK_{obj.DXRelationDefinitionMainElement.ObjectNameLeft}_{obj.DXRelationDefinitionMainElement.RelationNameRight}\" ");
            sb.Append($"FOREIGN KEY(\"{obj.DXRelationDefinitionMainElement.RelationNameRight}\") ");
            sb.Append($"REFERENCES \"{obj.DXRelationDefinitionMainElement.ObjectNameRight}\" (\"{rightColumnName}\") ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION; ");

            return sb.ToString();
        }

        public string GetSQLQueryToCreateRelationManyToMany(DXRelationDefinitionUnit obj, string connectionStr)
        {
            StringBuilder sb = new StringBuilder();

            var numberOfTable = this.GetNumberOfIntermediateTable(obj, connectionStr);

            var intermediateTableName = $"Relation_{obj.DXRelationDefinitionMainElement.ObjectNameLeft}_{obj.DXRelationDefinitionMainElement.ObjectNameRight}_{numberOfTable}";

            var leftColumnName = obj.DXRelationDefinitionMainElement.RelationColumnNameLeft;
            var leftColumnType = this.GetPostgreSQLDataType(obj.DXRelationDefinitionMainElement.RelationColumnTypeLeft.Value);
            var rightColumnName = obj.DXRelationDefinitionMainElement.RelationColumnNameRight;
            var rightColumnType = this.GetPostgreSQLDataType(obj.DXRelationDefinitionMainElement.RelationColumnTypeRight.Value);

            sb.Append($"CREATE TABLE IF NOT EXISTS \"{intermediateTableName}\"(");
            sb.Append($"\"{obj.DXRelationDefinitionMainElement.RelationNameLeft}\" {leftColumnType},");
            sb.Append($"\"{obj.DXRelationDefinitionMainElement.RelationNameRight}\" {rightColumnType}, ");
            sb.Append($"PRIMARY KEY(\"{obj.DXRelationDefinitionMainElement.RelationNameLeft}\", \"{obj.DXRelationDefinitionMainElement.RelationNameRight}\")");
            sb.Append(");");

            sb.Append($"ALTER TABLE \"{intermediateTableName}\" ");
            sb.Append($"ADD CONSTRAINT \"FK_{intermediateTableName}_{obj.DXRelationDefinitionMainElement.ObjectNameLeft}\" ");
            sb.Append($"FOREIGN KEY (\"{obj.DXRelationDefinitionMainElement.RelationNameLeft}\") ");
            sb.Append($"REFERENCES \"{obj.DXRelationDefinitionMainElement.ObjectNameLeft}\" (\"{leftColumnName}\") ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION;");

            sb.Append($"ALTER TABLE \"{intermediateTableName}\" ");
            sb.Append($"ADD CONSTRAINT \"FK_{intermediateTableName}_{obj.DXRelationDefinitionMainElement.ObjectNameRight}\" ");
            sb.Append($"FOREIGN KEY (\"{obj.DXRelationDefinitionMainElement.RelationNameRight}\") ");
            sb.Append($"REFERENCES \"{obj.DXRelationDefinitionMainElement.ObjectNameRight}\" (\"{rightColumnName}\") ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION;");

            obj.DXRelationDefinitionMainElement.RelationTable = intermediateTableName;

            return sb.ToString();
        }

        private int GetNumberOfIntermediateTable(DXRelationDefinitionUnit obj, string connectionStr)
        {
            //SELECT con.oid, nsp.nspname, con.conname, rel.relname
            //       FROM pg_catalog.pg_constraint con
            //INNER JOIN pg_catalog.pg_class rel
            //           ON rel.oid = con.conrelid
            //INNER JOIN pg_catalog.pg_namespace nsp
            //           ON nsp.oid = connamespace
            //WHERE

            //con.conname LIKE 'username_un%'

            //AND rel.relname = 'testclassbase2'

            var intermediateTableBaseName = $"Relation_{obj.DXRelationDefinitionMainElement.ObjectNameLeft}_{obj.DXRelationDefinitionMainElement.ObjectNameRight}";

            DataSet dataSet = new DataSet();

            using (var conn = new NpgsqlConnection(connectionStr))
            {
                conn.Open();

                var query = $"SELECT con.conname FROM pg_catalog.pg_constraint con INNER JOIN pg_catalog.pg_class rel ON rel.oid = con.conrelid INNER JOIN pg_catalog.pg_namespace nsp ON nsp.oid = connamespace WHERE con.conname LIKE '{intermediateTableBaseName}%' ORDER BY con.oid DESC";

                var adapter = new NpgsqlDataAdapter(query, conn);

                adapter.Fill(dataSet, "TempTable");
            }

            DataTable dataTable = dataSet.Tables["TempTable"];

            if (dataTable.Rows.Count == 0)
            {
                return 0;
            }
            else
            {
                var lastTableName = dataTable.Rows[0]["conname"].ToString();

                var number = Regex.Match(lastTableName, @"\d+$").Value;

                return int.Parse(number);
            }
        }

        public NpgsqlDataAdapter PopulateTableToDataSet(
            NpgsqlConnection conn,
            DataSet dataSet,
            string tableName,
            IEnumerable<string> columnNames = null,
            string whereClause = null,
            IDictionary<string, string> orderBy = null,
            int? limit = null)
        {
            var query = this.GetSQLQuery(tableName, columnNames, whereClause, orderBy, limit);

            var adapter = new NpgsqlDataAdapter(query, conn);

            adapter.Fill(dataSet, tableName);

            return adapter;
        }

        public string GetSQLQueryToCreateRelationToMany(DXRelationDefinitionUnit obj, bool isNullable, bool isUnique)
        {
            StringBuilder sb = new StringBuilder();

            var nullValue = isNullable ? "NULL" : "NOT NULL";
            var uniqueValue = isUnique ? "UNIQUE" : "";

            var leftColumnName = obj.DXRelationDefinitionMainElement.RelationColumnNameLeft;
            var leftColumnType = this.GetPostgreSQLDataType(obj.DXRelationDefinitionMainElement.RelationColumnTypeLeft.Value);

            sb.Append($"ALTER TABLE \"{obj.DXRelationDefinitionMainElement.ObjectNameRight}\" ");
            sb.Append($"ADD COLUMN \"{obj.DXRelationDefinitionMainElement.RelationNameLeft}\" {leftColumnType} {nullValue} {uniqueValue};");

            sb.Append($"ALTER TABLE \"{obj.DXRelationDefinitionMainElement.ObjectNameRight}\" ");
            sb.Append($"ADD CONSTRAINT \"FK_{obj.DXRelationDefinitionMainElement.ObjectNameRight}_{obj.DXRelationDefinitionMainElement.RelationNameLeft}\" ");
            sb.Append($"FOREIGN KEY(\"{obj.DXRelationDefinitionMainElement.RelationNameLeft}\") ");
            sb.Append($"REFERENCES \"{obj.DXRelationDefinitionMainElement.ObjectNameLeft}\" (\"{leftColumnName}\") ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION; ");

            return sb.ToString();
        }

        public string GetSQLQueryToCreateTable(DXObjectDefinitionUnit dataBlock)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"CREATE TABLE IF NOT EXISTS \"{dataBlock.DXUnitDefinitionMainElement.Name}\"(");

            var clmDefList = dataBlock.DXColumnDefinitionElement.Announced.Select(x => this.GetSQLColumnDefinitionToAddInTable(x));

            var clmUniqueList = dataBlock.DXUniqueColumnsElement.Announced.Select(x => this.GetSQLColumnsUniqueToAddInTable(dataBlock.DXUnitDefinitionMainElement.Name, x));

            sb.Append(string.Join(",", clmDefList));

            if (clmUniqueList.Count() > 0)
            {
                sb.Append(",");
                sb.Append(string.Join(",", clmUniqueList));
            }

            sb.Append(")");

            return sb.ToString();
        }

        public string GetSQLQueryToCreateTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit block)
        {
            if (obj == null || block == null)
                return null;

            var blockInEntityInfo = obj.DXElementInUnitDefinitionMainElement?.Announced.SingleOrDefault(x => x.DXElementDefinitionUnit == block.ID);

            if (blockInEntityInfo == null)
                return null;

            StringBuilder sb = new StringBuilder();

            sb.Append($"ALTER TABLE \"{block.DXUnitDefinitionMainElement.Name}\" ");
            sb.Append($"ADD COLUMN \"{obj.DXUnitDefinitionMainElement.Name}ID\" uuid; ");

            if (blockInEntityInfo.RelationType == DXElementInUnitTypeEnum.SingleOptional
            || blockInEntityInfo.RelationType == DXElementInUnitTypeEnum.SingleMandatory
            )
            {
                sb.Append($"ALTER TABLE \"{block.DXUnitDefinitionMainElement.Name}\" ");
                sb.Append($"ADD CONSTRAINT \"{obj.DXUnitDefinitionMainElement.Name}ID_unique\" UNIQUE(\"{obj.DXUnitDefinitionMainElement.Name}ID\"); ");
            }

            sb.Append($"ALTER TABLE \"{block.DXUnitDefinitionMainElement.Name}\" ");
            sb.Append($"ADD INDEX \"FK_{block.DXUnitDefinitionMainElement.Name}_{obj.DXUnitDefinitionMainElement.Name}_0000_idx\" (\"{obj.DXUnitDefinitionMainElement.Name}ID\" ASC) VISIBLE; ");
            sb.Append($"ALTER TABLE \"{block.DXUnitDefinitionMainElement.Name}\" ");
            sb.Append($"ADD CONSTRAINT \"FK_{block.DXUnitDefinitionMainElement.Name}_{obj.DXUnitDefinitionMainElement.Name}_0000\" ");
            sb.Append($"FOREIGN KEY (\"{obj.DXUnitDefinitionMainElement.Name}ID\") ");
            sb.Append($"REFERENCES \"{obj.DXUnitDefinitionMainElement.Name}\" (\"ID\") ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION;");


            return sb.ToString();
        }

        public string GetSQLQueryToDeleteRelationManyToOne(DXRelationDefinitionUnit obj)
        {
            var sb = new StringBuilder();

            sb.Append($"ALTER TABLE \"{obj.DXRelationDefinitionMainElement.ObjectNameLeft}\" ");
            sb.Append($"DROP CONSTRAINT \"FK_{obj.DXRelationDefinitionMainElement.ObjectNameLeft}_{obj.DXRelationDefinitionMainElement.RelationNameRight}\";");
            sb.Append($"ALTER TABLE \"{obj.DXRelationDefinitionMainElement.ObjectNameLeft}\" ");
            sb.Append($"DROP COLUMN \"{obj.DXRelationDefinitionMainElement.RelationNameRight}\";");
            //sb.Append($"DROP INDEX \"FK_{obj.DXRelationDefinitionMainElement.ObjectNameLeft}_{obj.DXRelationDefinitionMainElement.RelationNameRight}\";");

            return sb.ToString();
        }

        public string GetSQLQueryToDeleteRelationOneToMany(DXRelationDefinitionUnit obj)
        {
            var sb = new StringBuilder();

            sb.Append($"ALTER TABLE \"{obj.DXRelationDefinitionMainElement.ObjectNameRight}\" ");
            sb.Append($"DROP CONSTRAINT \"FK_{obj.DXRelationDefinitionMainElement.ObjectNameRight}_{obj.DXRelationDefinitionMainElement.RelationNameLeft}\";");
            sb.Append($"ALTER TABLE \"{obj.DXRelationDefinitionMainElement.ObjectNameRight}\" ");
            sb.Append($"DROP COLUMN \"{obj.DXRelationDefinitionMainElement.RelationNameLeft}\";");
            //sb.Append($"DROP INDEX \"FK_{obj.DXRelationDefinitionMainElement.ObjectNameRight}_{obj.DXRelationDefinitionMainElement.RelationNameLeft}\";");

            return sb.ToString();
        }

        public string GetSQLQueryToDeleteRelationOneToZeroOne(DXRelationDefinitionUnit obj)
        {
            var sb = new StringBuilder();

            sb.Append($"ALTER TABLE \"{obj.DXRelationDefinitionMainElement.ObjectNameRight}\" ");
            sb.Append($"DROP CONSTRAINT \"FK_{obj.DXRelationDefinitionMainElement.ObjectNameRight}_{obj.DXRelationDefinitionMainElement.RelationNameLeft}\";");
            sb.Append($"ALTER TABLE \"{obj.DXRelationDefinitionMainElement.ObjectNameRight}\" ");
            sb.Append($"DROP COLUMN \"{obj.DXRelationDefinitionMainElement.RelationNameLeft}\";");
            //sb.Append($"DROP INDEX \"{obj.DXRelationDefinitionMainElement.RelationNameLeft}\";");

            return sb.ToString();
        }

        public string GetSQLQueryToDeleteRelationZeroOneToOne(DXRelationDefinitionUnit obj)
        {
            var sb = new StringBuilder();

            sb.Append($"ALTER TABLE \"{obj.DXRelationDefinitionMainElement.ObjectNameLeft}\" ");
            sb.Append($"DROP CONSTRAINT \"FK_{obj.DXRelationDefinitionMainElement.ObjectNameLeft}_{obj.DXRelationDefinitionMainElement.RelationNameRight}\";");
            sb.Append($"ALTER TABLE \"{obj.DXRelationDefinitionMainElement.ObjectNameLeft}\" ");
            sb.Append($"DROP COLUMN \"{obj.DXRelationDefinitionMainElement.RelationNameRight}\";");
            //sb.Append($"DROP INDEX \"{obj.DXRelationDefinitionMainElement.RelationNameRight}\";");

            return sb.ToString();
        }

        public string GetSQLQueryToDropTable(DXObjectDefinitionUnit dataBlock)
        {
            // TODO: need to find solution how to drop table by ObjectID
            return GetSQLQueryToDropTable(dataBlock.DXUnitDefinitionMainElement.Name);
        }

        public string GetSQLQueryToDropTable(string tableName)
        {
            // TODO: need to find solution how to drop table by ObjectID
            return $"DROP TABLE IF EXISTS \"{tableName}\"";
        }

        public string GetSQLQueryToDropTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit block)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"ALTER TABLE \"{block.DXUnitDefinitionMainElement.Name}\" ");
            sb.Append($"DROP CONSTRAINT \"FK_{block.DXUnitDefinitionMainElement.Name}_{obj.DXUnitDefinitionMainElement.Name}_0000\"; ");
            sb.Append($"ALTER TABLE \"{block.DXUnitDefinitionMainElement.Name}\" ");
            sb.Append($"DROP INDEX \"FK_{block.DXUnitDefinitionMainElement.Name}_{obj.DXUnitDefinitionMainElement.Name}_0000_idx\";");
            sb.Append($"ALTER TABLE \"{block.DXUnitDefinitionMainElement.Name}\" ");
            sb.Append($"DROP COLUMN \"{obj.DXUnitDefinitionMainElement.Name}ID;\" ");

            return sb.ToString();
        }

        public string GetSQLQueryToSelectIDFromTable(string tableName)
        {
            return $"SELECT \"ID\" FROM \"{tableName}\"";
        }

        public void RunSQLQuery(string connectionString, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return;
            }

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                var transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted);

                try
                {
                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    command.ExecuteNonQuery();
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

        private void RunSQLQueryWithoutTransactionBlock(string connectionString, string query)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                try
                {
                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    command.ExecuteNonQuery();
                }
                catch (Exception exc)
                {
                    var exceptions = new List<Exception>() { exc };
                    try
                    {
                    }
                    catch (Exception exc2)
                    {
                        exceptions.Add(exc2);
                    }

                    throw new AggregateException(exceptions);
                }
            }
        }

        private bool IsDatabaseExisting(string connectionString, string dabName)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                try
                {
                    var query = $"SELECT COUNT(*) FROM pg_database WHERE datname = '{dabName}'";

                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    var count = (long)command.ExecuteScalar();

                    return count > 0;
                }
                catch (Exception exc)
                {
                    var exceptions = new List<Exception>() { exc };
                    try
                    {
                    }
                    catch (Exception exc2)
                    {
                        exceptions.Add(exc2);
                    }

                    throw new AggregateException(exceptions);
                }
            }
        }

        private string GetPostgreSQLDataType(DXColumnTypeEnum clmType)
        {
            string mysqlDataType = null;

            switch (clmType)
            {
                case DXColumnTypeEnum.Bool:
                    mysqlDataType = "boolean";
                    break;
                case DXColumnTypeEnum.DateTime:
                    //mysqlDataType = "time";
                    mysqlDataType = "timestamp with time zone";
                    break;
                case DXColumnTypeEnum.Decimal:
                    mysqlDataType = "decimal";
                    break;
                case DXColumnTypeEnum.GUID:
                    mysqlDataType = "uuid";
                    break;
                case DXColumnTypeEnum.Int:
                    mysqlDataType = "integer";
                    break;
                case DXColumnTypeEnum.String:
                    mysqlDataType = "varchar";
                    break;
                case DXColumnTypeEnum.TimeStamp:
                    mysqlDataType = "timestamp";
                    //mysqlDataType = "timestamp with time zone";
                    break;
                case DXColumnTypeEnum.Text:
                    mysqlDataType = "text";
                    break;
                case DXColumnTypeEnum.Short:
                    mysqlDataType = "smallint";
                    break;
                case DXColumnTypeEnum.Long:
                    mysqlDataType = "bigint";
                    break;
                case DXColumnTypeEnum.Float:
                    mysqlDataType = "real";
                    break;
                case DXColumnTypeEnum.Currency:
                    mysqlDataType = "NUMERIC(13,4)";
                    break;
                case DXColumnTypeEnum.Blob:
                    mysqlDataType = "bytea";
                    break;
            }

            return mysqlDataType;
        }

        public string GetWhereExpressionForID(Guid id)
        {
            return $"\"ID\" = '{id}'";
        }

        public string GetWhereExpressionForObjectID(Guid id)
        {
            return $"\"ObjectID\" = '{id}'";
        }

        public string GetWhereExpressionWithAnd(IDictionary<string, object> values)
        {
            if (values == null)
                return null;

            return string.Join(" AND ", values.Select(x => $"\"{x.Key}\" = '{x.Value}'"));
        }

        public string GetWhereExpressionForID(IEnumerable<Guid> ids)
        {
            string idsString = String.Join(",", ids.Select(x => $"'{x}'"));

            return $"\"ID\" IN ({idsString})";
        }

        public string GetWhereExpressionForObjectID(IEnumerable<Guid> ids)
        {
            string idsString = String.Join(",", ids.Select(x => $"'{x}'"));

            return $"\"ObjectID\" IN ({idsString})";
        }
    }
}
