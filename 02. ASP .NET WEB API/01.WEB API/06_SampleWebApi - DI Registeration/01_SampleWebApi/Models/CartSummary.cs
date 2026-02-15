namespace ShoppingCartAPI.Models
{
    public class CartSummary
    {
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public decimal deliveryFee { get; set; }
    }
}
