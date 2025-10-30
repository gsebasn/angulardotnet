using FluentValidation;
using StudyShop.Api.DTOs;

namespace StudyShop.Api.Validators;

/// <summary>
/// Validates a request to create a new order.
/// </summary>
public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(o => o.OrderNumber)
            .NotEmpty().WithMessage("Order number is required")
            .MaximumLength(50).WithMessage("Order number must be <= 50 characters");

        RuleFor(o => o.Items)
            .NotEmpty().WithMessage("Order must have at least one item")
            .Must(items => items != null && items.Count > 0)
            .WithMessage("Order must have at least one item");

        RuleForEach(o => o.Items).SetValidator(new OrderItemDtoValidator());
    }
}

/// <summary>
/// Validates an individual order item.
/// </summary>
public class OrderItemDtoValidator : AbstractValidator<OrderItemDto>
{
    public OrderItemDtoValidator()
    {
        RuleFor(item => item.ProductId)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0");

        RuleFor(item => item.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1");
    }
}

