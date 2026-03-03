using SampleWebAPI.Models;

namespace SampleWebAPI.Services
{
    /// <summary>
    /// In-memory implementation of IProductService.
    /// Registered as Singleton so the same list lives for the app's lifetime.
    /// </summary>
    public class ProductService : IProductService
    {
        // In-memory store — seeded with sample data
        private readonly List<Product> _products = new()
        {
            new Product { Id = 1, Name = "Laptop",     Description = "15-inch gaming laptop", Price = 1299.99m, Stock = 10 },
            new Product { Id = 2, Name = "Mouse",      Description = "Wireless ergonomic mouse", Price = 49.99m,   Stock = 50 },
            new Product { Id = 3, Name = "Keyboard",   Description = "Mechanical keyboard",     Price = 89.99m,   Stock = 30 },
        };

        // Auto-increment tracker
        private int _nextId = 4;

        // ── GET ALL ──────────────────────────────────────────────────────────
        /// <summary>Returns all products in the store.</summary>
        public IEnumerable<Product> GetAll() => _products;

        // ── GET BY ID ────────────────────────────────────────────────────────
        /// <summary>
        /// Finds a product by its ID.
        /// Returns null when no product matches — caller should respond with 404.
        /// </summary>
        public Product? GetById(int id) =>
            _products.FirstOrDefault(p => p.Id == id);

        // ── ADD ──────────────────────────────────────────────────────────────
        /// <summary>
        /// Assigns a new ID to the product, stores it, and returns it.
        /// </summary>
        public Product Add(Product product)
        {
            product.Id = _nextId++;
            _products.Add(product);
            return product;
        }
    }
}