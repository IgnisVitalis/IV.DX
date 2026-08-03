using IV.DX.PostgreSQL;
using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Hosting;
using IV.DX.Kernel.Attributes;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace IV.DX.Application.IntTests.Services
{
    [Collection("DX:one-time")]
    public class DXSecurityInitializationModeTests
    {
        private readonly DXTestFixture _fx;

        public DXSecurityInitializationModeTests(DXTestFixture fx)
        {
            _fx = fx;
        }

        [Fact]
        public async Task CoreOnlyMode_AccessChecker_AllowsWithoutSecurityInitialization()
        {
            const string databaseName = "IV.DX.Application.IntTests.CoreOnly";
            using var root = BuildIsolatedRoot(databaseName, withSecurity: false);
            using var scope = root.CreateScope();

            var coreRepo = scope.ServiceProvider.GetRequiredService<IDXUnitCoreRepository>();
            var accessChecker = scope.ServiceProvider.GetRequiredService<IDXUnitTypeAccessChecker>();

            coreRepo.DropDataBase();
            try
            {
                await root.StartDXAsync();

                var decision = accessChecker.CheckAccess("DXElementDefinitionUnit", DXUnitTypeAccessOperation.Read);
                Assert.Equal(DXAccessDecision.Allowed, decision);
            }
            finally
            {
                coreRepo.DropDataBase();
            }
        }

        [Fact]
        public async Task SecurityEnabledMode_DeniesAnonymous()
        {
            const string databaseName = "IV.DX.Application.IntTests.SecurityEnabled";
            using var root = BuildIsolatedRoot(databaseName, withSecurity: true);
            using var scope = root.CreateScope();

            var coreRepo = scope.ServiceProvider.GetRequiredService<IDXUnitCoreRepository>();
            var accessChecker = scope.ServiceProvider.GetRequiredService<IDXUnitTypeAccessChecker>();

            coreRepo.DropDataBase();
            try
            {
                await root.StartDXAsync();

                var decision = accessChecker.CheckAccess("DXElementDefinitionUnit", DXUnitTypeAccessOperation.Read);
                Assert.Equal(DXAccessDecision.Denied, decision);
            }
            finally
            {
                coreRepo.DropDataBase();
            }
        }

        [Fact]
        public async Task MigrationSystemBypassMode_WithSecurityEnabled_MigrationRunsWithoutExecutionContext()
        {
            const string databaseName = "IV.DX.Application.IntTests.MigrationBypass";
            using var root = BuildIsolatedRoot(databaseName, withSecurity: true);
            using var scope = root.CreateScope();

            var coreRepo = scope.ServiceProvider.GetRequiredService<IDXUnitCoreRepository>();
            var migrationService = scope.ServiceProvider.GetRequiredService<IDXMigrationService>();
            var accessChecker = scope.ServiceProvider.GetRequiredService<IDXUnitTypeAccessChecker>();

            coreRepo.DropDataBase();
            try
            {
                await root.StartDXAsync();

                var securityDecision = accessChecker.CheckAccess("DXElementDefinitionUnit", DXUnitTypeAccessOperation.Read);
                Assert.Equal(DXAccessDecision.Denied, securityDecision);

                var scriptsAssembly = Assembly.GetAssembly(typeof(DXUnitAttribute));
                await migrationService.MigrateCustomEmbeddedAsync(scriptsAssembly, "Migration/DXQuery.json");
            }
            finally
            {
                coreRepo.DropDataBase();
            }
        }

        private ServiceProvider BuildIsolatedRoot(string databaseName, bool withSecurity)
        {
            var connectionString = DXTestFixtureBase.ReplaceDatabase(_fx.ConnectionString, databaseName);

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Secrets:DatabaseConnectionString", connectionString },
                    { "Secrets:JwtSigningKey", "int-tests-signing-key-change-me-32-bytes" },
                    { "Secrets:EncryptionKey", "dGVzdC1lbmNyeXB0aW9uLWtleS0zMi1ieXRlcy0hISE=" }
                })
                .Build();

            var services = new ServiceCollection();
            var builder = services.AddDX(configuration).UsePostgreSQL();

            if (withSecurity)
                builder.AddSecurity();

            builder.Build();

            return services.BuildServiceProvider();
        }
    }
}
