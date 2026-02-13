using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXRoleUnit")]
    public class DXRoleUnit : DXUnit
    {
        [DXColumn("Name")]
        public string Name { get; set; }

        public DXMultiElementsContainer<DXUnitGrantElement> DXUnitGrantElement { get; set; } = new()
        {
            Announced = new HashSet<DXUnitGrantElement>()
        };
    }
}
