namespace ShoppingCartAPI.Services
{
    public interface IAppConfigService
    {
        decimal GetTaxRate();
        decimal GetDeliveryFee(decimal orderAmount);
    }
}
