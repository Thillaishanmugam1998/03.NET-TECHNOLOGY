using Microsoft.AspNetCore.Mvc;
using _01_SampleWebApi.Services;

namespace _01_SampleWebApi.Controllers
{
    /*
        =====================================================
        🔵 WITH DEPENDENCY INJECTION (LOOSE COUPLING)
        =====================================================

        What is happening here?

        ✔ Controller does NOT create ProductService.
        ✔ Controller asks for IProductService.
        ✔ ASP.NET Core injects ProductService automatically.
        ✔ This is Constructor Injection.
    */

    [ApiController]
    [Route("api/[controller]")]
    public class DIProductController : Controller
    {
        /*
            🔵 DEPENDENCY DECLARATION

            Controller depends on abstraction (Interface),
            NOT on concrete class.

            This is LOOSE COUPLING.
        */
        private readonly IProductService _productService;

        /*
            🔵 CONSTRUCTOR INJECTION

            ASP.NET Core automatically injects ProductService
            because we registered it in Program.cs.

            NO "new ProductService()" here ❌
        */
        public DIProductController(IProductService productService)
        {
            _productService = productService;
        }

        /*
            🔵 Using injected service
        */
        [HttpGet("GetProducts")]
        public IActionResult GetProducts()
        {
            var products = _productService.GetProducts();
            return Ok(products);
        }
    }
}
