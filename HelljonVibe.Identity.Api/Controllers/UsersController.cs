using HelljonVibe.Identity.Api.Models;
using HelljonVibe.Identity.Api.Services;
using HelljonVibe.Identity.Models.Constants;
using HelljonVibe.Identity.Models.DTOs.Responses;
using HelljonVibe.Identity.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HelljonVibe.Identity.Api.Controllers;

/// <summary>
/// Controller for user management operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase {
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="encryptionService">The encryption service.</param>
    /// <param name="logger">The logger.</param>
    public UsersController(
        IAuthService authService,
        IUnitOfWork unitOfWork,
        IEncryptionService encryptionService,
        ILogger<UsersController> logger) {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a list of all users (Admin only).
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="isApproved">Filter by approval status.</param>
    /// <param name="isActive">Filter by active status.</param>
    /// <returns>A paged list of user responses.</returns>
    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(PagedResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isApproved = null,
        [FromQuery] bool? isActive = null) {
        try {
            var query = _unitOfWork.Users.GetAll();

            // Apply filters
            if (isApproved.HasValue) {
                query = query.Where(u => u.IsApproved == isApproved.Value);
            }
            
            if (isActive.HasValue) {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Get paged data
            var users = await query
                .OrderBy(u => u.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to responses
            var userResponses = users.Select(u => new UserResponse {
                Id = u.Id,
                Username = _encryptionService.Decrypt(u.EncryptedUsername),
                Email = _encryptionService.Decrypt(u.EncryptedEmail),
                EmailConfirmed = u.EmailConfirmed,
                TwoFactorEnabled = u.TwoFactorEnabled,
                IsApproved = u.IsApproved,
                IsActive = u.IsActive,
                CreatedDate = u.CreatedDate,
                LastLoginDate = u.LastLoginDate,
                AccessFailedCount = u.AccessFailedCount,
                LockoutEndDateUtc = u.LockoutEndDateUtc,
                PrivacyPolicyAccepted = u.PrivacyPolicyAcceptedDate.HasValue,
                TermsAccepted = u.TermsAccepted
            }).ToList();

            return Ok(new PagedResponse<UserResponse> {
                Data = userResponses,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting users list");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred while getting users",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Gets a specific user by ID.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>The user response.</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(Guid id) {
        try {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            
            if (user == null) {
                return NotFound(new ErrorResponse {
                    Message = "User not found",
                    ErrorCode = "USER_NOT_FOUND"
                });
            }

            // Get user roles
            var userRoles = await _unitOfWork.UserRoles.GetByUserIdAsync(id);
            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
            var roles = await _unitOfWork.Roles.GetByIdsAsync(roleIds);
            var roleNames = roles.Select(r => r.Name).ToArray();

            return Ok(new UserResponse {
                Id = user.Id,
                Username = _encryptionService.Decrypt(user.EncryptedUsername),
                Email = _encryptionService.Decrypt(user.EncryptedEmail),
                EmailConfirmed = user.EmailConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                IsApproved = user.IsApproved,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate,
                AccessFailedCount = user.AccessFailedCount,
                LockoutEndDateUtc = user.LockoutEndDateUtc,
                PrivacyPolicyAccepted = user.PrivacyPolicyAcceptedDate.HasValue,
                TermsAccepted = user.TermsAccepted,
                Roles = roleNames
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred while getting the user",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Creates a new user (Admin only).
    /// </summary>
    /// <param name="request">The create user request.</param>
    /// <returns>The created user response.</returns>
    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request) {
        try {
            // Check if username or email already exists
            var encryptedUsername = _encryptionService.Encrypt(request.Username);
            var encryptedEmail = _encryptionService.Encrypt(request.Email);

            var usernameExists = await _unitOfWork.Users.UsernameExistsAsync(encryptedUsername);
            var emailExists = await _unitOfWork.Users.EmailExistsAsync(encryptedEmail);

            if (usernameExists) {
                return Conflict(new ErrorResponse {
                    Message = "Username already exists",
                    ErrorCode = "USERNAME_EXISTS"
                });
            }

            if (emailExists) {
                return Conflict(new ErrorResponse {
                    Message = "Email already exists",
                    ErrorCode = "EMAIL_EXISTS"
                });
            }

            // Hash password
            var (passwordHash, passwordSalt) = _encryptionService.HashPassword(request.Password);

            // Generate user encryption key
            var userKey = _encryptionService.GenerateUserKey();

            // Create user
            var user = new User {
                Id = Guid.NewGuid(),
                EncryptedEmail = encryptedEmail,
                EncryptedUsername = encryptedUsername,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                EmailConfirmed = request.EmailConfirmed,
                TwoFactorEnabled = request.TwoFactorEnabled,
                AccessFailedCount = 0,
                IsApproved = request.IsApproved,
                IsActive = request.IsActive,
                CreatedDate = DateTimeOffset.UtcNow,
                EncryptionKey = userKey,
                PrivacyPolicyAcceptedDate = request.PrivacyPolicyAccepted ? DateTimeOffset.UtcNow : null,
                PrivacyPolicyVersion = request.PrivacyPolicyAccepted ? GdprConstants.CurrentConsentVersion : null,
                TermsAccepted = request.TermsAccepted,
                TermsAcceptedDate = request.TermsAccepted ? DateTimeOffset.UtcNow : null,
                TermsVersion = request.TermsAccepted ? "1.0" : null
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CommitAsync();

            // Assign roles
            if (request.Roles != null && request.Roles.Any()) {
                foreach (var roleName in request.Roles) {
                    var role = await _unitOfWork.Roles.GetByNameAsync(roleName);
                    if (role != null) {
                        var userRole = new UserRole {
                            UserId = user.Id,
                            RoleId = role.Id,
                            AssignedDate = DateTimeOffset.UtcNow,
                            AssignedByUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("nameidentifier")?.Value)
                        };
                        await _unitOfWork.UserRoles.AddAsync(userRole);
                    }
                }
                await _unitOfWork.CommitAsync();
            }

            _logger.LogInformation("User {UserId} created by {AdminUserId}", user.Id, User.FindFirst("sub")?.Value);

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, new UserResponse {
                Id = user.Id,
                Username = request.Username,
                Email = request.Email,
                EmailConfirmed = user.EmailConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                IsApproved = user.IsApproved,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                Roles = request.Roles?.ToArray() ?? Array.Empty<string>()
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred while creating the user",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Updates a user (Admin only).
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="request">The update user request.</param>
    /// <returns>Success response.</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request) {
        try {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            
            if (user == null) {
                return NotFound(new ErrorResponse {
                    Message = "User not found",
                    ErrorCode = "USER_NOT_FOUND"
                });
            }

            // Update user properties
            if (request.IsApproved.HasValue) {
                user.IsApproved = request.IsApproved.Value;
            }
            
            if (request.IsActive.HasValue) {
                user.IsActive = request.IsActive.Value;
            }

            if (request.EmailConfirmed.HasValue) {
                user.EmailConfirmed = request.EmailConfirmed.Value;
            }

            if (request.TwoFactorEnabled.HasValue) {
                user.TwoFactorEnabled = request.TwoFactorEnabled.Value;
            }

            if (request.AccessFailedCount.HasValue) {
                user.AccessFailedCount = request.AccessFailedCount.Value;
            }

            if (request.LockoutEndDateUtc.HasValue) {
                user.LockoutEndDateUtc = request.LockoutEndDateUtc.Value;
            }

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            // Update roles if provided
            if (request.Roles != null) {
                // Get current user roles
                var currentRoles = await _unitOfWork.UserRoles.GetByUserIdAsync(id);
                var currentRoleIds = currentRoles.Select(ur => ur.RoleId).ToList();
                
                // Get requested role IDs
                var requestedRoles = await _unitOfWork.Roles.GetByNamesAsync(request.Roles);
                var requestedRoleIds = requestedRoles.Select(r => r.Id).ToList();
                
                // Remove roles not in requested list
                foreach (var roleId in currentRoleIds.Except(requestedRoleIds)) {
                    var roleToRemove = currentRoles.FirstOrDefault(ur => ur.RoleId == roleId);
                    if (roleToRemove != null) {
                        await _unitOfWork.UserRoles.DeleteAsync(roleToRemove);
                    }
                }
                
                // Add new roles
                foreach (var roleId in requestedRoleIds.Except(currentRoleIds)) {
                    var userRole = new UserRole {
                        UserId = id,
                        RoleId = roleId,
                        AssignedDate = DateTimeOffset.UtcNow,
                        AssignedByUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("nameidentifier")?.Value)
                    };
                    await _unitOfWork.UserRoles.AddAsync(userRole);
                }
                
                await _unitOfWork.CommitAsync();
            }

            _logger.LogInformation("User {UserId} updated by {AdminUserId}", id, User.FindFirst("sub")?.Value);

            return Ok(new UserResponse {
                Id = user.Id,
                Username = _encryptionService.Decrypt(user.EncryptedUsername),
                Email = _encryptionService.Decrypt(user.EncryptedEmail),
                EmailConfirmed = user.EmailConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                IsApproved = user.IsApproved,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate,
                AccessFailedCount = user.AccessFailedCount,
                LockoutEndDateUtc = user.LockoutEndDateUtc
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred while updating the user",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Updates the current user's profile.
    /// </summary>
    /// <param name="request">The update profile request.</param>
    /// <returns>Success response.</returns>
    [HttpPut("me/profile")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request) {
        try {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("nameidentifier")?.Value);
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            
            if (user == null) {
                return NotFound(new ErrorResponse {
                    Message = "User not found",
                    ErrorCode = "USER_NOT_FOUND"
                });
            }

            // Update profile (limited fields for self-update)
            if (!string.IsNullOrEmpty(request.Username)) {
                var encryptedUsername = _encryptionService.Encrypt(request.Username);
                if (await _unitOfWork.Users.UsernameExistsAsync(encryptedUsername) && 
                    _encryptionService.Decrypt(user.EncryptedUsername) != request.Username) {
                    return Conflict(new ErrorResponse {
                        Message = "Username already exists",
                        ErrorCode = "USERNAME_EXISTS"
                    });
                }
                user.EncryptedUsername = encryptedUsername;
            }

            if (!string.IsNullOrEmpty(request.Email)) {
                var encryptedEmail = _encryptionService.Encrypt(request.Email);
                if (await _unitOfWork.Users.EmailExistsAsync(encryptedEmail) && 
                    _encryptionService.Decrypt(user.EncryptedEmail) != request.Email) {
                    return Conflict(new ErrorResponse {
                        Message = "Email already exists",
                        ErrorCode = "EMAIL_EXISTS"
                    });
                }
                user.EncryptedEmail = encryptedEmail;
                user.EmailConfirmed = false; // Require re-confirmation
            }

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("User {UserId} updated their profile", userId);

            return Ok(new UserResponse {
                Id = user.Id,
                Username = _encryptionService.Decrypt(user.EncryptedUsername),
                Email = _encryptionService.Decrypt(user.EncryptedEmail),
                EmailConfirmed = user.EmailConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                IsApproved = user.IsApproved,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error updating profile for user {UserId}", User.FindFirst("sub")?.Value);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred while updating your profile",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Deletes a user (Admin only).
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>Success response.</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id) {
        try {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            
            if (user == null) {
                return NotFound(new ErrorResponse {
                    Message = "User not found",
                    ErrorCode = "USER_NOT_FOUND"
                });
            }

            // Soft delete - just deactivate
            user.IsActive = false;
            user.IsApproved = false;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("User {UserId} deactivated by {AdminUserId}", id, User.FindFirst("sub")?.Value);

            return Ok(new { Message = "User deactivated successfully" });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred while deleting the user",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Approves a user (Admin only).
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>Success response.</returns>
    [HttpPost("{id}/approve")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Approve(Guid id) {
        try {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            
            if (user == null) {
                return NotFound(new ErrorResponse {
                    Message = "User not found",
                    ErrorCode = "USER_NOT_FOUND"
                });
            }

            user.IsApproved = true;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("User {UserId} approved by {AdminUserId}", id, User.FindFirst("sub")?.Value);

            return Ok(new UserResponse {
                Id = user.Id,
                Username = _encryptionService.Decrypt(user.EncryptedUsername),
                Email = _encryptionService.Decrypt(user.EncryptedEmail),
                EmailConfirmed = user.EmailConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                IsApproved = user.IsApproved,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error approving user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred while approving the user",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Toggles user activation status (Admin only).
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>Success response.</returns>
    [HttpPost("{id}/toggle-activation")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ToggleActivation(Guid id) {
        try {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            
            if (user == null) {
                return NotFound(new ErrorResponse {
                    Message = "User not found",
                    ErrorCode = "USER_NOT_FOUND"
                });
            }

            user.IsActive = !user.IsActive;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("User {UserId} activation toggled by {AdminUserId}", id, User.FindFirst("sub")?.Value);

            return Ok(new UserResponse {
                Id = user.Id,
                Username = _encryptionService.Decrypt(user.EncryptedUsername),
                Email = _encryptionService.Decrypt(user.EncryptedEmail),
                EmailConfirmed = user.EmailConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                IsApproved = user.IsApproved,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error toggling activation for user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred while toggling user activation",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Resets a user's password (Admin only).
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>Success response.</returns>
    [HttpPost("{id}/reset-password")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResetPasswordAdmin(Guid id, [FromBody] string newPassword) {
        try {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            
            if (user == null) {
                return NotFound(new ErrorResponse {
                    Message = "User not found",
                    ErrorCode = "USER_NOT_FOUND"
                });
            }

            // Hash new password
            var (passwordHash, passwordSalt) = _encryptionService.HashPassword(newPassword);
            
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.AccessFailedCount = 0;
            user.LockoutEndDateUtc = null;
            
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Password reset for user {UserId} by {AdminUserId}", id, User.FindFirst("sub")?.Value);

            return Ok(new { Message = "Password reset successfully" });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error resetting password for user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred while resetting the password",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Gets users by role (Admin only).
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <returns>A list of user responses.</returns>
    [HttpGet("by-role/{roleName}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByRole(string roleName) {
        try {
            var role = await _unitOfWork.Roles.GetByNameAsync(roleName);
            
            if (role == null) {
                return NotFound(new ErrorResponse {
                    Message = "Role not found",
                    ErrorCode = "ROLE_NOT_FOUND"
                });
            }

            var userRoles = await _unitOfWork.UserRoles.GetAllAsync(ur => ur.RoleId == role.Id);
            var userIds = userRoles.Select(ur => ur.UserId).ToList();
            var users = await _unitOfWork.Users.GetByIdsAsync(userIds);

            var userResponses = users.Select(u => new UserResponse {
                Id = u.Id,
                Username = _encryptionService.Decrypt(u.EncryptedUsername),
                Email = _encryptionService.Decrypt(u.EncryptedEmail),
                EmailConfirmed = u.EmailConfirmed,
                TwoFactorEnabled = u.TwoFactorEnabled,
                IsApproved = u.IsApproved,
                IsActive = u.IsActive,
                CreatedDate = u.CreatedDate,
                Roles = new[] { roleName }
            }).ToList();

            return Ok(userResponses);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting users by role {RoleName}", roleName);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred while getting users by role",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }
}

/// <summary>
/// Paged Response DTO.
/// </summary>
/// <typeparam name="T">The data type.</typeparam>
public class PagedResponse<T> {
    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    public List<T> Data { get; set; } = new();

    /// <summary>
    /// Gets or sets the page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total count.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the total pages.
    /// </summary>
    public int TotalPages { get; set; }
}

/// <summary>
/// Extensions for IRepository to support user operations.
/// </summary>
public static class UserRepositoryExtensions {
    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <returns>A queryable collection of users.</returns>
    public static IQueryable<User> GetAll(this IUserRepository repository) {
        return repository.GetDbSet();
    }

    /// <summary>
    /// Gets users by IDs.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="ids">The user IDs.</param>
    /// <returns>A list of users.</returns>
    public static async Task<List<User>> GetByIdsAsync(this IUserRepository repository, List<Guid> ids) {
        return await repository.GetDbSet().Where(u => ids.Contains(u.Id)).ToListAsync();
    }

    /// <summary>
    /// Gets roles by names.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="names">The role names.</param>
    /// <returns>A list of roles.</returns>
    public static async Task<List<Role>> GetByNamesAsync(this IRepository<Role> repository, List<string> names) {
        return await repository.GetDbSet().Where(r => names.Contains(r.Name)).ToListAsync();
    }
}
