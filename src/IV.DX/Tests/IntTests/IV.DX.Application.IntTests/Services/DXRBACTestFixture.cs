using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IV.DX.Application.IntTests.Services
{
    public class DXRBACTestFixture : IAsyncLifetime
    {
        private readonly IServiceScope _scope;
        private readonly IDXUnitDataService _dataService;
        private readonly IDXUnitGenericRepository _genericRepo;
        private readonly IDXExecutionContextAccessor _contextAccessor;

        public Guid IdentityId { get; private set; }
        public Guid LoginId { get; private set; }
        public Guid SessionRecordId { get; private set; }
        public Guid SessionId { get; private set; }
        public Guid TenantId { get; private set; }

        public DXRBACTestFixture(DXTestFixture fx)
        {
            _scope = fx.Root.CreateScope();
            _dataService = _scope.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            _genericRepo = _scope.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
            _contextAccessor = _scope.ServiceProvider.GetRequiredService<IDXExecutionContextAccessor>();
        }

        public async Task InitializeAsync()
        {
            IdentityId = Guid.NewGuid();
            LoginId = Guid.NewGuid();
            SessionRecordId = Guid.NewGuid();
            SessionId = Guid.NewGuid();
            TenantId = Guid.NewGuid();

            var now = DateTime.UtcNow;

            using var _ = _contextAccessor.BeginScope(new DXExecutionContext
            {
                SubjectId = "system:rbac-fixture",
                IsSystem = true
            });

            _genericRepo.Insert(new DXIdentityUnit
            {
                Id = IdentityId,
                TimeStamp = now,
                Name = $"rbac-fixture-{IdentityId:N}"
            });

            _genericRepo.Insert(new DXIdentityLoginUnit
            {
                Id = LoginId,
                TimeStamp = now,
                Subject = $"rbac.fixture.{Guid.NewGuid():N}",
                SecretHash = "test-password",
                Provider = DXIdentityProviderTypeEnum.Local,
                Identity = IdentityId
            });

            _genericRepo.Insert(new DXAuthSessionUnit
            {
                Id = SessionRecordId,
                TimeStamp = now,
                SessionId = SessionId,
                RefreshTokenHash = "test-token",
                ExpiresAt = now.AddHours(1),
                CreatedAt = now,
                UserAgent = "rbac-fixture",
                IpAddress = "127.0.0.1",
                DeviceId = "rbac-fixture",
                IdentityLogin = LoginId
            });

            await _dataService.InsertAsync(new DXTenantUnit
            {
                Id = TenantId,
                TimeStamp = now,
                Name = $"rbac-fixture-tenant-{TenantId:N}"
            });
        }

        public async Task DisposeAsync()
        {
            using var _ = _contextAccessor.BeginScope(new DXExecutionContext
            {
                SubjectId = "system:rbac-fixture",
                IsSystem = true
            });

            try { await _dataService.DeleteAsync(new DXTenantUnit { Id = TenantId, TimeStamp = DateTime.UtcNow }); } catch { }
            try { var s = _genericRepo.GetDXUnit<DXAuthSessionUnit>(SessionRecordId); if (s != null) _genericRepo.Delete(s); } catch { }
            try { var l = _genericRepo.GetDXUnit<DXIdentityLoginUnit>(LoginId); if (l != null) _genericRepo.Delete(l); } catch { }
            try { var i = _genericRepo.GetDXUnit<DXIdentityUnit>(IdentityId); if (i != null) _genericRepo.Delete(i); } catch { }

            _scope.Dispose();
        }
    }
}
