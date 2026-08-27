using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Services
{
    internal sealed class DXUnitDtoService<TRequest, TResponse, TUnit, TMapper>
        : IDXUnitDtoService<TRequest, TResponse>
        where TUnit : DXUnit, new()
        where TMapper : DXUnitMapper<TRequest, TResponse, TUnit>
    {
        private readonly IDXUnitDataReader _reader;
        private readonly IDXOwnershipReader _ownershipReader;
        private readonly TMapper _mapper;
        private readonly DXUnitWriteOperations<TRequest, TUnit> _write;

        public DXUnitDtoService(
            IDXUnitDataReader reader,
            IDXOwnershipReader ownershipReader,
            IDXUnitDataService dataService,
            TMapper mapper)
        {
            _reader = reader;
            _ownershipReader = ownershipReader;
            _mapper = mapper;
            _write = new DXUnitWriteOperations<TRequest, TUnit>(dataService, mapper.ToUnitAsync);
        }

        public async Task<TResponse?> GetAsync(Guid id, CancellationToken ct = default)
        {
            var unit = await _reader.GetItemAsync<TUnit>(id, ct: ct);
            return unit is null ? default : await _mapper.ToDtoAsync(unit, ct);
        }

        public async Task<IEnumerable<TResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var units = await _reader.GetItemsAsync<TUnit>(ct: ct);
            return await MapManyAsync(units, ct);
        }

        public async Task<IEnumerable<TResponse>> GetAsync(string filter, CancellationToken ct = default)
        {
            var units = await _reader.GetItemsAsync<TUnit>(filter, ct: ct);
            return await MapManyAsync(units, ct);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TResponse>> GetOwnedAsync(
            DXUnitTypeAccessOperation operation = DXUnitTypeAccessOperation.Read,
            CancellationToken ct = default)
        {
            var ownedIds = await _ownershipReader.GetOwnedIdsAsync<TUnit>(operation, ct);
            if (ownedIds.Count == 0)
                return [];

            var units = await _reader.GetItemsAsync<TUnit>(ownedIds, ct: ct);
            return await MapManyAsync(units, ct);
        }

        public Task<Guid> CreateAsync(TRequest dto, CancellationToken ct = default)
            => _write.CreateAsync(dto, ct);

        public Task<bool> UpdateAsync(TRequest dto, CancellationToken ct = default)
            => _write.UpdateAsync(dto, ct);

        public Task<Guid> SaveAsync(TRequest dto, CancellationToken ct = default)
            => _write.SaveAsync(dto, ct);

        public Task DeleteAsync(Guid id, CancellationToken ct = default)
            => _write.DeleteAsync(id, ct);

        private async Task<IEnumerable<TResponse>> MapManyAsync(IEnumerable<TUnit> units, CancellationToken ct)
        {
            var result = new List<TResponse>();
            foreach (var unit in units)
                result.Add(await _mapper.ToDtoAsync(unit, ct));
            return result;
        }
    }
}
