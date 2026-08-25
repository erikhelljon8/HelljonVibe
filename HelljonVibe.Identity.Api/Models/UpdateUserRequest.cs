using System.ComponentModel.DataAnnotations;

namespace HelljonVibe.Identity.Api.Models;


/// <summary>
/// Request model for updating a user's information (admin only).
/// </summary>
public class UpdateUserRequest {

    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; set; }
    [MinLength(3)]
    [MaxLength(100)]
    public string? Username { get; set; }
    public bool? IsApproved { get; set; }
    public bool? IsActive { get; set; }
    public bool? EmailConfirmed { get; set; }
    public bool? TwoFactorEnabled { get; set; }
    public IList<string>? Roles { get; set; }
}
