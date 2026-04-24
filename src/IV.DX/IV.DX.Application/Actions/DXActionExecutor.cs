using IV.DX.Application.Contracts.Actions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.Actions
{
    internal sealed class DXActionExecutor(
        IDXActionRegistry registry,
        IServiceProvider serviceProvider) : IDXActionExecutor
    {
        public async Task<DXActionResult> ExecuteAsync(
            string module,
            string key,
            DXActionParameters? parameters = null,
            CancellationToken ct = default)
        {
            var actionType = registry.Resolve(module, key);
            if (actionType is null)
                return DXActionResult.Fail($"Action '{module}/{key}' is not registered.");

            var action = (DXActionBase)serviceProvider.GetRequiredService(actionType);

            return await action.ExecuteAsync(parameters ?? new DXActionParameters(), ct);
        }
    }
}
