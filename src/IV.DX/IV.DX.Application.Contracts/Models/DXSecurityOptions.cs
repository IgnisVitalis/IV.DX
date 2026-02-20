namespace IV.DX.Application.Contracts.Models
{
    public class DXSecurityOptions
    {
        public string JwtIssuer { get; set; } = "IV.DX";

        public string JwtAudience { get; set; } = "IV.DX.Client";

        public string? JwtSigningKey { get; set; }

        public bool JwtSigningKeyIsBase64 { get; set; }

        public int AccessTokenLifetimeMinutes { get; set; } = 15;

        public int RefreshTokenLifetimeDays { get; set; } = 30;
    }
}
