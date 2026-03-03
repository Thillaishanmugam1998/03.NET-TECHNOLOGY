using _01_SampleWebApi.Models;

namespace _01_SampleWebApi.Services
{
    /*
        🔹 What is Interface?

        Interface defines WHAT operations are allowed.
        It does NOT contain implementation.

        Controller talks to Interface,
        not directly to Service class.

        This is called:
            Dependency Inversion Principle (DIP)
    */

    public interface IProductService
    {
        List<Product> GetAll();

        Product? GetById(int id);

        Product Insert(Product newProduct);

        bool Update(int id, Product updateProduct);

        bool Delete(int id);

        (bool IsCreated, Product Product) Upsert(int id, Product newProduct);
    }
}
