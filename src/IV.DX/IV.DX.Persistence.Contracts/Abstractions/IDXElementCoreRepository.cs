using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXElementCoreRepository
    {
        Guid Insert(string dxModelType, DXSingleElement dxSingleDXElement);
        Guid Update(string dxModelType, DXSingleElement dxSingleDXElement);
        Guid InsertOrUpdate(string dxModelType, DXSingleElement dxSingleDXElement);
        bool Delete(string typeName, Guid dxUnitId);
        DXSingleElement? GetItem(DXElementDefinition container, Guid id);
        IEnumerable<DXSingleElement> GetItems(DXElementDefinition container, string dxFilter);
    }
}