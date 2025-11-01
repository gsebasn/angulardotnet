using FluentValidation;
using StudyShop.Application.Features.Products.Commands;

namespace StudyShop.Api.Features.Products.Commands;

/// <summary>
/// Validator for CreateProductCommand - uses FluentValidation.
/// </summary>
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Product name is required")
            .Length(2, 100).WithMessage("Name must be between 2 and 100 characters");

        RuleFor(c => c.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be >= 0");

        RuleFor(c => c.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock must be >= 0");
    }
}

