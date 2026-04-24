using IV.DX.Application.Contracts.Actions;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Actions
{
    [DXAction("IV.DX", "Ping")]
    [DXInParameter("Message", DXActionParameterTypeEnum.String)]
    [DXOutParameter("Response", DXActionParameterTypeEnum.String, Required = true)]
    [DXOutParameter("Timestamp", DXActionParameterTypeEnum.String, Required = true)]
    public class DXPingAction : DXActionBase
    {
        public override Task<DXActionResult> ExecuteAsync(DXActionParameters input, CancellationToken ct)
        {
            var message = input.Get<string>("Message") ?? string.Empty;

            var result = DXActionResult.Ok("Ping executed successfully.");
            result.Output.Set("Response", $"Pong: {message}");
            result.Output.Set("Timestamp", DateTime.UtcNow);
            return Task.FromResult(result);
        }
    }
}
