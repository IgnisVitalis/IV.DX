using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.PrivateModels.DXQueryUnit
{
    [DXElement("DXMigrationScriptsMainElement")]
    internal class DXQueryColumnElement : DXElement
    {
        [DXColumn("Name")]
        public string Name { get; set; }
        [DXColumn("Expression")]
        public string Expression { get; set; }
        [DXColumn("Order")]
        public int Order { get; set; }
    }
}
