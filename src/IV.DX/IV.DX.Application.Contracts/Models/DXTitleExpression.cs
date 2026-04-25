namespace IV.DX.Application.Contracts.Models
{
    public class DXTitleExpression
    {
        public string Type { get; set; } = null!;
        public Guid Id { get; set; }
        public string Expression { get; set; } = null!;
    }
}
