namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitQueryService<TResponse>
    {
        Task<TResponse?> GetAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<TResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<TResponse>> GetAsync(string filter, CancellationToken ct = default);
    }
}
