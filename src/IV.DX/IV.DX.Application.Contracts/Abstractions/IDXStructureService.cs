using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXStructureService
    {
        DXEnumDefinitionUnit GetDXEnum(string name);
        DXUnitDefinitionUnit GetDXUnit(string name);
        DXElementDefinitionUnit GetDXElement(string name);
        IEnumerable<DXRelationDefinitionUnit> GetDXRelations(string name);
    }
}
