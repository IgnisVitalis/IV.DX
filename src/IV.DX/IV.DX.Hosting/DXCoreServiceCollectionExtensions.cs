using IV.DX.Application;
using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Models;
using IV.DX.Application.Handlers;
using IV.DX.Application.Services;
using IV.DX.Kernel.Helpers;
using IV.DX.Persistence;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace IV.DX.Hosting
{
    public static class DXCoreServiceCollectionExtensions
    {
        public static DXBuilder AddDX(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDXCore(configuration);
            services.AddDXPipeline();
            return new DXBuilder(services);
        }

        public static IServiceCollection AddDXCustomData(
            this IServiceCollection services,
            string configPath)
        {
            services.Configure<DXStartupOptions>(o => o.CustomDataPaths.Add(configPath));
            return services;
        }

        internal static IServiceCollection AddDXCore(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.TryAddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.TryAddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

            services.AddOptions<DXSecretsOptions>()
                .Bind(configuration.GetSection(DXSecretsOptions.SectionName));

            services.AddOptions<DXDatabaseOptions>()
                .Bind(configuration.GetSection("Database"))
                .PostConfigure<IOptions<DXSecretsOptions>>((o, secrets) =>
                {
                    if (!string.IsNullOrWhiteSpace(secrets.Value.DatabaseConnectionString))
                        o.ConnectionString = secrets.Value.DatabaseConnectionString;
                });

            services.AddOptions<DXEncryptionOptions>()
                .PostConfigure<IOptions<DXSecretsOptions>>((o, secrets) =>
                {
                    if (!string.IsNullOrWhiteSpace(secrets.Value.EncryptionKey))
                        o.Key = secrets.Value.EncryptionKey;
                });

            services.AddOptions<DXSecurityOptions>()
                .Bind(configuration.GetSection("Security"))
                .PostConfigure<IOptions<DXSecretsOptions>>((o, secrets) =>
                {
                    if (!string.IsNullOrWhiteSpace(secrets.Value.JwtSigningKey))
                        o.JwtSigningKey = secrets.Value.JwtSigningKey;
                });

            services.AddSingleton<IDXEncryptionKeyProvider, DXConfiguredEncryptionKeyProvider>();
            services.AddSingleton<IDXStringProtector, DXAesGcmStringProtector>();
            services.AddSingleton<IDXExecutionContextAccessor, DXExecutionContextAccessor>();
            services.AddSingleton<IDXModuleRegistry, DXModuleRegistry>();
            services.AddSingleton<IDXSecurityState, DXSecurityState>();
            services.AddScoped<IDXExecutionContextResolver, DXExecutionContextResolver>();
            services.AddScoped<IDXUnitTypeAccessChecker, DXContextualUnitTypeAccessChecker>();

            services.AddSingleton<IDXStructureCache, DXStructureCache>();

            var func = (IServiceProvider sp) =>
            {
                var cache = sp.GetRequiredService<IDXStructureCache>();
                var schemaHelper = sp.GetRequiredService<ISQLSchemaHelper>();
                var dbProvider = sp.GetRequiredService<ISQLDbProvider>();
                var sqlQueryBuilder = sp.GetRequiredService<ISQLQueryBuilder>();
                var protector = sp.GetRequiredService<IDXStringProtector>();
                var accessChecker = sp.GetRequiredService<IDXUnitTypeAccessChecker>();

                var o = sp.GetRequiredService<IOptions<DXDatabaseOptions>>().Value;

                return new DXCoreRepository(new DXDatabaseOptions() { ConnectionString = o.ConnectionString }, cache, schemaHelper, dbProvider, sqlQueryBuilder, protector, accessChecker);
            };

            services.AddScoped<IDXRawReader, DXCoreRepository>(func);
            services.AddScoped<IDXUnitCoreRepository, DXCoreRepository>(func);
            services.AddScoped<IDXElementCoreRepository, DXCoreRepository>(func);
            services.AddScoped<IDXStructureRawReader, DXCoreRepository>(func);
            services.AddScoped<IDXStructureRepository, DXCoreRepository>(func);
            services.AddScoped<IDXEnumCoreRepository, DXCoreRepository>(func);
            services.AddScoped<IDXUnitGenericRepository, DXUnitGenericRepository>();
            services.AddScoped<IDXElementGenericRepository, DXElementGenericRepository>();
            services.AddScoped<ISQLQueryBuilder, SQLQueryBuilder>();

            // Every data service decides access through this one component, so the unit path and the
            // element path can never answer to different rules.
            services.AddScoped<IDXUnitAccessGate, DXUnitAccessGate>();

            services.AddScoped<IDXUnitDataService, DXUnitDataService>();
            services.AddScoped<IDXUnitDataReader, DXUnitDataReader>();
            services.AddScoped<IDXEncryptionMigrationService, DXEncryptionMigrationService>();
            services.AddScoped<IDXEnumDataService, DXEnumDataService>();
            services.AddScoped<IDXElementDataService, DXElementDataService>();
            services.AddSingleton<IDXMigrationDistributedLock, DXMigrationDistributedLock>();
            services.AddScoped<IDXSecurityService, DXSecurityService>();
            services.AddScoped<IDXMigrationService, MigrationService>();
            services.AddScoped<IDXStructureService, DXStructureService>();

            services.AddScoped<IDXQueryResultProvider, DXQueryResultProvider>();
            
            services.RegisterCoreHandlers();

            return services;
        }

        private static void RegisterCoreHandlers(this IServiceCollection services)
        {
            services.AddDXHandlers(typeof(DXElementDefinitionUnitHandler).Assembly);
        }
    }
}
