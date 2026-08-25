using FluentValidation;
using HelljonVibe.Identity.Models.DTOs.Requests;

namespace HelljonVibe.Identity.Api.Validators;

/// <summary>
/// Validator for email verification requests.
/// </summary>
public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest> {
    /// <summary>
    /// Initializes a new instance of the <see cref="VerifyEmailRequestValidator"/> class.
    /// </summary>
    public VerifyEmailRequestValidator() {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required");
    }
}
