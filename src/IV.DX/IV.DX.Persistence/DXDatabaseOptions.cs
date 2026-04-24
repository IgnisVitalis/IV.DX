namespace IV.DX.Persistence
{
    internal sealed class DXDatabaseOptions
    {
        public string Type { get; set; } = "PostgreSQL"; // MySQL | PostgreSQL
        public string ConnectionString { get; set; } = "";
        public bool MigrationLockEnabled { get; set; } = true;
        public int MigrationLockTimeoutSeconds { get; set; } = 30;
        public int MigrationLockPollIntervalMilliseconds { get; set; } = 250;
        public string MigrationLockName { get; set; } = "IV.DX.Migrations";
    }
}
