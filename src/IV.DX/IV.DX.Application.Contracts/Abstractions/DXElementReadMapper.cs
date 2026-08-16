using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    /// <summary>
    /// Abstract base for read-only element mappers. See <see cref="DXElementMapper{TRequest, TResponse, TElement, TUnit}"/>
    /// for why the owning unit type is a type argument.
    /// </summary>
    public abstract class DXElementReadMapper<TResponse, TElement, TUnit>
        where TElement : DXElement, new()
        where TUnit : DXUnit, new()
    {
        public abstract Task<TResponse> ToDtoAsync(TElement element, CancellationToken ct = default);
    }
}
