using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Persistence
{
    internal class DXElementGenericRepository(IDXCoreRepository coreRepo) : IDXElementGenericRepository
    {
        public Guid InsertDXElement(string dxModelType, DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(dxModelType);
            ArgumentNullException.ThrowIfNull(dxElement);

            var singleDXElement = dxElement.ConvertToSingleItem();

            return coreRepo.InsertSingleDXElement(dxModelType, singleDXElement);
        }

        public Guid UpdateDXElement(string dxModelType, DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(dxModelType);
            ArgumentNullException.ThrowIfNull(dxElement);

            var singleDXElement = dxElement.ConvertToSingleItem();

            return coreRepo.UpdateSingleDXElement(dxModelType, singleDXElement);
        }

        public bool DeleteDXElement(DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNull(dxElement);

            var singleDXElement = dxElement.ConvertToSingleItem();

            return coreRepo.DeleteSingleDXElement(singleDXElement.Name, dxElement.ID);
        }

        public T GetDXElement<T>(Guid id) where T : DXElement
        {
            var dxElementName = AttributeReader.GetDXElementTypeName(typeof(T));

            var dxElement = DXModelDefinitionConverter.Get(dxElementName, typeof(T));

            var result = coreRepo.GetSingleDXElement(dxElement, id);

            return DXUnitHelper.CreateDXElementInstance<T>(result);
        }
    }
}
