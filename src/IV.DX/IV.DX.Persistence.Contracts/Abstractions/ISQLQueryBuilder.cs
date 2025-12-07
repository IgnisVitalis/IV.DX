namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface ISQLQueryBuilder
    {
        string BuildSQLExpression(string typeName, IDictionary<string, string>? columns = default, string? dxFilter = default);
    }
}
