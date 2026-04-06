using Microsoft.EntityFrameworkCore;
using FinanceBackend.Core;
using FinanceBackend.Data;
using FinanceBackend.DTOs.Auth;
using FinanceBackend.DTOs.Users;
using FinanceBackend.Services.Interfaces;

namespace FinanceBackend.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService  _jwt;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IJwtService jwt, IConfiguration config)
    {
        _db     = db;
        _jwt    = jwt;
        _config = config;
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim());

        if (user is null || !PasswordService.Verify(request.Password, user.HashedPassword))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("This account has been deactivated.");

        var expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60");
        var token = _jwt.GenerateToken(user.Id, user.Email, user.Role);

        return new TokenResponse
        {
            AccessToken = token,
            TokenType   = "Bearer",
            ExpiresIn   = expiryMinutes * 60,
            Role        = user.Role.ToString()
        };
    }

    public async Task<UserResponse> GetCurrentUserAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return UserResponse.FromModel(user);
    }
}
