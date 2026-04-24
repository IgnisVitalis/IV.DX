namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXSecurityState
    {
        bool IsEnabled { get; }
        void LoadFromStructure();
        void SetEnabled(bool enabled);
    }
}

