namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitCommandService<TRequest>
    {
        /// <summary>
        /// Inserts a new record and returns its server-assigned id.
        /// Requires <c>Create</c> access; an existing record is never overwritten.
        /// </summary>
        Task<Guid> CreateAsync(TRequest dto, CancellationToken ct = default);

        /// <summary>
        /// Updates an existing record. Returns false when no record with that id exists.
        /// Requires <c>Update</c> access, or ownership of the record.
        /// </summary>
        Task<bool> UpdateAsync(TRequest dto, CancellationToken ct = default);

        /// <summary>
        /// Inserts or updates depending on whether the record already exists, and returns its id.
        /// Kept for import and synchronisation flows where the caller genuinely does not know;
        /// it needs whichever of <c>Create</c> or <c>Update</c> the resolved path turns out to use,
        /// so prefer <see cref="CreateAsync"/> or <see cref="UpdateAsync"/> when the intent is known.
        /// </summary>
        Task<Guid> SaveAsync(TRequest dto, CancellationToken ct = default);

        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
