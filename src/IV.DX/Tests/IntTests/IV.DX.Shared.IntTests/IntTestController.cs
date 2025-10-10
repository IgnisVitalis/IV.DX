using IV.DX.Application;
using IV.DX.Application.Handlers;
using IV.DX.Hosting;
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

        private readonly IServiceScope _scope;
        protected IServiceProvider ServiceProvider => _scope.ServiceProvider;

        protected ITestOutputHelper Output;

    //    static IConfiguration configuration = new ConfigurationBuilder()
    //            .AddInMemoryCollection(new Dictionary<string, string>
    //            {
    //                {"Database:Type", "PostgreSQL"},
    //                {"Database:ConnectionString", "Server=localhost;Database=IV.DX.TestDB;User ID=postgres;password=root;"},
				//	//{"Database:Type", "MySQL"},
				//	//{"Database:ConnectionString", "Server=159.89.98.54;Database=IV.DX.TestDB2;Uid=digilit_user;Pwd=Digilit2019!;"},
				//})
    //            .Build();

    //    static IntTestController()
    //    {
    //        var services = new ServiceCollection();

    //        services.AddLogging();
    //        services.AddDXCore(configuration);
    //        services.AddDXPipeline();
    //        services.AddDXHandlers(typeof(DXElementDefinitionUnitHandler).Assembly);
    //        services.AddDXInitializer();

    //        var sp = services.BuildServiceProvider();
    //        sp.InitializeDXHandlers();

    //        EntityHandlerProvider.InitCore(sp);
    //        EntityHandlerProvider.Init();

    //        using (var scope = sp.CreateScope())
    //        {
    //            var init = scope.ServiceProvider.GetRequiredService<IDXInitializer>();

    //            init.DropDatabase();
    //            init.InitCoreData();
    //            init.InitCustomData("MigrationScripts/Test.json");
    //            init.InitCacheAsync(scope).Wait();
    //        }            
    //    }

        public IntTestController(DXTestFixture fx, ITestOutputHelper output)
        {
            this.Output = output;
            _scope = fx.Root.CreateScope();

            _scope.ServiceProvider.GetRequiredService<IDXStructureCache>().WarmUpAsync().Wait();
        }

        public void Dispose()
        {
            if (this._finalizationAction != null)
            {
                this._finalizationAction.Invoke();
            }

            
            //_scope.Dispose();
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