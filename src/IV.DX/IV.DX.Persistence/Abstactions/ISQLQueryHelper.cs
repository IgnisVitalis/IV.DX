using IV.DX.Contracts.Persistence.ExpressionTree;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Models;
using System.Data.Common;

namespace IV.DX.Persistence.Abstractions
{
    public interface ISQLQueryHelper
    {
        void CreateDataBase(string connectionString);
        void DropDataBase(string connectionString);
        QueryContainer ConvertToQueryContainer(string entityType, string esqlWhereExpression, IEnumerable<DPRelationObject> relationInfos);
        string GetSQLQueryToCreateTable(DPObjectDescObject dataBlock);
        string GetSQLColumnDefinitionToAddInTable(DPColumnDescBlock clmDesc);
        string GetSQLQueryToDropTable(string tableName);
        string GetSQLQueryToDropTable(DPObjectDescObject dataBlock);
        string GetSQLQueryToAlterTable(DPObjectDescObject dataBlockNew, DPObjectDescObject dataBlockExisting);
        string GetSQLQueryToDeleteRelationZeroOneToOne(DPRelationObject obj);
        string GetSQLQueryToDeleteRelationOneToZeroOne(DPRelationObject obj);
        string GetSQLQueryToDeleteRelationManyToOne(DPRelationObject obj);
        string GetSQLQueryToDeleteRelationOneToMany(DPRelationObject obj);
        string GetSQLQueryToCreateRelationToMany(DPRelationObject obj, bool isNullable, bool isUnique);
        string GetSQLQueryToCreateRelationManyTo(DPRelationObject obj, bool isNullable, bool isUnique);
        string GetSQLQueryToCreateRelationManyToMany(DPRelationObject obj, string connectionStr);
        string GetSQLQueryToDropTable(DPEntityDescObject obj, DPBlockDescObject block);
        string GetSQLQueryToCreateTable(DPEntityDescObject obj, DPBlockDescObject block);
        DbCommandBuilder GetDbCommandBuilder(DbDataAdapter dataAdapter);
        DbDataAdapter GetDbDataAdapter(DbConnection dbconnection, string query);
        DbConnection GetDBConnection(string connectionStr);
        string GetSQLQuery(string tableName, IEnumerable<string> columnNames = null, string whereClause = null, IDictionary<string, string> orderBy = null, int? limit = null);
        void RunSQLQuery(string connectionString, string query);
        string GetSQLQueryToSelectIDFromTable(string tableName);
        string GetSelectQuery(CoreNode coreNode);
        string GetQuery(string typeName, string esqlWhereExpression, IEnumerable<DPRelationObject> relationInfos);
        string GetQueryToSetEntityInheritance(string childEntity, string baseEntity);
        string GetWhereExpressionForID(Guid id);
        string GetWhereExpressionForObjectID(Guid id);
        string GetWhereExpressionForID(IEnumerable<Guid> ids);
        string GetWhereExpressionForObjectID(IEnumerable<Guid> ids);
        string GetWhereExpressionWithAnd(IDictionary<string, object> values);
    }
}
