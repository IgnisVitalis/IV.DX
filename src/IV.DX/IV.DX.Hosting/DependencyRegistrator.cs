using IV.DX.Application;
using IV.DX.Application.DataHandlers;
using IV.DX.Contracts;
using IV.DX.Contracts.Application;
using IV.DX.Contracts.Common.Helpers;
using IV.DX.Contracts.Persistence;
using IV.DX.Persistence;
using IV.DX.Persistence.SQLQueryHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DataProvider.Persistence.Services
{
    public class DependencyRegistrator : IDependencyRegistrator
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

            this._container.AddSingleton<IEnumCoreRepository, CoreRepository>();
            this._container.AddSingleton<IDataStructureRepository, CoreRepository>();
            this._container.AddSingleton<ICoreRepository, CoreRepository>();

            this._container.AddSingleton<IGenericRepository, GenericRepository>();
            this._container.AddSingleton<IDataService, DataService>();
            this._container.AddSingleton<ICoreModelHandler, CoreModelHandler>();
            this._container.AddSingleton<IMigrationService, MigrationService>();
        }

        public void InitCache(IServiceProvider serviceProvider)
        {
            var dataStructureRepo = serviceProvider.GetService<IDataStructureRepository>();
            var coreRepo = serviceProvider.GetService<ICoreRepository>();

            dataStructureRepo.UpdateCache();
            (coreRepo as IDataStructureRepository).UpdateCache();
        }

        public void InitEntityHandlerProvider(IServiceProvider serviceProvider)
        {
            EntityHandlerProvider.InitCore(serviceProvider);
            EntityHandlerProvider.Init();
        }

        public void DropDatabase(IServiceProvider serviceProvider)
        {
            var coreRepo = serviceProvider.GetService<ICoreRepository>();

            coreRepo.DropDataBase();
        }

        public void InitCoreData(IServiceProvider serviceProvider)
        {
            var dataStructureRepo = serviceProvider.GetService<IDataStructureRepository>();
            var coreRepo = serviceProvider.GetService<ICoreRepository>();
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