namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXMigrationService
    {
        void LoadStructure(string path);
        void LoadCoreStructure();
    }
}