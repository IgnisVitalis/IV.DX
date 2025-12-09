using IV.DX.Contracts.Persistence.ExpressionTree;
using IV.DX.Kernel.Models;
using System.Data.Common;

namespace IV.DX.Persistence.Abstractions
{
    internal interface ISQLQueryDXHelper
    {
        void CreateDataBase(string connectionString);
        void DropDataBase(string connectionString);
        string GetSQLQueryToCreateTable(DXObjectDefinitionUnit dataDXElement);
        string GetSQLQueryToSetUniqueColumns(DXObjectDefinitionUnit dataDXElement);
        string GetSQLColumnDefinitionToAddInTable(DXColumnDefinitionElement clmDesc);
        string GetSQLQueryToDropTable(string tableName);
        string GetSQLQueryToDropTable(DXObjectDefinitionUnit dataDXElement);
        string GetSQLQueryToAlterTable(DXObjectDefinitionUnit dataDXElementNew, DXObjectDefinitionUnit dataDXElementExisting);
        string GetSQLQueryToDeleteRelationZeroOneToOne(DXRelationDefinitionUnit obj);
        string GetSQLQueryToDeleteRelationOneToZeroOne(DXRelationDefinitionUnit obj);
        string GetSQLQueryToDeleteRelationManyToOne(DXRelationDefinitionUnit obj);
        string GetSQLQueryToDeleteRelationOneToMany(DXRelationDefinitionUnit obj);
        string GetSQLQueryToCreateRelationToMany(DXRelationDefinitionUnit obj, bool isNullable, bool isUnique);
        string GetSQLQueryToCreateRelationManyTo(DXRelationDefinitionUnit obj, bool isNullable, bool isUnique);
        string GetSQLQueryToCreateRelationManyToMany(DXRelationDefinitionUnit obj, string connectionStr);
        string GetSQLQueryToDropTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement);
        string GetSQLQueryToCreateTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement);
        DbCommandBuilder GetDbCommandBuilder(DbDataAdapter dataAdapter);
        DbDataAdapter GetDbDataAdapter(DbConnection dbconnection, string query);
        DbConnection GetDBConnection(string connectionStr);       
        void RunSQLQuery(string connectionString, string query);
        string GetSQLQueryToSelectIDFromTable(string tableName);
        string GetSelectQuery(DXCoreNode coreNode);
        string GetQueryToSetDXUnitInheritance(string childDXUnit, string baseDXUnit);
    }
}
