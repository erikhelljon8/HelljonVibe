using FluentValidation;
using HelljonVibe.Identity.Api.Configuration;
using HelljonVibe.Identity.Models.DTOs.Requests;
using Microsoft.Extensions.Options;

namespace HelljonVibe.Identity.Api.Validators;

/// <summary>
/// Validator for registration requests.
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest> {
    private readonly AppSettings _appSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterRequestValidator"/> class.
    /// </summary>
    /// <param name="appSettings">The application settings.</param>
    public RegisterRequestValidator(IOptions<AppSettings> appSettings) {
        _appSettings = appSettings?.Value ?? throw new ArgumentNullException(nameof(appSettings));

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(100).WithMessage("Username must be less than 100 characters")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email must be less than 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(_appSettings.PasswordMinLength).WithMessage($