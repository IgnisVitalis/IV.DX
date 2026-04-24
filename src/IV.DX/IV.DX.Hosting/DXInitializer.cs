using IV.DX.Application.Contracts;
using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Attributes;
using IV.DX.Persistence;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace IV.DX.Hosting
{
    internal sealed class DXInitializer : IDXInitializer
    {
        private readonly IDXUnitCoreRepository _coreRepo;
        private readonly IDXMigrationService _migration;
        private readonly IDXStructureCache _dXStructureCache;
        private readonly IDXSecurityState _securityState;
        private readonly IDXModuleRegistry _moduleRegistry;
        private readonly ILogger<DXInitializer> _logger;

        private bool _isInitialized;

        public DXInitializer(
            IDXUnitCoreRepository coreRepo,
            IDXMigrationService migration,
            IDXStructureCache dXStructureCache,
            IDXSecurityState securityState,
            IDXModuleRegistry moduleRegistry,
            ILogger<DXInitializer> logger)
        {
            _coreRepo = coreRepo;
            _migration = migration;
            _dXStructureCache = dXStructureCache;
            _securityState = securityState;
            _moduleRegistry = moduleRegistry;
            _logger = logger;
        }

        public async Task InitAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("DX initialization starting.");

            _coreRepo.CreateDataBase();
            _logger.LogDebug("Database schema verified.");

            DXMaintenanceToken.StartMaintenanceCore();

            await _dXStructureCache.RefreshAsync(ct);

            await _migration.MigrateCoreAsync(
                Assembly.GetAssembly(typeof(DXUnitAttribute))!,
                "Migration/DXCorePreInit.json",
                "Migration/DXCorePostInit.json", ct);
            DXMaintenanceToken.StopMaintenanceCore();

            await _dXStructureCache.RefreshAsync(ct);
            _securityState.LoadFromStructure();
            _logger.LogDebug("Security state loaded from structure.");

            await MigrateCustomEmbeddedAsync("Migration/DXQuery.json", ct);
            await MigrateCustomEmbeddedAsync("Migration/DXAction.json", ct);

            _isInitialized = true;
            _logger.LogInformation("DX initialization completed.");
        }

        public async Task InitDXSecurityDataAsync(CancellationToken ct = default)
        {
            if (!_isInitialized)
                throw new Exception("Please call InitAsync method before");

            await MigrateCustomEmbeddedAsync("Migration/DXSecurity.json", ct);
            _securityState.SetEnabled(true);
            _moduleRegistry.Register(DXModuleIds.Security);
            _logger.LogInformation("DX security module enabled.");
        }

        public async Task InitCustomDataAsync(string configPath, CancellationToken ct = default)
        {
            if (!_isInitialized)
                throw new Exception("Please call InitAsync method before");

            await _migration.MigrateCustomAsync(configPath, ct);

            await _dXStructureCache.RefreshAsync(ct);
        }

        private async Task MigrateCustomEmbeddedAsync(string path, CancellationToken ct = default)
        {
            await _migration.MigrateCustomEmbeddedAsync(Assembly.GetAssembly(typeof(DXUnitAttribute))!, path , ct);

            await _dXStructureCache.RefreshAsync(ct);
        }
    }
}
