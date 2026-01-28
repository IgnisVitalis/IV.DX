using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXElementCoreRepository
    {
        Guid Insert(string dxModelType, DXSingleElement dxSingleDXElement);
        Guid Update(string dxModelType, DXSingleElement dxSingleDXElement);
        Guid InsertOrUpdate(string dxModelType, DXSingleElement dxSingleDXElement);
        bool Delete(string typeName, Guid dxUnitId);
        DXSingleElement? GetItem(DXTableDefinition container, Guid id);
        IEnumerable<DXSingleElement> GetItems(DXTableDefinition container, string dxFilter);
    }
}