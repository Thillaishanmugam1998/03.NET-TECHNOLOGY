namespace ShoppingCartAPI.Services
{
    public interface IDiscountService
    {
        decimal CalculateDiscount(decimal amount);
    }
}