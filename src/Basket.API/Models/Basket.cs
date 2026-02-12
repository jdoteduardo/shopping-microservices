namespace Basket.API.Models;

public class Basket
{
    public string UserId { get; set; } = string.Empty;
    public List<BasketItem> Items { get; set; } = new();
    
    public decimal TotalPrice => Items.Sum(item => item.Subtotal);
}
