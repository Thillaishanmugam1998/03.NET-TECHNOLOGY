namespace ShoppingCartAPI.Services
{
    public class AppConfigService: IAppConfigService
    {
        private readonly decimal taxRate = 0.18m; // GST 18% tax rate

        public decimal GetTaxRate()
        {
            return taxRate;
        }
       

        public decimal GetDeliveryFee(decimal orderAmount)
        {
            if (orderAmount < 500)
            {
                return 5.00m; // Flat delivery fee for orders below 50
            }
            else if (orderAmount >= 500 && orderAmount  <= 2000 )
            {
                return 30.00m; // Free delivery for orders $50 and above
            }
            else
            {
                return 0.00m; // Free delivery for orders above $2000
            }
        }

    }
}
