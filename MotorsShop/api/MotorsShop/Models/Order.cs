namespace MotorsShop.Models;

public enum OrderStatus { Pending, Paid, Shipped, Delivered, Cancelled }

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public List<OrderItem> Items { get; set; } = new();

    // Computed, not stored
    public decimal Total => Items.Sum(i => i.UnitPrice * i.Quantity);
}