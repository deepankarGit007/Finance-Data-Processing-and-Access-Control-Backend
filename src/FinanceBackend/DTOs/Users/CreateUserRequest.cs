using System.ComponentModel.DataAnnotations;
using FinanceBackend.Core;

namespace FinanceBackend.DTOs.Users;

public class CreateUserRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    public string FullName { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Viewer;
}
