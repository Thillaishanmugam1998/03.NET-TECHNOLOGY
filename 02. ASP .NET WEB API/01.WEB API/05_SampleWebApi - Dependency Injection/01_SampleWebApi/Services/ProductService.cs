namespace _01_SampleWebApi.Services
{
    /*
        🔹 This is a Service Class.
        🔹 It contains business logic (Product operations).
        🔹 In real-time applications, data usually comes from:
            - Database
            - External API
            - File system
    */

    public class ProductService: IProductService
    {
        // 🔹 This is in-memory data (Hardcoded list)
        public List<string> product = new List<string>()
        {
            "Laptop",
            "Mobile",
            "Tablet"
        };

        // 🔹 Returns all products
        public List<string> GetProducts()
        {
            return product;
        }

        // 🔹 Adds a new product
        public void AddProduct(string newProduct)
        {
            product.Add(newProduct);
        }

        // 🔹 Removes a product
        public void RemoveProduct(string productToRemove)
        {
            product.Remove(productToRemove);
        }

        // 🔹 Updates an existing product
        public void UpdateProduct(string oldProduct, string newProduct)
        {
            int index = product.IndexOf(oldProduct);

            if (index != -1)
            {
                product[index] = newProduct;
            }
        }
    }
}
