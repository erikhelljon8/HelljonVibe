using System.ComponentModel.DataAnnotations;

namespace HelljonVibe.Identity.Api.Models;

/// <summary>
/// Request model for changing the current user's password.
/// </summary>
public class ChangePasswordRequest {

    [Required]
    [MinLength(12)]
    [MaxLength(128)]
    public string CurrentPassword { get; set; } = string.Empty;
    [Required]
    [MinLength(12)]
    [MaxLength(128)]
    public string NewPassword { get; set; } = string.Empty;
    [Required]
    [Compare("NewPassword")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}