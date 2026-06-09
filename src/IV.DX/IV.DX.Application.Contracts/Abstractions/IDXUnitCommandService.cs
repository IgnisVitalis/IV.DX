namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitCommandService<TRequest>
    {
        Task<Guid> SaveAsync(TRequest dto, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
