using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Hosting
{
    public interface IDXInitializer
    {
        void DropDatabase();
        void InitCoreData();
        void InitCustomData(string configPath);
        Task InitCacheAsync(IServiceScope scope, CancellationToken ct = default);
    }
}
