namespace IV.DX.Application.Contracts.Actions
{
    public interface IDXActionRegistry
    {
        void Register(Type actionType);
        Type? Resolve(string module, string key);
    }
}
