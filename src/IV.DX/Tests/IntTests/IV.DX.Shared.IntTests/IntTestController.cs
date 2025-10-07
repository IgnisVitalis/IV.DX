using IV.DataProvider.Persistence.Services;
using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Persistence.Abstractions;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xunit.Abstractions;

namespace IV.DX.Shared.IntTests
{
    public abstract class IntTestController : IDisposable
    {
        protected Action _finalizationAction;
     
        protected IDataService _dataService;        
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