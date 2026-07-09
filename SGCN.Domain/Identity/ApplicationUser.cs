using Microsoft.AspNetCore.Identity;

namespace SGCN.Domain.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? NifOrCin { get; set; }
    public bool IsActive { get; set; } = true;
    public bool ForcePasswordChange { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
