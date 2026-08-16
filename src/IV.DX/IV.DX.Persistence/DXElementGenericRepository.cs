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

            var block = DXRecordWriter.ToBlock(dxElement, new DXRecordWriteOptions
            {
                DXUnitContext = dxModelType,
                IsRequired = isRequired
            });

            return dxElementCoreRepo.InsertOrUpdate(block);
        }

        public Guid Update(string dxModelType, DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(dxModelType);
            ArgumentNullException.ThrowIfNull(dxElement);

            var dxElementTypeName = AttributeReader.GetDXElementTypeName(dxElement.GetType());
            var relationType = dxStructureCache.GetElementInUnitRelationType(dxModelType, dxElementTypeName);
            var isRequired = relationType == DXElementInUnitTypeEnum.SingleMandatory || relationType == DXElementInUnitTypeEnum.MultiMandatory;

            var block = DXRecordWriter.ToBlock(dxElement, new DXRecordWriteOptions
            {
                DXUnitContext = dxModelType,
                IsRequired = isRequired
            });

            return dxElementCoreRepo.InsertOrUpdate(block);
        }

        public bool Delete(DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNull(dxElement);

            var elementTypeName = AttributeReader.GetDXElementTypeName(dxElement.GetType());
            return dxElementCoreRepo.Delete(elementTypeName, [dxElement.Id]);
        }

        public bool Delete(string dxElementTypeName, IEnumerable<Guid> ids)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(dxElementTypeName);

            return dxElementCoreRepo.Delete(dxElementTypeName, ids);
        }

        public Guid GetOwnerDXUnitId(string dxUnitTypeName, string dxElementTypeName, Guid id)
        {
            return dxElementCoreRepo.GetOwnerDXUnitId(dxUnitTypeName, dxElementTypeName, id);
        }

        public T GetItem<T>(string dxUnitTypeName, Guid id) where T : DXElement, new()
        {
            var dxElementName = AttributeReader.GetDXElementTypeName(typeof(T));

            // TODO : need to rework using info about relation type
            var dxElement = DXTableDefinitionConverter.ToDXTableDefinition(dxElementName, dxUnitTypeName, typeof(T), false);

            var result = dxElementCoreRepo.GetItemRecord(dxElement, id);
            if (result == null) return default!;

            return DXRecordConverter.ToDXElement<T>(result);
        }

        public IEnumerable<T> GetItemsByUnits<T>(string dxUnitTypeName, IEnumerable<Guid> dxUnitIds) where T : DXElement, new()
        {
            var dxElementName = AttributeReader.GetDXElementTypeName(typeof(T));

            // TODO : need to rework using info about relation type
            var dxElement = DXTableDefinitionConverter.ToDXTableDefinition(dxElementName, dxUnitTypeName, typeof(T), false);

            var result = dxElementCoreRepo.GetItemsRecordByUnits(dxElement, dxUnitIds);

            return result.Select(DXRecordConverter.ToDXElement<T>).ToList();
        }

        public IEnumerable<T> GetItems<T>(string dxUnitTypeName, string dxFilter) where T : DXElement, new()
        {
            var dxElementName = AttributeReader.GetDXElementTypeName(typeof(T));

            // TODO : need to rework using info about relation type
            var dxElement = DXTableDefinitionConverter.ToDXTableDefinition(dxElementName, dxUnitTypeName, typeof(T), false);

            var result = dxElementCoreRepo.GetItemsRecord(dxElement, dxFilter);

            return result.Select(DXRecordConverter.ToDXElement<T>).ToList();
        }
    }
}
