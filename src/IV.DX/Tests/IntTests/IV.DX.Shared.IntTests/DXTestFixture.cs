using IV.DX.Hosting;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Sockets;
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

                const int maxAttempts = 3;
                for (var attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    try
                    {
                        coreRepo.DropDataBase();
                        init.InitDXCoreDataAsync().Wait();
                        init.InitDXQueryDataAsync().Wait();
                        init.InitDXSecurityDataAsync().Wait();
                        init.InitCustomDataAsync("MigrationScripts/Test.json").Wait();
                        break;
                    }
                    catch (Exception ex) when (attempt < maxAttempts && IsTransientTransportFailure(ex))
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(attempt * 2));
                    }
                }
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

        private static bool IsTransientTransportFailure(Exception exception)
        {
            foreach (var ex in FlattenExceptions(exception))
            {
                if (ex is SocketException)
                {
                    return true;
                }

                if (ex.Message.IndexOf("Exception while writing to stream", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                if (ex.Message.IndexOf("forcibly closed by the remote host", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<Exception> FlattenExceptions(Exception exception)
        {
            if (exception is AggregateException aggregate)
            {
                foreach (var inner in aggregate.Flatten().InnerExceptions)
                {
                    foreach (var flattened in FlattenExceptions(inner))
                    {
                        yield return flattened;
                    }
                }

                yield break;
            }

            yield return exception;

            if (exception.InnerException != null)
            {
                foreach (var inner in FlattenExceptions(exception.InnerException))
                {
                    yield return inner;
                }
            }
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => Task.CompletedTask;
        public void Dispose() => Root.Dispose();
    }
}
