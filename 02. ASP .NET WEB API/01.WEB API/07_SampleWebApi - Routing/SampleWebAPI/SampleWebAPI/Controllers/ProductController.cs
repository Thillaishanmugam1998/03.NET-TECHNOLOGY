using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SampleWebAPI.Models;
using SampleWebAPI.Services;
using System.Net;
using System.Runtime.InteropServices;

namespace SampleWebAPI.Controllers
{
    /// <summary>
    /// Products API — all endpoints require authentication.
    /// Rate limiter "fixed" (5 req/min) is applied at controller level.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]       // → /api/products
    [Authorize]                        // ← requires valid Basic Auth on every action
    [EnableRateLimiting("fixed")]      // ← 5 requests per minute per client
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        // IProductService is injected by the DI container (registered in Program.cs)
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // ── GET /api/products ────────────────────────────────
        // Both Admin and ApiUser can get all products
        [HttpGet]
        [Authorize(Roles = "Admin,ApiUser")]
        public ActionResult<IEnumerable<Product>> GetAll()
        {
            return Ok(_productService.GetAll());
        }

        // ── GET /api/products/{id} ───────────────────────────
        // Both Admin and ApiUser can get by ID
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,ApiUser")]
        public ActionResult<Product> GetById(int id)
        {
            var product = _productService.GetById(id);
            if (product is null)
                return NotFound(new { message = $"Product with ID {id} was not found." });
            return Ok(product);
        }

        // ── POST /api/products ───────────────────────────────
        // ⛔ ONLY Admin can add a product
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult<Product> Add([FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = _productService.Add(product);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // Example of [AllowAnonymous] — no login needed for this endpoint
        // Useful for health checks, public endpoints
        [HttpGet("health")]
        [AllowAnonymous]   // overrides the class-level [Authorize]
        [DisableRateLimiting]  // overrides the class-level [EnableRateLimiting]
        public ActionResult HealthCheck()
        {
            return Ok(new { status = "API is running", time = DateTime.UtcNow });
        }
    }
}