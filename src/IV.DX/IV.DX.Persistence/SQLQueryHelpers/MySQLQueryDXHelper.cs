using IV.DX.Contracts.Persistence.ExpressionTree;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Abstractions;
using IV.DX.Persistence.Models;
using Npgsql;


//using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;

namespace IV.DX.Persistence.SQLQueryHelpers
{
    internal class MySQLQueryDXHelper : ISQLQueryDXHelper
    {
        public void BulkInsert(NpgsqlConnection conn, DataTable table, string tableName)
        {
            throw new NotImplementedException();
        }

        public void BulkUpsert(NpgsqlConnection conn, DataTable table, string tableName, string keyColumn = "ID")
        {
            throw new NotImplementedException();
        }

        public DXQueryContainer ConvertToQueryContainer(string dxUnitType, string dxsqlWhereExpression, IEnumerable<DXRelationDefinitionUnit> relationInfos)
        {
            throw new NotImplementedException();
        }

        public void CreateDataBase(string connectionString)
        {
            throw new NotImplementedException();
        }

        public void DropDataBase(string connectionString)
        {
            throw new NotImplementedException();
        }

        public DbCommandBuilder GetDbCommandBuilder(DbDataAdapter dataAdapter)
        {
            throw new NotImplementedException();
        }

        public DbConnection GetDBConnection(string connectionStr)
        {
            throw new NotImplementedException();
        }

        public DbDataAdapter GetDbDataAdapter(DbConnection dbconnection, string query)
        {
            throw new NotImplementedException();
        }

        public string GetQuery(string typeName, string dxsqlWhereExpression, IEnumerable<DXRelationDefinitionUnit> relationInfos)
        {
            throw new NotImplementedException();
        }

        public string GetQueryToSetDXUnitInheritance(string childDXUnit, string baseDXUnit)
        {
            throw new NotImplementedException();
        }

        public string GetSelectQuery(DXCoreNode coreNode)
        {
            throw new NotImplementedException();
        }

        public string GetSQLColumnDefinitionToAddInTable(DXColumnDefinitionElement clmDesc)
        {
            throw new NotImplementedException();
        }

        public string GetSQLQuery(string tableName, IEnumerable<string> columnNames = null, string whereClause = null, IDictionary<string, string> orderBy = null, int? limit = null)
        {
            throw new NotImplementedException();
        }

        public string GetSQLQueryToAlterTable(DXObjectDefinitionUnit dataDXElementNew, DXObjectDefinitionUnit dataDXElementExisting)
        {
            throw new NotImplementedException();
        }

        public string GetSQLQueryToCreateRelationManyTo(DXRelationDefinitionUnit obj, bool isNullable, bool isUnique)
        {
            throw new NotImplementedException();
        }

        public string GetSQLQueryToCreateRelationManyToMany(DXRelationDefinitionUnit obj, string connectionStr)
        {
            throw new NotImplementedException();
        }

        public string GetSQLQueryToCreateRelationToMany(DXRelationDefinitionUnit obj, bool isNullable, bool isUnique)
        {
            throw new NotImplementedException();
        }

        public string GetSQLQueryToCreateTable(DXObjectDefinitionUnit dataDXElement)
        {
            throw new NotImplementedException();
        }

        public string GetSQLQueryToCreateTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement)
        {
            throw new NotImplementedException();
        }

        public string GetSQLQueryToDeleteRelationManyToOne(DXRelationDefinitionUnit obj)
        {
            throw new NotImplementedException();
        }

        public string GetSQLQueryToDeleteRelationOneToMany(DXRelationDefinitionUnit obj)
        {
            throw new NotImplementedException();
        }

        public string GetSQLQueryToDeleteRelationOneToZeroOne(DXRelationDefinitionUnit obj)
        {
            throw new NotImplementedException();
        }

        public string GetSQLQueryToDeleteRelationZeroOneToOne(DXRelationDefinitionUnit obj)
        {
            throw new NotImplementedException();
        }

        public string GetSQLQueryToDropTable(string tableName)
        {
            throw new NotImplementedException();
        }

        public string GetSQLQueryToDropTable(DXObjectDefinitionUnit dataDXElement)
        {
            throw new NotImplementedException();
        }

        public string GetSQLQueryToDropTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement)
        {
            throw new NotImplementedException();
        }

        public string GetSQLQueryToSelectIDFromTable(string tableName)
        {
            throw new NotImplementedException();
        }

        public string GetWhereExpressionForID(Guid id)
        {
            throw new NotImplementedException();
        }

        public string GetWhereExpressionForID(IEnumerable<Guid> ids)
        {
            throw new NotImplementedException();
        }

        public string GetWhereExpressionForDXUnitID(Guid id)
        {
            throw new NotImplementedException();
        }

        public string GetWhereExpressionForDXUnitID(IEnumerable<Guid> ids)
        {
            throw new NotImplementedException();
        }

        public string GetWhereExpressionWithAnd(IDictionary<string, object> values)
        {
            throw new NotImplementedException();
        }

        public void RunSQLQuery(string connectionString, string query)
        {
            throw new NotImplementedException();
        }
    }


    // Disable for now because MySql.Data uses a lot of dependencies.
    //internal class MySQLQueryDXHelper : ISQLQueryDXHelper
    //{
    //    public MySQLQueryDXHelper()
    //    {
    //    }

    //    public string GetQuery(string typeName, string dxsqlWhereExpression, IEnumerable<DXRelationDefinitionUnit> relationInfos)
    //    {
    //        if (string.IsNullOrEmpty(dxsqlWhereExpression))
    //            return GetSQLQueryToSelectIDFromTable(typeName);

    //        var result = this.ConvertToQueryContainer(typeName, dxsqlWhereExpression, relationInfos);

    //        return result.Query;
    //    }

    //    public DXQueryContainer ConvertToQueryContainer(
    //       string dxUnitType,
    //       string dxsqlWhereExpression,
    //       IEnumerable<DXRelationDefinitionUnit> relationInfos)
    //    {
    //        DXOrientedTree expressionTree = DXOrientedTree.CreateInstance(dxUnitType);

    //        expressionTree.Load(dxsqlWhereExpression);

    //        expressionTree.LoadAdditionalInfosToNodes(relationInfos);

    //        DXQueryContainer result = new DXQueryContainer
    //        {
    //            SelectExpression = this.GetSelectQuery(expressionTree.CoreNode)
    //        };

    //        IEnumerable<string> leftJoins = Enumerable.Empty<string>();

    //        foreach (var item in expressionTree.AllNodesWithoutCoreAndLeaves)
    //        {
    //            var nodeAsDXUnitNode = item as DXUnitNode;
    //            var nodeAsDXElementNode = item as DXElementNode;

    //            if (nodeAsDXUnitNode != null)
    //            {
    //                leftJoins = leftJoins.Concat(nodeAsDXUnitNode.QueryInfos.Select(x => this.GetLeftJoinQuery(x)));
    //            }
    //            else if (nodeAsDXElementNode != null)
    //            {
    //                leftJoins = leftJoins.Append(this.GetLeftJoinQuery(nodeAsDXElementNode.QueryInfo));
    //            }
    //        }

    //        result.LeftJoinsExpression = string.Join(" ", leftJoins);

    //        result.WhereExpression = string.Join(" ", expressionTree.Leaves.OrderBy(x => x.ExpressionOrder).Select(x => this.GetWhereExpressionWithPropertyAndLogicOpeation(x)));

    //        return result;
    //    }

    //    private string GetLeftJoinQuery(DXJoinedQueryInfo queryInfo)
    //    {
    //        return $"LEFT JOIN {queryInfo.JoinedTableName} AS {queryInfo.JoinedTableAlias} ON {queryInfo.JoinedTableAlias}.{queryInfo.JoinedTableKey} = {queryInfo.MainTableAlias}.{queryInfo.MainTableKey}";
    //    }

    //    public string GetWhereExpressionWithPropertyAndLogicOpeation(DXPropertyNode propertyNode)
    //    {
    //        StringBuilder sb = new StringBuilder();

    //        if (propertyNode.ExpressionOrder > 0)
    //        {
    //            sb.Append(propertyNode.LogicOperation);
    //            sb.Append(" ");
    //        }

    //        sb.Append($"{propertyNode.Mother.TableNameAliasToJoin}.{propertyNode.Value}");

    //        return sb.ToString();
    //    }


    //    public string GetSelectQuery(DXCoreNode coreNode)
    //    {
    //        return $"SELECT {coreNode.MainTableAlias}.ID FROM {coreNode.Value} AS {coreNode.MainTableAlias}";
    //    }

    //    public string GetSQLQueryToCreateTable(DXObjectDefinitionUnit dataDXElement)
    //    {
    //        // CREATE TABLE IF NOT EXISTS tasks (
    //        //     task_id INT AUTO_INCREMENT PRIMARY KEY,
    //        //     title VARCHAR(255) NOT NULL,
    //        //     start_date DATE,
    //        //     due_date DATE,
    //        //     status TINYINT NOT NULL,
    //        //     priority TINYINT NOT NULL,
    //        //     description TEXT,
    //        //     created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    //        // )  ENGINE=INNODB;

    //        StringBuilder sb = new StringBuilder();

    //        sb.Append($"CREATE TABLE IF NOT EXISTS {dataDXElement.DXObjectDefinitionMainElement.Name}(");

    //        var clmDefList = dataDXElement.DXColumnDefinitionElement.Announced.Select(x => this.GetSQLColumnDefinitionToAddInTable(x));

    //        var clmUniqueList = dataDXElement.DXUniqueColumnsElement.Announced.Select(x => this.GetSQLColumnsUniqueToAddInTable(x));

    //        sb.Append(string.Join(",", clmDefList));

    //        if (clmUniqueList.Count() > 0)
    //        {
    //            sb.Append(",");
    //            sb.Append(string.Join(",", clmUniqueList));
    //        }

    //        sb.Append(")ENGINE=INNODB");

    //        return sb.ToString();
    //    }

    //    public string GetSQLColumnDefinitionToChangeInTable(
    //        DXColumnDefinitionElement DXColumnDefinitionElementNew,
    //        DXColumnDefinitionElement DXColumnDefinitionElementExisting)
    //    {
    //        // CHANGE COLUMN `name` `nameNew` DECIMAL NULL DEFAULT 10,

    //        var mySQLQueryToChangeColumn = $"CHANGE COLUMN `{DXColumnDefinitionElementExisting.Name}` {this.GetSQLColumnDefinitionToAddInTable(DXColumnDefinitionElementNew)}";

    //        return mySQLQueryToChangeColumn;
    //    }

    //    public string GetSQLQueryToDropTable(DXObjectDefinitionUnit dataDXElement)
    //    {
    //        // TODO: need to find solution how to drop table by DXUnitID
    //        return GetSQLQueryToDropTable(dataDXElement.DXObjectDefinitionMainElement.Name);
    //    }

    //    public string GetSQLQueryToDropTable(string tableName)
    //    {
    //        // TODO: need to find solution how to drop table by DXUnitID
    //        return $"DROP TABLE IF EXISTS {tableName}";
    //    }

    //    private string GetSQLColumnsUniqueToAddInTable(DXUniqueColumnsElement clmDesc)
    //    {
    //        var columns = clmDesc.Columns.Split(',').Select(x => x.Trim());

    //        var columnsWithBrackets = columns.Select(x => $"`{x}`");

    //        var uniqueKeyName = $"UC_{string.Join("_", columns)}";

    //        string result = $"CONSTRAINT {uniqueKeyName} UNIQUE({string.Join(",", columnsWithBrackets)})";

    //        return result;
    //    }

    //    public string GetSQLColumnDefinitionToAddInTable(DXColumnDefinitionElement clmDesc)
    //    {
    //        string mysqlClmDef = "";

    //        mysqlClmDef = $"`{clmDesc.Name}` {this.GetMySQLDataType(clmDesc.ColumnType)}";

    //        if (clmDesc.Length.HasValue)
    //        {
    //            mysqlClmDef += $"({clmDesc.Length.Value})";
    //        }

    //        if ((!clmDesc.AllowNull || clmDesc.Name == "DXUnitID") && clmDesc.Name != "ID")
    //        {
    //            mysqlClmDef += $" NOT NULL";
    //        }

    //        if (!string.IsNullOrEmpty(clmDesc.DefaultValue)
    //        && clmDesc.Name != "ID"
    //        && clmDesc.Name != "DXUnitID")
    //        {
    //            mysqlClmDef += $" DEFAULT {clmDesc.DefaultValue}";
    //        }

    //        if (clmDesc.Name == "ID")
    //        {
    //            mysqlClmDef += $" PRIMARY KEY UNIQUE";
    //        }

    //        return mysqlClmDef;
    //    }

    //    public string GetSQLQueryToAlterTable(
    //        DXObjectDefinitionUnit dataDXElementNew,
    //        DXObjectDefinitionUnit dataDXElementExisting)
    //    {
    //        // ALTER TABLE `new_table` 
    //        // DROP COLUMN `pwd`,
    //        // ADD COLUMN `newColumn` VARCHAR(45) NOT NULL DEFAULT 'default text',
    //        // CHANGE COLUMN `name` `nameNew` DECIMAL NULL DEFAULT 10,
    //        // RENAME TO `new_table_Updated` ;
    //        StringBuilder sb = new StringBuilder();

    //        var columnsToDrop = this.GetColumnDescDXElementsToDrop(dataDXElementNew, dataDXElementExisting);
    //        var columnsToAdd = this.GetColumnDescDXElementsToAdd(dataDXElementNew, dataDXElementExisting);
    //        var columnsToChange = this.GetColumnDescDXElementsToChange(dataDXElementNew, dataDXElementExisting);

    //        var columnsToDropMySQLCommand = columnsToDrop.Select(x => $"DROP COLUMN `{x.Name}`");
    //        var columnsToAddMySQLCommand = columnsToAdd.Select(x => $"ADD COLUMN {this.GetSQLColumnDefinitionToAddInTable(x)}");
    //        var columnsToChangeMySQLCommand = columnsToChange.Select(x =>
    //            this.GetSQLColumnDefinitionToChangeInTable(
    //                dataDXElementNew.DXColumnDefinitionElement.Announced.Single(y => y.ID == x),
    //                dataDXElementExisting.DXColumnDefinitionElement.Announced.Single(y => y.ID == x)));

    //        sb.Append($"ALTER TABLE {dataDXElementExisting.DXObjectDefinitionMainElement.Name} ");
    //        if (columnsToDropMySQLCommand != null && columnsToDropMySQLCommand.Count() > 0)
    //        {
    //            sb.Append($"{string.Join(",", columnsToDropMySQLCommand)},");
    //        }
    //        if (columnsToAddMySQLCommand != null && columnsToAddMySQLCommand.Count() > 0)
    //        {
    //            sb.Append($"{string.Join(",", columnsToAddMySQLCommand)},");
    //        }
    //        if (columnsToChangeMySQLCommand != null && columnsToChangeMySQLCommand.Count() > 0)
    //        {
    //            sb.Append($"{string.Join(",", columnsToChangeMySQLCommand)},");
    //        }
    //        sb.Append($"RENAME TO {dataDXElementNew.DXObjectDefinitionMainElement.Name}");

    //        return sb.ToString();
    //    }

    //    public string GetSQLQueryToDeleteRelationZeroOneToOne(DXRelationDefinitionUnit obj)
    //    {
    //        var sb = new StringBuilder();

    //        sb.Append($"ALTER TABLE {obj.DXRelationDefinitionMainElement.ObjectNameLeft} ");
    //        sb.Append($"DROP FOREIGN KEY `FK_{obj.DXRelationDefinitionMainElement.ObjectNameLeft}_{obj.DXRelationDefinitionMainElement.RelationNameRight}`;");
    //        sb.Append($"ALTER TABLE {obj.DXRelationDefinitionMainElement.ObjectNameLeft} ");
    //        sb.Append($"DROP COLUMN `{obj.DXRelationDefinitionMainElement.RelationNameRight}`, ");
    //        sb.Append($"DROP INDEX `{obj.DXRelationDefinitionMainElement.RelationNameRight}`;");

    //        return sb.ToString();
    //    }

    //    public string GetSQLQueryToDeleteRelationOneToZeroOne(DXRelationDefinitionUnit obj)
    //    {
    //        var sb = new StringBuilder();

    //        sb.Append($"ALTER TABLE {obj.DXRelationDefinitionMainElement.ObjectNameRight} ");
    //        sb.Append($"DROP FOREIGN KEY `FK_{obj.DXRelationDefinitionMainElement.ObjectNameRight}_{obj.DXRelationDefinitionMainElement.RelationNameLeft}`;");
    //        sb.Append($"ALTER TABLE {obj.DXRelationDefinitionMainElement.ObjectNameRight} ");
    //        sb.Append($"DROP COLUMN `{obj.DXRelationDefinitionMainElement.RelationNameLeft}`, ");
    //        sb.Append($"DROP INDEX `{obj.DXRelationDefinitionMainElement.RelationNameLeft}`;");

    //        return sb.ToString();
    //    }

    //    public string GetSQLQueryToDropTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement)
    //    {
    //        // ALTER TABLE `IV.DX.TestDB`.`Table1` 
    //        // DROP FOREIGN KEY `fk_Table1_Table2_0000`;
    //        // ALTER TABLE `IV.DX.TestDB`.`Table1` 
    //        // DROP INDEX `fk_Table1_Table2_0000_idx` ;
    //        // ALTER TABLE `IV.DX.TestDB`.`Table1` 
    //        // DROP COLUMN Table2ID;

    //        StringBuilder sb = new StringBuilder();

    //        sb.Append($"ALTER TABLE {dxElement.DXObjectDefinitionMainElement.Name} ");
    //        sb.Append($"DROP FOREIGN KEY `FK_{dxElement.DXObjectDefinitionMainElement.Name}_{obj.DXObjectDefinitionMainElement.Name}_0000`; ");
    //        sb.Append($"ALTER TABLE {dxElement.DXObjectDefinitionMainElement.Name} ");
    //        sb.Append($"DROP INDEX `FK_{dxElement.DXObjectDefinitionMainElement.Name}_{obj.DXObjectDefinitionMainElement.Name}_0000_idx`;");
    //        sb.Append($"ALTER TABLE {dxElement.DXObjectDefinitionMainElement.Name} ");
    //        sb.Append($"DROP COLUMN {obj.DXObjectDefinitionMainElement.Name}ID; ");

    //        return sb.ToString();
    //    }

    //    public string GetSQLQueryToCreateTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement)
    //    {
    //        // ALTER TABLE `IV.DX.TestDB`.`Table1` 
    //        // ADD COLUMN Table2ID CHAR(36) CHARACTER SET UTF8MB4; ;
    //        // ALTER TABLE `IV.DX.TestDB`.`Table1` 
    //        // ADD INDEX `fk_Table1_Table2_0000_idx` (`Table2ID` ASC) VISIBLE;
    //        // ;
    //        // ALTER TABLE `IV.DX.TestDB`.`Table1` 
    //        // ADD CONSTRAINT `fk_Table1_Table2_0000`
    //        //   FOREIGN KEY (`Table2ID`)
    //        //   REFERENCES `IV.DX.TestDB`.`Table2` (`ID`)
    //        //   ON DELETE NO ACTION
    //        //   ON UPDATE NO ACTION;

    //        if (obj == null || dxElement == null)
    //            return null;

    //        var dxElementInDXUnitInfo = obj.DXElementInUnitDefinitionElement?.Announced.SingleOrDefault(x => x.DXElementDefinitionUnit == dxElement.ID);

    //        if (dxElementInDXUnitInfo == null)
    //            return null;

    //        StringBuilder sb = new StringBuilder();

    //        sb.Append($"ALTER TABLE {dxElement.DXObjectDefinitionMainElement.Name} ");
    //        sb.Append($"ADD COLUMN {obj.DXObjectDefinitionMainElement.Name}ID CHAR(36) CHARACTER SET UTF8MB4; ");

    //        if (dxElementInDXUnitInfo.RelationType == DXElementInUnitTypeEnum.SingleOptional
    //        || dxElementInDXUnitInfo.RelationType == DXElementInUnitTypeEnum.SingleMandatory
    //        )
    //        {
    //            sb.Append($"ALTER TABLE {dxElement.DXObjectDefinitionMainElement.Name} ");
    //            sb.Append($"ADD CONSTRAINT {obj.DXObjectDefinitionMainElement.Name}ID_unique UNIQUE({obj.DXObjectDefinitionMainElement.Name}ID); ");
    //        }

    //        sb.Append($"ALTER TABLE {dxElement.DXObjectDefinitionMainElement.Name} ");
    //        sb.Append($"ADD INDEX `FK_{dxElement.DXObjectDefinitionMainElement.Name}_{obj.DXObjectDefinitionMainElement.Name}_0000_idx` (`{obj.DXObjectDefinitionMainElement.Name}ID` ASC) VISIBLE; ");
    //        sb.Append($"ALTER TABLE {dxElement.DXObjectDefinitionMainElement.Name} ");
    //        sb.Append($"ADD CONSTRAINT `FK_{dxElement.DXObjectDefinitionMainElement.Name}_{obj.DXObjectDefinitionMainElement.Name}_0000` ");
    //        sb.Append($"FOREIGN KEY (`{obj.DXObjectDefinitionMainElement.Name}ID`) ");
    //        sb.Append($"REFERENCES `{obj.DXObjectDefinitionMainElement.Name}` (`ID`) ");
    //        sb.Append($"ON DELETE NO ACTION ");
    //        sb.Append($"ON UPDATE NO ACTION;");

    //        return sb.ToString();
    //    }

    //    public string GetSQLQueryToDeleteRelationManyToOne(DXRelationDefinitionUnit obj)
    //    {
    //        var sb = new StringBuilder();

    //        sb.Append($"ALTER TABLE {obj.DXRelationDefinitionMainElement.ObjectNameLeft} ");
    //        sb.Append($"DROP FOREIGN KEY `FK_{obj.DXRelationDefinitionMainElement.ObjectNameLeft}_{obj.DXRelationDefinitionMainElement.RelationNameRight}`;");
    //        sb.Append($"ALTER TABLE {obj.DXRelationDefinitionMainElement.ObjectNameLeft} ");
    //        sb.Append($"DROP COLUMN `{obj.DXRelationDefinitionMainElement.RelationNameRight}`, ");
    //        sb.Append($"DROP INDEX `FK_{obj.DXRelationDefinitionMainElement.ObjectNameLeft}_{obj.DXRelationDefinitionMainElement.RelationNameRight}`;");

    //        return sb.ToString();
    //    }


    //    public string GetSQLQueryToDeleteRelationOneToMany(DXRelationDefinitionUnit obj)
    //    {
    //        var sb = new StringBuilder();

    //        sb.Append($"ALTER TABLE {obj.DXRelationDefinitionMainElement.ObjectNameRight} ");
    //        sb.Append($"DROP FOREIGN KEY `FK_{obj.DXRelationDefinitionMainElement.ObjectNameRight}_{obj.DXRelationDefinitionMainElement.RelationNameLeft}`;");
    //        sb.Append($"ALTER TABLE {obj.DXRelationDefinitionMainElement.ObjectNameRight} ");
    //        sb.Append($"DROP COLUMN `{obj.DXRelationDefinitionMainElement.RelationNameLeft}`, ");
    //        sb.Append($"DROP INDEX `FK_{obj.DXRelationDefinitionMainElement.ObjectNameRight}_{obj.DXRelationDefinitionMainElement.RelationNameLeft}`;");

    //        return sb.ToString();
    //    }

    //    public string GetSQLQueryToCreateRelationManyTo(DXRelationDefinitionUnit obj, bool isNullable, bool isUnique)
    //    {
    //        StringBuilder sb = new StringBuilder();

    //        var nullValue = isNullable ? "NULL" : "NOT NULL";
    //        var uniqueValue = isUnique ? "UNIQUE" : "";

    //        var rightColumnName = obj.DXRelationDefinitionMainElement.RelationColumnNameRight;
    //        var rightColumnType = this.GetMySQLDataType(obj.DXRelationDefinitionMainElement.RelationColumnTypeRight.Value);

    //        sb.Append($"ALTER TABLE {obj.DXRelationDefinitionMainElement.ObjectNameLeft} ");
    //        sb.Append($"ADD COLUMN {obj.DXRelationDefinitionMainElement.RelationNameRight} {rightColumnType} {nullValue} {uniqueValue} AFTER `TimeStamp`;");

    //        sb.Append($"ALTER TABLE {obj.DXRelationDefinitionMainElement.ObjectNameLeft} ");
    //        sb.Append($"ADD CONSTRAINT `FK_{obj.DXRelationDefinitionMainElement.ObjectNameLeft}_{obj.DXRelationDefinitionMainElement.RelationNameRight}` ");
    //        sb.Append($"FOREIGN KEY(`{obj.DXRelationDefinitionMainElement.RelationNameRight}`) ");
    //        sb.Append($"REFERENCES `{obj.DXRelationDefinitionMainElement.ObjectNameRight}` (`{rightColumnName}`) ");
    //        sb.Append($"ON DELETE NO ACTION ");
    //        sb.Append($"ON UPDATE NO ACTION; ");

    //        return sb.ToString();
    //    }

    //    public string GetSQLQueryToCreateRelationToMany(DXRelationDefinitionUnit obj, bool isNullable, bool isUnique)
    //    {
    //        StringBuilder sb = new StringBuilder();

    //        var nullValue = isNullable ? "NULL" : "NOT NULL";
    //        var uniqueValue = isUnique ? "UNIQUE" : "";

    //        var leftColumnName = obj.DXRelationDefinitionMainElement.RelationColumnNameLeft;
    //        var leftColumnType = this.GetMySQLDataType(obj.DXRelationDefinitionMainElement.RelationColumnTypeLeft.Value);

    //        sb.Append($"ALTER TABLE {obj.DXRelationDefinitionMainElement.ObjectNameRight} ");
    //        sb.Append($"ADD COLUMN {obj.DXRelationDefinitionMainElement.RelationNameLeft} {leftColumnType} {nullValue} {uniqueValue} AFTER `TimeStamp`;");

    //        sb.Append($"ALTER TABLE {obj.DXRelationDefinitionMainElement.ObjectNameRight} ");
    //        sb.Append($"ADD CONSTRAINT `FK_{obj.DXRelationDefinitionMainElement.ObjectNameRight}_{obj.DXRelationDefinitionMainElement.RelationNameLeft}` ");
    //        sb.Append($"FOREIGN KEY(`{obj.DXRelationDefinitionMainElement.RelationNameLeft}`) ");
    //        sb.Append($"REFERENCES `{obj.DXRelationDefinitionMainElement.ObjectNameLeft}` (`{leftColumnName}`) ");
    //        sb.Append($"ON DELETE NO ACTION ");
    //        sb.Append($"ON UPDATE NO ACTION; ");

    //        return sb.ToString();
    //    }

    //    public string GetSQLQueryToCreateRelationManyToMany(DXRelationDefinitionUnit obj, string connectionStr)
    //    {
    //        StringBuilder sb = new StringBuilder();

    //        var numberOfTable = this.GetNumberOfIntermediateTable(obj, connectionStr);

    //        var intermediateTableName = $"Relation_{obj.DXRelationDefinitionMainElement.ObjectNameLeft}_{obj.DXRelationDefinitionMainElement.ObjectNameRight}_{numberOfTable}";

    //        var leftColumnName = obj.DXRelationDefinitionMainElement.RelationColumnNameLeft;
    //        var leftColumnType = this.GetMySQLDataType(obj.DXRelationDefinitionMainElement.RelationColumnTypeLeft.Value);
    //        var rightColumnName = obj.DXRelationDefinitionMainElement.RelationColumnNameRight;
    //        var rightColumnType = this.GetMySQLDataType(obj.DXRelationDefinitionMainElement.RelationColumnTypeRight.Value);

    //        sb.Append($"CREATE TABLE IF NOT EXISTS {intermediateTableName}(");
    //        sb.Append($"{obj.DXRelationDefinitionMainElement.RelationNameLeft} {leftColumnType},");
    //        sb.Append($"{obj.DXRelationDefinitionMainElement.RelationNameRight} {rightColumnType}, ");
    //        sb.Append($"PRIMARY KEY({obj.DXRelationDefinitionMainElement.RelationNameLeft}, {obj.DXRelationDefinitionMainElement.RelationNameRight})");
    //        sb.Append(")ENGINE=INNODB;");

    //        sb.Append($"ALTER TABLE {intermediateTableName} ");
    //        sb.Append($"ADD CONSTRAINT `FK_{intermediateTableName}_{obj.DXRelationDefinitionMainElement.ObjectNameLeft}` ");
    //        sb.Append($"FOREIGN KEY (`{obj.DXRelationDefinitionMainElement.RelationNameLeft}`) ");
    //        sb.Append($"REFERENCES `{obj.DXRelationDefinitionMainElement.ObjectNameLeft}` (`{leftColumnName}`) ");
    //        sb.Append($"ON DELETE NO ACTION ");
    //        sb.Append($"ON UPDATE NO ACTION;");

    //        sb.Append($"ALTER TABLE {intermediateTableName} ");
    //        sb.Append($"ADD CONSTRAINT `FK_{intermediateTableName}_{obj.DXRelationDefinitionMainElement.ObjectNameRight}` ");
    //        sb.Append($"FOREIGN KEY (`{obj.DXRelationDefinitionMainElement.RelationNameRight}`) ");
    //        sb.Append($"REFERENCES `{obj.DXRelationDefinitionMainElement.ObjectNameRight}` (`{rightColumnName}`) ");
    //        sb.Append($"ON DELETE NO ACTION ");
    //        sb.Append($"ON UPDATE NO ACTION;");

    //        obj.DXRelationDefinitionMainElement.RelationTable = intermediateTableName;

    //        return sb.ToString();
    //    }

    //    private int GetNumberOfIntermediateTable(DXRelationDefinitionUnit obj, string connectionStr)
    //    {
    //        var intermediateTableBaseName = $"Relation_{obj.DXRelationDefinitionMainElement.ObjectNameLeft}_{obj.DXRelationDefinitionMainElement.ObjectNameRight}";

    //        DataSet dataSet = new DataSet();

    //        using (MySqlConnection conn = new MySqlConnection(connectionStr))
    //        {
    //            conn.Open();

    //            this.PopulateTableToDataSet(
    //                conn,
    //                dataSet,
    //                "information_schema.tables",
    //                new List<string> { "table_name" },
    //                $"table_name LIKE '{intermediateTableBaseName}%' AND TABLE_SCHEMA = 'IV.DX.TestDB'",
    //                new Dictionary<string, string>() { { "CREATE_TIME", "DESC" } },
    //                1);
    //        }

    //        DataTable dataTable = dataSet.Tables["information_schema.tables"];

    //        if (dataTable.Rows.Count == 0)
    //        {
    //            return 0;
    //        }
    //        else
    //        {
    //            var lastTableName = dataTable.Rows[0][0].ToString();

    //            var number = Regex.Match(lastTableName, @"\d+$").Value;

    //            return int.Parse(number);
    //        }
    //    }

    //    public MySqlDataAdapter PopulateTableToDataSet(
    //        MySqlConnection conn,
    //        DataSet dataSet,
    //        string tableName,
    //        IEnumerable<string> columnNames = null,
    //        string whereClause = null,
    //        IDictionary<string, string> orderBy = null,
    //        int? limit = null)
    //    {
    //        StringBuilder sb = new StringBuilder();

    //        string columnNamesString = columnNames == null ? "*" : string.Join(",", ProtectReservedMySQLNames(columnNames));

    //        sb.Append($"SELECT {columnNamesString} FROM {tableName}");

    //        if (!string.IsNullOrEmpty(whereClause))
    //        {
    //            sb.Append($" WHERE {whereClause}");
    //        }

    //        if (orderBy != null && orderBy.Count() > 0)
    //        {
    //            string orderByString = string.Join(",", orderBy.Select(x => $"{x.Key} {x.Value}"));

    //            sb.Append($" ORDER BY {orderByString}");
    //        }

    //        if (limit.HasValue)
    //        {
    //            sb.Append($" LIMIT {limit.Value}");
    //        }

    //        sb.Append(";");

    //        var adapter = new MySqlDataAdapter(sb.ToString(), conn);

    //        adapter.Fill(dataSet, tableName);

    //        return adapter;
    //    }

    //    private IEnumerable<string> ProtectReservedMySQLNames(IEnumerable<string> income)
    //    {
    //        var reserevedMySQLNames = new List<string>()
    //        {
    //            "Precision"
    //        };

    //        return income.Select(x =>
    //        {
    //            if (reserevedMySQLNames.Contains(x))
    //            {
    //                return $"`{x}`";
    //            }
    //            else
    //            {
    //                return x;
    //            }

    //        }).ToList();
    //    }

    //    private IEnumerable<DXColumnDefinitionElement> GetColumnDescDXElementsToDrop(
    //       DXObjectDefinitionUnit dataDXElementNew,
    //       DXObjectDefinitionUnit dataDXElementExisting
    //       )
    //    {
    //        var columnDescDXElementNewIds = dataDXElementNew.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.ID);
    //        var columnDescDXElementExistingIds = dataDXElementExisting.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.ID);

    //        var idsToRemove = columnDescDXElementExistingIds.Where(x => !columnDescDXElementNewIds.Contains(x));

    //        return dataDXElementExisting.DXColumnDefinitionElement.Announced.Where(x => idsToRemove.Contains(x.ID)).ToList();
    //    }

    //    private IEnumerable<DXColumnDefinitionElement> GetColumnDescDXElementsToAdd(
    //        DXObjectDefinitionUnit dataDXElementNew,
    //        DXObjectDefinitionUnit dataDXElementExisting)
    //    {
    //        var columnDescDXElementNewIds = dataDXElementNew.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.ID);
    //        var columnDescDXElementExistingIds = dataDXElementExisting.DXColumnDefinitionElement.Announced.Where(x => this.FilterForNonSystemColumns(x.Name)).Select(x => x.ID);

    //        var idsToAdd = columnDescDXElementNewIds.Where(x => !columnDescDXElementExistingIds.Contains(x));

    //        return dataDXElementNew.DXColumnDefinitionElement.Announced.Where(x => idsToAdd.Contains(x.ID)).ToList();
    //    }

    //    private bool FilterForNonSystemColumns(string columnName)
    //    {
    //        return columnName != "ID" && columnName != "DXUnitID" && columnName != "TimeStamp";
    //    }

    //    private IEnumerable<Guid> GetColumnDescDXElementsToChange(
    //        DXObjectDefinitionUnit dataDXElementNew,
    //        DXObjectDefinitionUnit dataDXElementExisting)
    //    {
    //        var columnDescDXElementNewIds = dataDXElementNew.DXColumnDefinitionElement.Announced.Where(x => x.Name != "ID" && x.Name != "DXUnitID").Select(x => x.ID);
    //        var columnDescDXElementExistingIds = dataDXElementExisting.DXColumnDefinitionElement.Announced.Where(x => x.Name != "ID" && x.Name != "DXUnitID").Select(x => x.ID);

    //        var idsToChange = columnDescDXElementNewIds.Intersect(columnDescDXElementExistingIds).Where(x =>
    //        {
    //            var DXColumnDefinitionElementNew = dataDXElementNew.DXColumnDefinitionElement.Announced.Single(y => y.ID == x);
    //            var DXColumnDefinitionElementExisting = dataDXElementExisting.DXColumnDefinitionElement.Announced.Single(y => y.ID == x);

    //            var result = !(DXColumnDefinitionElementNew.AllowNull == DXColumnDefinitionElementExisting.AllowNull
    //            && this.AreEqual(DXColumnDefinitionElementNew.DefaultValue, DXColumnDefinitionElementExisting.DefaultValue)
    //            && DXColumnDefinitionElementNew.ColumnType == DXColumnDefinitionElementExisting.ColumnType
    //            && DXColumnDefinitionElementNew.Length == DXColumnDefinitionElementExisting.Length
    //            && DXColumnDefinitionElementNew.Name == DXColumnDefinitionElementExisting.Name);

    //            return result;
    //        });

    //        //return dataDXElementNew.DXColumnDefinitionElement.Where(x => idsToChange.Contains(x.ID)).ToList();
    //        return idsToChange;
    //    }

    //    private string GetMySQLDataType(DXColumnTypeEnum clmType)
    //    {
    //        string mysqlDataType = null;

    //        switch (clmType)
    //        {
    //            case DXColumnTypeEnum.Bool:
    //                mysqlDataType = "TINYINT";
    //                break;
    //            case DXColumnTypeEnum.DateTime:
    //                mysqlDataType = "DATETIME";
    //                break;
    //            case DXColumnTypeEnum.Decimal:
    //                mysqlDataType = "DECIMAL";
    //                break;
    //            case DXColumnTypeEnum.GUID:
    //                mysqlDataType = "CHAR(36) CHARACTER SET UTF8MB4";
    //                break;
    //            case DXColumnTypeEnum.Int:
    //                mysqlDataType = "INT";
    //                break;
    //            case DXColumnTypeEnum.String:
    //                mysqlDataType = "NVARCHAR";
    //                break;
    //            case DXColumnTypeEnum.TimeStamp:
    //                mysqlDataType = "TIMESTAMP";
    //                break;
    //            case DXColumnTypeEnum.Text:
    //                mysqlDataType = "LONGTEXT";
    //                break;
    //            case DXColumnTypeEnum.Short:
    //                mysqlDataType = "SMALLINT";
    //                break;
    //            case DXColumnTypeEnum.Long:
    //                mysqlDataType = "BIGINT";
    //                break;
    //            case DXColumnTypeEnum.Float:
    //                mysqlDataType = "FLOAT";
    //                break;
    //            case DXColumnTypeEnum.Currency:
    //                mysqlDataType = "DECIMAL(13,4)";
    //                break;
    //            case DXColumnTypeEnum.Blob:
    //                mysqlDataType = "BLOB";
    //                break;
    //        }

    //        return mysqlDataType;
    //    }

    //    private bool AreEqual(string a, string b)
    //    {
    //        if (string.IsNullOrEmpty(a))
    //        {
    //            return string.IsNullOrEmpty(b);
    //        }
    //        else
    //        {
    //            return string.Equals(a, b);
    //        }
    //    }

    //    public DbDataAdapter GetDbDataAdapter(DbConnection dbconnection, string query)
    //    {
    //        var conn = dbconnection as MySqlConnection;

    //        return new MySqlDataAdapter(query, conn);
    //    }

    //    public DbCommandBuilder GetDbCommandBuilder(DbDataAdapter dataAdapter)
    //    {
    //        DbCommandBuilder commandBuilder = new MySqlCommandBuilder
    //        {
    //            DataAdapter = dataAdapter as MySqlDataAdapter
    //        };

    //        return commandBuilder;
    //    }

    //    public string GetSQLQuery(string tableName, IEnumerable<string> columnNames = null, string whereClause = null, IDictionary<string, string> orderBy = null, int? limit = null)
    //    {
    //        StringBuilder sb = new StringBuilder();

    //        string columnNamesString = columnNames == null ? "*" : string.Join(",", ProtectReservedMySQLNames(columnNames));

    //        sb.Append($"SELECT {columnNamesString} FROM {tableName}");

    //        if (!string.IsNullOrEmpty(whereClause))
    //        {
    //            sb.Append($" WHERE {whereClause}");
    //        }

    //        if (orderBy != null && orderBy.Count() > 0)
    //        {
    //            string orderByString = string.Join(",", orderBy.Select(x => $"{x.Key} {x.Value}"));

    //            sb.Append($" ORDER BY {orderByString}");
    //        }

    //        if (limit.HasValue)
    //        {
    //            sb.Append($" LIMIT {limit.Value}");
    //        }

    //        sb.Append(";");

    //        return sb.ToString();
    //    }

    //    public DbConnection GetDBConnection(string connectionStr)
    //    {
    //        return new MySqlConnection(connectionStr);
    //    }

    //    public void RunSQLQuery(string connectionString, string query)
    //    {
    //        using (var conn = new MySqlConnection(connectionString))
    //        {
    //            conn.Open();
    //            var transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted);

    //            try
    //            {
    //                MySqlCommand mysqlCommand = new MySqlCommand(query, conn);
    //                mysqlCommand.ExecuteNonQuery();
    //                transaction.Commit();
    //            }
    //            catch (Exception exc)
    //            {
    //                var exceptions = new List<Exception>() { exc };
    //                try
    //                {
    //                    transaction.Rollback();
    //                }
    //                catch (Exception exc2)
    //                {
    //                    exceptions.Add(exc2);
    //                }

    //                throw new AggregateException(exceptions);
    //            }
    //        }
    //    }

    //    public string GetSQLQueryToSelectIDFromTable(string tableName)
    //    {
    //        return $"SELECT ID FROM {tableName}";
    //    }

    //    public void DropDataBase(string connectionString)
    //    {
    //        var args = this.GetParametersToCreateOrDeleteBD(connectionString);

    //        if (args == null)
    //            return;

    //        this.RunSQLQuery(args.Item2, $"DROP SCHEMA IF EXISTS `{args.Item1}`");
    //    }

    //    public void CreateDataBase(string connectionString)
    //    {
    //        var args = this.GetParametersToCreateOrDeleteBD(connectionString);

    //        if (args == null)
    //            return;

    //        this.RunSQLQuery(args.Item2, $"CREATE SCHEMA IF NOT EXISTS `{args.Item1}`");
    //    }

    //    private Tuple<string, string> GetParametersToCreateOrDeleteBD(string connectionString)
    //    {
    //        var parameters = connectionString.Split(';').Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim());

    //        var dbNameParameter = parameters.SingleOrDefault(x => x.Length > 8 && x.ToLower().Substring(0, 8) == "database");

    //        if (dbNameParameter == null)
    //            return null;

    //        var parametersWithoutDB = parameters.Where(x => x.Length < 8 || x.ToLower().Substring(0, 8) != "database");

    //        var connectionStringWithoutDatabase = string.Join(';', parametersWithoutDB);

    //        var dbName = dbNameParameter.Substring(dbNameParameter.IndexOf("=") + 1, dbNameParameter.Length - dbNameParameter.IndexOf("=") - 1).Trim();

    //        return new Tuple<string, string>(dbName, connectionStringWithoutDatabase);
    //    }

    //    public string GetQueryToSetDXUnitInheritance(string childDXUnit, string baseDXUnit)
    //    {
    //        StringBuilder sb = new StringBuilder();

    //        sb.Append($"ALTER TABLE {childDXUnit} ");
    //        sb.Append($"ADD CONSTRAINT `FK_{childDXUnit}_{baseDXUnit}_Base` ");
    //        sb.Append($"FOREIGN KEY (`ID`) ");
    //        sb.Append($"REFERENCES `{baseDXUnit}` (`ID`) ");
    //        sb.Append($"ON DELETE NO ACTION ");
    //        sb.Append($"ON UPDATE NO ACTION;");

    //        return sb.ToString();
    //    }

    //    public string GetWhereExpressionForID(Guid id)
    //    {
    //        return $"ID = '{id}'";
    //    }

    //    public string GetWhereExpressionForDXUnitID(Guid id)
    //    {
    //        return $"DXUnitID = '{id}'";
    //    }

    //    public string GetWhereExpressionWithAnd(IDictionary<string, object> values)
    //    {
    //        if (values == null)
    //            return null;

    //        return string.Join(" AND ", values.Select(x => $"{x.Key} = '{x.Value}'"));
    //    }

    //    public string GetWhereExpressionForID(IEnumerable<Guid> ids)
    //    {
    //        string idsString = String.Join(",", ids.Select(x => $"'{x}'"));

    //        return $"ID IN ({idsString})";
    //    }

    //    public string GetWhereExpressionForDXUnitID(IEnumerable<Guid> ids)
    //    {
    //        string idsString = String.Join(",", ids.Select(x => $"'{x}'"));

    //        return $"DXUnitID IN ({idsString})";
    //    }
    //}
}