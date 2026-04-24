namespace IV.DX.Application.Contracts.Models
{
    public class DXTitleExpression
    {
        public string Type { get; set; } = null!;
        public Guid ID { get; set; }
        public string Expression { get; set; } = null!;
    }
}
