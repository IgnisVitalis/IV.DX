using IV.DX.Kernel.Converters.DXModelConverters;
using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Persistence
{
    internal class DXElementGenericRepository(IDXCoreRepository coreRepo, IDXStructureCache dxStructureCache) : IDXElementGenericRepository
    {
        public Guid InsertDXElement(string dxModelType, DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(dxModelType);
            ArgumentNullException.ThrowIfNull(dxElement);

            var dxElementTypeName  = AttributeReader.GetDXElementTypeName(dxElement.GetType());
            var relationType = dxStructureCache.GetElementInUnitRelationType(dxModelType, dxElementTypeName);
            var isRequired = relationType == DXElementInUnitTypeEnum.SingleMandatory || relationType == DXElementInUnitTypeEnum.MultiMandatory;

            var singleDXElement = dxElement.ToDXSingleElement(isRequired);

            return coreRepo.InsertSingleDXElement(dxModelType, singleDXElement);
        }

        public Guid UpdateDXElement(string dxModelType, DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(dxModelType);
            ArgumentNullException.ThrowIfNull(dxElement);

            var dxElementTypeName = AttributeReader.GetDXElementTypeName(dxElement.GetType());
            var relationType = dxStructureCache.GetElementInUnitRelationType(dxModelType, dxElementTypeName);
            var isRequired = relationType == DXElementInUnitTypeEnum.SingleMandatory || relationType == DXElementInUnitTypeEnum.MultiMandatory;

            var singleDXElement = dxElement.ToDXSingleElement(isRequired);

            return coreRepo.UpdateSingleDXElement(dxModelType, singleDXElement);
        }

        public bool DeleteDXElement(DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNull(dxElement);
                       
            // TODO : need to rework using info about relation type
            var singleDXElement = dxElement.ToDXSingleElement(false);

            return coreRepo.DeleteSingleDXElement(singleDXElement.Name, dxElement.ID);
        }

        public T GetDXElement<T>(Guid id) where T : DXElement
        {
            var dxElementName = AttributeReader.GetDXElementTypeName(typeof(T));

            // TODO : need to rework using info about relation type
            var dxElement = DXElementDefinitionConverter.ToDXElementDefinition(dxElementName, typeof(T), false);

            var result = coreRepo.GetSingleDXElement(dxElement, id);

            return DXElementConverter.ToDXElement<T>(result);
        }
    }
}
