using Microsoft.EntityFrameworkCore;
using FinanceBackend.Core;
using FinanceBackend.Data;
using FinanceBackend.DTOs.Users;
using FinanceBackend.Models;
using FinanceBackend.Services.Interfaces;

namespace FinanceBackend.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<UserResponse>> GetAllAsync()
    {
        var users = await _db.Users
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return users.Select(UserResponse.FromModel);
    }

    public async Task<UserResponse> GetByIdAsync(Guid id)
    {
        var user = await _db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"User '{id}' not found.");

        return UserResponse.FromModel(user);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        var emailLower = request.Email.ToLower().Trim();

        if (await _db.Users.AnyAsync(u => u.Email == emailLower))
            throw new InvalidOperationException($"A user with email '{request.Email}' already exists.");

        var user = new User
        {
            Email          = emailLower,
            FullName       = request.FullName.Trim(),
            HashedPassword = PasswordService.Hash(request.Password),
            Role           = request.Role,
            IsActive       = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return UserResponse.FromModel(user);
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"User '{id}' not found.");

        if (request.FullName is not null)
            user.FullName = request.FullName.Trim();

        if (request.Role.HasValue)
            user.Role = request.Role.Value;

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return UserResponse.FromModel(user);
    }

    public async Task DeactivateAsync(Guid id)
    {
        var user = await _db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"User '{id}' not found.");

        user.IsActive  = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}
