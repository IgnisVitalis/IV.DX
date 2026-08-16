using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXAuthSessionUnit")]
    public class DXAuthSessionUnit : DXUnit
    {
        // Widths the table accepts. An oversized value fails the insert instead of being
        // trimmed, so whoever fills a session clamps against these. They are declared
        // here because the columns belong to this unit - callers cannot know them.
        public const int UserAgentMaxLength = 100;

        public const int DeviceIdMaxLength = 50;

        [DXColumn("SessionId")]
        public Guid SessionId { get; set; }

        [DXColumn("RefreshTokenHash")]
        public string RefreshTokenHash { get; set; } = null!;

        [DXColumn("ExpiresAt")]
        public DateTime ExpiresAt { get; set; }

        [DXColumn("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [DXColumn("LastUsedAt")]
        public DateTime? LastUsedAt { get; set; }

        [DXColumn("RevokedAt")]
        public DateTime? RevokedAt { get; set; }

        [DXColumn("UserAgent")]
        public string? UserAgent { get; set; }

        [DXColumn("IpAddress")]
        public string? IpAddress { get; set; }

        [DXColumn("DeviceId")]
        public string? DeviceId { get; set; }

        [DXColumn("IdentityLogin", "IdentityLogin", DXLoadingType.Base)]
        public Guid IdentityLogin { get; set; }

        [DXColumn("ReplacedBySession", "ReplacedBySession", DXLoadingType.Base)]
        public Guid? ReplacedBySession { get; set; }
    }
}
