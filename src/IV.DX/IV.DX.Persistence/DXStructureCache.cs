using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Xml.Linq;

namespace IV.DX.Persistence
{
    internal sealed class DXStructureCache : IDXStructureCache
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private volatile Snapshot _snapshot = Snapshot.Empty;
        private readonly object _gate = new();

        public DXStructureCache(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

        public IReadOnlyList<DXElementDefinitionUnit> DXElements => _snapshot.DXElements;
        public IReadOnlyList<DXUnitDefinitionUnit> DXUnits => _snapshot.DXUnits;
        public IReadOnlyList<DXEnumDefinitionUnit> DXEnums => _snapshot.DXEnums;
        public IReadOnlyList<DXRelationDefinitionUnit> DXRelations => _snapshot.DXRelations;

        public int Version => _snapshot.Version;

        public Task RefreshAsync(CancellationToken ct = default)
        {
            lock (_gate)
            {
                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IDXStructureRawReader>();


                var dxRelations = repo.LoadDXRelationInfosRaw();
                var dxUnits = repo.LoadDXUnitInfosRaw();
                var dxElements = repo.LoadDXElementInfosRaw();
                var dxEnums = repo.LoadDXEnumInfosRaw();

                this.SetColumnsFromRelations(dxRelations, dxUnits);
                this.SetColumnsFromRelations(dxRelations, dxElements);
                this.SetColumnsFromRelations(dxRelations, dxEnums);

                var snap = new Snapshot(
                    dxElements.ToImmutableArray(),
                    dxUnits.ToImmutableArray(),
                    dxEnums.ToImmutableArray(),
                    dxRelations.ToImmutableArray(),
                    _snapshot.Version + 1);

                Volatile.Write(ref _snapshot, snap);
                return Task.CompletedTask;
            }
        }

        private void SetColumnsFromRelations(IEnumerable<DXRelationDefinitionUnit> dxRelations, IEnumerable<DXObjectDefinitionUnit> dxObjects)
        {
            foreach (var dxObject in dxObjects)
            {
                var dxElementRelations = dxRelations.Where(x => x.DXRelationDefinitionMainElement.ObjectNameLeft == dxObject.Name).ToList();

                foreach (var dxElementRelation in dxElementRelations)
                {
                    var columnName = dxElementRelation.DXRelationDefinitionMainElement.RelationColumnNameLeft;
                    var columnType = dxElementRelation.DXRelationDefinitionMainElement.RelationColumnTypeLeft;

                    if (!dxObject.DXColumnDefinitionElement.Announced.Any(x => x.Name == columnName))
                    {
                        dxObject.DXColumnDefinitionElement.AddToAnnounced(
                             new DXColumnDefinitionElement()
                             {
                                 Name = columnName,
                                 ColumnType = columnType.Value
                             });
                    }
                }
            }
        }

        public DXEnumDefinitionUnit GetDXEnum(string name)
        {
            return this.DXEnums.SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public DXUnitDefinitionUnit GetDXUnit(string name)
        {
            return this.DXUnits.SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public DXElementDefinitionUnit GetDXElement(string name)
        {
            return this.DXElements.SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public DXElementInUnitTypeEnum GetElementInUnitRelationType(string dxUnitTypeName, string dxElementTypeName)
        {
            var dxUnit = this.GetDXUnit(dxUnitTypeName);
            var dxElement = this.GetDXElement(dxElementTypeName);

            var relation = dxUnit.DXElementInUnitDefinitionElement.Announced.Single(x => x.DXElementDefinitionUnit == dxElement.ID);

            return relation.RelationType;
        }

        public IEnumerable<DXRelationDefinitionUnit> GetDXRelations(string name)
        {
            return this.DXRelations.Where(x => x.DXRelationDefinitionMainElement.ObjectNameLeft.Equals(name)).ToList();
        }

        private sealed record Snapshot(
            ImmutableArray<DXElementDefinitionUnit> DXElements,
            ImmutableArray<DXUnitDefinitionUnit> DXUnits,
            ImmutableArray<DXEnumDefinitionUnit> DXEnums,
            ImmutableArray<DXRelationDefinitionUnit> DXRelations,
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
