using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Services
{
    internal class DXStructureService(IDXStructureCache dxStructureCache) : IDXStructureService
    {
        public DXElementDefinitionUnit GetDXElement(string name)
        {
            return dxStructureCache.GetDXElement(name);
        }

        public DXEnumDefinitionUnit GetDXEnum(string name)
        {
            return dxStructureCache.GetDXEnum(name);
        }

        public IEnumerable<DXRelationDefinitionUnit> GetDXRelations(string name)
        {
            return dxStructureCache.GetDXRelations(name);
        }

        public DXUnitDefinitionUnit GetDXUnit(string name)
        {
            return dxStructureCache.GetDXUnit(name);
        }
    }
}