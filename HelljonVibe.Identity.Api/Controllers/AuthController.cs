using HelljonVibe.Identity.Api.Models;
using HelljonVibe.Identity.Api.Services;
using HelljonVibe.Identity.Models.DTOs.Requests;
using HelljonVibe.Identity.Models.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace HelljonVibe.Identity.Api.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController : ControllerBase {
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    /// <param name="logger">The logger.</param>
    public AuthController(IAuthService authService, ILogger<AuthController> logger) {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <returns>The authentication response.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request) {
        try {
            if (await _authService.UsernameExistsAsync(request.Username)) {
                return Conflict(new ErrorResponse {
                    Message = "Username already exists",
                    ErrorCode = "USERNAME_EXISTS"
                });
            }

            if (await _authService.UserExistsAsync(request.Email)) {
                return Conflict(new ErrorResponse {
                    Message = "Email already exists",
                    ErrorCode = "EMAIL_EXISTS"
                });
            }

            var (user, authResponse) = await _authService.RegisterAsync(request);
            
            _logger.LogInformation("User {UserId} registered successfully", user.Id);
            
            return CreatedAtAction(nameof(Register), new { userId = user.Id }, authResponse);
        } catch (InvalidOperationException ex) {
            _logger.LogWarning(ex, "Registration failed for {Email}", request.Email);
            return BadRequest(new ErrorResponse {
                Message = ex.Message,
                ErrorCode = "REGISTRATION_FAILED"
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error during registration for {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred during registration",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Logs in a user.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <returns>The authentication response.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request) {
        try {
            var authResponse = await _authService.LoginAsync(request);
            
            _logger.LogInformation("User {Username} logged in successfully", request.UsernameOrEmail);
            
            return Ok(authResponse);
        } catch (UnauthorizedAccessException ex) {
            _logger.LogWarning(ex, "Login failed for {Username}", request.UsernameOrEmail);
            return Unauthorized(new ErrorResponse {
                Message = ex.Message,
                ErrorCode = "LOGIN_FAILED"
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error during login for {Username}", request.UsernameOrEmail);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred during login",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Refreshes the access token.
    /// </summary>
    /// <param name="request">The refresh token request.</param>
    /// <returns>The authentication response with new tokens.</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request) {
        try {
            var authResponse = await _authService.RefreshTokenAsync(request.RefreshToken);
            
            _logger.LogInformation("Token refreshed for user {UserId}", authResponse.UserId);
            
            return Ok(authResponse);
        } catch (UnauthorizedAccessException ex) {
            _logger.LogWarning(ex, "Token refresh failed");
            return Unauthorized(new ErrorResponse {
                Message = ex.Message,
                ErrorCode = "REFRESH_FAILED"
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred during token refresh",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Logs out a user.
    /// </summary>
    /// <returns>Success response.</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout() {
        try {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("nameidentifier")?.Value);
            var refreshToken = Request.Headers["X-Refresh-Token"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(refreshToken)) {
                return BadRequest(new ErrorResponse {
                    Message = "Refresh token is required",
                    ErrorCode = "MISSING_REFRESH_TOKEN"
                });
            }

            await _authService.LogoutAsync(userId, refreshToken);
            
            _logger.LogInformation("User {UserId} logged out successfully", userId);
            
            return Ok(new { Message = "Logged out successfully" });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred during logout",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Initiates the password reset process.
    /// </summary>
    /// <param name="request">The forgot password request.</param>
    /// <returns>Success response.</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request) {
        try {
            var result = await _authService.ForgotPasswordAsync(request.Email);
            
            if (!result) {
                // Don't reveal whether email exists or not
                return Ok(new { Message = "If the email exists, a password reset link has been sent" });
            }

            _logger.LogInformation("Password reset requested for {Email}", request.Email);
            
            return Ok(new { Message = "If the email exists, a password reset link has been sent" });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error during password reset request for {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred during password reset request",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Resets a user's password.
    /// </summary>
    /// <param name="request">The password reset request.</param>
    /// <returns>Success response.</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request) {
        try {
            var result = await _authService.ResetPasswordAsync(request);
            
            if (!result) {
                return NotFound(new ErrorResponse {
                    Message = "Invalid or expired password reset token",
                    ErrorCode = "INVALID_TOKEN"
                });
            }

            _logger.LogInformation("Password reset for user {UserId}", request.UserId);
            
            return Ok(new { Message = "Password reset successfully" });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error during password reset for user {UserId}", request.UserId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred during password reset",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Verifies a user's email.
    /// </summary>
    /// <param name="request">The email verification request.</param>
    /// <returns>Success response.</returns>
    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request) {
        try {
            var result = await _authService.VerifyEmailAsync(request);
            
            if (!result) {
                return NotFound(new ErrorResponse {
                    Message = "Invalid or expired email verification token",
                    ErrorCode = "INVALID_TOKEN"
                });
            }

            _logger.LogInformation("Email verified for user {UserId}", request.UserId);
            
            return Ok(new { Message = "Email verified successfully" });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error during email verification for user {UserId}", request.UserId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred during email verification",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Confirms email using token from email link.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="token">The confirmation token.</param>
    /// <returns>Success response.</returns>
    [HttpGet("confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmEmail(Guid userId, string token) {
        try {
            var result = await _authService.ConfirmEmailAsync(userId, token);
            
            if (!result) {
                return NotFound(new ErrorResponse {
                    Message = "Invalid or expired email confirmation token",
                    ErrorCode = "INVALID_TOKEN"
                });
            }

            _logger.LogInformation("Email confirmed for user {UserId}", userId);
            
            // Return HTML page for browser
            return Content($@"
                <html>
                <head><title>Email Confirmed</title></head>
                <body>
                    <h1>Email Confirmed</h1>
                    <p>Your email address has been successfully confirmed.</p>
                    <p>You can now <a href="{Request.Scheme}://{Request.Host}/login">login</a> to your account.</p>
                </body>
                </html>", "text/html");
        } catch (Exception ex) {
            _logger.LogError(ex, "Error during email confirmation for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred during email confirmation",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="request">The change password request.</param>
    /// <returns>Success response.</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request) {
        try {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("nameidentifier")?.Value);
            var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            
            if (!result) {
                return BadRequest(new ErrorResponse {
                    Message = "Current password is incorrect",
                    ErrorCode = "INVALID_CURRENT_PASSWORD"
                });
            }

            _logger.LogInformation("Password changed for user {UserId}", userId);
            
            return Ok(new { Message = "Password changed successfully" });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error during password change for user {UserId}", User.FindFirst("sub")?.Value);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred during password change",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Gets the current user's information.
    /// </summary>
    /// <returns>The user response.</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser() {
        try {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("nameidentifier")?.Value);
            var userResponse = await _authService.GetCurrentUserAsync(userId);
            
            return Ok(userResponse);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting current user information");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred while getting user information",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Checks if a username is available.
    /// </summary>
    /// <param name="username">The username to check.</param>
    /// <returns>True if the username is available.</returns>
    [HttpGet("check-username/{username}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckUsernameAvailable(string username) {
        try {
            var isAvailable = !await _authService.UsernameExistsAsync(username);
            return Ok(new { Available = isAvailable });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error checking username availability for {Username}", username);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred while checking username availability",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Checks if an email is available.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <returns>True if the email is available.</returns>
    [HttpGet("check-email/{email}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckEmailAvailable(string email) {
        try {
            var isAvailable = !await _authService.UserExistsAsync(email);
            return Ok(new { Available = isAvailable });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error checking email availability for {Email}", email);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse {
                Message = "An error occurred while checking email availability",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }
}

/// <summary>
/// Refresh Token Request DTO.
/// </summary>
public class RefreshTokenRequest {
    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
