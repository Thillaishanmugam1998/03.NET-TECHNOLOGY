namespace ShoppingCartAPI.Services
{
    public class DiscountService : IDiscountService
    {
        public DiscountService(ILogger<DiscountService> logger)
        {
            // Log whenever a new DiscountService instance is created
            // (helps visualize Transient lifetime → multiple per request possible)
            logger.LogInformation("DiscountService (Transient) instance created.");
        }

        public decimal CalculateDiscount(decimal amount)
        {
            // Tier-based discount calculation
            decimal discountPercent = 0;

            if (amount >= 5000 && amount <= 20000)
                discountPercent = 5;   // 5% discount for mid-level orders
            else if (amount > 20000 && amount <= 50000)
                discountPercent = 10;  // 10% discount for higher orders
            else if (amount > 50000)
                discountPercent = 15;  // 15% discount for premium orders

            // Calculate discount amount
            decimal discount = amount * discountPercent / 100;

            return discount;
        }
    }
}