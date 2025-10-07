namespace IV.DX.Contracts.Application
{
    public interface IMigrationService
    {
        void LoadStructure(string path);
        void LoadCoreStructure();
    }
}