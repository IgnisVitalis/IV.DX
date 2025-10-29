using IV.DX.Kernel.Enums;
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

        public IReadOnlyList<DXElementDefinitionUnit> DXElements => _snapshot.DXElements;
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
            
                var dxElements = repo.LoadDXElementInfosRaw();
                var entities = repo.LoadDXUnitInfosRaw();
                var enums = repo.LoadDXEnumInfosRaw();
                var relations = repo.LoadDXRelationInfosRaw();

                var snap = new Snapshot(
                    dxElements.ToImmutableArray(),
                    entities.ToImmutableArray(),
                    enums.ToImmutableArray(),
                    relations.ToImmutableArray(),
                    _snapshot.Version + 1);

                Volatile.Write(ref _snapshot, snap);
                return Task.CompletedTask;
            }
        }

        public DXEnumDefinitionUnit GetDXEnum(string name)
        {
            return this.DXEnums.SingleOrDefault(x => x.DXObjectDefinitionMainElement.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public DXUnitDefinitionUnit GetDXUnit(string name)
        {
            return this.DXUnits.SingleOrDefault(x => x.DXObjectDefinitionMainElement.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public DXElementDefinitionUnit GetDXElement(string name)
        {
            return this.DXElements.SingleOrDefault(x => x.DXObjectDefinitionMainElement.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public DXElementInUnitTypeEnum GetElementInUnitRelationType(string dxUnitTypeName, string dxElementTypeName)
        {
            var dxUnit = this.GetDXUnit(dxUnitTypeName);
            var dxElement = this.GetDXElement(dxElementTypeName);

            var relation = dxUnit.DXElementInUnitDefinitionElement.Announced.Single(x => x.DXElementDefinitionUnit == dxElement.ID);

            return relation.RelationType;
        }

        private sealed record Snapshot(
            ImmutableArray<DXElementDefinitionUnit> DXElements,
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
