using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXElementGenericRepository
    {
        Guid Insert(string dxUnitTypeName, DXElement dxElement);
        Guid Update(string dxUnitTypeName, DXElement dxElement);
        bool Delete(DXElement dxElement);
        bool Delete(string dxElementTypeName, IEnumerable<Guid> ids);
        T GetItem<T>(string dxUnitTypeName, Guid id) where T : DXElement, new();

        /// <summary>
        /// Elements belonging to the given units, selected on the element table's own owner column.
        /// </summary>
        IEnumerable<T> GetItemsByUnits<T>(string dxUnitTypeName, IEnumerable<Guid> dxUnitIds) where T : DXElement, new();

        /// <summary>
        /// Elements of every unit matching <paramref name="dxFilter"/>. The filter is evaluated
        /// against the <em>unit</em> table, not the element table.
        /// </summary>
        IEnumerable<T> GetItems<T>(string dxUnitTypeName, string dxFilter) where T : DXElement, new();

        /// <summary>
        /// The unit a stored element belongs to, or <see cref="Guid.Empty"/> when no such element
        /// exists. Reads the owner from storage rather than trusting a caller-supplied one.
        /// </summary>
        Guid GetOwnerDXUnitId(string dxUnitTypeName, string dxElementTypeName, Guid id);
    }
}
