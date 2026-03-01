using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Hosting;
using IV.DX.Kernel.Attributes;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace IV.DX.Application.IntTests.Services
{
    public class DXSecurityInitializationModeTests
    {
        [Fact]
        public async Task CoreOnlyMode_AccessChecker_AllowsWithoutSecurityInitialization()
        {
            var databaseName = $"IV.DX.Application.IntTests.CoreOnly.{Guid.NewGuid():N}";
            using var root = BuildIsolatedRoot(databaseName);
            using var scope = root.CreateScope();

            var coreRepo = scope.ServiceProvider.GetRequiredService<IDXUnitCoreRepository>();
            var initializer = scope.ServiceProvider.GetRequiredService<IDXInitializer>();
            var accessChecker = scope.ServiceProvider.GetRequiredService<IDXUnitTypeAccessChecker>();

            coreRepo.DropDataBase();
            await initializer.InitDXCoreDataAsync();

            var decision = accessChecker.CheckAccess("DXElementDefinitionUnit", DXUnitTypeAccessOperation.Read);
            Assert.Equal(DXAccessDecision.Allowed, decision);
        }

        [Fact]
        public async Task SecurityEnabledMode_AfterStartupLoad_DeniesAnonymous()
        {
            var databaseName = $"IV.DX.Application.IntTests.SecurityEnabled.{Guid.NewGuid():N}";

            using (var seedRoot = BuildIsolatedRoot(databaseName))
            using (var seedScope = seedRoot.CreateScope())
            {
                var coreRepo = seedScope.ServiceProvider.GetRequiredService<IDXUnitCoreRepository>();
                var initializer = seedScope.ServiceProvider.GetRequiredService<IDXInitializer>();

                coreRepo.DropDataBase();
                await initializer.InitDXCoreDataAsync();
                await initializer.InitDXSecurityDataAsync();
            }

            using var appRoot = BuildIsolatedRoot(databaseName);
            using var appScope = appRoot.CreateScope();

            var appInitializer = appScope.ServiceProvider.GetRequiredService<IDXInitializer>();
            var appAccessChecker = appScope.ServiceProvider.GetRequiredService<IDXUnitTypeAccessChecker>();

            await appInitializer.InitDXCoreDataAsync();

            var decision = appAccessChecker.CheckAccess("DXElementDefinitionUnit", DXUnitTypeAccessOperation.Read);
            Assert.Equal(DXAccessDecision.Denied, decision);
        }

        [Fact]
        public async Task MigrationSystemBypassMode_WithSecurityEnabled_MigrationRunsWithoutExecutionContext()
        {
            var databaseName = $"IV.DX.Application.IntTests.MigrationBypass.{Guid.NewGuid():N}";
            using var root = BuildIsolatedRoot(databaseName);
            using var scope = root.CreateScope();

            var coreRepo = scope.ServiceProvider.GetRequiredService<IDXUnitCoreRepository>();
            var initializer = scope.ServiceProvider.GetRequiredService<IDXInitializer>();
            var migrationService = scope.ServiceProvider.GetRequiredService<IDXMigrationService>();
            var accessChecker = scope.ServiceProvider.GetRequiredService<IDXUnitTypeAccessChecker>();

            coreRepo.DropDataBase();
            await initializer.InitDXCoreDataAsync();
            await initializer.InitDXSecurityDataAsync();

            var securityDecision = accessChecker.CheckAccess("DXElementDefinitionUnit", DXUnitTypeAccessOperation.Read);
            Assert.Equal(DXAccessDecision.Denied, securityDecision);

            var scriptsAssembly = Assembly.GetAssembly(typeof(DXUnitAttribute));
            await migrationService.MigrateCustomEmbeddedAsync(scriptsAssembly!, "Data/DXQuery.json");
        }

        private static ServiceProvider BuildIsolatedRoot(string databaseName)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Database:Type", "PostgreSQL"},
                    { "Database:ConnectionString", $"Server=localhost;Database={databaseName};User ID=postgres;password=root;" },
                    { "Security:JwtSigningKey", "int-tests-signing-key-change-me-32-bytes" }
                })
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();
            services.AddDXCore(configuration);
            services.AddDXPipeline();
            services.AddDXInitializer();

            var root = services.BuildServiceProvider();
            root.InitializeDXHandlers();

            return root;
        }
    }
}

