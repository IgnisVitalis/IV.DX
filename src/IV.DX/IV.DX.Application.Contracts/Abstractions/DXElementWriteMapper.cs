using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    /// <summary>
    /// Abstract base for write-only element mappers. See <see cref="DXElementMapper{TRequest, TResponse, TElement, TUnit}"/>
    /// for why the owning unit type is a type argument.
    /// </summary>
    public abstract class DXElementWriteMapper<TRequest, TElement, TUnit>
        where TElement : DXElement, new()
        where TUnit : DXUnit, new()
    {
        public abstract Task<TElement> ToElementAsync(TRequest dto, CancellationToken ct = default);
    }
}
