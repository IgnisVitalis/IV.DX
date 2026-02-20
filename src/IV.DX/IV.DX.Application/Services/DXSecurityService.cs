using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Models;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IV.DX.Application.Services
{
    internal class DXSecurityService(
        IDXUnitGenericRepository dxUnitGenericRepository,
        IOptions<DXSecurityOptions> securityOptions) : IDXSecurityService
    {
        private readonly DXSecurityOptions _securityOptions = securityOptions?.Value ?? new DXSecurityOptions();

        public Task<DXAuthResult> RegisterLocalAsync(DXRegisterLocalRequest request, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Subject);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Password);

            var existing = FindLocalIdentityLogin(request.Subject);
            if (existing != null)
            {
                throw new InvalidOperationException($"Local login '{request.Subject}' already exists.");
            }

            var now = DateTime.UtcNow;
            var identity = new DXIdentityUnit
            {
                ID = Guid.NewGuid(),
                TimeStamp = now,
                Name = string.IsNullOrWhiteSpace(request.Name) ? request.Subject : request.Name
            };

            dxUnitGenericRepository.Insert(identity);

            var identityLogin = new DXIdentityLoginUnit
            {
                ID = Guid.NewGuid(),
                TimeStamp = now,
                Subject = request.Subject,
                SecretHash = request.Password,
                Provider = DXIdentityProviderTypeEnum.Local,
                Identity = identity.ID
            };

            dxUnitGenericRepository.Insert(identityLogin);

            return Task.FromResult(CreateAuthResult(
                identityLogin,
                request.UserAgent,
                request.IpAddress,
                request.DeviceId));
        }

        public Task<DXAuthResult> LoginLocalAsync(DXLoginLocalRequest request, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Subject);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Password);

            var identityLogin = FindLocalIdentityLogin(request.Subject);

            if (identityLogin == null || !DXPasswordHashHelper.Verify(request.Password, identityLogin.SecretHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            return Task.FromResult(CreateAuthResult(
                identityLogin,
                request.UserAgent,
                request.IpAddress,
                request.DeviceId));
        }

        public Task<DXAuthResult> RefreshAsync(DXRefreshRequest request, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.RefreshToken);

            if (request.SessionId == Guid.Empty)
            {
                throw new ArgumentException("SessionId is required.", nameof(request.SessionId));
            }

            var now = DateTime.UtcNow;
            var session = FindSessionBySessionId(request.SessionId);
            if (session == null)
            {
                throw new UnauthorizedAccessException("Session is not found.");
            }

            if (session.RevokedAt.HasValue || session.ExpiresAt <= now)
            {
                throw new UnauthorizedAccessException("Session is not active.");
            }

            if (!DXPasswordHashHelper.Verify(request.RefreshToken, session.RefreshTokenHash))
            {
                session.RevokedAt = now;
                session.LastUsedAt = now;
                dxUnitGenericRepository.Update(session);

                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            var identityLogin = dxUnitGenericRepository.GetDXUnit<DXIdentityLoginUnit>(session.IdentityLogin);
            if (identityLogin == null)
            {
                throw new UnauthorizedAccessException("Identity login is not found.");
            }

            var next = CreateSession(
                identityLogin.ID,
                Coalesce(request.UserAgent, session.UserAgent),
                Coalesce(request.IpAddress, session.IpAddress),
                Coalesce(request.DeviceId, session.DeviceId),
                now,
                out var refreshToken);

            dxUnitGenericRepository.Insert(next);

            session.LastUsedAt = now;
            session.RevokedAt = now;
            session.ReplacedBySession = next.ID;
            dxUnitGenericRepository.Update(session);

            return Task.FromResult(BuildAuthResult(identityLogin, next, refreshToken, now));
        }

        public Task LogoutAsync(DXLogoutRequest request, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request);

            if (request.SessionId == Guid.Empty)
            {
                throw new ArgumentException("SessionId is required.", nameof(request.SessionId));
            }

            var session = FindSessionBySessionId(request.SessionId);
            if (session == null || session.RevokedAt.HasValue)
            {
                return Task.CompletedTask;
            }

            session.RevokedAt = DateTime.UtcNow;
            dxUnitGenericRepository.Update(session);

            return Task.CompletedTask;
        }

        public Task LogoutAllAsync(DXLogoutAllRequest request, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request);

            if (request.IdentityLoginID == Guid.Empty)
            {
                throw new ArgumentException("IdentityLoginID is required.", nameof(request.IdentityLoginID));
            }

            var filter = $"IdentityLogin = '{request.IdentityLoginID}' AND RevokedAt IS NULL";
            var sessions = dxUnitGenericRepository.GetDXUnits<DXAuthSessionUnit>(filter).ToList();
            if (sessions.Count == 0)
            {
                return Task.CompletedTask;
            }

            var now = DateTime.UtcNow;
            foreach (var session in sessions)
            {
                session.RevokedAt = now;
                dxUnitGenericRepository.Update(session);
            }

            return Task.CompletedTask;
        }

        private DXIdentityLoginUnit? FindLocalIdentityLogin(string subject)
        {
            var filter = $"Provider = {(int)DXIdentityProviderTypeEnum.Local}";
            return dxUnitGenericRepository
                .GetDXUnits<DXIdentityLoginUnit>(filter)
                .FirstOrDefault(x => string.Equals(x.Subject, subject, StringComparison.Ordinal));
        }

        private DXAuthSessionUnit? FindSessionBySessionId(Guid sessionId)
        {
            var filter = $"SessionId = '{sessionId}'";
            return dxUnitGenericRepository.GetDXUnits<DXAuthSessionUnit>(filter).FirstOrDefault();
        }

        private DXAuthResult CreateAuthResult(
            DXIdentityLoginUnit identityLogin,
            string? userAgent,
            string? ipAddress,
            string? deviceId)
        {
            var now = DateTime.UtcNow;
            var session = CreateSession(identityLogin.ID, userAgent, ipAddress, deviceId, now, out var refreshToken);

            dxUnitGenericRepository.Insert(session);

            return BuildAuthResult(identityLogin, session, refreshToken, now);
        }

        private DXAuthSessionUnit CreateSession(
            Guid identityLoginId,
            string? userAgent,
            string? ipAddress,
            string? deviceId,
            DateTime now,
            out string refreshToken)
        {
            refreshToken = GenerateToken(64);
            var refreshLifetimeDays = _securityOptions.RefreshTokenLifetimeDays <= 0
                ? 30
                : _securityOptions.RefreshTokenLifetimeDays;

            return new DXAuthSessionUnit
            {
                ID = Guid.NewGuid(),
                TimeStamp = now,
                SessionId = Guid.NewGuid(),
                RefreshTokenHash = refreshToken,
                ExpiresAt = now.AddDays(refreshLifetimeDays),
                CreatedAt = now,
                LastUsedAt = null,
                RevokedAt = null,
                UserAgent = Coalesce(userAgent, string.Empty),
                IpAddress = Coalesce(ipAddress, string.Empty),
                DeviceId = Coalesce(deviceId, string.Empty),
                IdentityLogin = identityLoginId,
                ReplacedBySession = null
            };
        }

        private static DXAuthResult BuildAuthResult(
            DXIdentityLoginUnit identityLogin,
            DXAuthSessionUnit session,
            string refreshToken,
            string accessToken,
            DateTime accessTokenExpiresAt)
        {
            return new DXAuthResult
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = session.ExpiresAt,
                SessionId = session.SessionId,
                IdentityID = identityLogin.Identity,
                IdentityLoginID = identityLogin.ID
            };
        }

        private DXAuthResult BuildAuthResult(
            DXIdentityLoginUnit identityLogin,
            DXAuthSessionUnit session,
            string refreshToken,
            DateTime now)
        {
            var accessTokenLifetimeMinutes = _securityOptions.AccessTokenLifetimeMinutes <= 0
                ? 15
                : _securityOptions.AccessTokenLifetimeMinutes;

            var accessTokenExpiresAt = now.AddMinutes(accessTokenLifetimeMinutes);
            var accessToken = GenerateAccessToken(identityLogin, session, accessTokenExpiresAt);

            return BuildAuthResult(
                identityLogin,
                session,
                refreshToken,
                accessToken,
                accessTokenExpiresAt);
        }

        private string GenerateAccessToken(
            DXIdentityLoginUnit identityLogin,
            DXAuthSessionUnit session,
            DateTime expiresAt)
        {
            var signingKeyBytes = ResolveSigningKeyBytes();
            var key = new SymmetricSecurityKey(signingKeyBytes);
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(DXSecurityClaimNames.Subject, identityLogin.Identity.ToString()),
                new(DXSecurityClaimNames.IdentityLoginId, identityLogin.ID.ToString()),
                new(DXSecurityClaimNames.SessionId, session.SessionId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _securityOptions.JwtIssuer,
                audience: _securityOptions.JwtAudience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private byte[] ResolveSigningKeyBytes()
        {
            if (string.IsNullOrWhiteSpace(_securityOptions.JwtSigningKey))
            {
                throw new InvalidOperationException("Security:JwtSigningKey is required.");
            }

            if (_securityOptions.JwtSigningKeyIsBase64)
            {
                return Convert.FromBase64String(_securityOptions.JwtSigningKey);
            }

            return Encoding.UTF8.GetBytes(_securityOptions.JwtSigningKey);
        }

        private static string Coalesce(string? value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static string GenerateToken(int bytesLength)
        {
            var bytes = RandomNumberGenerator.GetBytes(bytesLength);
            return Convert
                .ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
