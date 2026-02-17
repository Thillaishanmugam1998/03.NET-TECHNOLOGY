using ShoppingCartAPI.Models;
namespace ShoppingCartAPI.Services
{
    // Service responsible for generating the Cart Summary
    public class CartSummaryService : ICartSummaryService
    {
        // Dependencies injected via constructor
        private readonly ICartService _cartService;       // To fetch items already added to the cart
        private readonly IDiscountService _discount1;     // First transient discount service
        private readonly IDiscountService _discount2;     // Second transient discount service (for demo showing new instance)
        private readonly IAppConfigService _config;       // For global configuration (tax rate, delivery fee)

        // Constructor injection - services are provided by DI container
        public CartSummaryService(
            ICartService cartService,
            IDiscountService discount1,
            IDiscountService discount2,
            IAppConfigService config)
        {
            _cartService = cartService;
            _discount1 = discount1;
            _discount2 = discount2;
            _config = config;
        }

        // Main method to calculate and return cart summary
        public CartSummary GenerateSummary()
        {
            // Fetch all cart items from the cart service
            var items = _cartService.GetItems();

            // Calculate subtotal = sum of price * quantity for all items
            decimal subTotal = items.Sum(i => i.Price * i.Quantity);

            // Apply discounts using two transient instances
            // (each transient service will likely give different results,
            // to demonstrate lifetime differences)
            decimal discount1 = _discount1.CalculateDiscount(subTotal);
            decimal discount2 = _discount2.CalculateDiscount(subTotal);

            // Calculate tax using the Singleton AppConfigService
            decimal tax = subTotal * _config.GetTaxRate();

            // Calculate delivery fee using dynamic business rules
            decimal delivery = _config.GetDeliveryFee(subTotal);

            // Build and return the summary object
            return new CartSummary
            {
                SubTotal = subTotal,                              // Original total before tax/discounts
                Discount = (discount1 + discount2) / 2,           // Average of both discounts (demo purpose)
                Tax = tax,                                        // Calculated tax
                DeliveryFee = delivery,                           // Delivery fee based on subtotal
                Total = subTotal - ((discount1 + discount2) / 2)  // Net total = subtotal - discount + tax + delivery
                        + tax
                        + delivery
            };
        }
    }
}