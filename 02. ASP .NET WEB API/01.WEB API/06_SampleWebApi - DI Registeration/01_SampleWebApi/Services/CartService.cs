
using Microsoft.Extensions.Caching.Memory;

namespace ShoppingCartAPI.Services
{
    public class CartService: ICartService
    {
        private readonly IMemoryCache _cache; //Used to store cart items in memory
        private readonly string _userId; //Unique identifier for the user from request headers

        public CartService(IMemoryCache cache,IHttpContextAccessor httpContextAccessor,ILogger<CartService> logger )
        {
            // Initialize the memory cache 
            _cache = cache;

            //Get the user ID from the request headers, or use a default value if not provided
            _userId = httpContextAccessor.HttpContext?.Request.Headers["UserId"].FirstOrDefault() ?? "guest";

            //Lof only when the a new instance of CartService is created, which happens once per request due to the scoped lifetime
            logger.LogInformation("CartService instance created for user: {UserId}", _userId);

        }



    }
}
