using FinanceBackend.Core;

namespace FinanceBackend.DTOs.Users;

public class UpdateUserRequest
{
    public string?   FullName { get; set; }
    public UserRole? Role     { get; set; }
    public bool?     IsActive { get; set; }
}
