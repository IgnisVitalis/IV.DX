using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Abstractions;
using IV.DX.Persistence.Contracts.Abstractions;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;

namespace IV.DX.Persistence.SQLQueryHelpers
{
    internal class PGSQLQueryDXHelper :
        ISQLDialect,
        ISQLSchemaHelper,
        ISQLDbProvider,
        ISQLMigrationLockHelper,
        IDXBulkInsertCapable
    {
        private readonly string closeSessionToDatabaseQuery = "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE pid <> pg_backend_pid() AND datname = '{0}';";

        public void CreateDataBase(string connectionString)
        {
            var args = this.GetParametersToCreateOrDeleteBD(connectionString);

            if (args == null)
                return;

            if (!this.IsDatabaseExisting(args.Item2, args.Item1))
            {
                this.RunSQLQuery(args.Item2, string.Format(closeSessionToDatabaseQuery, "template1"));
                this.RunSQLQueryWithoutTransactionDXElement(args.Item2, $"CREATE DATABASE \"{args.Item1}\";");
            }
        }

        private Tuple<string, string>? GetParametersToCreateOrDeleteBD(string connectionString)
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
            this.RunSQLQueryWithoutTransactionDXElement(args.Item2, $"DROP DATABASE IF EXISTS \"{args.Item1}\" WITH (FORCE);");
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

            return new NpgsqlDataAdapter(query, conn!)
            {

            };
        }

        public string GetQueryToSetDXUnitInheritance(string childDXUnit, string baseDXUnit)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"ALTER TABLE \"{childDXUnit}\" ");
            sb.Append($"ADD CONSTRAINT \"FK_{childDXUnit}_{baseDXUnit}_Base\" ");
            sb.Append($"FOREIGN KEY (\"Id\") ");
            sb.Append($"REFERENCES \"{baseDXUnit}\" (\"Id\") ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION;");

            return sb.ToString();
        }

        public string GetSQLColumnDefinitionToAddInTable(DXColumnDefinitionElement clmDesc)
        {
            string sqlClmDef = "";

            sqlClmDef = $"\"{clmDesc.Name}\" {this.GetPostgreSQLDataType(clmDesc.ColumnType)}";

            if (clmDesc.Length.HasValue
                && (clmDesc.ColumnType == DXColumnTypeEnum.String
                    || clmDesc.ColumnType == DXColumnTypeEnum.HashedString))
            {
                sqlClmDef += $"({clmDesc.Length.Value})";
            }

            if ((!clmDesc.AllowNull || clmDesc.Name == "DXUnitId") && clmDesc.Name != "Id")
            {
                sqlClmDef += $" NOT NULL";
            }

            if (!string.IsNullOrEmpty(clmDesc.DefaultValue)
            && clmDesc.Name != "Id"
            && clmDesc.Name != "DXUnitId")
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

            if (clmDesc.Name == "Id")
            {
                sqlClmDef += $" PRIMARY KEY";
            }

            return sqlClmDef;
        }

        //private bool IsDXColumnEnum(DXColumnDefinitionElement clmDesc)
        //{
        //    return clmDesc.EnumKey.HasValue && clmDesc.EnumType.HasValue;
        //}

        private string GetUniqueConstraintName(string tableName, string[] columns)
        {
            return $"UC_{tableName}_{string.Join("_", columns.OrderBy(x => x))}";
        }

        private string GetSQLToAddColumnsUniqueToAlterTable(string tableName, string[] clmDesc)
        {
            var columns = clmDesc.OrderBy(x => x).ToList();
            var uniqueKeyName = GetUniqueConstraintName(tableName, clmDesc);
            return $"ADD CONSTRAINT \"{uniqueKeyName}\" UNIQUE({string.Join(",", columns.Select(x => $"\"{x}\""))})";
        }

        private string GetSQLToDropColumnsUniqueFromAlterTable(string tableName, string[] clmDesc)
        {
            var uniqueKeyName = GetUniqueConstraintName(tableName, clmDesc);
            return $"DROP CONSTRAINT IF EXISTS \"{uniqueKeyName}\"";
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

            if (DXColumnDefinitionElementNew.Length.HasValue
                && (DXColumnDefinitionElementNew.ColumnType == DXColumnTypeEnum.String
                    || DXColumnDefinitionElementNew.ColumnType == DXColumnTypeEnum.HashedString))
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

        public string GetSQLQueryToAlterTable(DXObjectDefinitionUnit dataDXElementNew, DXObjectDefinitionUnit dataDXElementExisting)
        {
            StringBuilder sb = new StringBuilder();

            var columnsToAdd = this.GetColumnDescDXElementsToAdd(dataDXElementNew, dataDXElementExisting);
            var columnsToDrop = this.GetColumnDescDXElementsToDrop(dataDXElementNew, dataDXElementExisting);
            var columnIDsToChange = this.GetColumnDescDXElementIDsToChange(dataDXElementNew, dataDXElementExisting);

            var columnsToDropMySQLCommand = columnsToDrop.Select(x => $"DROP COLUMN \"{x.Name}\"");
            var columnsToAddMySQLCommand = columnsToAdd.Select(x => $"ADD COLUMN {this.GetSQLColumnDefinitionToAddInTable(x)}");
            var columnsToAlterColumnSetTypeCommand = columnIDsToChange.Select(x =>
                this.GetSQLColumnDefinitionToAlterColumnSetType(
                    dataDXElementNew.DXColumnDefinitionElement.Announced.Single(y => y.Id == x),
                    dataDXElementExisting.DXColumnDefinitionElement.Announced.Single(y => y.Id == x)));

            var columnsToAlterColumnSetAllowNullCommand = columnIDsToChange.Select(x =>
            this.GetSQLColumnDefinitionToAlterColumnSetAllowNull(
                dataDXElementNew.DXColumnDefinitionElement.Announced.Single(y => y.Id == x),
                dataDXElementExisting.DXColumnDefinitionElement.Announced.Single(y => y.Id == x)));

            var columnsToAlterColumnSetDefaultValueCommand = columnIDsToChange.Select(x =>
            this.GetSQLColumnDefinitionToAlterColumnSetDefaultValue(
                dataDXElementNew.DXColumnDefinitionElement.Announced.Single(y => y.Id == x),
                dataDXElementExisting.DXColumnDefinitionElement.Announced.Single(y => y.Id == x)));

            var columnsToChangeNamesCommand = columnIDsToChange.Select(x =>
              this.GetSQLColumnDefinitionToChangeColumnNames(
                  dataDXElementNew.DXColumnDefinitionElement.Announced.Single(y => y.Id == x),
                  dataDXElementExisting.DXColumnDefinitionElement.Announced.Single(y => y.Id == x)));

            if (columnsToDropMySQLCommand != null && columnsToDropMySQLCommand.Count() > 0)
            {
                sb.Append($"ALTER TABLE \"{dataDXElementExisting.Name}\" ");
                sb.Append($"{string.Join(",", columnsToDropMySQLCommand)}");
                sb.Append(";");
            }

            if (columnsToAddMySQLCommand != null && columnsToAddMySQLCommand.Count() > 0)
            {
                sb.Append($"ALTER TABLE \"{dataDXElementExisting.Name}\" ");
                sb.Append($"{string.Join(",", columnsToAddMySQLCommand)}");
                sb.Append(";");
            }

            if (columnsToAlterColumnSetTypeCommand != null && columnsToAlterColumnSetTypeCommand.Count() > 0)
            {
                sb.Append($"ALTER TABLE \"{dataDXElementExisting.Name}\" ");
                sb.Append($"{string.Join(",", columnsToAlterColumnSetTypeCommand)}");
                sb.Append(";");
            }

            if (columnsToAlterColumnSetAllowNullCommand != null && columnsToAlterColumnSetAllowNullCommand.Count() > 0)
            {
                sb.Append($"ALTER TABLE \"{dataDXElementExisting.Name}\" ");
                sb.Append($"{string.Join(",", columnsToAlterColumnSetAllowNullCommand)}");
                sb.Append(";");
            }

            if (columnsToAlterColumnSetDefaultValueCommand != null && columnsToAlterColumnSetDefaultValueCommand.Count() > 0)
            {
                sb.Append($"ALTER TABLE \"{dataDXElementExisting.Name}\" ");
                sb.Append($"{string.Join(",", columnsToAlterColumnSetDefaultValueCommand)}");
                sb.Append(";");
            }

            if (columnsToChangeNamesCommand != null && columnsToChangeNamesCommand.Count() > 0)
            {
                foreach (var item in columnsToChangeNamesCommand)
                {
                    sb.Append($"ALTER TABLE \"{dataDXElementExisting.Name}\" ");
                    sb.Append(item);
                    sb.Append(";");
                }
            }

            if (!dataDXElementExisting.Name.Equals(dataDXElementNew.Name))
            {
                sb.Append($"ALTER TABLE \"{dataDXElementExisting.Name}\" ");
                sb.Append($"RENAME TO \"{dataDXElementNew.Name}\";");
            }

            return sb.ToString();
        }

        private IEnumerable<Guid> GetColumnDescDXElementIDsToChange(
            DXObjectDefinitionUnit dataDXElementNew,
            DXObjectDefinitionUnit dataDXElementExisting)
        {
            if (dataDXElementNew.DXColumnDefinitionElement.Mode == MultiElementsMode.Target)
            {
                var columnDescDXElementExistingIds = dataDXElementExisting.DXColumnDefinitionElement.Announced
                    .Where(x => this.FilterForNonSystemColumns(x.Name))
                    .Select(x => x.Id);

                return dataDXElementNew.DXColumnDefinitionElement.Announced
                    .Where(x => this.FilterForNonSystemColumns(x.Name))
                    .Where(x => columnDescDXElementExistingIds.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToList();
            }
            else
            {
                var columnDescDXElementNewIds = dataDXElementNew.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.Id);
                var columnDescDXElementExistingIds = dataDXElementExisting.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.Id);

                var idsToChange = columnDescDXElementNewIds.Intersect(columnDescDXElementExistingIds).Where(x =>
                {
                    var DXColumnDefinitionElementNew = dataDXElementNew.DXColumnDefinitionElement.Announced.Single(y => y.Id == x);
                    var DXColumnDefinitionElementExisting = dataDXElementExisting.DXColumnDefinitionElement.Announced.Single(y => y.Id == x);

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

        private bool AreEqual(string? a, string? b)
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

        private IEnumerable<DXColumnDefinitionElement> GetColumnDescDXElementsToDrop(
            DXObjectDefinitionUnit dataDXElementNew,
            DXObjectDefinitionUnit dataDXElementExisting)
        {
            if (dataDXElementNew.DXColumnDefinitionElement.Mode == MultiElementsMode.Target)
            {
                var deletedIds = dataDXElementNew.DXColumnDefinitionElement.Deleted
                    .Select(x => x.Id)
                    .ToHashSet();

                return dataDXElementExisting.DXColumnDefinitionElement.Announced
                    .Where(x => this.FilterForNonSystemColumns(x.Name))
                    .Where(x => deletedIds.Contains(x.Id))
                    .ToList();
            }
            else
            {
                var columnDescDXElementNewIds = dataDXElementNew.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.Id);
                var columnDescDXElementExistingIds = dataDXElementExisting.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.Id);

                var idsToRemove = columnDescDXElementExistingIds.Where(x => !columnDescDXElementNewIds.Contains(x));

                return dataDXElementExisting.DXColumnDefinitionElement.Announced.Where(x => idsToRemove.Contains(x.Id)).ToList();
            }
        }

        private IEnumerable<DXColumnDefinitionElement> GetColumnDescDXElementsToAdd(
            DXObjectDefinitionUnit dataDXElementNew,
            DXObjectDefinitionUnit dataDXElementExisting)
        {
            if (dataDXElementNew.DXColumnDefinitionElement.Mode == MultiElementsMode.Target)
            {
                var columnDescDXElementExistingIds = dataDXElementExisting.DXColumnDefinitionElement.Announced
                    .Where(x => this.FilterForNonSystemColumns(x.Name))
                    .Select(x => x.Id);

                return dataDXElementNew.DXColumnDefinitionElement.Announced
                    .Where(x => this.FilterForNonSystemColumns(x.Name))
                    .Where(x => !columnDescDXElementExistingIds.Contains(x.Id))
                    .ToList();
            }
            else
            {
                var columnDescDXElementNewIds = dataDXElementNew.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.Id);
                var columnDescDXElementExistingIds = dataDXElementExisting.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.Id);

                var idsToAdd = columnDescDXElementNewIds.Where(x => !columnDescDXElementExistingIds.Contains(x));

                return dataDXElementNew.DXColumnDefinitionElement.Announced.Where(x => idsToAdd.Contains(x.Id)).ToList();
            }
        }

        private bool FilterForNonSystemColumns(string columnName)
        {
            return columnName != "Id" && columnName != "DXUnitId" && columnName != "TimeStamp";
        }

        public string GetSQLQueryToCreateRelationManyTo(DXRelationDefinitionUnit obj, bool isNullable, bool isUnique)
        {
            StringBuilder sb = new StringBuilder();

            var nullValue = isNullable ? "NULL" : "NOT NULL";
            var uniqueValue = isUnique ? "UNIQUE" : "";

            var rightColumnName = obj.RelationColumnNameRight;
            var rightColumnType = this.GetPostgreSQLDataType(obj.RelationColumnTypeRight!.Value);

            sb.Append($"ALTER TABLE \"{obj.ObjectNameLeft}\" ");
            sb.Append($"ADD COLUMN \"{obj.RelationNameRight}\" {rightColumnType} {nullValue} {uniqueValue};");

            sb.Append($"ALTER TABLE \"{obj.ObjectNameLeft}\" ");
            sb.Append($"ADD CONSTRAINT \"FK_{obj.ObjectNameLeft}_{obj.RelationNameRight}\" ");
            sb.Append($"FOREIGN KEY(\"{obj.RelationNameRight}\") ");
            sb.Append($"REFERENCES \"{obj.ObjectNameRight}\" (\"{rightColumnName}\") ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION; ");

            return sb.ToString();
        }

        public string GetSQLQueryToCreateRelationManyToMany(DXRelationDefinitionUnit obj, string connectionStr)
        {
            StringBuilder sb = new StringBuilder();

            var numberOfTable = this.GetNumberOfIntermediateTable(obj, connectionStr);

            var intermediateTableName = $"Relation_{obj.ObjectNameLeft}_{obj.ObjectNameRight}_{numberOfTable}";

            var leftColumnName = obj.RelationColumnNameLeft;
            var leftColumnType = this.GetPostgreSQLDataType(obj.RelationColumnTypeLeft!.Value);
            var rightColumnName = obj.RelationColumnNameRight;
            var rightColumnType = this.GetPostgreSQLDataType(obj.RelationColumnTypeRight!.Value);

            sb.Append($"CREATE TABLE IF NOT EXISTS \"{intermediateTableName}\"(");
            sb.Append($"\"{obj.RelationNameLeft}\" {leftColumnType},");
            sb.Append($"\"{obj.RelationNameRight}\" {rightColumnType}, ");
            sb.Append($"PRIMARY KEY(\"{obj.RelationNameLeft}\", \"{obj.RelationNameRight}\")");
            sb.Append(");");

            sb.Append($"ALTER TABLE \"{intermediateTableName}\" ");
            sb.Append($"ADD CONSTRAINT \"FK_{intermediateTableName}_{obj.ObjectNameLeft}\" ");
            sb.Append($"FOREIGN KEY (\"{obj.RelationNameLeft}\") ");
            sb.Append($"REFERENCES \"{obj.ObjectNameLeft}\" (\"{leftColumnName}\") ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION;");

            sb.Append($"ALTER TABLE \"{intermediateTableName}\" ");
            sb.Append($"ADD CONSTRAINT \"FK_{intermediateTableName}_{obj.ObjectNameRight}\" ");
            sb.Append($"FOREIGN KEY (\"{obj.RelationNameRight}\") ");
            sb.Append($"REFERENCES \"{obj.ObjectNameRight}\" (\"{rightColumnName}\") ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION;");

            obj.RelationTable = intermediateTableName;

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

            var intermediateTableBaseName = $"Relation_{obj.ObjectNameLeft}_{obj.ObjectNameRight}";

            DataSet dataSet = new DataSet();

            using (var conn = new NpgsqlConnection(connectionStr))
            {
                conn.Open();

                var query = $"SELECT con.conname FROM pg_catalog.pg_constraint con INNER JOIN pg_catalog.pg_class rel ON rel.oid = con.conrelid INNER JOIN pg_catalog.pg_namespace nsp ON nsp.oid = connamespace WHERE con.conname LIKE '{intermediateTableBaseName}%' ORDER BY con.oid DESC";

                var adapter = new NpgsqlDataAdapter(query, conn);

                adapter.Fill(dataSet, "TempTable");
            }

            DataTable dataTable = dataSet.Tables["TempTable"]!;

            if (dataTable.Rows.Count == 0)
            {
                return 0;
            }
            else
            {
                var lastTableName = dataTable.Rows[0]["conname"].ToString();

                var number = Regex.Match(lastTableName!, @"\d+$").Value;

                return int.Parse(number);
            }
        }

        public string GetSQLQueryToCreateRelationToMany(DXRelationDefinitionUnit obj, bool isNullable, bool isUnique)
        {
            var result = GetSQLQueryToCreateRelationToMany(
                    obj.ObjectNameRight,
                    obj.ObjectNameLeft,
                    obj.RelationNameLeft,
                    obj.RelationColumnNameLeft!,
                    obj.RelationColumnTypeLeft!.Value,
                    isNullable,
                    isUnique);

            return result;
        }

        private string GetSQLQueryToCreateRelationToMany(
            string ObjectNameRight,
            string ObjectNameLeft,
            string RelationNameLeft,
            string RelationColumnNameLeft,
            DXColumnTypeEnum RelationColumnTypeLeft,
            bool isNullable,
            bool isUnique)
        {
            StringBuilder sb = new StringBuilder();

            var nullValue = isNullable ? "NULL" : "NOT NULL";
            var uniqueValue = isUnique ? "UNIQUE" : "";

            var leftColumnName = RelationColumnNameLeft;
            var leftColumnType = this.GetPostgreSQLDataType(RelationColumnTypeLeft);

            sb.Append($"ALTER TABLE \"{ObjectNameRight}\" ");
            sb.Append($"ADD COLUMN \"{RelationNameLeft}\" {leftColumnType} {nullValue} {uniqueValue};");

            sb.Append($"ALTER TABLE \"{ObjectNameRight}\" ");
            sb.Append($"ADD CONSTRAINT \"FK_{ObjectNameRight}_{RelationNameLeft}\" ");
            sb.Append($"FOREIGN KEY(\"{RelationNameLeft}\") ");
            sb.Append($"REFERENCES \"{ObjectNameLeft}\" (\"{leftColumnName}\") ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION; ");

            return sb.ToString();
        }

        public string GetSQLQueryToCreateTable(DXObjectDefinitionUnit dataDXElement)
        {
            StringBuilder sb = new StringBuilder();

            // Create base table with column
            sb.Append($"CREATE TABLE IF NOT EXISTS \"{dataDXElement.Name}\"(");

            var clmDefList = dataDXElement.DXColumnDefinitionElement.Announced
                //.Where(x => !IsDXColumnEnum(x))
                .Select(x => this.GetSQLColumnDefinitionToAddInTable(x))
                .ToList();

            sb.Append(string.Join(",", clmDefList));
            sb.Append(");");

            //// Alter Enum relations
            //var clmDefEnumList = dataDXElement.DXColumnDefinitionElement.Announced
            //   .Where(x => IsDXColumnEnum(x))             
            //   .ToList();

            //if (clmDefEnumList.Count() > 0)
            //{
            //    foreach (var clmDefEnum in clmDefEnumList)
            //    {
            //        var query = this.GetSQLQueryToCreateRelationToMany(
            //            dataDXElement.Name,
            //            clmDefEnum.);

            //        sb.Append(query);
            //    }
            //}

            // Alter Unique constrains



            return sb.ToString();
        }

        public string GetSQLQueryToProcessConstraintsForUniqueColumns(string dxObjectName, IEnumerable<string[]> uniqueColumnsToAdd, IEnumerable<string[]> uniqueColumnsToRemove)
        {
            // Drops must come before adds in case a constraint is being recreated under the same name
            var clauses = uniqueColumnsToRemove
                .Select(x => this.GetSQLToDropColumnsUniqueFromAlterTable(dxObjectName, x))
                .Concat(uniqueColumnsToAdd.Select(x => this.GetSQLToAddColumnsUniqueToAlterTable(dxObjectName, x)))
                .ToList();

            if (clauses.Count == 0)
                return string.Empty;

            return $"ALTER TABLE \"{dxObjectName}\" {string.Join(",", clauses)};";
        }
        public string? GetSQLQueryToCreateTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement)
        {
            if (obj == null || dxElement == null)
                return null;

            var dxElementInDXUnitInfo = obj.DXElementInUnitDefinitionElement?.Announced.SingleOrDefault(x => x.DXElementDefinitionUnit == dxElement.Id);

            if (dxElementInDXUnitInfo == null)
                return null;

            StringBuilder sb = new StringBuilder();

            if (dxElement.IsCommon)
            {
                // Common DXElement stores the owning DXUnit via (DXUnitId, DXUnitType) instead of per-unit nullable <DXUnitTypeName>Id columns.
                sb.Append($"ALTER TABLE \"{dxElement.Name}\" ");
                sb.Append($"ADD COLUMN IF NOT EXISTS \"DXUnitType\" uuid; ");

                var fkName = $"FK_{dxElement.Name}_DXUnitType_0000";
                sb.Append($@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = '{fkName}') THEN
        ALTER TABLE ""{dxElement.Name}""
        ADD CONSTRAINT ""{fkName}""
        FOREIGN KEY (""DXUnitType"")
        REFERENCES ""DXUnitDefinitionUnit"" (""Id"")
        ON DELETE NO ACTION
        ON UPDATE NO ACTION;
    END IF;
END $$;
");

                sb.Append($"CREATE INDEX IF NOT EXISTS \"IX_{dxElement.Name}_DXUnitType_DXUnitId\" ");
                sb.Append($"ON \"{dxElement.Name}\" (\"DXUnitType\", \"DXUnitId\"); ");

                // Per-unit cardinality enforcement via partial index (needed because a common DXElement can be Single in one DXUnit and Multi in another).
                if (dxElementInDXUnitInfo.RelationType == DXElementInUnitTypeEnum.SingleOptional
                    || dxElementInDXUnitInfo.RelationType == DXElementInUnitTypeEnum.SingleMandatory)
                {
                    sb.Append($"CREATE UNIQUE INDEX IF NOT EXISTS \"UX_{dxElement.Name}_{obj.Name}_DXUnitId\" ");
                    sb.Append($"ON \"{dxElement.Name}\" (\"DXUnitId\") ");
                    sb.Append($"WHERE \"DXUnitType\" = '{obj.Id}'; ");
                }
                else
                {
                    sb.Append($"CREATE INDEX IF NOT EXISTS \"IX_{dxElement.Name}_{obj.Name}_DXUnitId\" ");
                    sb.Append($"ON \"{dxElement.Name}\" (\"DXUnitId\") ");
                    sb.Append($"WHERE \"DXUnitType\" = '{obj.Id}'; ");
                }

                return sb.ToString();
            }

            sb.Append($"ALTER TABLE \"{dxElement.Name}\" ");
            sb.Append($"ADD COLUMN \"{obj.Name}Id\" uuid; ");

            if (dxElementInDXUnitInfo.RelationType == DXElementInUnitTypeEnum.SingleOptional
            || dxElementInDXUnitInfo.RelationType == DXElementInUnitTypeEnum.SingleMandatory
            )
            {
                sb.Append($"ALTER TABLE \"{dxElement.Name}\" ");
                sb.Append($"ADD CONSTRAINT \"{obj.Name}ID_unique\" UNIQUE(\"{obj.Name}Id\"); ");
            }

            sb.Append($"ALTER TABLE \"{dxElement.Name}\" ");
            sb.Append($"ADD INDEX \"FK_{dxElement.Name}_{obj.Name}_0000_idx\" (\"{obj.Name}Id\" ASC) VISIBLE; ");
            sb.Append($"ALTER TABLE \"{dxElement.Name}\" ");
            sb.Append($"ADD CONSTRAINT \"FK_{dxElement.Name}_{obj.Name}_0000\" ");
            sb.Append($"FOREIGN KEY (\"{obj.Name}Id\") ");
            sb.Append($"REFERENCES \"{obj.Name}\" (\"Id\") ");
            sb.Append($"ON DELETE NO ACTION ");
            sb.Append($"ON UPDATE NO ACTION;");


            return sb.ToString();
        }

        public string GetSQLQueryToDeleteRelationManyToOne(DXRelationDefinitionUnit obj)
        {
            var sb = new StringBuilder();

            sb.Append($"ALTER TABLE \"{obj.ObjectNameLeft}\" ");
            sb.Append($"DROP CONSTRAINT \"FK_{obj.ObjectNameLeft}_{obj.RelationNameRight}\";");
            sb.Append($"ALTER TABLE \"{obj.ObjectNameLeft}\" ");
            sb.Append($"DROP COLUMN \"{obj.RelationNameRight}\";");
            //sb.Append($"DROP INDEX \"FK_{obj.ObjectNameLeft}_{obj.RelationNameRight}\";");

            return sb.ToString();
        }

        public string GetSQLQueryToDeleteRelationOneToMany(DXRelationDefinitionUnit obj)
        {
            var sb = new StringBuilder();

            sb.Append($"ALTER TABLE \"{obj.ObjectNameRight}\" ");
            sb.Append($"DROP CONSTRAINT \"FK_{obj.ObjectNameRight}_{obj.RelationNameLeft}\";");
            sb.Append($"ALTER TABLE \"{obj.ObjectNameRight}\" ");
            sb.Append($"DROP COLUMN \"{obj.RelationNameLeft}\";");
            //sb.Append($"DROP INDEX \"FK_{obj.ObjectNameRight}_{obj.RelationNameLeft}\";");

            return sb.ToString();
        }

        public string GetSQLQueryToDeleteRelationOneToZeroOne(DXRelationDefinitionUnit obj)
        {
            var sb = new StringBuilder();

            sb.Append($"ALTER TABLE \"{obj.ObjectNameRight}\" ");
            sb.Append($"DROP CONSTRAINT \"FK_{obj.ObjectNameRight}_{obj.RelationNameLeft}\";");
            sb.Append($"ALTER TABLE \"{obj.ObjectNameRight}\" ");
            sb.Append($"DROP COLUMN \"{obj.RelationNameLeft}\";");
            //sb.Append($"DROP INDEX \"{obj.RelationNameLeft}\";");

            return sb.ToString();
        }

        public string GetSQLQueryToDeleteRelationZeroOneToOne(DXRelationDefinitionUnit obj)
        {
            var sb = new StringBuilder();

            sb.Append($"ALTER TABLE \"{obj.ObjectNameLeft}\" ");
            sb.Append($"DROP CONSTRAINT \"FK_{obj.ObjectNameLeft}_{obj.RelationNameRight}\";");
            sb.Append($"ALTER TABLE \"{obj.ObjectNameLeft}\" ");
            sb.Append($"DROP COLUMN \"{obj.RelationNameRight}\";");
            //sb.Append($"DROP INDEX \"{obj.RelationNameRight}\";");

            return sb.ToString();
        }

        public string GetSQLQueryToDropTable(DXObjectDefinitionUnit dataDXElement)
        {
            // TODO: need to find solution how to drop table by DXUnitId
            return GetSQLQueryToDropTable(dataDXElement.Name);
        }

        public string GetSQLQueryToDropTable(string tableName)
        {
            // TODO: need to find solution how to drop table by DXUnitId
            return $"DROP TABLE IF EXISTS \"{tableName}\"";
        }

        public string? GetSQLQueryToDropTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement)
        {
            if (obj == null || dxElement == null)
                return null;

            StringBuilder sb = new StringBuilder();

            if (dxElement.IsCommon)
            {
                sb.Append($"DROP INDEX IF EXISTS \"UX_{dxElement.Name}_{obj.Name}_DXUnitId\"; ");
                sb.Append($"DROP INDEX IF EXISTS \"IX_{dxElement.Name}_{obj.Name}_DXUnitId\"; ");
                return sb.ToString();
            }

            sb.Append($"ALTER TABLE \"{dxElement.Name}\" ");
            sb.Append($"DROP CONSTRAINT IF EXISTS \"FK_{dxElement.Name}_{obj.Name}_0000\"; ");
            sb.Append($"DROP INDEX IF EXISTS \"FK_{dxElement.Name}_{obj.Name}_0000_idx\"; ");
            sb.Append($"ALTER TABLE \"{dxElement.Name}\" ");
            sb.Append($"DROP COLUMN IF EXISTS \"{obj.Name}Id\"; ");

            return sb.ToString();
        }

        public string GetSQLQueryToSelectIDFromTable(string tableName)
        {
            return $"SELECT \"Id\" FROM \"{tableName}\"";
        }

        public string GetSQLQueryToSetColumnNotNull(string tableName, string columnName)
        {
            return $"ALTER TABLE \"{tableName}\" ALTER COLUMN \"{columnName}\" SET NOT NULL";
        }

        public string GetSQLQueryToUpdateColumn(string tableName, string columnName, object value, Guid id)
        {
            return $"UPDATE \"{tableName}\" SET \"{columnName}\" = {FormatSQLValue(value)} WHERE \"Id\" = '{id}'";
        }

        public string GetSQLQueryToUpdateColumn(string tableName, string columnName, object value, IDictionary<string, object> whereConditions)
        {
            var whereClauses = whereConditions
                .Select(kv => $"\"{kv.Key}\" = {FormatSQLValue(kv.Value)}");
            var whereClause = string.Join(" AND ", whereClauses);
            return $"UPDATE \"{tableName}\" SET \"{columnName}\" = {FormatSQLValue(value)} WHERE {whereClause}";
        }

        private static string? FormatSQLValue(object value) => value switch
        {
            Guid g => $"'{g}'",
            string s => $"'{s.Replace("'", "''")}'",
            bool b => b ? "true" : "false",
            null => "NULL",
            _ => value.ToString()
        };

        public string QuoteIdentifier(string identifier)
        {
            return $"\"{identifier}\"";
        }

        public string FormatTableAlias(string tableName, string alias)
        {
            return $"{QuoteIdentifier(tableName)} AS {QuoteIdentifier(alias)}";
        }

        public string FormatColumnReference(string tableAlias, string columnName)
        {
            return $"{QuoteIdentifier(tableAlias)}.{QuoteIdentifier(columnName)}";
        }

        public string FormatColumnAlias(string columnExpression, string alias)
        {
            return $"{columnExpression} AS {QuoteIdentifier(alias)}";
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

                    exceptions.Add(new Exception($"Query : {query}"));

                    throw new AggregateException(exceptions);
                }
            }
        }

        public async Task<bool> TryAcquireMigrationLockAsync(
            DbConnection connection,
            string lockName,
            CancellationToken cancellationToken)
        {
            return await ExecuteBoolScalarAsync(
                    connection,
                    "SELECT pg_try_advisory_lock(hashtext(@p0), 0);",
                    cancellationToken,
                    ("@p0", lockName))
                .ConfigureAwait(false);
        }

        public async Task ReleaseMigrationLockAsync(
            DbConnection connection,
            string lockName,
            CancellationToken cancellationToken)
        {
            await ExecuteBoolScalarAsync(
                    connection,
                    "SELECT pg_advisory_unlock(hashtext(@p0), 0);",
                    cancellationToken,
                    ("@p0", lockName))
                .ConfigureAwait(false);
        }

        private static async Task<bool> ExecuteBoolScalarAsync(
            DbConnection connection,
            string commandText,
            CancellationToken cancellationToken,
            params (string Name, object Value)[] parameters)
        {
            using var command = connection.CreateCommand();
            command.CommandText = commandText;

            foreach (var (name, value) in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = value ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }

            var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return ToBool(result);
        }

        private static bool ToBool(object? value)
        {
            if (value == null || value is DBNull)
            {
                return false;
            }

            return value switch
            {
                bool b => b,
                byte b => b != 0,
                short s => s != 0,
                int i => i != 0,
                long l => l != 0,
                decimal d => d != 0,
                string s => string.Equals(s, "true", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(s, "t", StringComparison.OrdinalIgnoreCase)
                    || s == "1",
                _ => Convert.ToInt64(value) != 0
            };
        }

        private void RunSQLQueryWithoutTransactionDXElement(string connectionString, string query)
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
                    var count = (long)command.ExecuteScalar()!;

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
            string? pgbSqlDataType = null;

            switch (clmType)
            {
                case DXColumnTypeEnum.Bool:
                    pgbSqlDataType = "boolean";
                    break;
                case DXColumnTypeEnum.DateTime:
                    //mysqlDataType = "time";
                    pgbSqlDataType = "timestamp with time zone";
                    break;
                case DXColumnTypeEnum.Decimal:
                    pgbSqlDataType = "decimal";
                    break;
                case DXColumnTypeEnum.GUID:
                    pgbSqlDataType = "uuid";
                    break;
                case DXColumnTypeEnum.Int:
                    pgbSqlDataType = "integer";
                    break;
                case DXColumnTypeEnum.String:
                    pgbSqlDataType = "varchar";
                    break;
                case DXColumnTypeEnum.HashedString:
                    pgbSqlDataType = "varchar";
                    break;
                case DXColumnTypeEnum.EncryptedString:
                    pgbSqlDataType = "text";
                    break;
                case DXColumnTypeEnum.TimeStamp:
                    //pgbSqlDataType = "timestamp";
                    pgbSqlDataType = "timestamp with time zone";
                    break;
                case DXColumnTypeEnum.Text:
                    pgbSqlDataType = "text";
                    break;
                case DXColumnTypeEnum.Short:
                    pgbSqlDataType = "smallint";
                    break;
                case DXColumnTypeEnum.Long:
                    pgbSqlDataType = "bigint";
                    break;
                case DXColumnTypeEnum.Float:
                    pgbSqlDataType = "real";
                    break;
                case DXColumnTypeEnum.Currency:
                    pgbSqlDataType = "NUMERIC(13,4)";
                    break;
                case DXColumnTypeEnum.Blob:
                    pgbSqlDataType = "bytea";
                    break;
                default: throw new Exception($"There are no supported type for {clmType}");
            }

            return pgbSqlDataType;
        }

        public string GetWhereExpressionForId(Guid id)
        {
            return $"\"Id\" = '{id}'";
        }

        public string GetWhereExpressionForDXUnitId(Guid id)
        {
            return $"\"DXUnitId\" = '{id}'";
        }

        public string? GetWhereExpressionWithAnd(IDictionary<string, object> values)
        {
            if (values == null)
                return null;

            return string.Join(" AND ", values.Select(x => $"\"{x.Key}\" = '{x.Value}'"));
        }

        public string GetWhereExpressionForId(IEnumerable<Guid> ids)
        {
            string idsString = String.Join(",", ids.Select(x => $"'{x}'"));

            return $"\"Id\" IN ({idsString})";
        }

        public string GetWhereExpressionForDXUnitId(IEnumerable<Guid> ids)
        {
            string idsString = String.Join(",", ids.Select(x => $"'{x}'"));

            return $"\"DXUnitId\" IN ({idsString})";
        }

        public void BulkInsert(DbConnection connection, DataTable table, string tableName)
        {
            if (connection is not NpgsqlConnection conn)
                throw new ArgumentException("BulkInsert requires NpgsqlConnection.", nameof(connection));

            if (table.Rows.Count == 0) return;

            var columns = string.Join(",", table.Columns.Cast<DataColumn>().Select(c => $"\"{c.ColumnName}\""));
            var sql = $"COPY \"{tableName}\" ({columns}) FROM STDIN (FORMAT BINARY)";

            using var writer = conn.BeginBinaryImport(sql);
            foreach (DataRow row in table.Rows)
            {
                writer.StartRow();
                foreach (DataColumn col in table.Columns)
                {
                    var val = row[col];
                    if (val == DBNull.Value)
                    {
                        writer.WriteNull();
                        continue;
                    }

                    if (val is DateTime dt)
                    {
                        if (dt.Kind == DateTimeKind.Unspecified)
                            dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);

                        writer.Write(dt, NpgsqlDbType.TimestampTz);
                        continue;
                    }

                    writer.Write(val);
                }
            }
            writer.Complete();
        }

        public void BulkUpsert(DbConnection connection, DataTable table, string tableName, string keyColumn = "Id")
        {
            if (connection is not NpgsqlConnection conn)
                throw new ArgumentException("BulkUpsert requires NpgsqlConnection.", nameof(connection));

            if (table.Rows.Count == 0) return;

            var temp = $"temp_{tableName}_{Guid.NewGuid():N}";

            using (var cmd = new NpgsqlCommand($@"CREATE TEMP TABLE ""{temp}"" AS SELECT * FROM ""{tableName}"" WITH NO DATA;", conn))
                cmd.ExecuteNonQuery();

            BulkInsert(conn, table, temp);

            var allCols = table.Columns.Cast<DataColumn>().Select(c => $"\"{c.ColumnName}\"");
            var updates = string.Join(", ",
                table.Columns.Cast<DataColumn>()
                    .Where(c => !string.Equals(c.ColumnName, keyColumn, StringComparison.OrdinalIgnoreCase))
                    .Select(c => $"\"{c.ColumnName}\" = EXCLUDED.\"{c.ColumnName}\""));

            var mergeSql = $@"
                INSERT INTO ""{tableName}"" ({string.Join(",", allCols)})
                SELECT {string.Join(",", allCols)} FROM ""{temp}""
                ON CONFLICT (""{keyColumn}"") DO UPDATE SET {updates};";

            using (var cmd = new NpgsqlCommand(mergeSql, conn))
                cmd.ExecuteNonQuery();
        }
    }
}
