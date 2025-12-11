using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.PrivateModels.DXQueryUnit
{
    [DXElement("DXQueryMainElement")]
    internal class DXQueryMainElement : DXElement
    {
        [DXColumn("Name")]
        public string Name { get; set; }
        [DXColumn("Description")]
        public string Description { get; set; }
    }
}
