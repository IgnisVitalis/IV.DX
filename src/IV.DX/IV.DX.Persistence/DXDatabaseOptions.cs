namespace IV.DX.Persistence
{
    internal sealed class DXDatabaseOptions
    {
        public string Type { get; set; } = "PostgreSQL"; // MySQL | PostgreSQL
        public string ConnectionString { get; set; } = "";
    }
}
