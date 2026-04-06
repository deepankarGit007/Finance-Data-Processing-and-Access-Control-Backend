using FinanceBackend.Core;
using FinanceBackend.Models;

namespace FinanceBackend.DTOs.Users;

public class UserResponse
{
    public Guid     Id        { get; set; }
    public string   Email     { get; set; } = string.Empty;
    public string   FullName  { get; set; } = string.Empty;
    public UserRole Role      { get; set; }
    public bool     IsActive  { get; set; }
    public DateTime CreatedAt { get; set; }

    public static UserResponse FromModel(User u) => new()
    {
        Id        = u.Id,
        Email     = u.Email,
        FullName  = u.FullName,
        Role      = u.Role,
        IsActive  = u.IsActive,
        CreatedAt = u.CreatedAt
    };
}
