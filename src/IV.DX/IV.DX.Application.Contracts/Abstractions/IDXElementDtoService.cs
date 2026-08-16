namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXElementDtoService<TRequest, TResponse>
        : IDXElementQueryService<TResponse>
        , IDXElementCommandService<TRequest>
    {
    }
}
