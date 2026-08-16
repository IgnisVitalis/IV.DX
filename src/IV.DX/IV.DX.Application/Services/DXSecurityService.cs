using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Models;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.Logging;
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
        IOptions<DXSecurityOptions> securityOptions,
        ILogger<DXSecurityService> logger) : IDXSecurityService
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
                logger.LogWarning("Local registration rejected because subject {Subject} already exists.", request.Subject);
                throw new InvalidOperationException($"Local login '{request.Subject}' already exists.");
            }

            var now = DateTime.UtcNow;
            var identity = new DXIdentityUnit
            {
                Id = Guid.CreateVersion7(),
                TimeStamp = now,
                Name = string.IsNullOrWhiteSpace(request.Name) ? request.Subject : request.Name
            };

            dxUnitGenericRepository.Insert(identity);

            var identityLogin = new DXIdentityLoginUnit
            {
                Id = Guid.CreateVersion7(),
                TimeStamp = now,
                Subject = request.Subject,
                SecretHash = request.Password,
                Provider = DXIdentityProviderTypeEnum.Local,
                Identity = identity.Id
            };

            dxUnitGenericRepository.Insert(identityLogin);
            logger.LogInformation(
                "Local identity {IdentityId} registered for subject {Subject}.",
                identity.Id,
                request.Subject);

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

            if (identityLogin == null || !DXPasswordHashHelper.Verify(request.Password, identityLogin.SecretHash!))
            {
                logger.LogWarning("Local login failed for subject {Subject}.", request.Subject);
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            logger.LogInformation(
                "Local login succeeded for subject {Subject} and identity login {IdentityLoginId}.",
                request.Subject,
                identityLogin.Id);

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
                LogRefreshRejected(request.SessionId, "session not found");
                throw new UnauthorizedAccessException("Session is not found.");
            }

            if (session.RevokedAt.HasValue || session.ExpiresAt <= now)
            {
                LogRefreshRejected(request.SessionId, "session not active");
                throw new UnauthorizedAccessException("Session is not active.");
            }

            if (!DXPasswordHashHelper.Verify(request.RefreshToken, session.RefreshTokenHash))
            {
                session.RevokedAt = now;
                session.LastUsedAt = now;
                dxUnitGenericRepository.Update(session);
                LogRefreshRejected(request.SessionId, "refresh token validation failed");
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            var identityLogin = dxUnitGenericRepository.GetDXUnit<DXIdentityLoginUnit>(session.IdentityLogin);
            if (identityLogin == null)
            {
                LogRefreshRejected(request.SessionId, "identity login not found", session.IdentityLogin);
                throw new UnauthorizedAccessException("Identity login is not found.");
            }

            var next = CreateSession(
                identityLogin.Id,
                Coalesce(request.UserAgent, session.UserAgent),
                Coalesce(request.IpAddress, session.IpAddress),
                Coalesce(request.DeviceId, session.DeviceId),
                now,
                out var refreshToken);

            dxUnitGenericRepository.Insert(next);

            session.LastUsedAt = now;
            session.RevokedAt = now;
            session.ReplacedBySession = next.Id;
            dxUnitGenericRepository.Update(session);
            logger.LogInformation(
                "Refresh succeeded for session {SessionId}. Replacement session {ReplacementSessionId} created.",
                request.SessionId,
                next.SessionId);

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
                logger.LogDebug("Logout ignored because session {SessionId} is missing or already revoked.", request.SessionId);
                return Task.CompletedTask;
            }

            session.RevokedAt = DateTime.UtcNow;
            dxUnitGenericRepository.Update(session);
            logger.LogInformation("Session {SessionId} logged out.", request.SessionId);

            return Task.CompletedTask;
        }

        public Task LogoutAllAsync(DXLogoutAllRequest request, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request);

            if (request.IdentityLoginId == Guid.Empty)
            {
                throw new ArgumentException("IdentityLoginId is required.", nameof(request.IdentityLoginId));
            }

            var filter = $"IdentityLogin = '{request.IdentityLoginId}' AND RevokedAt IS NULL";
            var sessions = dxUnitGenericRepository.GetDXUnits<DXAuthSessionUnit>(filter).ToList();
            if (sessions.Count == 0)
            {
                logger.LogDebug(
                    "LogoutAll found no active sessions for identity login {IdentityLoginId}.",
                    request.IdentityLoginId);
                return Task.CompletedTask;
            }

            var now = DateTime.UtcNow;
            foreach (var session in sessions)
            {
                session.RevokedAt = now;
                dxUnitGenericRepository.Update(session);
            }

            logger.LogInformation(
                "Revoked {SessionCount} active session(s) for identity login {IdentityLoginId}.",
                sessions.Count,
                request.IdentityLoginId);

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
            var session = CreateSession(identityLogin.Id, userAgent, ipAddress, deviceId, now, out var refreshToken);

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
                Id = Guid.CreateVersion7(),
                TimeStamp = now,
                SessionId = Guid.CreateVersion7(),
                RefreshTokenHash = refreshToken,
                ExpiresAt = now.AddDays(refreshLifetimeDays),
                CreatedAt = now,
                LastUsedAt = null,
                RevokedAt = null,
                // Callers pass these through from whatever they collected about the
                // client, so they are clamped to what the columns hold. IpAddress needs
                // no clamp: 45 characters covers any address it can carry.
                UserAgent = Truncate(Coalesce(userAgent, string.Empty), DXAuthSessionUnit.UserAgentMaxLength),
                IpAddress = Coalesce(ipAddress, string.Empty),
                DeviceId = Truncate(Coalesce(deviceId, string.Empty), DXAuthSessionUnit.DeviceIdMaxLength),
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
                IdentityId = identityLogin.Identity,
                IdentityLoginId = identityLogin.Id
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
                new(DXSecurityClaimNames.IdentityLoginId, identityLogin.Id.ToString()),
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
                throw new InvalidOperationException(
                    "Secrets:JwtSigningKey is not configured. Provide it via environment variable 'Secrets__JwtSigningKey'.");

            return Encoding.UTF8.GetBytes(_securityOptions.JwtSigningKey);
        }

        private static string? Coalesce(string? value, string? fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static string? Truncate(string? value, int maxLength)
        {
            return value != null && value.Length > maxLength ? value[..maxLength] : value;
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

        private void LogRefreshRejected(Guid sessionId, string reason, Guid? identityLoginId = null)
        {
            logger.LogWarning(
                "Refresh rejected for session {SessionId}, identity login {IdentityLoginId}. {Reason}.",
                sessionId,
                identityLoginId,
                reason);
        }
    }
}
