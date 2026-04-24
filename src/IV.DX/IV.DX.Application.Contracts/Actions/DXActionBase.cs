namespace IV.DX.Application.Contracts.Actions
{
    public abstract class DXActionBase
    {
        public abstract Task<DXActionResult> ExecuteAsync(DXActionParameters input, CancellationToken ct);
    }
}
