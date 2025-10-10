using IV.DX.Application.Handlers;
using IV.DX.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace IV.DX.Shared.IntTests
{
   

    //public sealed class DXTestFixtureInit : IAsyncLifetime, IDisposable
    //{
    //    public ServiceProvider Root { get; private set; } = default!;

    //    static IConfiguration configuration = new ConfigurationBuilder()
    //        .AddInMemoryCollection(new Dictionary<string, string>
    //        {
    //                {"Database:Type", "PostgreSQL"},
    //                {"Database:ConnectionString", "Server=localhost;Database=IV.DX.TestDB;User ID=postgres;password=root;"},
    //            //{"Database:Type", "MySQL"},
    //            //{"Database:ConnectionString", "Server=159.89.98.54;Database=IV.DX.TestDB2;Uid=digilit_user;Pwd=Digilit2019!;"},
    //        })
    //        .Build();

    //    public async Task InitializeAsync()
    //    {
    //        var services = new ServiceCollection();

    //        services.AddLogging();
    //        services.AddDXCore(configuration);
    //        services.AddDXPipeline();
    //        services.AddDXHandlers(typeof(DXElementDefinitionUnitHandler).Assembly);
    //        services.AddDXInitializer();

    //        Root = services.BuildServiceProvider(new ServiceProviderOptions
    //        {
    //            ValidateOnBuild = true,
    //            ValidateScopes = true
    //        });

    //        Root.InitializeDXHandlers();

    //        // ❗ Создаём/греем кэш из ROOT (не из scope)
    //        await Root.GetRequiredService<IDXStructureCache>().WarmUpAsync();

    //        using var scope = Root.CreateScope();
    //        var init = scope.ServiceProvider.GetRequiredService<IDXInitializer>();

    //        init.DropDatabase();
    //        init.InitCoreData();
    //        init.InitCustomData("MigrationScripts/Test.json");

    //        // После миграций переснять снапшот (если нужно)
    //        await Root.GetRequiredService<IDXStructureCache>().RefreshAsync();
    //    }

    //    public Task DisposeAsync() => Task.CompletedTask;
    //    public void Dispose() => Root.Dispose();
    //}

    public sealed class DXTestFixture : IAsyncLifetime, IDisposable
    {
        public ServiceProvider Root { get; private set; } = default!;

        static IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                    {"Database:Type", "PostgreSQL"},
                    {"Database:ConnectionString", "Server=localhost;Database=IV.DX.TestDB;User ID=postgres;password=root;"},
                //{"Database:Type", "MySQL"},
                //{"Database:ConnectionString", "Server=159.89.98.54;Database=IV.DX.TestDB2;Uid=digilit_user;Pwd=Digilit2019!;"},
            })
            .Build();

        public DXTestFixture()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddDXCore(configuration);
            services.AddDXPipeline();
            services.AddDXHandlers(typeof(DXElementDefinitionUnitHandler).Assembly);
            services.AddDXInitializer();

            Root = services.BuildServiceProvider();

            Root.InitializeDXHandlers();

            using var scope = Root.CreateScope();
            var init = scope.ServiceProvider.GetRequiredService<IDXInitializer>();

            init.DropDatabase();
            init.InitCoreData();
            init.InitCustomData("MigrationScripts/Test.json");
            init.InitCacheAsync(scope).Wait();
            //await Root.GetRequiredService<IDXStructureCache>().RefreshAsync();
        }

        public async Task InitializeAsync()
        {
           
        }

        public Task DisposeAsync() => Task.CompletedTask;
        public void Dispose() => Root.Dispose();
    }
}
