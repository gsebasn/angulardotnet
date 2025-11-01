using FluentValidation;

namespace StudyShop.Application.Validators;

public class CreateOrderDtoValidator : AbstractValidator<object>
{
    public CreateOrderDtoValidator()
    {
        // Placeholder for future order DTO validation
        RuleFor(_ => 1).Equal(1);
    }
}


