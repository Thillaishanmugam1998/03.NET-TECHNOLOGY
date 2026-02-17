
using Microsoft.Extensions.Caching.Memory;
using ShoppingCartAPI.Models;

namespace ShoppingCartAPI.Services
{
    public class CartService : ICartService
    {
        private readonly IMemoryCache _cache;  // Used to store carts in memory
        private readonly string _userId;       // Unique identifier for the user (from request header)
        
        // Constructor: runs once per HTTP request (because CartService is registered as Scoped)
        public CartService(IMemoryCache cache, IHttpContextAccessor httpContextAccessor, ILogger<CartService> logger)
        {
            // Initialize Memory Cache
            _cache = cache;
            // Get UserId from request header if available, otherwise fallback to "guest"
            _userId = httpContextAccessor.HttpContext?.Request.Headers["UserId"].FirstOrDefault() ?? "guest";
            // Log only when a new instance of CartService is created (helps visualize Scoped lifetime)
            logger.LogInformation("CartService (Scoped) instance created for user {UserId}", _userId);
        }
        
        // Add or update a cart item for the current user
        public void AddItem(CartItem item)
        {
            // Try to get cart from memory; if not found, create a new one with 30-minute sliding expiration
            var cart = _cache.GetOrCreate(_userId, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30); // Reset expiration timer when accessed
                return new List<CartItem>(); // Start with empty cart for this user
            })!; // The "!" tells compiler: "this will never be null"
            
            // Check if product already exists in the user's cart
            var existing = cart.FirstOrDefault(x => x.ProductId == item.ProductId);
            
            if (existing != null)
                existing.Quantity += item.Quantity; // If already exists, increase quantity
            else
                cart.Add(item); // Otherwise, add as a new product
            
            // Save the updated cart back into memory cache for this user
            _cache.Set(_userId, cart);
        }
        // Get all items from the user's cart
        public List<CartItem> GetItems()
        {
            // Try to retrieve cart from cache
            if (_cache.TryGetValue(_userId, out List<CartItem>? cart))
            {
                // If found, return cart (safe-checked with ?? to avoid null warning)
                return cart ?? new List<CartItem>();
            }
            // If cart not found for this user, return empty list
            return new List<CartItem>();
        }
        // Clear all items in the user's cart
        public void ClearCart()
        {
            // Simply remove the entry from memory cache
            _cache.Remove(_userId);
        }
    }
}

