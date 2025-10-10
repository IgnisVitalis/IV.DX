using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXStructureCache
    {
        IReadOnlyList<DXElementDefinitionUnit> Blocks { get; }
        IReadOnlyList<DXUnitDefinitionUnit> Entities { get; }
        IReadOnlyList<DXEnumDefinitionUnit> Enums { get; }
        IReadOnlyList<DXRelationDefinitionUnit> Relations { get; }

        int Version { get; }
        Task WarmUpAsync(CancellationToken ct = default);
        Task RefreshAsync(CancellationToken ct = default);
    }
}