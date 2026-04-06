using FinanceBackend.Core;

namespace FinanceBackend.Core;

/// <summary>
/// Marks a controller action/class as requiring a minimum role level.
/// Viewer=0, Analyst=1, Admin=2
/// Usage: [RequireRole(UserRole.Admin)]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRoleAttribute : Attribute
{
    public UserRole MinimumRole { get; }

    public RequireRoleAttribute(UserRole minimumRole)
    {
        MinimumRole = minimumRole;
    }
}
