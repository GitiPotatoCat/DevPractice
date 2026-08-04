using System.ComponentModel.DataAnnotations.Schema;

namespace MotorsShop.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int MotorcycleId { get; set; }
    public Motorcycle? Motorcycle { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }   // price snapshot at order time
}