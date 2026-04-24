namespace IV.DX.Application.Contracts.Models
{
    public class DXRefreshRequest
    {
        public Guid SessionId { get; set; }

        public string RefreshToken { get; set; } = null!;

        public string UserAgent { get; set; } = null!;

        public string IpAddress { get; set; } = null!;

        public string DeviceId { get; set; } = null!;
    }
}
