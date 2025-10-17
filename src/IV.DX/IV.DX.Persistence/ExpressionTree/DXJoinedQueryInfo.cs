namespace IV.DX.Contracts.Persistence.ExpressionTree
{
    internal class DXJoinedQueryInfo
    {
        public string JoinedTableName { get; set; }
        public string JoinedTableAlias { get; set; }
        public string JoinedTableKey { get; set; }
        public string MainTableAlias { get; set; }
        public string MainTableKey { get; set; }
    }
}