namespace ECommerceApp.Models;

public class Order : BaseAuditableEntity
{
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int OrderStatusId { get; set; }
    public OrderStatus OrderStatus { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
