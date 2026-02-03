namespace IV.DX.Persistence.Abstractions
{
    internal interface ISQLDialect
    {
        string QuoteIdentifier(string identifier);
        string FormatTableAlias(string tableName, string alias);
        string FormatColumnReference(string tableAlias, string columnName);
        string FormatColumnAlias(string columnExpression, string alias);
    }
}
