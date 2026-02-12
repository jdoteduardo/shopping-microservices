using Ordering.API.DTOs;
using Ordering.API.Models;
using FluentValidation;

namespace Ordering.API.Validators;

public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid order status")
            .Must(status => status != OrderStatus.Pending)
            .WithMessage("Cannot set status back to Pending");
    }
}
