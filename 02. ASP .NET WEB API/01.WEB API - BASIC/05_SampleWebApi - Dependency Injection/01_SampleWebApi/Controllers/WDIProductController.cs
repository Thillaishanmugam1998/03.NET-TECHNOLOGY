using Microsoft.AspNetCore.Mvc;
using _01_SampleWebApi.Services;

namespace _01_SampleWebApi.Controllers
{
    /*
        ==========================================================
        🔴 THIS CONTROLLER IS WITHOUT DEPENDENCY INJECTION
        ==========================================================

        ❓ What is Dependency Injection (DI)?

        Dependency Injection means:
        👉 A class should NOT create its own dependencies.
        👉 Dependencies should be PROVIDED from outside.

        In this example:
        - Controller depends on ProductService.
        - But Controller itself is creating ProductService.
        - This is called TIGHT COUPLING.
    */

    [ApiController]
    [Route("api/[controller]")]
    public class WDIProductController : Controller
    {
        // 🔴 DEPENDENCY
        // Controller depends on ProductService
        WDIProductService productService;

        public WDIProductController()
        {
            /*
                🔴 THIS IS THE PROBLEM LINE

                Here controller is CREATING the dependency:

                    productService = new ProductService();

                This means:

                ❌ Controller is tightly coupled with ProductService.
                ❌ Controller directly depends on concrete class.
                ❌ If ProductService changes, Controller must change.
                ❌ Hard to test (cannot mock ProductService).
                ❌ Cannot easily replace with another service (like DbProductService).

                This is called:
                🔴 TIGHT COUPLING
            */
            productService = new WDIProductService();
        }

        [HttpGet("GetProduct")]
        public IActionResult GetProduct()
        {
            return Ok(productService.GetProducts());
        }
    }
}
