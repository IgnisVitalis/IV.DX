using IV.DX.Application;
using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Models;
using IV.DX.Application.Handlers;
using IV.DX.Application.Services;
using IV.DX.Persistence;
using IV.DX.Persistence.Abstractions;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Persistence.SQLQueryHelpers;
using IV.DX.Kernel.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IV.DX.Hosting
{
    public static class DXCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddDXCore(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<DXDatabaseOptions>()
                .Bind(configuration.GetSection("Database"));

            services.AddOptions<DXEncryptionOptions>()
                .Bind(configuration.GetSection("Encryption"));
            services.AddOptions<DXSecurityOptions>()
                .Bind(configuration.GetSection("Security"));

            services.AddSingleton<IDXEncryptionKeyProvider, DXConfiguredEncryptionKeyProvider>();
            services.AddSingleton<IDXStringProtector, DXAesGcmStringProtector>();
            services.AddSingleton<IDXExecutionContextAccessor, DXExecutionContextAccessor>();
            services.AddScoped<IDXExecutionContextResolver, DXExecutionContextResolver>();
            services.AddScoped<IDXUnitTypeAccessChecker, DXContextualUnitTypeAccessChecker>();

            services.AddSingleton<ISQLDialect>(sp => (ISQLDialect)GetHelper(sp));
            services.AddSingleton<ISQLSchemaHelper>(sp => (ISQLSchemaHelper)GetHelper(sp));
            services.AddSingleton<ISQLDbProvider>(sp => (ISQLDbProvider)GetHelper(sp));
            services.AddSingleton<ISQLMigrationLockHelper>(sp => (ISQLMigrationLockHelper)GetHelper(sp));

            services.AddSingleton<MySQLQueryDXHelper>();
            services.AddSingleton<PGSQLQueryDXHelper>();

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

            services.AddScoped<IDXUnitDataService, DXUnitDataService>();
            services.AddScoped<IDXUnitDataReader, DXUnitDataReader>();
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

        private static object GetHelper(IServiceProvider sp)
        {
            var o = sp.GetRequiredService<IOptions<DXDatabaseOptions>>().Value;

            var typeLowerCase = o.Type?.Trim().ToLowerInvariant();

            return typeLowerCase switch
            {
                "mysql" => sp.GetRequiredService<MySQLQueryDXHelper>(),
                _ => sp.GetRequiredService<PGSQLQueryDXHelper>()
            };
        }
    }
}
