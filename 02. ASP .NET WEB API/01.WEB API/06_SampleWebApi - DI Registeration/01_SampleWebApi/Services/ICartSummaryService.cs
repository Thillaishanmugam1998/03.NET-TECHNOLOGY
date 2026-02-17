using ShoppingCartAPI.Models;
namespace ShoppingCartAPI.Services
{
    public interface ICartSummaryService
    {
        CartSummary GenerateSummary();
    }
}