using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Services
{
    /// <summary>
    /// Write half of the element DTO services. Both <see cref="DXElementDtoService{TRequest, TResponse, TElement, TUnit, TMapper}"/>
    /// and <see cref="DXElementCommandService{TRequest, TElement, TUnit, TWriteMapper}"/> delegate here,
    /// since their mappers share no common base and would otherwise carry identical implementations.
    /// </summary>
    internal sealed class DXElementWriteOperations<TRequest, TElement, TUnit>(
        IDXElementDataService dataService,
        Func<TRequest, CancellationToken, Task<TElement>> toElementAsync)
        where TElement : DXElement, new()
        where TUnit : DXUnit, new()
    {
        private static readonly string UnitTypeName = AttributeReader.GetDXUnitTypeName(typeof(TUnit));
        private static readonly string ElementTypeName = AttributeReader.GetDXElementTypeName(typeof(TElement));

        public async Task<Guid> CreateAsync(Guid dxUnitId, TRequest dto, CancellationToken ct = default)
        {
            var element = await toElementAsync(dto, ct);
            element.DXUnitId = dxUnitId;

            return await dataService.InsertAsync(UnitTypeName, element, ct);
        }

        public async Task<bool> UpdateAsync(Guid id, TRequest dto, CancellationToken ct = default)
        {
            var element = await ToElementAtAsync(id, dto, ct);

            // Access is enforced inside UpdateAsync, which reports a missing element as Guid.Empty.
            // Checking existence here instead would demand Read access the caller may not hold.
            return await dataService.UpdateAsync(UnitTypeName, element, ct) != Guid.Empty;
        }

        public async Task<bool> UpdateAsync(Guid dxUnitId, Guid id, TRequest dto, CancellationToken ct = default)
        {
            var element = await ToElementAtAsync(id, dto, ct);

            return await dataService.UpdateAsync(UnitTypeName, dxUnitId, element, ct) != Guid.Empty;
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
            => dataService.DeleteAsync(DeleteBlock(id), ct);

        public Task<bool> DeleteAsync(Guid dxUnitId, Guid id, CancellationToken ct = default)
            => dataService.DeleteAsync<TElement>(UnitTypeName, dxUnitId, id, ct);

        /// <summary>
        /// Maps the payload and stamps the identity onto it. The id comes from the caller, never from
        /// the mapper, so a request DTO carrying a stale or absent id cannot redirect the write.
        /// </summary>
        private async Task<TElement> ToElementAtAsync(Guid id, TRequest dto, CancellationToken ct)
        {
            var element = await toElementAsync(dto, ct);

            element.Id = id;

            // Cleared, not carried: the owner of an existing element is settled by storage, and a
            // value a mapper happened to put here would either be redundant or an attempt to move it.
            element.DXUnitId = Guid.Empty;

            return element;
        }

        private static DXDataBlock<DXElementRecord> DeleteBlock(Guid id) => new()
        {
            Meta = new DXMeta
            {
                Kind = "DXElement",
                Type = ElementTypeName,
                DXUnitContext = UnitTypeName
            },
            Data = new DXData<DXElementRecord>
            {
                Delete = [new DXDeleteRef { Id = id }]
            }
        };
    }
}
