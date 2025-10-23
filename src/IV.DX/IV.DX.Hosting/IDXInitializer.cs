namespace IV.DX.Hosting
{
    public interface IDXInitializer
    {
        Task InitCoreDataAsync(CancellationToken ct = default);
        Task InitCustomDataAsync(string configPath, CancellationToken ct = default);
        Task InitCacheAsync(CancellationToken ct = default);
    }
}
