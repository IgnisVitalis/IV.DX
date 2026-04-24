namespace IV.DX.Hosting
{
    internal interface IDXInitializer
    {
        Task InitAsync(CancellationToken ct = default);
        Task InitDXSecurityDataAsync(CancellationToken ct = default);
        Task InitCustomDataAsync(string configPath, CancellationToken ct = default);
    }
}
