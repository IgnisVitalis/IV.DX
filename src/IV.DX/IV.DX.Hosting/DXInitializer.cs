using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Persistence;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Hosting
{
    internal sealed class DXInitializer : IDXInitializer
    {
        private readonly IDXCoreRepository _coreRepo;
        private readonly IDXStructureRepository _structureRepo;
        private readonly IDXMigrationService _migration;

        public DXInitializer(
            IDXCoreRepository coreRepo,
            IDXStructureRepository structureRepo,
            IDXMigrationService migration)
        {
            _coreRepo = coreRepo;
            _structureRepo = structureRepo;
            _migration = migration;
        }

        public void DropDatabase() => _coreRepo.DropDataBase();

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

        public async Task InitCacheAsync(IServiceScope scope, CancellationToken ct = default)
        {
            await scope.ServiceProvider.GetRequiredService<IDXStructureCache>().WarmUpAsync(ct);

            //_structureRepo.UpdateCache();
            //(_coreRepo as IDXStructureRepository)?.UpdateCache();
        }
    }
}
