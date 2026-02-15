namespace IV.DX.Persistence.Contracts.Abstractions
{
    public sealed class DXExecutionContext
    {
        public string? SubjectId { get; init; }
        public bool IsSystem { get; init; }
        public IReadOnlyCollection<string>? AllowedReadUnitTypes { get; init; }
        public IReadOnlyCollection<string>? AllowedWriteUnitTypes { get; init; }
    }
}

