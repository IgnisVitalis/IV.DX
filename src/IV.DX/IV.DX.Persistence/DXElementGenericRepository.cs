using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Persistence
{
    internal class DXElementGenericRepository(IDXCoreRepository coreRepo) : IDXElementGenericRepository
    {
        public Guid InsertBlock(string dxModelType, DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(dxModelType);
            ArgumentNullException.ThrowIfNull(dxElement);

            var singleBlock = dxElement.ConvertToSingleItem();

            return coreRepo.InsertSingleBlock(dxModelType, singleBlock);
        }

        public Guid UpdateBlock(string dxModelType, DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(dxModelType);
            ArgumentNullException.ThrowIfNull(dxElement);

            var singleBlock = dxElement.ConvertToSingleItem();

            return coreRepo.UpdateSingleBlock(dxModelType, singleBlock);
        }

        public bool DeleteBlock(DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNull(dxElement);

            var singleBlock = dxElement.ConvertToSingleItem();

            return coreRepo.DeleteSingleBlock(singleBlock.Name, dxElement.ID);
        }

        public T GetBlock<T>(Guid id) where T : DXElement
        {
            var blockName = AttributeReader.GetDXElementTypeName(typeof(T));

            var block = DXModelDefinitionHelper.GetDXElementDefinition(blockName, typeof(T));

            var result = coreRepo.GetSingleBlock(block, id);

            return DXUnitHelper.CreateBlockInstance<T>(result);
        }
    }
}
