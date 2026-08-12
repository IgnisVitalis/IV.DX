namespace IV.DX.Application.Contracts.Abstractions
{
    /// <summary>
    /// Request DTO that identifies the record it targets.
    /// Required by the REST controller bases so an update can bind the id from the route;
    /// the DTO services themselves place no constraint on the request type.
    /// </summary>
    public interface IDXUnitRequest
    {
        Guid Id { get; set; }
    }
}
