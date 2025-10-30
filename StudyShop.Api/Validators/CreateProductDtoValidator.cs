using FluentValidation;
using StudyShop.Api.DTOs;

namespace StudyShop.Api.Validators;

/// <summary>
/// Validates a request to create a new product.
/// Uses FluentValidation library for clean, reusable validation rules.
/// </summary>
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Product name is required")
            .Length(2, 100).WithMessage("Name must be between 2 and 100 characters");

        RuleFor(p => p.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be >= 0");

        RuleFor(p => p.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock must be >= 0");
    }
}

