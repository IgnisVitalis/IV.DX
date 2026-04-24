using IV.DX.Application.Contracts.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXSecurityService
    {
        Task<DXAuthResult> RegisterLocalAsync(DXRegisterLocalRequest request, CancellationToken ct = default);

        Task<DXAuthResult> LoginLocalAsync(DXLoginLocalRequest request, CancellationToken ct = default);

        Task<DXAuthResult> RefreshAsync(DXRefreshRequest request, CancellationToken ct = default);

        Task LogoutAsync(DXLogoutRequest request, CancellationToken ct = default);

        Task LogoutAllAsync(DXLogoutAllRequest request, CancellationToken ct = default);
    }
}
