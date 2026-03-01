namespace IV.DX.Persistence.Contracts.Abstractions
{
    public interface IDXSecurityState
    {
        bool IsEnabled { get; }
        void LoadFromStructure();
        void SetEnabled(bool enabled);
    }
}

