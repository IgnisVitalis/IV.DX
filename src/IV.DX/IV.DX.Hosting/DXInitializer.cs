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
            await _migration.LoadCoreStructureAsync(
                Assembly.GetAssembly(typeof(DXUnitAttribute)), 
                "CoreData/CorePreInit.json",
                "CoreData/CorePostInit.json", ct);
            DXMaintenanceToken.StopMaintenanceCore();
        }

        public async Task InitCustomDataAsync(string configPath, CancellationToken ct = default)
        {
            await _migration.LoadStructureAsync(configPath, ct);
        }

        public async Task InitCacheAsync(CancellationToken ct = default)
        {
            await this._dXStructureCache.WarmUpAsync(ct);
        }
    }
}
