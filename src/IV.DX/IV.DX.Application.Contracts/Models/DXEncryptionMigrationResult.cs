namespace IV.DX.Application.Contracts.Models
{
    public sealed class DXEncryptionMigrationResult
    {
        public int Reencrypted { get; init; }
        public int Failed { get; init; }
        public bool IsComplete => Failed == 0;
    }
}
