using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Abstractions
{
    internal interface ISQLSchemaHelper
    {
        void CreateDataBase(string connectionString);
        void DropDataBase(string connectionString);
        string GetSQLQueryToCreateTable(DXObjectDefinitionUnit dataDXElement);
        string GetSQLQueryToProcessConstraintsForUniqueColumns(string dxObjectName, IEnumerable<string[]> uniqueColumnsToAdd, IEnumerable<string[]> uniqueColumnsToRemove);
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
        string? GetSQLQueryToDropTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement);
        string? GetSQLQueryToCreateTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement);
        string GetSQLQueryToSelectIDFromTable(string tableName);
        string GetQueryToSetDXUnitInheritance(string childDXUnit, string baseDXUnit);
        string GetSQLQueryToUpdateColumn(string tableName, string columnName, object value, Guid id);
        string GetSQLQueryToUpdateColumn(string tableName, string columnName, object value, IDictionary<string, object> whereConditions);
        string GetSQLQueryToSetColumnNotNull(string tableName, string columnName);
    }
}
