using FluentValidation;
using HelljonVibe.Identity.Api.Models;

namespace HelljonVibe.Identity.Api.Validators;

/// <summary>
/// Validator for update user requests.
/// </summary>
public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest> {
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserRequestValidator"/> class.
    /// </summary>
    public UpdateUserRequestValidator() {
        RuleFor(x => x.Username)
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(100).WithMessage("Username must be less than 100 characters")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores")
            .When(x => !string.IsNullOrEmpty(x.Username));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email must be less than 256 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.AccessFailedCount)
            .GreaterThanOrEqualTo(0).WithMessage("Access failed count must be greater than or equal to 0")
            .When(x => x.AccessFailedCount.HasValue);
    }
}
