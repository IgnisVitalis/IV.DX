namespace IV.DX.Application.Contracts.Models
{
    public class DXLoginLocalRequest
    {
        public string Subject { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string UserAgent { get; set; } = null!;

        public string IpAddress { get; set; } = null!;

        public string DeviceId { get; set; } = null!;
    }
}
