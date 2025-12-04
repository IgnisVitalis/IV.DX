namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface ISQLQueryBuilder
    {
        string BuildSQLExpression(string typeName, string? dxFilter = default);
    }
}
