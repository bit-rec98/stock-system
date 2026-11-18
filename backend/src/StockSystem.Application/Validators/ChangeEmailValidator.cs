using FluentValidation;
using StockSystem.Application.DTOs.Auth;

namespace StockSystem.Application.Validators;

public class ChangeEmailValidator : AbstractValidator<ChangeEmailDto>
{
    public ChangeEmailValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("New email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");
    }
}
