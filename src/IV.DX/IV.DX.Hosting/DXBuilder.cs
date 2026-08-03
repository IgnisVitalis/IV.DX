using IV.DX.Application.Actions;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace IV.DX.Hosting
{
    public sealed class DXBuilder
    {
        private readonly IServiceCollection _services;
        private readonly List<Assembly> _actionAssemblies = new();
        private bool _securityEnabled;

        internal DXBuilder(IServiceCollection services)
        {
            _services = services;
        }

        /// <summary>
        /// The service collection DX is being registered into.
        /// Database provider packages use this to contribute their implementations.
        /// </summary>
        public IServiceCollection Services => _services;

        public DXBuilder AddSecurity()
        {
            _securityEnabled = true;
            return this;
        }

        public DXBuilder AddCustomData(string configPath)
        {
            _services.AddDXCustomData(configPath);
            return this;
        }

        public DXBuilder AddHandlers(params Assembly[] assemblies)
        {
            _services.AddDXHandlers(assemblies);
            return this;
        }

        public DXBuilder AddActions(params Assembly[] assemblies)
        {
            _actionAssemblies.AddRange(assemblies);
            return this;
        }

        public IServiceCollection Build()
        {
            RegisterCore();
            return _services;
        }

        public IServiceCollection RegisterHostedService()
        {
            RegisterCore();
            _services.AddHostedService<DXStartupHostedService>();
            _services.AddHostedService<DXEncryptionRotationService>();
            return _services;
        }

        private void RegisterCore()
        {
            EnsureProviderRegistered();

            _services.AddScoped<IDXInitializer, DXInitializer>();
            _services.Configure<DXStartupOptions>(o => o.SecurityEnabled = _securityEnabled);

            var allActionAssemblies = new List<Assembly> { typeof(DXPingAction).Assembly };
            allActionAssemblies.AddRange(_actionAssemblies);
            _services.AddDXActions(allActionAssemblies.Distinct().ToArray());
        }

        private void EnsureProviderRegistered()
        {
            if (_services.Any(x => x.ServiceType == typeof(ISQLDbProvider)))
                return;

            throw new InvalidOperationException(
                "No IV.DX database provider is registered. " +
                "Reference a provider package and select it on the builder, " +
                "e.g. services.AddDX(configuration).UsePostgreSQL() from IV.DX.PostgreSQL.");
        }
    }
}
