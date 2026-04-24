namespace IV.DX.Hosting
{
    public sealed class DXSecretsOptions
    {
        public const string SectionName = "Secrets";

        /// <summary>Database connection string.</summary>
        public string? DatabaseConnectionString { get; set; }

        /// <summary>Database type: "PostgreSQL", "MySQL", "MSSQL", etc.</summary>
        public string? DatabaseType { get; set; }

        /// <summary>Base64-encoded 32-byte AES encryption key. Generate: openssl rand -base64 32</summary>
        public string? EncryptionKey { get; set; }

        /// <summary>JWT signing key, min 32 chars. Overrides Security:JwtSigningKey.</summary>
        public string? JwtSigningKey { get; set; }
    }
}
