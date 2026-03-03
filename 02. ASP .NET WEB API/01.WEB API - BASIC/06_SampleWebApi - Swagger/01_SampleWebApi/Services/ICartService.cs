using ShoppingCartAPI.Models;

namespace ShoppingCartAPI.Services
{
    public interface ICartService
    {
        void AddItem(CartItem item);
        List<CartItem> GetItems();
        void ClearCart();
    }
}
