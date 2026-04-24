using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXQueryUnit")]
    public class DXQueryUnit : DXUnit
    {
        [DXColumn("DXUnitDefinition")]
        public Guid DXUnitDefinition { get; set; }

        [DXColumn("Name")]
        public string Name { get; set; } = null!;

        [DXColumn("Description")]
        public string? Description { get; set; }

        [DXColumn("FilterExpression")]
        public string? FilterExpression { get; set; }

        public DXMultiElementsContainer<DXQueryColumnElement> DXQueryColumnElement { get; set; } = new();
    }
}
