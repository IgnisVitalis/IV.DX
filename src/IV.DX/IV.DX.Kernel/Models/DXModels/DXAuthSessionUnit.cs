using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXAuthSessionUnit")]
    public class DXAuthSessionUnit : DXUnit
    {
        [DXColumn("SessionId")]
        public Guid SessionId { get; set; }

        [DXColumn("RefreshTokenHash")]
        public string RefreshTokenHash { get; set; }

        [DXColumn("ExpiresAt")]
        public DateTime ExpiresAt { get; set; }

        [DXColumn("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [DXColumn("LastUsedAt")]
        public DateTime? LastUsedAt { get; set; }

        [DXColumn("RevokedAt")]
        public DateTime? RevokedAt { get; set; }

        [DXColumn("UserAgent")]
        public string UserAgent { get; set; }

        [DXColumn("IpAddress")]
        public string IpAddress { get; set; }

        [DXColumn("DeviceId")]
        public string DeviceId { get; set; }

        [DXColumn("IdentityLogin", "IdentityLogin", DXLoadingType.Base)]
        public Guid IdentityLogin { get; set; }

        [DXColumn("ReplacedBySession", "ReplacedBySession", DXLoadingType.Base)]
        public Guid? ReplacedBySession { get; set; }
    }
}
