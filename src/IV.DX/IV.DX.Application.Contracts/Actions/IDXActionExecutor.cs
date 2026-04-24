namespace IV.DX.Application.Contracts.Actions
{
    public interface IDXActionExecutor
    {
        Task<DXActionResult> ExecuteAsync(
            string module,
            string key,
            DXActionParameters? parameters = null,
            CancellationToken ct = default);
    }
}
