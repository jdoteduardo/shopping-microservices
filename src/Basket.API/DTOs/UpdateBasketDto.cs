namespace Basket.API.DTOs;

public class UpdateBasketDto
{
    public string UserId { get; set; } = string.Empty;
    public List<UpdateBasketItemDto> Items { get; set; } = new();
}

public class UpdateBasketItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
