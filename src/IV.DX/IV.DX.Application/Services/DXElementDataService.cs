using IV.DX.Kernel.Enums;
using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Helpers.DXObjectHelpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Services
{
    /// <summary>
    /// Element-scoped reads and writes. See <see cref="IDXElementDataService"/> for the access rules
    /// and for what this path deliberately does not do.
    /// </summary>
    internal class DXElementDataService(
        IDXElementGenericRepository dxElementGenericRepo,
        IDXElementCoreRepository dxElementCoreRepository,
        IDXUnitCoreRepository dxUnitCoreRepository,
        IDXUnitAccessGate accessGate) : IDXElementDataService
    {
        public Task<T?> GetItemAsync<T>(string dxUnitTypeName, Guid id, CancellationToken ct = default) where T : DXElement, new()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dxUnitTypeName);

            var scope = accessGate.EnsureReadAccess(dxUnitTypeName);

            var element = dxElementGenericRepo.GetItem<T>(dxUnitTypeName, id);
            if (element == null)
                return Task.FromResult<T?>(null);

            // The element carries its owner, so the narrowed scope is applied to the unit the element
            // belongs to. Answering "not found" rather than "denied" keeps the element's existence
            // from leaking to a caller who cannot see its unit.
            if (scope == DXReadScope.VisibleOnly && !accessGate.IsReadVisible(dxUnitTypeName, element.DXUnitId))
                return Task.FromResult<T?>(null);

            return Task.FromResult<T?>(element);
        }

        public async Task<T?> GetItemAsync<T>(string dxUnitTypeName, Guid dxUnitId, Guid id, CancellationToken ct = default) where T : DXElement, new()
        {
            var element = await GetItemAsync<T>(dxUnitTypeName, id, ct);

            // An element of a different unit is reported as absent, not returned. Under a nested
            // route the owner is part of the address, so answering with something that lives
            // somewhere else would make the address a lie.
            return element is not null && element.DXUnitId == dxUnitId ? element : null;
        }

        public Task<IEnumerable<T>> GetItemsByUnitAsync<T>(string dxUnitTypeName, Guid dxUnitId, CancellationToken ct = default) where T : DXElement, new()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dxUnitTypeName);

            var scope = accessGate.EnsureReadAccess(dxUnitTypeName);

            if (dxUnitId == Guid.Empty)
                return Task.FromResult(Enumerable.Empty<T>());

            if (scope == DXReadScope.VisibleOnly && !accessGate.IsReadVisible(dxUnitTypeName, dxUnitId))
                return Task.FromResult(Enumerable.Empty<T>());

            return Task.FromResult(dxElementGenericRepo.GetItemsByUnits<T>(dxUnitTypeName, [dxUnitId]));
        }

        public Task<IEnumerable<T>> GetItemsByUnitFilterAsync<T>(string dxUnitTypeName, string dxFilter, CancellationToken ct = default) where T : DXElement, new()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dxUnitTypeName);

            var scope = accessGate.EnsureReadAccess(dxUnitTypeName);

            if (scope != DXReadScope.VisibleOnly)
                return Task.FromResult(dxElementGenericRepo.GetItems<T>(dxUnitTypeName, dxFilter));

            // Narrowed scope: take the elements of the visible units and then keep only those the
            // filter also selected, so the filter can never widen the set beyond what is visible.
            var visibleIds = accessGate.CollectVisibleIds(dxUnitTypeName);
            if (visibleIds.Count == 0)
                return Task.FromResult(Enumerable.Empty<T>());

            var selected = dxElementGenericRepo.GetItems<T>(dxUnitTypeName, dxFilter)
                .Where(x => visibleIds.Contains(x.DXUnitId));

            return Task.FromResult(selected);
        }

        public Task<Guid> InsertAsync<T>(string dxUnitTypeName, T dxElement, CancellationToken ct = default) where T : DXElement
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dxUnitTypeName);
            ArgumentNullException.ThrowIfNull(dxElement);

            var elementTypeName = AttributeReader.GetDXElementTypeName(dxElement.GetType());

            // The server assigns the id, so one supplied by the caller is ignored rather than
            // honoured - the same rule the unit path applies on insert.
            dxElement.Id = Guid.Empty;
            dxElement.DXUnitId = EnsureWritable(dxUnitTypeName, elementTypeName, Guid.Empty, dxElement.DXUnitId);
            dxElement.Id = Guid.CreateVersion7();

            return Task.FromResult(dxElementGenericRepo.Update(dxUnitTypeName, dxElement));
        }

        public Task<Guid> UpdateAsync<T>(string dxUnitTypeName, T dxElement, CancellationToken ct = default) where T : DXElement
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dxUnitTypeName);
            ArgumentNullException.ThrowIfNull(dxElement);

            if (dxElement.Id == Guid.Empty)
                throw new InvalidOperationException("Id is required to update a DXElement.");

            var elementTypeName = AttributeReader.GetDXElementTypeName(dxElement.GetType());
            var storedOwner = dxElementGenericRepo.GetOwnerDXUnitId(dxUnitTypeName, elementTypeName, dxElement.Id);

            // Reported rather than thrown, so a caller can tell "no such element" from a refusal -
            // and without needing Read access to find out, which an update does not require.
            if (storedOwner == Guid.Empty)
                return Task.FromResult(Guid.Empty);

            dxElement.DXUnitId = EnsureWritable(dxUnitTypeName, elementTypeName, dxElement.Id, dxElement.DXUnitId);

            return Task.FromResult(dxElementGenericRepo.Update(dxUnitTypeName, dxElement));
        }

        public Task<Guid> UpdateAsync<T>(string dxUnitTypeName, Guid dxUnitId, T dxElement, CancellationToken ct = default) where T : DXElement
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dxUnitTypeName);
            ArgumentNullException.ThrowIfNull(dxElement);

            if (dxElement.Id == Guid.Empty)
                throw new InvalidOperationException("Id is required to update a DXElement.");

            var elementTypeName = AttributeReader.GetDXElementTypeName(dxElement.GetType());

            // Belonging to another unit is reported the same way as not existing. The unscoped
            // overload treats a disagreeing owner as a caller error and throws, which is right when
            // the owner came from a request body; here it came from the address, and an address that
            // does not resolve is a 404, not a fault.
            if (!OwnedBy(dxUnitTypeName, elementTypeName, dxElement.Id, dxUnitId))
                return Task.FromResult(Guid.Empty);

            dxElement.DXUnitId = EnsureWritable(dxUnitTypeName, elementTypeName, dxElement.Id, dxUnitId);

            return Task.FromResult(dxElementGenericRepo.Update(dxUnitTypeName, dxElement));
        }

        public Task<bool> DeleteAsync<T>(string dxUnitTypeName, Guid dxUnitId, Guid id, CancellationToken ct = default) where T : DXElement
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dxUnitTypeName);

            var elementTypeName = AttributeReader.GetDXElementTypeName(typeof(T));

            if (id == Guid.Empty || !OwnedBy(dxUnitTypeName, elementTypeName, id, dxUnitId))
                return Task.FromResult(false);

            accessGate.EnsureInstanceAccess(dxUnitTypeName, dxUnitId, DXUnitTypeAccessOperation.Update);

            return Task.FromResult(dxElementGenericRepo.Delete(elementTypeName, [id]));
        }

        public Task<Guid> InsertOrUpdateAsync<T>(string dxUnitTypeName, T dxElement, CancellationToken ct = default) where T : DXElement
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dxUnitTypeName);
            ArgumentNullException.ThrowIfNull(dxElement);

            var elementTypeName = AttributeReader.GetDXElementTypeName(dxElement.GetType());
            var owner = EnsureWritable(dxUnitTypeName, elementTypeName, dxElement.Id, dxElement.DXUnitId);

            dxElement.DXUnitId = owner;

            if (dxElement.Id == Guid.Empty)
                dxElement.Id = Guid.CreateVersion7();

            return Task.FromResult(dxElementGenericRepo.Update(dxUnitTypeName, dxElement));
        }

        public Task<IEnumerable<Guid>> InsertOrUpdateAsync(DXDataBlock<DXElementRecord> block, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(block);

            var dxUnitTypeName = RequireUnitContext(block);
            var elementTypeName = RequireElementType(block);

            var records = block.Data?.Items?.Where(x => x != null).ToList() ?? [];
            if (records.Count == 0)
                return Task.FromResult(Enumerable.Empty<Guid>());

            // Every record is settled before any of them is written: the block is one transaction, so
            // a denial on the last record must not have let the first one through.
            foreach (var record in records)
            {
                var declaredOwner = DXObjectHelper.GetDeclaredDXUnitId(record, dxUnitTypeName);
                record.DXUnitId = EnsureWritable(dxUnitTypeName, elementTypeName, record.Id, declaredOwner);

                if (record.Id == Guid.Empty)
                    record.Id = Guid.CreateVersion7();
            }

            dxElementCoreRepository.InsertOrUpdate(block);

            return Task.FromResult<IEnumerable<Guid>>(records.Select(x => x.Id).ToList());
        }

        public Task<bool> DeleteAsync(DXDataBlock<DXElementRecord> block, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(block);

            var dxUnitTypeName = RequireUnitContext(block);
            var elementTypeName = RequireElementType(block);

            var ids = block.Data?.Delete?
                .Where(x => x != null && x.Id != Guid.Empty)
                .Select(x => x.Id)
                .Distinct()
                .ToList() ?? [];

            if (ids.Count == 0)
                return Task.FromResult(false);

            var present = new List<Guid>();

            foreach (var id in ids)
            {
                var owner = dxElementGenericRepo.GetOwnerDXUnitId(dxUnitTypeName, elementTypeName, id);

                // Deleting what is not there is not an error, and must not report on whether it
                // existed - that would answer a question the caller has no access to ask.
                if (owner == Guid.Empty)
                    continue;

                accessGate.EnsureInstanceAccess(dxUnitTypeName, owner, DXUnitTypeAccessOperation.Update);
                present.Add(id);
            }

            return Task.FromResult(present.Count != 0 && dxElementGenericRepo.Delete(elementTypeName, present));
        }

        /// <summary>
        /// Whether a stored element belongs to the given unit. False when it does not exist at all,
        /// so a caller cannot tell the two apart - which is the point under a nested address.
        /// </summary>
        private bool OwnedBy(string dxUnitTypeName, string elementTypeName, Guid elementId, Guid dxUnitId)
        {
            if (dxUnitId == Guid.Empty)
                return false;

            return dxElementGenericRepo.GetOwnerDXUnitId(dxUnitTypeName, elementTypeName, elementId) == dxUnitId;
        }

        /// <summary>
        /// Settles who owns the element being written and whether the caller may change that unit.
        /// Returns the owner to write the element under.
        /// </summary>
        /// <remarks>
        /// For an element that already exists the owner comes from storage, never from the request.
        /// A request naming another unit's element next to a unit of the caller's own would otherwise
        /// pass the check against their unit and then rewrite the other one - and move it across in
        /// the process. Disagreement is rejected rather than honoured: nothing here is a way to
        /// reparent an element.
        /// </remarks>
        private Guid EnsureWritable(string dxUnitTypeName, string elementTypeName, Guid elementId, Guid declaredOwner)
        {
            var storedOwner = elementId == Guid.Empty
                ? Guid.Empty
                : dxElementGenericRepo.GetOwnerDXUnitId(dxUnitTypeName, elementTypeName, elementId);

            if (storedOwner != Guid.Empty)
            {
                if (declaredOwner != Guid.Empty && declaredOwner != storedOwner)
                    throw new InvalidOperationException(
                        $"DXElement '{elementTypeName}' with Id '{elementId}' belongs to '{dxUnitTypeName}' instance '{storedOwner}', not '{declaredOwner}'.");

                accessGate.EnsureInstanceAccess(dxUnitTypeName, storedOwner, DXUnitTypeAccessOperation.Update);
                return storedOwner;
            }

            if (declaredOwner == Guid.Empty)
                throw new InvalidOperationException($"DXUnitId is required to write a '{elementTypeName}'.");

            accessGate.EnsureInstanceAccess(dxUnitTypeName, declaredOwner, DXUnitTypeAccessOperation.Update);

            // Checked after access so that a caller who may not touch the unit learns nothing about
            // whether it exists.
            if (!dxUnitCoreRepository.IsItemExisting(dxUnitTypeName, declaredOwner))
                throw new InvalidOperationException($"There are no '{dxUnitTypeName}' with Id '{declaredOwner}'.");

            return declaredOwner;
        }

        private static string RequireUnitContext(DXDataBlock<DXElementRecord> block)
        {
            var dxUnitTypeName = block.Meta?.DXUnitContext;

            if (string.IsNullOrWhiteSpace(dxUnitTypeName))
                throw new InvalidOperationException("DXElement block Meta.DXUnitContext is required.");

            return dxUnitTypeName;
        }

        private static string RequireElementType(DXDataBlock<DXElementRecord> block)
        {
            var elementTypeName = block.Meta?.Type;

            if (string.IsNullOrWhiteSpace(elementTypeName))
                throw new InvalidOperationException("DXElement block Meta.Type is required.");

            return elementTypeName;
        }
    }
}
