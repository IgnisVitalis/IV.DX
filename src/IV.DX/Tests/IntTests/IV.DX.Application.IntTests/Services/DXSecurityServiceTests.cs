using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Models;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.Services
{
    [Collection("DX:one-time")]
    public class DXSecurityServiceTests : IntTestController
    {
        private readonly IDXSecurityService _securityService;
        private readonly IDXUnitGenericRepository _unitRepository;

        public DXSecurityServiceTests(DXTestFixture fx, ITestOutputHelper output)
            : base(fx, output)
        {
            _securityService = base.ServiceProvider.GetRequiredService<IDXSecurityService>();
            _unitRepository = base.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
        }

        [Fact]
        public async Task RegisterLocalAsync_StoresHashedSecretAndRefreshToken_Ok()
        {
            var subject = CreateSubject();
            var password = "P@ssw0rd-Register-001";

            var auth = await _securityService.RegisterLocalAsync(CreateRegisterRequest(subject, password));

            Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(auth.RefreshToken));
            Assert.NotEqual(Guid.Empty, auth.SessionId);
            Assert.NotEqual(Guid.Empty, auth.IdentityLoginID);

            var identityLogin = _unitRepository.GetDXUnit<DXIdentityLoginUnit>(auth.IdentityLoginID);
            Assert.NotNull(identityLogin);
            Assert.NotEqual(password, identityLogin!.SecretHash);
            Assert.True(DXPasswordHashHelper.Verify(password, identityLogin.SecretHash));

            var session = GetSessionBySessionId(auth.SessionId);
            Assert.NotEqual(auth.RefreshToken, session.RefreshTokenHash);
            Assert.True(DXPasswordHashHelper.Verify(auth.RefreshToken, session.RefreshTokenHash));
        }

        [Fact]
        public async Task RefreshAsync_RotatesSessionAndRevokesPreviousSession_Ok()
        {
            var subject = CreateSubject();
            var password = "P@ssw0rd-Refresh-001";

            var auth = await _securityService.RegisterLocalAsync(CreateRegisterRequest(subject, password));

            var refreshed = await _securityService.RefreshAsync(new DXRefreshRequest
            {
                SessionId = auth.SessionId,
                RefreshToken = auth.RefreshToken,
                UserAgent = "int-tests-agent",
                IpAddress = "127.0.0.1",
                DeviceId = "int-tests-device"
            });

            Assert.NotEqual(auth.SessionId, refreshed.SessionId);
            Assert.NotEqual(auth.RefreshToken, refreshed.RefreshToken);

            var previousSession = GetSessionBySessionId(auth.SessionId);
            var nextSession = GetSessionBySessionId(refreshed.SessionId);

            Assert.True(previousSession.RevokedAt.HasValue);
            Assert.Equal(nextSession.ID, previousSession.ReplacedBySession);
            Assert.Null(nextSession.RevokedAt);
            Assert.True(DXPasswordHashHelper.Verify(refreshed.RefreshToken, nextSession.RefreshTokenHash));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _securityService.RefreshAsync(new DXRefreshRequest
            {
                SessionId = auth.SessionId,
                RefreshToken = auth.RefreshToken,
                UserAgent = "int-tests-agent",
                IpAddress = "127.0.0.1",
                DeviceId = "int-tests-device"
            }));
        }

        [Fact]
        public async Task LogoutAsync_RevokesSession_Ok()
        {
            var subject = CreateSubject();
            var password = "P@ssw0rd-Logout-001";

            var auth = await _securityService.RegisterLocalAsync(CreateRegisterRequest(subject, password));

            await _securityService.LogoutAsync(new DXLogoutRequest
            {
                SessionId = auth.SessionId
            });

            var session = GetSessionBySessionId(auth.SessionId);
            Assert.True(session.RevokedAt.HasValue);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _securityService.RefreshAsync(new DXRefreshRequest
            {
                SessionId = auth.SessionId,
                RefreshToken = auth.RefreshToken,
                UserAgent = "int-tests-agent",
                IpAddress = "127.0.0.1",
                DeviceId = "int-tests-device"
            }));
        }

        [Fact]
        public async Task LogoutAllAsync_RevokesAllSessionsForIdentityLogin_Ok()
        {
            var subject = CreateSubject();
            var password = "P@ssw0rd-LogoutAll-001";

            var auth1 = await _securityService.RegisterLocalAsync(CreateRegisterRequest(subject, password));
            var auth2 = await _securityService.LoginLocalAsync(new DXLoginLocalRequest
            {
                Subject = subject,
                Password = password,
                UserAgent = "int-tests-agent",
                IpAddress = "127.0.0.1",
                DeviceId = "int-tests-device-2"
            });

            Assert.NotEqual(auth1.SessionId, auth2.SessionId);

            await _securityService.LogoutAllAsync(new DXLogoutAllRequest
            {
                IdentityLoginID = auth1.IdentityLoginID
            });

            var sessions = _unitRepository
                .GetDXUnits<DXAuthSessionUnit>($"IdentityLogin = '{auth1.IdentityLoginID}'")
                .ToList();

            Assert.True(sessions.Count >= 2);
            Assert.All(sessions, x => Assert.True(x.RevokedAt.HasValue));
        }

        private static DXRegisterLocalRequest CreateRegisterRequest(string subject, string password)
        {
            return new DXRegisterLocalRequest
            {
                Subject = subject,
                Password = password,
                Name = subject,
                UserAgent = "int-tests-agent",
                IpAddress = "127.0.0.1",
                DeviceId = "int-tests-device"
            };
        }

        private DXAuthSessionUnit GetSessionBySessionId(Guid sessionId)
        {
            var session = _unitRepository
                .GetDXUnits<DXAuthSessionUnit>($"SessionId = '{sessionId}'")
                .FirstOrDefault();

            Assert.NotNull(session);
            return session!;
        }

        private static string CreateSubject()
        {
            return $"security.int.{Guid.NewGuid():N}";
        }
    }
}
