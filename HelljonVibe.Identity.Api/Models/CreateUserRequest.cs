using System.ComponentModel.DataAnnotations;

namespace HelljonVibe.Identity.Api.Models;

/// <summary>
/// Request model for creating a new user (admin only).
/// </summary>
public class CreateUserRequest {
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    [Required]
    [MinLength(12)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; } = false;
    public IList<string> Roles { get; set; } = new List<string>();
}
