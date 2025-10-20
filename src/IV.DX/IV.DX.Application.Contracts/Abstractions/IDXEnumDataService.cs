namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXEnumDataService
    {
        Task<IDictionary<int, string>> GetItemsAsync(string enumTypeName, CancellationToken ct = default);
    }
}