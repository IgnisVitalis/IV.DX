namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitDtoService<TRequest, TResponse>
        : IDXUnitQueryService<TResponse>
        , IDXUnitCommandService<TRequest>
    {
    }
}
