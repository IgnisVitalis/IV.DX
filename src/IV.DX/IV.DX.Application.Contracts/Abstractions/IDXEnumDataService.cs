namespace IV.DX.Application.Contracts.Abstractions
{
    internal interface IDXEnumDataService
    {
        Task<IDictionary<int, string>> GetItemsAsync(string enumTypeName, CancellationToken ct = default);
    }
}