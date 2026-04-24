namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXModuleRegistry
    {
        void Register(string moduleId);
        bool IsRegistered(string moduleId);
    }
}
