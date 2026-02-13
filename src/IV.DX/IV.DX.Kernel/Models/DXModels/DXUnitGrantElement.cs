using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXUnitGrantElement")]
    public class DXUnitGrantElement : DXElement
    {
        [DXColumn("Read")]
        public bool Read { get; set; }

        [DXColumn("Write")]
        public bool Write { get; set; }

        [DXColumn("Delete")]
        public bool Delete { get; set; }

        [DXColumn("Effect")]
        public DXGrantEffectEnum Effect { get; set; }

        [DXColumn("DXUnit", "E2U(DXUnit).ID", DXLoadingType.Base)]
        public Guid TargetDXUnitID { get; set; }
    }
}
