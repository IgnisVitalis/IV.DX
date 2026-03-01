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
        private readonly IDXSecurityState _securityState;

        private bool _isCoreInitialized;

        public DXInitializer(
            IDXUnitCoreRepository coreRepo,
            IDXMigrationService migration,
            IDXStructureCache dXStructureCache,
            IDXSecurityState securityState)
        {
            _coreRepo = coreRepo;
            _migration = migration;
            _dXStructureCache = dXStructureCache;
            _securityState = securityState;
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
            _securityState.LoadFromStructure();
            
            this._isCoreInitialized = true;
        }

        public async Task InitDXQueryDataAsync(CancellationToken ct = default)
        {
            if (!this._isCoreInitialized)
            {
                throw new Exception("Please call InitDXCoreDataAsync method before");
            }

            await MigrateCustomEmbeddedAsync("Data/DXQuery.json", ct);
        }

        public async Task InitDXSecurityDataAsync(CancellationToken ct = default)
        {
            if (!this._isCoreInitialized)
            {
                throw new Exception("Please call InitDXCoreDataAsync method before");
            }

            await MigrateCustomEmbeddedAsync("Data/DXSecurity.json", ct);
            _securityState.SetEnabled(true);
        }

        public async Task InitCustomDataAsync(string configPath, CancellationToken ct = default)
        {
            if (!this._isCoreInitialized)
            {
                throw new Exception("Please call InitDXCoreDataAsync method before");
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
