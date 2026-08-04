namespace MotorsShop.Models;

public class Brand
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public List<Motorcycle> Motorcycles { get; set; } = new();
}