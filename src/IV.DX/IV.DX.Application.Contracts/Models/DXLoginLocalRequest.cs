namespace IV.DX.Application.Contracts.Models
{
    public class DXLoginLocalRequest
    {
        public string Subject { get; set; }

        public string Password { get; set; }

        public string UserAgent { get; set; }

        public string IpAddress { get; set; }

        public string DeviceId { get; set; }
    }
}
