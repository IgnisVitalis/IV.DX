using System.Reflection;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXMigrationService
    {
        Task LoadStructureAsync(string path, CancellationToken ct = default);
        Task LoadCoreStructureAsync(Assembly assembly, string preInitListPath, string postInitListPath, CancellationToken ct = default);
    }
}