using Microsoft.AspNetCore.Mvc;
using _01_SampleWebApi.Models;
using _01_SampleWebApi.Services;

namespace _01_SampleWebApi.Controllers
{
    #region 🔹 What is Controller?

    /*
        Controller handles HTTP requests only.

        Controller Responsibilities:
        ✔ Receive HTTP request
        ✔ Validate input
        ✔ Call Service layer
        ✔ Return proper HTTP response

        Controller SHOULD NOT:
        ❌ Contain business logic
        ❌ Access database directly

        Business logic now moved to ProductService.
    */

    #endregion

    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        #region 🔹 Dependency Injection

        /*
            Instead of creating:

                new ProductService(); ❌

            ASP.NET Core automatically injects service
            because we registered it in Program.cs:

                builder.Services.AddScoped<IProductService, ProductService>();

            This is called Constructor Injection.
        */

        #endregion

        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        #region --- 01. GET ALL PRODUCTS ---

        [HttpGet]
        public ActionResult<List<Product>> GetProduct()
        {
            var products = _productService.GetAll();

            if (products == null || products.Count == 0)
                return NotFound("No products available");

            return Ok(products);
        }

        #endregion


        #region --- 02. GET PRODUCT BY ID ---

        [HttpGet("{id}")]
        public ActionResult<Product> GetById(int id)
        {
            var product = _productService.GetById(id);

            if (product == null)
                return NotFound("Product not found");

            return Ok(product);
        }

        #endregion


        #region --- 03. INSERT PRODUCT ---

        [HttpPost]
        public ActionResult<Product> Insert(Product newProduct)
        {
            if (newProduct == null)
                return BadRequest("Product data is null");

            var createdProduct = _productService.Insert(newProduct);

            return CreatedAtAction(nameof(GetById),
                new { id = createdProduct.Id },
                createdProduct);
        }

        #endregion


        #region --- 04. UPDATE PRODUCT ---

        [HttpPut("{id}")]
        public ActionResult Update(int id, Product updateProduct)
        {
            if (id != updateProduct.Id)
                return BadRequest("ID mismatch");

            var isUpdated = _productService.Update(id, updateProduct);

            if (!isUpdated)
                return NotFound("Product not found");

            return Ok("Product updated successfully!");
        }

        #endregion


        #region --- 05. DELETE PRODUCT ---

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var isDeleted = _productService.Delete(id);

            if (!isDeleted)
                return NotFound("Product not found");

            return Ok("Product deleted successfully!");
        }

        #endregion


        #region --- 06. UPSERT PRODUCT (Complex Logic) ---

        /*
            Upsert = Update + Insert

            ✔ If product not exists → Insert
            ✔ If exists → Update
        */

        [HttpPost("Upsert/{id}")]
        public ActionResult Upsert(int id, Product newProduct)
        {
            if (id != newProduct.Id)
                return BadRequest("ID mismatch");

            var result = _productService.Upsert(id, newProduct);

            if (result.IsCreated)
            {
                return CreatedAtAction(nameof(GetById),
                    new { id = result.Product.Id },
                    result.Product);
            }

            return Ok("Product updated successfully!");
        }

        #endregion


        #region --- 07. GET GREETING ---

        [HttpGet("greeting")]
        public IActionResult GetGreeting()
        {
            return Ok("Hello World!");
        }

        #endregion


        #region --- 08. SIMPLE POST ---

        [HttpPost("simple")]
        public IActionResult Post()
        {
            return Ok("Product created successfully!");
        }

        #endregion
    }
}
