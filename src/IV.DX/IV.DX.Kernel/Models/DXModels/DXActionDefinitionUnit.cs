using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXActionDefinitionUnit")]
    public class DXActionDefinitionUnit : DXUnit
    {
        [DXColumn("Name")]
        public string Name { get; set; } = null!;

        [DXColumn("Description")]
        public string? Description { get; set; }

        [DXColumn("Module")]
        public string Module { get; set; } = null!;

        [DXColumn("Key")]
        public string Key { get; set; } = null!;

        [DXColumn("IsEnabled")]
        public bool IsEnabled { get; set; }

        [DXColumn("Kind")]
        public DXActionKind Kind { get; set; }

        public DXMultiElementsContainer<DXActionInParameterElement> DXActionInParameterElement { get; set; } = new()
        {
            Announced = new HashSet<DXActionInParameterElement>()
        };

        public DXMultiElementsContainer<DXActionOutParameterElement> DXActionOutParameterElement { get; set; } = new()
        {
            Announced = new HashSet<DXActionOutParameterElement>()
        };
    }
}