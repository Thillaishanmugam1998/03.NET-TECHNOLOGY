using _01_SampleWebApi.Models;

namespace _01_SampleWebApi.Services
{
    #region 🔹 What is Service?

    /*
        Service Layer handles BUSINESS LOGIC.

        Controller  → Handles HTTP
        Service     → Handles Business Logic
        Database    → Handles Data Storage

        Why move logic here?

        ✔ Clean architecture
        ✔ Reusability
        ✔ Easy unit testing
        ✔ Controller becomes clean
    */

    #endregion

    public class ProductService : IProductService
    {
        #region 🔹 In-Memory Data (Temporary Database)

        /*
            In real projects:
                This data comes from Database (SQL, EF Core, etc.)

            Here we simulate database using List<Product>
        */

        private readonly List<Product> products = new List<Product>
        {
            new Product { Id = 1, Name = "Laptop", Price = 999.99m },
            new Product { Id = 2, Name = "SmartPhone", Price = 777.7m },
            new Product { Id = 3, Name = "Tablet", Price = 499.5m }
        };

        #endregion

        #region --- 01. Get All Products ---

        public List<Product> GetAll()
        {
            return products;
        }

        #endregion

        #region --- 02. Get Product By Id ---

        public Product? GetById(int id)
        {
            return products.FirstOrDefault(p => p.Id == id);
        }

        #endregion

        #region --- 03. Insert Product ---

        public Product Insert(Product newProduct)
        {
            products.Add(newProduct);
            return newProduct;
        }

        #endregion

        #region --- 04. Update Product ---

        public bool Update(int id, Product updateProduct)
        {
            var product = products.FirstOrDefault(p => p.Id == id);

            if (product == null)
                return false;

            product.Name = updateProduct.Name;
            product.Price = updateProduct.Price;

            return true;
        }

        #endregion

        #region --- 05. Delete Product ---

        public bool Delete(int id)
        {
            var product = products.FirstOrDefault(p => p.Id == id);

            if (product == null)
                return false;

            products.Remove(product);
            return true;
        }

        #endregion

        #region --- 06. Upsert Product (Complex Logic) ---

        /*
            Upsert = Update + Insert

            ✔ If product exists → Update
            ✔ If product not exists → Insert

            We return tuple:

                (IsCreated, Product)

            IsCreated = true  → Insert happened
            IsCreated = false → Update happened
        */

        public (bool IsCreated, Product Product) Upsert(int id, Product newProduct)
        {
            var existingProduct = products.FirstOrDefault(p => p.Id == id);

            if (existingProduct == null)
            {
                products.Add(newProduct);
                return (true, newProduct);
            }
            else
            {
                existingProduct.Name = newProduct.Name;
                existingProduct.Price = newProduct.Price;

                return (false, existingProduct);
            }
        }

        #endregion
    }
}
