using IV.DataProvider.Persistence.Services;
using IV.DX.Contracts.Application;
using IV.DX.Contracts.Common.Helpers;
using IV.DX.Contracts.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xunit.Abstractions;

namespace IV.DataProvider.Persistence.Shared.IntTests
{
    public abstract class IntTestController : IDisposable
    {
        protected Action _finalizationAction;

        protected ICoreRepository _coreRepo;
        protected IGenericRepository _genericRepo;
        protected ISQLQueryHelper _sqlQueryHelper;
        protected IDataService _dataService;
        protected IDataStructureRepository _dataStructureRepo;
        protected IServiceProvider ServiceProvider;
        protected IMigrationService _migrationService;

        protected ITestOutputHelper Output;

        static IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Database:Type", "PostgreSQL"},
                    {"Database:ConnectionString", "Server=localhost;Database=IV.DataProvider.TestDB;User ID=postgres;password=root;"},
					//{"Database:Type", "MySQL"},
					//{"Database:ConnectionString", "Server=159.89.98.54;Database=IV.DataProvider.TestDB2;Uid=digilit_user;Pwd=Digilit2019!;"},
				})
                .Build();

        static IntTestController()
        {
            IServiceCollection services = new ServiceCollection();

            var coreDI = new DependencyRegistrator(configuration, services);
            services.AddSingleton(configuration);

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            //coreDI.InitCache(serviceProvider);
            coreDI.InitEntityHandlerProvider(serviceProvider);

            coreDI.DropDatabase(serviceProvider);
            coreDI.InitCoreData(serviceProvider);
            coreDI.InitCustomData(serviceProvider, "MigrationScripts/Test.json");
        }

        public IntTestController(ITestOutputHelper output)
        {
            this.Output = output;

            // Init
            IServiceCollection services = new ServiceCollection();

            var coreDI = new DependencyRegistrator(configuration, services);
            services.AddSingleton(configuration);

            this.ServiceProvider = services.BuildServiceProvider();

            coreDI.InitCache(this.ServiceProvider);
            coreDI.InitEntityHandlerProvider(this.ServiceProvider);

            // Resolve types
            this._dataStructureRepo = this.ServiceProvider.GetService<IDataStructureRepository>();
            this._sqlQueryHelper = this.ServiceProvider.GetService<ISQLQueryHelper>();
            this._coreRepo = this.ServiceProvider.GetService<ICoreRepository>();
            this._genericRepo = this.ServiceProvider.GetService<IGenericRepository>();
            this._dataService = this.ServiceProvider.GetService<IDataService>();
            this._migrationService = this.ServiceProvider.GetService<IMigrationService>();
        }

        public void Dispose()
        {
            if (this._finalizationAction != null)
            {
                this._finalizationAction.Invoke();
            }
        }

        protected void RunActionSafety(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception)
            {

            }
        }

        protected void EstimatePerformance(Action action, string message)
        {
            if (action == null)
                return;

            Stopwatch sw = new Stopwatch();

            sw.Start();

            action.Invoke();

            sw.Stop();

            string result = $"{message} : {sw.ElapsedMilliseconds} ms; {sw.ElapsedTicks} ticks;\n";

            Output.WriteLine(result);

            File.AppendAllText("Performance.txt", result);
        }
    }
}