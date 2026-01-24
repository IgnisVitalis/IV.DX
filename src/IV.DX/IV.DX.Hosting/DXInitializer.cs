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

        private bool _isCoreInitialized;

        public DXInitializer(
            IDXUnitCoreRepository coreRepo,
            IDXMigrationService migration,
            IDXStructureCache dXStructureCache)
        {
            _coreRepo = coreRepo;
            _migration = migration;
            _dXStructureCache = dXStructureCache;
        }

        public async Task InitDXCoreDataAsync(CancellationToken ct = default)
        {
            _coreRepo.CreateDataBase();

            DXMaintenanceToken.StartMaintenanceCore();

            await _dXStructureCache.RefreshAsync(ct);

            await _migration.MigrateCoreAsync(
                Assembly.GetAssembly(typeof(DXUnitAttribute)),
                "Data/DXCorePreInit.json",
                "Data/DXCorePostInit.json", ct);
            DXMaintenanceToken.StopMaintenanceCore();

            await _dXStructureCache.RefreshAsync(ct);
            
            this._isCoreInitialized = true;
        }

        public async Task InitDXQueryDataAsync(CancellationToken ct = default)
        {
            await MigrateCustomEmbeddedAsync("Data/DXQuery.json", ct);
        }

        public async Task InitDXSecurityDataAsync(CancellationToken ct = default)
        {
            await MigrateCustomEmbeddedAsync("Data/DXSecurity.json", ct);
        }

        public async Task InitCustomDataAsync(string configPath, CancellationToken ct = default)
        {
            if (!this._isCoreInitialized)
            {
                throw new Exception("Please call InitCoreDataAsync method before");
            }

            await _migration.MigrateCustomAsync(configPath, ct);

            await _dXStructureCache.RefreshAsync(ct);
        }

        private async Task MigrateCustomEmbeddedAsync(string path, CancellationToken ct = default)
        {
            await _migration.MigrateCustomEmbeddedAsync(Assembly.GetAssembly(typeof(DXUnitAttribute)), path , ct);

            await _dXStructureCache.RefreshAsync(ct);
        }
    }
}