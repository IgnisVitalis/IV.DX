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

        public async Task<bool> UpdateAsync(TRequest dto, CancellationToken ct = default)
        {
            var element = await toElementAsync(dto, ct);

            // Access is enforced inside UpdateAsync, which reports a missing element as Guid.Empty.
            // Checking existence here instead would demand Read access the caller may not hold.
            return await dataService.UpdateAsync(UnitTypeName, element, ct) != Guid.Empty;
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
            => dataService.DeleteAsync(DeleteBlock(id), ct);

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
