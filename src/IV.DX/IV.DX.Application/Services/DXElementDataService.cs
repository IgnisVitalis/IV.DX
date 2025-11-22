using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Services
{
    internal class DXElementDataService(IDXCoreRepository coreRepo) : IDXElementDataService
    {
        public Task<T> GetItems<T>(string dxFilter, CancellationToken ct = default) where T : DXElement, new()
        {
            //DXModelDefinitionConverter.ToDXModelDefinition(typeof(T)

            //    coreRepo.GetSingleDXElement();
            throw new NotImplementedException();
        }
    }
}
