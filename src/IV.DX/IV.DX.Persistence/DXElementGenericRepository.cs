using IV.DX.Kernel.Converters.DXModelConverters;
using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Persistence
{
    internal class DXElementGenericRepository(IDXElementCoreRepository dxElementCoreRepo, IDXStructureCache dxStructureCache) : IDXElementGenericRepository
    {
        public Guid Insert(string dxModelType, DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(dxModelType);
            ArgumentNullException.ThrowIfNull(dxElement);

            var dxElementTypeName = AttributeReader.GetDXElementTypeName(dxElement.GetType());
            var relationType = dxStructureCache.GetElementInUnitRelationType(dxModelType, dxElementTypeName);
            var isRequired = relationType == DXElementInUnitTypeEnum.SingleMandatory || relationType == DXElementInUnitTypeEnum.MultiMandatory;

            var singleDXElement = dxElement.ToDXSingleElement(isRequired);

            return dxElementCoreRepo.Insert(dxModelType, singleDXElement);
        }

        public Guid Update(string dxModelType, DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(dxModelType);
            ArgumentNullException.ThrowIfNull(dxElement);

            var dxElementTypeName = AttributeReader.GetDXElementTypeName(dxElement.GetType());
            var relationType = dxStructureCache.GetElementInUnitRelationType(dxModelType, dxElementTypeName);
            var isRequired = relationType == DXElementInUnitTypeEnum.SingleMandatory || relationType == DXElementInUnitTypeEnum.MultiMandatory;

            var singleDXElement = dxElement.ToDXSingleElement(isRequired);

            return dxElementCoreRepo.Update(dxModelType, singleDXElement);
        }

        public bool Delete(DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNull(dxElement);

            // TODO : need to rework using info about relation type
            var singleDXElement = dxElement.ToDXSingleElement(false);

            return dxElementCoreRepo.Delete(singleDXElement.Name, dxElement.ID);
        }

        public T GetItem<T>(string dxUnitTypeName, Guid id) where T : DXElement
        {
            var dxElementName = AttributeReader.GetDXElementTypeName(typeof(T));

            // TODO : need to rework using info about relation type
            var dxElement = DXTableDefinitionConverter.ToDXTableDefinition(dxElementName, dxUnitTypeName, typeof(T), false);

            var result = dxElementCoreRepo.GetItem(dxElement, id);

            return DXElementConverter.ToDXElement<T>(result);
        }

        public IEnumerable<T> GetItems<T>(string dxUnitTypeName, string dxFilter) where T : DXElement
        {
            var dxElementName = AttributeReader.GetDXElementTypeName(typeof(T));

            // TODO : need to rework using info about relation type
            var dxElement = DXTableDefinitionConverter.ToDXTableDefinition(dxUnitTypeName, dxElementName, typeof(T), false);

            var result = dxElementCoreRepo.GetItems(dxElement, dxFilter);

            return result.Select(x => DXElementConverter.ToDXElement<T>(x)).ToList();
        }
    }
}
