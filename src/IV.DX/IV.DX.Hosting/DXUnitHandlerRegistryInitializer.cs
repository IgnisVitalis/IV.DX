using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace IV.DX.Hosting
{
    internal sealed class DXUnitHandlerRegistryInitializer : IHostedService
    {
        private readonly IServiceProvider _root;
        private readonly Assembly[] _assemblies;

        public DXUnitHandlerRegistryInitializer(IServiceProvider root, IEnumerable<Assembly> assembliesToScan)
        {
            _root = root;
            _assemblies = assembliesToScan?.ToArray() ?? Array.Empty<Assembly>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var handlerTypes = DXUnitHandlerScanner.FindHandlerTypes(_assemblies);

            using var scope = _root.CreateScope();

            var insertProv = scope.ServiceProvider.GetRequiredService<IDXUnitInsertHandlerProvider>();
            var updateProv = scope.ServiceProvider.GetRequiredService<IDXUnitUpdateHandlerProvider>();
            var deleteProv = scope.ServiceProvider.GetRequiredService<IDXUnitDeleteHandlerProvider>();
            var getProv = scope.ServiceProvider.GetRequiredService<IDXUnitGetHandlerProvider>();

            foreach (var hType in handlerTypes)
            {
                var handler = scope.ServiceProvider.GetRequiredService(hType);

                foreach (var (handlerType, openInterface, unitType) in DXUnitHandlerScanner.EnumerateDxHandlerInterfaces(hType))
                {
                    var closedIface = openInterface.MakeGenericType(unitType);

                    if (openInterface == typeof(IDXBeforeInsert<>))
                        CallRegister(insertProv, nameof(insertProv.Register), closedIface, handler);
                    else if (openInterface == typeof(IDXAfterInsert<>))
                        CallRegister(insertProv, nameof(insertProv.Register), closedIface, handler);
                    else if (openInterface == typeof(IDXBeforeUpdate<>))
                        CallRegister(updateProv, nameof(updateProv.Register), closedIface, handler);
                    else if (openInterface == typeof(IDXAfterUpdate<>))
                        CallRegister(updateProv, nameof(updateProv.Register), closedIface, handler);
                    else if (openInterface == typeof(IDXBeforeDelete<>))
                        CallRegister(deleteProv, nameof(deleteProv.Register), closedIface, handler);
                    else if (openInterface == typeof(IDXAfterDelete<>))
                        CallRegister(deleteProv, nameof(deleteProv.Register), closedIface, handler);
                    else if (openInterface == typeof(IDXBeforeGet<>))
                        CallRegister(getProv, nameof(getProv.Register), closedIface, handler);
                    else if (openInterface == typeof(IDXAfterGet<>))
                        CallRegister(getProv, nameof(getProv.Register), closedIface, handler);
                }
            }

            await Task.CompletedTask;

            static void CallRegister(object provider, string methodName, Type handlerInterface, object handlerInstance)
            {
                var provType = provider.GetType();
                var mi = provType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .First(m =>
                        m.Name == methodName &&
                        m.IsGenericMethodDefinition &&
                        m.GetParameters().Length == 1 &&
                        m.GetParameters()[0].ParameterType.IsGenericType);

                var t = handlerInterface.GetGenericArguments()[0];

                var closed = mi.MakeGenericMethod(t);
                closed.Invoke(provider, new[] { handlerInstance });
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
