using System.ComponentModel.DataAnnotations;

namespace HelljonVibe.Identity.Api.Models;

/// <summary>
/// Request model for updating the current user's profile.
/// </summary>
public class UpdateProfileRequest {

    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; set; }

    [MinLength(3)]
    [MaxLength(100)]
    public string? Username { get; set; }
}