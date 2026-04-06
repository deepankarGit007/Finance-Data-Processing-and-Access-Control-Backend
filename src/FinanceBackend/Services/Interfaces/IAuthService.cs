using FinanceBackend.DTOs.Auth;
using FinanceBackend.DTOs.Users;

namespace FinanceBackend.Services.Interfaces;

public interface IAuthService
{
    Task<TokenResponse> LoginAsync(LoginRequest request);
    Task<UserResponse> GetCurrentUserAsync(Guid userId);
}
