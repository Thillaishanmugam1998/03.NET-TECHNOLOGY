using SampleWebAPI.Models;

namespace SampleWebAPI.Services
{
    /// <summary>
    /// Contract for product operations.
    /// </summary>
    public interface IProductService
    {
        /// <summary>Returns all products.</summary>
        IEnumerable<Product> GetAll();

        /// <summary>Returns a single product by ID, or null if not found.</summary>
        Product? GetById(int id);

        /// <summary>Adds a new product and returns it with its assigned ID.</summary>
        Product Add(Product product);
    }
}