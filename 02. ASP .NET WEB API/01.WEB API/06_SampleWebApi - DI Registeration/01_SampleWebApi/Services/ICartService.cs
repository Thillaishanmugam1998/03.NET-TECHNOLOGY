using ShoppingCartAPI.Models;

namespace ShoppingCartAPI.Services
{
    public interface ICartService
    {
        void AddItem(CartItem item);
        List<CartItem> GetCartItems();
        void ClearCart();
    }
}
