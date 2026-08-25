using FluentValidation;
using HelljonVibe.Identity.Api.Configuration;
using HelljonVibe.Identity.Api.Models;
using Microsoft.Extensions.Options;

namespace HelljonVibe.Identity.Api.Validators;

/// <summary>
/// Validator for change password requests.
/// </summary>
public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest> {
    private readonly AppSettings _appSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordRequestValidator"/> class.
    /// </summary>
    /// <param name="appSettings">The application settings.</param>
    public ChangePasswordRequestValidator(IOptions<AppSettings> appSettings) {
        _appSettings = appSettings?.Value ?? throw new ArgumentNullException(nameof(appSettings));

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(_appSettings.PasswordMinLength)
                .WithMessage($"Password must be at least {_appSettings.PasswordMinLength} characters")
            .MaximumLength(_appSettings.PasswordMaxLength)
                .WithMessage($"Password must be less than {_appSettings.PasswordMaxLength} characters")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password")
            .Must(ContainUppercase).WithMessage("Password must contain at least one uppercase letter")
            .Must(ContainLowercase).WithMessage("Password must contain at least one lowercase letter")
            .Must(ContainDigit).WithMessage("Password must contain at least one digit")
            .Must(ContainSpecialChar).WithMessage("Password must contain at least one special character");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Password confirmation is required")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }

    /// <summary>
    /// Validates that the password contains at least one uppercase letter.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>True if the password contains an uppercase letter; otherwise, false.</returns>
    private static bool ContainUppercase(string password) {
        return !string.IsNullOrEmpty(password) && password.Any(char.IsUpper);
    }

    /// <summary>
    /// Validates that the password contains at least one lowercase letter.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>True if the password contains a lowercase letter; otherwise, false.</returns>
    private static bool ContainLowercase(string password) {
        return !string.IsNullOrEmpty(password) && password.Any(char.IsLower);
    }

    /// <summary>
    /// Validates that the password contains at least one digit.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>True if the password contains a digit; otherwise, false.</returns>
    private static bool ContainDigit(string password) {
        return !string.IsNullOrEmpty(password) && password.Any(char.IsDigit);
    }

    /// <summary>
    /// Validates that the password contains at least one special character.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>True if the password contains a special character; otherwise, false.</returns>
    private static bool ContainSpecialChar(string password) {
        if (string.IsNullOrEmpty(password)) {
            return false;
        }
        return password.Any(c => !char.IsLetterOrDigit(c));
    }
}
