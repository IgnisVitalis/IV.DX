using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Persistence
{
    internal class DXElementGenericRepository(IDXCoreRepository coreRepo) : IDXElementGenericRepository
    {
        public Guid InsertBlock(string esqlModelType, DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(esqlModelType);
            ArgumentNullException.ThrowIfNull(dxElement);

            var singleBlock = dxElement.ConvertToSingleItem();

            return coreRepo.InsertSingleBlock(esqlModelType, singleBlock);
        }

        public Guid UpdateBlock(string esqlModelType, DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(esqlModelType);
            ArgumentNullException.ThrowIfNull(dxElement);

            var singleBlock = dxElement.ConvertToSingleItem();

            return coreRepo.UpdateSingleBlock(esqlModelType, singleBlock);
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
