using IV.DX.Hosting;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Xunit;

namespace IV.DX.Shared.IntTests
{
    public abstract class DXTestFixtureBase : IAsyncLifetime, IDisposable
    {
        public ServiceProvider Root { get; private set; } = default!;

        protected abstract string Database { get; }

        public DXTestFixtureBase()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Database:Type", "PostgreSQL"},
                    { "Database:ConnectionString", $"Server=localhost;Database={Database};User ID=postgres;password=root;" }
                })
                .AddEnvironmentVariables()
                .Build();

            configuration["Database:ConnectionString"] = $"{ReplaceDatabase(configuration["Database:ConnectionString"], Database)}";

            var services = new ServiceCollection();

            services.AddDXCore(configuration);
            services.AddDXPipeline();
            services.AddDXInitializer();

            Root = services.BuildServiceProvider();

            Root.InitializeDXHandlers();

            using var scope = Root.CreateScope();
            var init = scope.ServiceProvider.GetRequiredService<IDXInitializer>();

            var coreRepo = scope.ServiceProvider.GetRequiredService<IDXUnitCoreRepository>();

            coreRepo.DropDataBase();
            init.InitDXCoreDataAsync().Wait();
            init.InitDXQueryDataAsync().Wait();
            init.InitDXSecurityDataAsync().Wait();

            init.InitCustomDataAsync("MigrationScripts/Test.json").Wait();
        }

        public static string ReplaceDatabase(string connectionString, string newDatabase)
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };

            var possibleKeys = new[] { "Database", "Initial Catalog" };

            foreach (var key in possibleKeys)
            {
                if (builder.ContainsKey(key))
                {
                    builder[key] = newDatabase;
                    return builder.ConnectionString;
                }
            }

            builder["Database"] = newDatabase;
            return builder.ConnectionString;
        }

        public async Task InitializeAsync()
        {

        }

        public Task DisposeAsync() => Task.CompletedTask;
        public void Dispose() => Root.Dispose();
    }
}
