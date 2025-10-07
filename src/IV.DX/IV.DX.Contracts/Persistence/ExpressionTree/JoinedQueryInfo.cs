namespace IV.DX.Contracts.Persistence.ExpressionTree
{
    public class JoinedQueryInfo
    {
        public string JoinedTableName { get; set; }
        public string JoinedTableAlias { get; set; }
        public string JoinedTableKey { get; set; }
        public string MainTableAlias { get; set; }
        public string MainTableKey { get; set; }
    }
}