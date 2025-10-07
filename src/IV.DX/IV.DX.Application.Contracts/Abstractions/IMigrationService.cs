namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IMigrationService
    {
        void LoadStructure(string path);
        void LoadCoreStructure();
    }
}