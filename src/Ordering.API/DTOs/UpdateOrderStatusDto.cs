using Ordering.API.Models;

namespace Ordering.API.DTOs;

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}
