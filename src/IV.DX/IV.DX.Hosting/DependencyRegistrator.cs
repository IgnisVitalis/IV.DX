using IV.DX.Application;
using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.DataHandlers;
using IV.DX.Application.Handlers;
using IV.DX.Application.Pipeline;
using IV.DX.Kernel.Models;
using IV.DX.Persistence;
using IV.DX.Persistence.Abstractions;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Persistence.SQLQueryHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace IV.DX.Hosting
{
    //public class DependencyRegistrator
    //{
    //    IConfiguration _configuration;
    //    IServiceCollection _container;

    //    public DependencyRegistrator(IConfiguration configuration, IServiceCollection container)
    //    {
    //        this._configuration = configuration;
    //        this._container = container;

    //        this.Register();
    //    }

    //    private void Register()
    //    {
    //        this.RegisterCore();
    //    }

    //    private void RegisterCore()
    //    {
    //        if (this._configuration["Database:Type"].Equals("MySQL", StringComparison.OrdinalIgnoreCase))
    //        {
    //            this._container.AddSingleton<MySQLQueryDXHelper, MySQLQueryDXHelper>();
    //        }
    //        else if (this._configuration["Database:Type"].Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
    //        {
    //            this._container.AddSingleton<ISQLQueryDXHelper, PGSQLQueryDXHelper>();
    //        }

    //        this._container.AddSingleton<IDXEnumCoreRepository, DXCoreRepository>();
    //        this._container.AddSingleton<IDXStructureRepository, DXCoreRepository>();
    //        this._container.AddSingleton<IDXCoreRepository, DXCoreRepository>();

    //        this._container.AddSingleton<IDXGenericRepository, DXGenericRepository>();
    //        this._container.AddSingleton<IDXUnitDataService, DXUnitDataService>();
    //        this._container.AddSingleton<IDXCoreHandler, CoreModelHandler>();
    //        this._container.AddSingleton<IDXMigrationService, MigrationService>();

    //        this._container.AddSingleton<IDXUnitGetHandlerProvider, DXUnitGetHandlerProvider>();
    //        this._container.AddSingleton<IDXUnitInsertHandlerProvider, DXUnitInsertHandlerProvider>();
    //        this._container.AddSingleton<IDXUnitUpdateHandlerProvider, DXUnitUpdateHandlerProvider>();
    //        this._container.AddSingleton<IDXUnitDeleteHandlerProvider, DXUnitDeleteHandlerProvider>();
    //        this._container.AddSingleton<IDXPipelineExecutor, DXPipelineExecutor>();
    //    }

    //    private void InitCoreHandlers(IServiceProvider serviceProvider)
    //    {
    //        var insertHandlerProvider = serviceProvider.GetService<IDXUnitInsertHandlerProvider>();

    //        insertHandlerProvider.Register<DXElementDefinitionUnit>(new DXElementDefinitionUnitHandler());
    //    }

    //    public void InitCache(IServiceProvider serviceProvider)
    //    {
    //        var dataStructureRepo = serviceProvider.GetService<IDXStructureRepository>();
    //        var coreRepo = serviceProvider.GetService<IDXCoreRepository>();

    //        dataStructureRepo.UpdateCache();
    //        (coreRepo as IDXStructureRepository).UpdateCache();
    //    }

    //    public void InitEntityHandlerProvider(IServiceProvider serviceProvider)
    //    {
    //        EntityHandlerProvider.InitCore(serviceProvider);
    //        EntityHandlerProvider.Init();
    //    }

    //    public void DropDatabase(IServiceProvider serviceProvider)
    //    {
    //        var coreRepo = serviceProvider.GetService<IDXCoreRepository>();

    //        coreRepo.DropDataBase();
    //    }

    //    public void InitCoreData(IServiceProvider serviceProvider)
    //    {
    //        var dataStructureRepo = serviceProvider.GetService<IDXStructureRepository>();
    //        var coreRepo = serviceProvider.GetService<IDXCoreRepository>();
    //        var migrationService = serviceProvider.GetService<IDXMigrationService>();

    //        coreRepo.CreateDataBase();

    //        DXMaintenanceToken.StartMaintenanceCore();
           

    //        migrationService.LoadCoreStructure();
    //        DXMaintenanceToken.StopMaintenanceCore();
    //    }

    //    public void InitCustomData(IServiceProvider serviceProvider, string configPath)
    //    {
    //        var migrationService = serviceProvider.GetService<IDXMigrationService>();

    //        migrationService.LoadStructure(configPath);
    //    }

    //    private class Config
    //    {
    //        public Database Database { get; set; }
    //    }

    //    private class Database
    //    {
    //        public string Type { get; set; }
    //        public string ConnectionString { get; set; }
    //    }
    //}
}