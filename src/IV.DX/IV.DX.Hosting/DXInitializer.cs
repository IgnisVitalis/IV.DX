using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Persistence;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Hosting
{
    internal sealed class DXInitializer : IDXInitializer
    {
        private readonly IDXCoreRepository _coreRepo;
        private readonly IDXMigrationService _migration;
        private readonly IDXStructureCache _dXStructureCache;

        public DXInitializer(
            IDXCoreRepository coreRepo,
            IDXMigrationService migration, 
            IDXStructureCache dXStructureCache)
        {
            _coreRepo = coreRepo;
            _migration = migration;
            _dXStructureCache = dXStructureCache;
        }

        public void InitCoreData()
        {
            _coreRepo.CreateDataBase();
            DXMaintenanceToken.StartMaintenanceCore();
            _migration.LoadCoreStructure();
            DXMaintenanceToken.StopMaintenanceCore();
        }

        public void InitCustomData(string configPath)
        {
            _migration.LoadStructure(configPath);
        }

        public void InitCache()
        {
            this._dXStructureCache.WarmUpAsync().Wait();
        }
    }
}
