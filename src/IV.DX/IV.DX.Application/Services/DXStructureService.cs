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

        public IEnumerable<DXElementDefinitionUnit> DXElements
        {
            get
            {
                return dxStructureCache.DXElements;
            }
        }


        public DXEnumDefinitionUnit GetDXEnum(string name)
        {
            return dxStructureCache.GetDXEnum(name);
        }

        public IEnumerable<DXEnumDefinitionUnit> DXEnums
        {
            get
            {
                return dxStructureCache.DXEnums;
            }
        }

        public IEnumerable<DXRelationDefinitionUnit> GetDXRelations(string name)
        {
            return dxStructureCache.GetDXRelations(name);
        }

        public IEnumerable<DXRelationDefinitionUnit> DXRelations
        {
            get
            {
                return dxStructureCache.DXRelations;
            }
        }

        public DXUnitDefinitionUnit GetDXUnit(string name)
        {
            return dxStructureCache.GetDXUnit(name);
        }

        public IEnumerable<DXUnitDefinitionUnit> DXUnits
        {
            get
            {
                return dxStructureCache.DXUnits;
            }
        }
    }
}