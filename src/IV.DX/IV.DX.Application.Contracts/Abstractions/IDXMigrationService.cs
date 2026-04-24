using System.Reflection;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXMigrationService
    {
        Task MigrateCoreAsync(Assembly assembly, string preInitListPath, string postInitListPath, CancellationToken ct = default);
        Task MigrateCustomAsync(string path, CancellationToken ct = default);
        Task MigrateCustomEmbeddedAsync(Assembly assembly, string path, CancellationToken ct = default);
        Task MigrateFromContentAsync(IEnumerable<DXMigrationScriptContent> scripts, CancellationToken ct = default);
    }

    public sealed record DXMigrationScriptContent(string FileName, string Content);
}