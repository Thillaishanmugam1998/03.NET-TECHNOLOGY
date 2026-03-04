namespace ECommerceApp.Models;

public class OrderStatus : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
