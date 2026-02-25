using IV.DX.Hosting;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
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
                    { "Database:ConnectionString", $"Server=localhost;Database={Database};User ID=postgres;password=root;" },
                    { "Security:JwtSigningKey", "int-tests-signing-key-change-me-32-bytes" }
                })
                .AddEnvironmentVariables()
                .Build();

            configuration["Database:ConnectionString"] = $"{ReplaceDatabase(configuration["Database:ConnectionString"], Database)}";
            var connectionString = configuration["Database:ConnectionString"];
            var resolvedDatabase = GetDatabaseName(connectionString);

            Console.WriteLine($"[DX IntTests] Initializing fixture for DB '{resolvedDatabase}'.");

            var services = new ServiceCollection();

            services.AddDXCore(configuration);
            services.AddDXPipeline();
            services.AddDXInitializer();

            Root = services.BuildServiceProvider();

            Root.InitializeDXHandlers();

            using var scope = Root.CreateScope();
            var init = scope.ServiceProvider.GetRequiredService<IDXInitializer>();

            var coreRepo = scope.ServiceProvider.GetRequiredService<IDXUnitCoreRepository>();
            using var migrationMutex = new Mutex(false, "IV.DX.IntTests.DbInit");
            var isLocked = false;

            try
            {
                isLocked = migrationMutex.WaitOne(TimeSpan.FromMinutes(5));
                if (!isLocked)
                {
                    throw new TimeoutException("Timeout while waiting for IV.DX integration test database initialization mutex.");
                }

                coreRepo.DropDataBase();
                init.InitDXCoreDataAsync().Wait();
                init.InitDXQueryDataAsync().Wait();
                init.InitDXSecurityDataAsync().Wait();

                init.InitCustomDataAsync("MigrationScripts/Test.json").Wait();
            }
            finally
            {
                if (isLocked)
                {
                    migrationMutex.ReleaseMutex();
                }
            }
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

        private static string GetDatabaseName(string connectionString)
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };

            var possibleKeys = new[] { "Database", "Initial Catalog" };
            foreach (var key in possibleKeys)
            {
                if (builder.ContainsKey(key) && builder[key] != null)
                {
                    return builder[key].ToString();
                }
            }

            return "<unknown>";
        }

        public async Task InitializeAsync()
        {

        }

        public Task DisposeAsync() => Task.CompletedTask;
        public void Dispose() => Root.Dispose();
    }
}
