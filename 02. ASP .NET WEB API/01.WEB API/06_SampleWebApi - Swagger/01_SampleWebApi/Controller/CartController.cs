using Microsoft.AspNetCore.Mvc;
using ShoppingCartAPI.Models;
using ShoppingCartAPI.Services;

namespace ShoppingCartAPI.Controllers
{
    // Marks this as a Web API controller
    // [ApiController] enables automatic model validation and better request handling
    [ApiController]

    // Base route → all endpoints will be under "api/cart"
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        // Injected dependency for cart operations (Scoped service)
        private readonly ICartService _cartService;

        // Constructor Injection
        // ASP.NET Core automatically resolves ICartService from the DI container
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // POST: api/cart/add
        // Adds an item to the user's cart
        [HttpPost("add")]
        public IActionResult AddItem(CartItem item)
        {
            // Call cart service to add item to the in-memory cache
            _cartService.AddItem(item);

            // Return success response with a message
            return Ok(new { Message = $"{item.ProductName} added to cart." });
        }

        // GET: api/cart/items
        // Returns all items currently in the user's cart
        [HttpGet("items")]
        public IActionResult GetItems()
        {
            // Fetch items from cart service
            return Ok(_cartService.GetItems());
        }

        // GET: api/cart/summary
        // Returns the calculated cart summary (subtotal, discounts, tax, delivery fee, total)
        [HttpGet("summary")]
        public IActionResult GetSummary([FromServices] ICartSummaryService summaryService)
        {
            // Here, CartSummaryService is injected per-request (Scoped)
            // It internally uses:
            //   - ICartService (Scoped, manages cart data)
            //   - IDiscountService (Transient, two different instances for discount calculations)
            //   - IAppConfigService (Singleton, global tax & delivery fee rules)
            var summary = summaryService.GenerateSummary();

            // Return summary object as JSON response
            return Ok(summary);
        }

        // DELETE: api/cart/clear
        // Clears all items from the user's cart
        [HttpDelete("clear")]
        public IActionResult ClearCart()
        {
            // Clear user's cart from cache
            _cartService.ClearCart();

            // Return success response
            return Ok(new { Message = "Cart cleared." });
        }
    }
}