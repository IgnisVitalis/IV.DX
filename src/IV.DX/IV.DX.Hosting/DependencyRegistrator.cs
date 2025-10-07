using IV.DX.Application;
using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.DataHandlers;
using IV.DX.Persistence;
using IV.DX.Persistence.Abstractions;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Persistence.SQLQueryHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DataProvider.Persistence.Services
{
    public class DependencyRegistrator
    {
        IConfiguration _configuration;
        IServiceCollection _container;

        public DependencyRegistrator(IConfiguration configuration, IServiceCollection container)
        {
            this._configuration = configuration;
            this._container = container;

            this.Register();
        }

        private void Register()
        {
            this.RegisterCore();
        }

        private void RegisterCore()
        {
            if (this._configuration["Database:Type"].Equals("MySQL", StringComparison.OrdinalIgnoreCase))
            {
                this._container.AddSingleton<MySQLQueryHelper, MySQLQueryHelper>();
            }
            else if (this._configuration["Database:Type"].Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                this._container.AddSingleton<ISQLQueryHelper, PGSQLQueryHelper>();
            }

            //container.AddSingleton<CoreRepository>(new InjectionConstructor(new object[] { config.Database.ConnectionString, serviceProvider.GetService<ISQLQueryHelper>() }));

            this._container.AddSingleton<IDXEnumCoreRepository, CoreRepository>();
            this._container.AddSingleton<IDXStructureRepository, CoreRepository>();
            this._container.AddSingleton<IDXCoreRepository, CoreRepository>();

            this._container.AddSingleton<IDXGenericRepository, GenericRepository>();
            this._container.AddSingleton<IDataService, DataService>();
            this._container.AddSingleton<ICoreModelHandler, CoreModelHandler>();
            this._container.AddSingleton<IMigrationService, MigrationService>();
        }

        public void InitCache(IServiceProvider serviceProvider)
        {
            var dataStructureRepo = serviceProvider.GetService<IDXStructureRepository>();
            var coreRepo = serviceProvider.GetService<IDXCoreRepository>();

            dataStructureRepo.UpdateCache();
            (coreRepo as IDXStructureRepository).UpdateCache();
        }

        public void InitEntityHandlerProvider(IServiceProvider serviceProvider)
        {
            EntityHandlerProvider.InitCore(serviceProvider);
            EntityHandlerProvider.Init();
        }

        public void DropDatabase(IServiceProvider serviceProvider)
        {
            var coreRepo = serviceProvider.GetService<IDXCoreRepository>();

            coreRepo.DropDataBase();
        }

        public void InitCoreData(IServiceProvider serviceProvider)
        {
            var dataStructureRepo = serviceProvider.GetService<IDXStructureRepository>();
            var coreRepo = serviceProvider.GetService<IDXCoreRepository>();
            var migrationService = serviceProvider.GetService<IMigrationService>();

            coreRepo.CreateDataBase();

            MaintenanceToken.StartMaintenanceCore();
            //dataStructureRepo.Init(coreRepo);

            migrationService.LoadCoreStructure();
            MaintenanceToken.StopMaintenanceCore();
        }

        public void InitCustomData(IServiceProvider serviceProvider, string configPath)
        {
            var migrationService = serviceProvider.GetService<IMigrationService>();

            migrationService.LoadStructure(configPath);
        }

        private class Config
        {
            public Database Database { get; set; }
        }

        private class Database
        {
            public string Type { get; set; }
            public string ConnectionString { get; set; }
        }
    }
}