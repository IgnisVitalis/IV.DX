namespace IV.DX.Application.Contracts.Models
{
    public class DXAuthResult
    {
        public string AccessToken { get; set; }

        public DateTime AccessTokenExpiresAt { get; set; }

        public string RefreshToken { get; set; }

        public DateTime RefreshTokenExpiresAt { get; set; }

        public Guid SessionId { get; set; }

        public Guid IdentityID { get; set; }

        public Guid IdentityLoginID { get; set; }
    }
}
