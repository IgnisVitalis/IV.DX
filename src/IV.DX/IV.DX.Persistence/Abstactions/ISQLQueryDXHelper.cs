using IV.DX.Contracts.Persistence.ExpressionTree;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Models;
using System.Data.Common;

namespace IV.DX.Persistence.Abstractions
{
    internal interface ISQLQueryDXHelper
    {
        void CreateDataBase(string connectionString);
        void DropDataBase(string connectionString);
        QueryContainer ConvertToQueryContainer(string entityType, string esqlWhereExpression, IEnumerable<DXRelationDefinitionUnit> relationInfos);
        string GetSQLQueryToCreateTable(DXObjectDefinitionUnit dataBlock);
        string GetSQLColumnDefinitionToAddInTable(DXColumnDefinitionElement clmDesc);
        string GetSQLQueryToDropTable(string tableName);
        string GetSQLQueryToDropTable(DXObjectDefinitionUnit dataBlock);
        string GetSQLQueryToAlterTable(DXObjectDefinitionUnit dataBlockNew, DXObjectDefinitionUnit dataBlockExisting);
        string GetSQLQueryToDeleteRelationZeroOneToOne(DXRelationDefinitionUnit obj);
        string GetSQLQueryToDeleteRelationOneToZeroOne(DXRelationDefinitionUnit obj);
        string GetSQLQueryToDeleteRelationManyToOne(DXRelationDefinitionUnit obj);
        string GetSQLQueryToDeleteRelationOneToMany(DXRelationDefinitionUnit obj);
        string GetSQLQueryToCreateRelationToMany(DXRelationDefinitionUnit obj, bool isNullable, bool isUnique);
        string GetSQLQueryToCreateRelationManyTo(DXRelationDefinitionUnit obj, bool isNullable, bool isUnique);
        string GetSQLQueryToCreateRelationManyToMany(DXRelationDefinitionUnit obj, string connectionStr);
        string GetSQLQueryToDropTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit block);
        string GetSQLQueryToCreateTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit block);
        DbCommandBuilder GetDbCommandBuilder(DbDataAdapter dataAdapter);
        DbDataAdapter GetDbDataAdapter(DbConnection dbconnection, string query);
        DbConnection GetDBConnection(string connectionStr);
        string GetSQLQuery(string tableName, IEnumerable<string> columnNames = null, string whereClause = null, IDictionary<string, string> orderBy = null, int? limit = null);
        void RunSQLQuery(string connectionString, string query);
        string GetSQLQueryToSelectIDFromTable(string tableName);
        string GetSelectQuery(CoreNode coreNode);
        string GetQuery(string typeName, string esqlWhereExpression, IEnumerable<DXRelationDefinitionUnit> relationInfos);
        string GetQueryToSetEntityInheritance(string childEntity, string baseEntity);
        string GetWhereExpressionForID(Guid id);
        string GetWhereExpressionForObjectID(Guid id);
        string GetWhereExpressionForID(IEnumerable<Guid> ids);
        string GetWhereExpressionForObjectID(IEnumerable<Guid> ids);
        string GetWhereExpressionWithAnd(IDictionary<string, object> values);
    }
}
