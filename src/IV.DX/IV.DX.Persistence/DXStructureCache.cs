using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace IV.DX.Persistence
{
    internal sealed class DXStructureCache : IDXStructureCache
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private volatile Snapshot _snapshot = Snapshot.Empty;
        private readonly object _gate = new();

        public DXStructureCache(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

        public IReadOnlyList<DXElementDefinitionUnit> DXElements => _snapshot.Blocks;
        public IReadOnlyList<DXUnitDefinitionUnit> DXUnits => _snapshot.Entities;
        public IReadOnlyList<DXEnumDefinitionUnit> DXEnums => _snapshot.Enums;
        public IReadOnlyList<DXRelationDefinitionUnit> DXRelations => _snapshot.Relations;
        public int Version => _snapshot.Version;

        public async Task WarmUpAsync(CancellationToken ct = default)
        {
            if (_snapshot.Version != 0) return;
            await RefreshAsync(ct);
        }

        public Task RefreshAsync(CancellationToken ct = default)
        {
            lock (_gate)
            {
                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IDXStructureRawReader>();
            
                var blocks = repo.LoadDXElementInfosRaw();
                var entities = repo.LoadDXUnitInfosRaw();
                var enums = repo.LoadDXEnumInfosRaw();
                var relations = repo.LoadDXRelationInfosRaw();

                var snap = new Snapshot(
                    blocks.ToImmutableArray(),
                    entities.ToImmutableArray(),
                    enums.ToImmutableArray(),
                    relations.ToImmutableArray(),
                    _snapshot.Version + 1);

                Volatile.Write(ref _snapshot, snap);
                return Task.CompletedTask;
            }
        }

        private sealed record Snapshot(
            ImmutableArray<DXElementDefinitionUnit> Blocks,
            ImmutableArray<DXUnitDefinitionUnit> Entities,
            ImmutableArray<DXEnumDefinitionUnit> Enums,
            ImmutableArray<DXRelationDefinitionUnit> Relations,
            int Version)
        {
            public static readonly Snapshot Empty = new(
                ImmutableArray<DXElementDefinitionUnit>.Empty,
                ImmutableArray<DXUnitDefinitionUnit>.Empty,
                ImmutableArray<DXEnumDefinitionUnit>.Empty,
                ImmutableArray<DXRelationDefinitionUnit>.Empty,
                0);
        }
    }
}
