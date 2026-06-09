namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitDtoService<TDto>
    {
        Task<TDto?> GetAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<TDto>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<TDto>> GetAsync(string filter, CancellationToken ct = default);
        Task<Guid> SaveAsync(TDto dto, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
