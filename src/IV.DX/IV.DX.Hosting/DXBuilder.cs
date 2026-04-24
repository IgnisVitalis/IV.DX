using IV.DX.Application.Actions;
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
            _services.AddScoped<IDXInitializer, DXInitializer>();
            _services.Configure<DXStartupOptions>(o => o.SecurityEnabled = _securityEnabled);

            var allActionAssemblies = new List<Assembly> { typeof(DXPingAction).Assembly };
            allActionAssemblies.AddRange(_actionAssemblies);
            _services.AddDXActions(allActionAssemblies.Distinct().ToArray());
        }
    }
}
