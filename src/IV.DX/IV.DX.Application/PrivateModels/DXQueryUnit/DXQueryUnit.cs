using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.PrivateModels.DXQueryUnit
{
    [DXUnit("DXQueryUnit")]
    internal class DXQueryUnit : DXUnit
    {
        [DXColumn("DXUnitName", "R(DXUnitDefinition).Name")]
        public string DXUnitName { get; set; }

        [DXColumn("Name")]
        public string Name { get; set; }
        [DXColumn("Description")]
        public string Description { get; set; }
        public DXMultiElementsContainer<DXQueryColumnElement> DXQueryColumnElement { get; set; }
    }
}
