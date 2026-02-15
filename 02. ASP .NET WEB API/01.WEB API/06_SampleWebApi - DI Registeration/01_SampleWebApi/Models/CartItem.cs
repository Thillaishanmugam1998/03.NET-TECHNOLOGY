namespace ShoppingCartAPI.Models
{
    public class CartItem
    {
        public int productId { get; set; }
        public string productName { get; set; } = string.Empty;
        public decimal price { get; set; }
        public int quantity { get; set; }

    }
}
