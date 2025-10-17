using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXStructureCache
    {
        IReadOnlyList<DXElementDefinitionUnit> DXElements { get; }
        IReadOnlyList<DXUnitDefinitionUnit> DXUnits { get; }
        IReadOnlyList<DXEnumDefinitionUnit> DXEnums { get; }
        IReadOnlyList<DXRelationDefinitionUnit> DXRelations { get; }

        int Version { get; }
        Task WarmUpAsync(CancellationToken ct = default);
        Task RefreshAsync(CancellationToken ct = default);
    }
}