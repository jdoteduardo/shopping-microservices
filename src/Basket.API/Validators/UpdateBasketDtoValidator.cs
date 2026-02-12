using Basket.API.DTOs;
using FluentValidation;

namespace Basket.API.Validators;

public class UpdateBasketDtoValidator : AbstractValidator<UpdateBasketDto>
{
    public UpdateBasketDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required")
            .MaximumLength(50).WithMessage("UserId cannot exceed 50 characters");

        RuleFor(x => x.Items)
            .NotNull().WithMessage("Items cannot be null");

        RuleForEach(x => x.Items).SetValidator(new UpdateBasketItemDtoValidator());
    }
}

public class UpdateBasketItemDtoValidator : AbstractValidator<UpdateBasketItemDto>
{
    public UpdateBasketItemDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("ProductId must be greater than 0");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("ProductName is required")
            .MaximumLength(200).WithMessage("ProductName cannot exceed 200 characters");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100 items");
    }
}
