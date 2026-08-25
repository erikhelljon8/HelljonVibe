using FluentValidation;
using HelljonVibe.Identity.Models.DTOs.Requests;

namespace HelljonVibe.Identity.Api.Validators;

/// <summary>
/// Validator for forgot password requests.
/// </summary>
public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest> {
    /// <summary>
    /// Initializes a new instance of the <see cref="ForgotPasswordRequestValidator"/> class.
    /// </summary>
    public ForgotPasswordRequestValidator() {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email must be less than 256 characters");
    }
}
