using FluentValidation;
using StockSystem.Application.DTOs.User;

namespace StockSystem.Application.Validators;

public class UserFilterValidator : AbstractValidator<UserFilterParams>
{
    public UserFilterValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters");
    }
}
