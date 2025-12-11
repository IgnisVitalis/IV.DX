using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Attributes;
using IV.DX.Persistence;
using IV.DX.Persistence.Contracts.Abstractions;
using System.Reflection;

namespace IV.DX.Hosting
{
    internal sealed class DXInitializer : IDXInitializer
    {
        private readonly IDXUnitCoreRepository _coreRepo;
        private readonly IDXMigrationService _migration;
        private readonly IDXStructureCache _dXStructureCache;

        public DXInitializer(
            IDXUnitCoreRepository coreRepo,
            IDXMigrationService migration, 
            IDXStructureCache dXStructureCache)
        {
            _coreRepo = coreRepo;
            _migration = migration;
            _dXStructureCache = dXStructureCache;
        }

        public async Task InitCoreDataAsync(CancellationToken ct = default)
        {
            _coreRepo.CreateDataBase();

            DXMaintenanceToken.StartMaintenanceCore();

            await _dXStructureCache.RefreshAsync(ct);

            await _migration.MigrateCoreAsync(
                Assembly.GetAssembly(typeof(DXUnitAttribute)), 
                "Data/CorePreInit.json",
                "Data/CorePostInit.json", ct);
            DXMaintenanceToken.StopMaintenanceCore();

            await _dXStructureCache.RefreshAsync(ct);

            await _migration.MigrateCustomEmbeddedAsync(Assembly.GetAssembly(typeof(DXUnitAttribute)), "Data/Add.json", ct);

            await _dXStructureCache.RefreshAsync(ct);
        }

        public async Task InitCustomDataAsync(string configPath, CancellationToken ct = default)
        {
            await _migration.MigrateCustomAsync(configPath, ct);

            await _dXStructureCache.RefreshAsync(ct);
        }
    }
}