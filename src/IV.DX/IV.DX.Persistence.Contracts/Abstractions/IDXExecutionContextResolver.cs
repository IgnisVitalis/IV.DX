namespace IV.DX.Persistence.Contracts.Abstractions
{
    public interface IDXExecutionContextResolver
    {
        Task<DXExecutionContext> ResolveAsync(Guid identityLoginId, Guid sessionId, string? subjectId, CancellationToken ct = default);
    }
}
