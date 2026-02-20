namespace IV.DX.Application.Contracts.Models
{
    public class DXRefreshRequest
    {
        public Guid SessionId { get; set; }

        public string RefreshToken { get; set; }

        public string UserAgent { get; set; }

        public string IpAddress { get; set; }

        public string DeviceId { get; set; }
    }
}
