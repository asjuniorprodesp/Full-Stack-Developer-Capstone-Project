using Microsoft.AspNetCore.Identity;

namespace SkillSnap.Api.Models;

public class ApplicationUser : IdentityUser
{
	public string? FullName { get; set; }
	public string? ProfileImageUrl { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? UpdatedAt { get; set; }
	public bool IsActive { get; set; } = true;
}
