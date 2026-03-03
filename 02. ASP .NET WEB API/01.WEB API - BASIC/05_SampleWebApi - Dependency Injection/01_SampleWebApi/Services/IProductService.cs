namespace _01_SampleWebApi.Services
{
    /*
        🔵 INTERFACE (Abstraction)

        Why Interface?

        ✔ Controller should depend on abstraction
        ✔ Not on concrete class
        ✔ This creates LOOSE COUPLING
    */

    public interface IProductService
    {
        List<string> GetProducts();
        void AddProduct(string newProduct);
        void RemoveProduct(string productToRemove);
        void UpdateProduct(string oldProduct, string newProduct);
    }
}
