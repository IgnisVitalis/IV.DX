using IV.DX.Hosting;
using IV.DX.Kernel.Models;
using IV.DX.Shared.IntTests.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Xunit;

namespace IV.DX.Shared.IntTests
{
    public abstract class DXTestFixtureBase : IAsyncLifetime, IDisposable
    {
        public ServiceProvider Root { get; private set; } = default!;
        public string ConnectionString { get; private set; } = default!;

        private readonly PostgreSqlContainer _pgContainer;

        protected abstract string Database { get; }

        public DXTestFixtureBase()
        {
            _pgContainer = new PostgreSqlBuilder("postgres:17-alpine")
                .WithDatabase(Database)
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();
        }

        public static string ReplaceDatabase(string connectionString, string newDatabase)
        {
            var builder = new System.Data.Common.DbConnectionStringBuilder { ConnectionString = connectionString };

            foreach (var key in new[] { "Database", "Initial Catalog" })
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
            await _pgContainer.StartAsync();

            ConnectionString = _pgContainer.GetConnectionString();

            Console.WriteLine($"[DX IntTests] Initializing fixture for DB '{Database}'.");

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Secrets:DatabaseConnectionString", _pgContainer.GetConnectionString() },
                    { "Secrets:DatabaseType", "PostgreSQL" },
                    { "Secrets:JwtSigningKey", "int-tests-signing-key-change-me-32-bytes" },
                    { "Secrets:EncryptionKey", "dGVzdC1lbmNyeXB0aW9uLWtleS0zMi1ieXRlcy0hISE=" }
                })
                .Build();

            var services = new ServiceCollection();

            services.AddDX(configuration)
                    .AddSecurity()
                    .AddCustomData("MigrationScripts/Test.json")
                    .Build();

            services.AddSingleton<IDXTestSchemaHelper>(sp =>
            {
                var secrets = sp.GetRequiredService<IOptions<DXSecretsOptions>>().Value;
                return new PGSQLTestSchemaHelper(secrets.DatabaseConnectionString);
            });

            ConfigureAdditionalServices(services);

            Root = services.BuildServiceProvider();

            await Root.DropDXDatabaseAsync();
            await Root.StartDXAsync();
        }

        public async Task DisposeAsync()
        {
            await _pgContainer.DisposeAsync();
        }

        protected virtual void ConfigureAdditionalServices(IServiceCollection services) { }

        public void Dispose() => Root?.Dispose();
    }
}
