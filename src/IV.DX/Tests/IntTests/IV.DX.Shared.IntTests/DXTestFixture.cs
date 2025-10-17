using IV.DX.Application.Handlers;
using IV.DX.Hosting;
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
            .AddEnvironmentVariables()
            .Build();

            if (configuration["Database:Type"] == null)
            {
                configuration["Database:Type"] = "PostgreSQL";
            }

            if (configuration["Database:ConnectionString"] == null)
            {
                configuration["Database:ConnectionString"] = $"Server=localhost;Database={Database};User ID=postgres;password=root;";
            }
            else
            {
                configuration["Database:ConnectionString"] = $"{ReplaceDatabase(configuration["Database:ConnectionString"], Database)}";
            }

            var services = new ServiceCollection();

            //services.AddLogging();
            services.AddDXCore(configuration);
            services.AddDXPipeline();
            //services.AddDXHandlers(typeof(DXElementDefinitionUnitHandler).Assembly);
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
