using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    /// <summary>
    /// Abstract base for full CRUD element mappers. Implement both directions.
    /// </summary>
    /// <remarks>
    /// <typeparamref name="TUnit"/> is a type argument rather than something inferred from
    /// <typeparamref name="TElement"/> because an element type does not determine its owner: an
    /// element declared <c>IsCommon</c> can belong to several unit types, and the access rules and
    /// the storage layout both depend on which one is meant.
    /// </remarks>
    public abstract class DXElementMapper<TRequest, TResponse, TElement, TUnit>
        where TElement : DXElement, new()
        where TUnit : DXUnit, new()
    {
        public abstract Task<TResponse> ToDtoAsync(TElement element, CancellationToken ct = default);
        public abstract Task<TElement> ToElementAsync(TRequest dto, CancellationToken ct = default);
    }
}
