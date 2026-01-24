namespace IV.DX.Hosting
{
    public interface IDXInitializer
    {
        Task InitDXCoreDataAsync(CancellationToken ct = default);
        Task InitDXQueryDataAsync(CancellationToken ct = default);
        Task InitDXSecurityDataAsync(CancellationToken ct = default);
        Task InitCustomDataAsync(string configPath, CancellationToken ct = default);
    }
}
