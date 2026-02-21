// =============================================================================
// File Name: ReturnTypesAndStatusCodes.cs
// Topic    : ASP.NET Core Web API – Return Types and HTTP Status Codes
// Author   : Senior .NET Architect (16+ Years Experience)
// Purpose  : Complete, production-level reference covering ALL action return types
//            and ALL major HTTP status codes with real-world examples.
//
// This ONE file teaches:
//   ✔ IActionResult vs ActionResult<T> vs Specific Types
//   ✔ HTTP 200, 201, 202, 204
//   ✔ HTTP 301, 302, 304
//   ✔ HTTP 400, 401, 403, 404, 405
//   ✔ HTTP 500, 501, 503, 504
//   ✔ Global HTTP Method configuration
//   ✔ Production-level patterns and best practices
// =============================================================================

#region Namespace

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

#endregion

// =============================================================================
#region Models
// =============================================================================
// These are the data contracts (DTOs) used across all API examples.
// Think of them as the "forms" your API accepts and returns.
// =============================================================================

namespace ReturnTypesAndStatusCodes.Models
{
    // -------------------------------------------------------------------------
    // Product Model — used in GET / POST / PUT / DELETE examples
    // -------------------------------------------------------------------------
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be 2-100 characters")]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999999.99")]
        public decimal Price { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // -------------------------------------------------------------------------
    // Order Model — used for async order processing (202 Accepted)
    // -------------------------------------------------------------------------
    public class Order
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed
        public string TrackingUrl { get; set; } = string.Empty;
    }

    // -------------------------------------------------------------------------
    // CreateProductRequest — DTO for POST request body
    // -------------------------------------------------------------------------
    public class CreateProductRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;
    }

    // -------------------------------------------------------------------------
    // PlaceOrderRequest — DTO for order placement
    // -------------------------------------------------------------------------
    public class PlaceOrderRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; }

        [Required]
        public string CustomerEmail { get; set; } = string.Empty;
    }

    // -------------------------------------------------------------------------
    // ApiResponse<T> — Standard envelope for all API responses
    // In real production, ALL responses should be wrapped in a standard envelope.
    // This gives clients a consistent shape to parse.
    // -------------------------------------------------------------------------
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();

        // Static factory methods for creating standard responses
        public static ApiResponse<T> Ok(T data, string message = "Success")
            => new() { Success = true, Message = message, Data = data };

        public static ApiResponse<T> Fail(string message, List<string>? errors = null)
            => new() { Success = false, Message = message, Errors = errors ?? new() };
    }

    // -------------------------------------------------------------------------
    // ProblemDetails-style error — for RFC 7807 standard error responses
    // ASP.NET Core has built-in ProblemDetails support, this shows custom extension.
    // -------------------------------------------------------------------------
    public class ApiProblemDetails
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public string Detail { get; set; } = string.Empty;
        public string Instance { get; set; } = string.Empty;
        public Dictionary<string, object> Extensions { get; set; } = new();
    }
}

#endregion

// =============================================================================
#region Interfaces
// =============================================================================
// Interfaces define the contract for service classes.
// Using interfaces enables Dependency Injection and unit testing.
// =============================================================================

namespace ReturnTypesAndStatusCodes.Interfaces
{
    using ReturnTypesAndStatusCodes.Models;

    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<Product> CreateAsync(CreateProductRequest request);
        Task<bool> UpdateAsync(int id, Product product);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> IsServiceAvailableAsync(); // used for 503 demo
    }

    public interface IOrderService
    {
        Task<Order> PlaceOrderAsync(PlaceOrderRequest request);
        Task<Order?> GetOrderStatusAsync(int orderId);
    }
}

#endregion

// =============================================================================
#region Services
// =============================================================================
// Service layer contains the business logic.
// Controllers call services; services never talk back to controllers.
// This is the standard N-Tier architecture in real enterprise projects.
// =============================================================================

namespace ReturnTypesAndStatusCodes.Services
{
    using ReturnTypesAndStatusCodes.Interfaces;
    using ReturnTypesAndStatusCodes.Models;

    // -------------------------------------------------------------------------
    // ProductService — Simulates a real product repository with in-memory data
    // In production, this would use Entity Framework Core + SQL Server / Cosmos DB
    // -------------------------------------------------------------------------
    public class ProductService : IProductService
    {
        // Simulated in-memory database (in production: EF Core DbContext)
        private static readonly List<Product> _products = new()
        {
            new Product { Id = 1, Name = "Laptop Pro",    Price = 1299.99m, Category = "Electronics" },
            new Product { Id = 2, Name = "Wireless Mouse",Price = 29.99m,   Category = "Accessories" },
            new Product { Id = 3, Name = "Mechanical Keyboard", Price = 149.99m, Category = "Accessories" }
        };

        private static int _nextId = 4;

        // Flag to simulate service outage (for 503 demo)
        private static bool _simulateOutage = false;

        public Task<IEnumerable<Product>> GetAllAsync()
            => Task.FromResult<IEnumerable<Product>>(_products.Where(p => p.IsAvailable));

        public Task<Product?> GetByIdAsync(int id)
            => Task.FromResult(_products.FirstOrDefault(p => p.Id == id));

        public Task<Product> CreateAsync(CreateProductRequest request)
        {
            var product = new Product
            {
                Id        = _nextId++,
                Name      = request.Name,
                Price     = request.Price,
                Category  = request.Category,
                CreatedAt = DateTime.UtcNow
            };
            _products.Add(product);
            return Task.FromResult(product);
        }

        public Task<bool> UpdateAsync(int id, Product updated)
        {
            var existing = _products.FirstOrDefault(p => p.Id == id);
            if (existing == null) return Task.FromResult(false);

            existing.Name      = updated.Name;
            existing.Price     = updated.Price;
            existing.Category  = updated.Category;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null) return Task.FromResult(false);

            // Soft delete — mark as unavailable instead of removing
            // In production, you almost never hard-delete records!
            product.IsAvailable = false;
            return Task.FromResult(true);
        }

        public Task<bool> ExistsAsync(int id)
            => Task.FromResult(_products.Any(p => p.Id == id));

        public Task<bool> IsServiceAvailableAsync()
            => Task.FromResult(!_simulateOutage);

        // Method to toggle outage simulation for demo purposes
        public static void SimulateOutage(bool outage) => _simulateOutage = outage;
    }

    // -------------------------------------------------------------------------
    // OrderService — Simulates async order processing (used for 202 Accepted)
    // In production, this would push to Azure Service Bus / RabbitMQ queue
    // -------------------------------------------------------------------------
    public class OrderService : IOrderService
    {
        private static readonly List<Order> _orders = new();
        private static int _nextOrderId = 1;

        public Task<Order> PlaceOrderAsync(PlaceOrderRequest request)
        {
            // Simulates queuing the order for background processing
            // Real world: publish to message queue, return immediately
            var order = new Order
            {
                Id         = _nextOrderId++,
                ProductId  = request.ProductId,
                Quantity   = request.Quantity,
                Status     = "Pending",
                TrackingUrl = $"/api/orders/{_nextOrderId - 1}/status"
            };
            _orders.Add(order);

            // Simulate background processing (in real apps: Worker Service / Azure Function)
            _ = Task.Run(async () =>
            {
                await Task.Delay(5000); // Simulate 5s processing
                order.Status = "Completed";
            });

            return Task.FromResult(order);
        }

        public Task<Order?> GetOrderStatusAsync(int orderId)
            => Task.FromResult(_orders.FirstOrDefault(o => o.Id == orderId));
    }
}

#endregion

// =============================================================================
#region Controllers
// =============================================================================

namespace ReturnTypesAndStatusCodes.Controllers
{
    using ReturnTypesAndStatusCodes.Interfaces;
    using ReturnTypesAndStatusCodes.Models;

    // =========================================================================
    // SECTION 1: ACTION RETURN TYPES IN ASP.NET CORE WEB API
    // =========================================================================
    //
    // ASP.NET Core offers 3 main return type patterns:
    //
    // Pattern A: Specific Type (e.g., Product, List<Product>)
    //   → Simple, but NO control over HTTP status codes
    //   → Use for: Swagger simplicity in read-only endpoints
    //
    // Pattern B: IActionResult
    //   → Full control over status codes
    //   → Swagger cannot infer the return type automatically
    //
    // Pattern C: ActionResult<T>  ← INDUSTRY STANDARD ✔
    //   → Best of both worlds: type safety + status code control
    //   → Swagger can infer type AND you can return typed status codes
    //
    // =========================================================================

    /// <summary>
    /// Demonstrates all three return type patterns.
    /// In production, always use ActionResult<T> for Web API controllers.
    /// </summary>
    [ApiController]
    [Route("api/return-types")]
    public class ReturnTypePatternController : ControllerBase
    {
        // -----------------------------------------------------------------
        // PATTERN A: Specific Type Return
        // Swagger sees the type. You cannot return 404 or 400 cleanly.
        // Use case: Internal-only APIs or very simple read endpoints.
        // -----------------------------------------------------------------
        [HttpGet("pattern-a/products")]
        public IEnumerable<Product> GetAllProductsSpecificType()
        {
            // Works fine for happy path, but what if the list is null?
            // You can't return a 404 here — you're forced to return the type.
            return new List<Product>
            {
                new Product { Id = 1, Name = "Laptop", Price = 999m, Category = "Electronics" }
            };
        }

        // -----------------------------------------------------------------
        // PATTERN B: IActionResult
        // Full status code control, but Swagger won't know the type.
        // Workaround: Use [ProducesResponseType] attributes.
        // -----------------------------------------------------------------
        [HttpGet("pattern-b/products/{id}")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetProductIActionResult(int id)
        {
            if (id <= 0)
                return NotFound(); // Swagger knows this returns 404

            var product = new Product { Id = id, Name = "Sample Product", Price = 99m, Category = "Test" };
            return Ok(product); // Swagger knows this returns 200 with Product
        }

        // -----------------------------------------------------------------
        // PATTERN C: ActionResult<T>  ← RECOMMENDED IN PRODUCTION
        // Combines type safety + status code control.
        // Swagger automatically sees both the type AND possible status codes.
        // -----------------------------------------------------------------
        [HttpGet("pattern-c/products/{id}")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Product> GetProductActionResultT(int id)
        {
            if (id <= 0)
                return NotFound(); // Clean 404

            var product = new Product { Id = id, Name = "Sample Product", Price = 99m, Category = "Test" };
            return product; // Implicit 200 OK — no need to wrap in Ok()
            // OR: return Ok(product); — explicit, both work the same
        }
    }

    // =========================================================================
    // SECTION 2: HTTP 2xx SUCCESS STATUS CODES
    // =========================================================================

    /// <summary>
    /// MAIN PRODUCT CONTROLLER
    /// Demonstrates HTTP 200, 201, 202, 204 with full production patterns.
    ///
    /// REAL-WORLD ANALOGY: Think of this as the Product Management section
    /// of an e-commerce platform like Amazon or Flipkart.
    /// </summary>
    [ApiController]
    [Route("api/products")]
    [Produces("application/json")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        // Constructor injection — always use DI, never instantiate services directly
        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // -----------------------------------------------------------------
        // HTTP 200 OK — Standard Success Response
        //
        // When to use: Data was found and returned successfully.
        // Real analogy: ATM shows your balance — you asked, it delivered. ✔
        // -----------------------------------------------------------------

        /// <summary>
        /// HTTP 200 OK: Returns a list of all available products.
        /// This is the most common status code — "everything went fine."
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Product>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<Product>>>> GetAllProducts()
        {
            _logger.LogInformation("Fetching all products");

            var products = await _productService.GetAllAsync();

            // Always wrap in a standard envelope in production
            return Ok(ApiResponse<IEnumerable<Product>>.Ok(products, "Products retrieved successfully"));
            // HTTP 200 OK is returned automatically by Ok()
        }

        /// <summary>
        /// HTTP 200 OK: Returns a single product by ID.
        /// Covers both 200 (found) and 404 (not found).
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<Product>>> GetProductById(int id)
        {
            // 400 Bad Request — invalid input (see Section 4 for full 400 coverage)
            if (id <= 0)
            {
                return BadRequest(new ApiProblemDetails
                {
                    Title  = "Invalid ID",
                    Detail = "Product ID must be a positive integer",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var product = await _productService.GetByIdAsync(id);

            // 404 Not Found — product doesn't exist (see Section 4)
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                return NotFound(new ApiProblemDetails
                {
                    Title    = "Product Not Found",
                    Detail   = $"No product found with ID {id}",
                    Status   = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            }

            // 200 OK — product found
            return Ok(ApiResponse<Product>.Ok(product));
        }

        // -----------------------------------------------------------------
        // HTTP 201 Created — Resource Created Successfully
        //
        // When to use: POST request that creates a new resource.
        // MANDATORY: Include Location header pointing to the new resource.
        //
        // Real analogy: You fill a form at the bank to open a new account.
        // The bank confirms: "Account #12345 created." and gives you the ID.
        // -----------------------------------------------------------------

        /// <summary>
        /// HTTP 201 Created: Creates a new product.
        ///
        /// KEY RULE: 201 MUST include a Location header.
        /// Use CreatedAtAction() or CreatedAtRoute() — NOT Created() alone.
        /// CreatedAtAction() automatically sets: Location: /api/products/{newId}
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Product>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<Product>>> CreateProduct(
            [FromBody] CreateProductRequest request)
        {
            // ASP.NET Core automatically validates [Required], [Range] etc.
            // and returns 400 if ModelState is invalid (when [ApiController] is present).
            // No need to check ModelState manually — it's handled by the framework.

            _logger.LogInformation("Creating new product: {ProductName}", request.Name);

            var createdProduct = await _productService.CreateAsync(request);

            var response = ApiResponse<Product>.Ok(createdProduct, "Product created successfully");

            // CreatedAtAction() does TWO things:
            // 1. Sets HTTP status to 201 Created
            // 2. Sets Location header: /api/products/{id}
            return CreatedAtAction(
                actionName: nameof(GetProductById),     // Points to GET /api/products/{id}
                routeValues: new { id = createdProduct.Id }, // Route parameter value
                value: response                         // Response body
            );

            // Alternatively, use CreatedAtRoute() if you use named routes:
            // return CreatedAtRoute("GetProductById", new { id = createdProduct.Id }, response);
        }

        // -----------------------------------------------------------------
        // HTTP 202 Accepted — Request Accepted, Processing Asynchronously
        //
        // When to use: The request is valid but processing will take time.
        // The server queues it and responds immediately — client polls later.
        //
        // Real analogy: You place an online order. The website says
        // "Order Accepted! Track your order here: /orders/12345/status"
        // Your product hasn't shipped yet, but the request is accepted.
        // -----------------------------------------------------------------

        /// <summary>
        /// HTTP 202 Accepted: Simulates queuing a bulk import task.
        ///
        /// Use cases in production:
        /// - Bulk CSV import of 100,000 products
        /// - Sending mass email campaigns
        /// - Report generation
        /// - Video encoding jobs
        ///
        /// Always return a tracking URL so the client can check progress!
        /// </summary>
        [HttpPost("bulk-import")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkImportProducts(
            [FromBody] List<CreateProductRequest> requests)
        {
            if (!requests.Any())
                return BadRequest("Product list cannot be empty");

            // In production: push to Azure Service Bus / RabbitMQ / Hangfire
            // Here we simulate by accepting and returning a job ID
            var jobId = Guid.NewGuid().ToString("N")[..8].ToUpper();

            _logger.LogInformation("Bulk import job {JobId} accepted for {Count} products",
                jobId, requests.Count);

            // 202 Accepted — processing will happen asynchronously
            return Accepted(
                uri: $"/api/jobs/{jobId}/status",    // Location for polling
                value: new
                {
                    JobId      = jobId,
                    Message    = "Bulk import accepted and queued for processing",
                    StatusUrl  = $"/api/jobs/{jobId}/status",
                    EstimatedCompletionMinutes = Math.Ceiling(requests.Count / 100.0)
                }
            );
        }

        // -----------------------------------------------------------------
        // HTTP 204 No Content — Success with No Response Body
        //
        // When to use: Operation succeeded, but there is NOTHING to return.
        // Typically used for: PUT (update), DELETE, or PATCH operations.
        //
        // IMPORTANT RULE: 204 means "success but I have nothing to say."
        // Do NOT send a body with 204 — it violates HTTP spec.
        //
        // Real analogy: You update your password in the bank app.
        // The app shows a spinner and then says "Done." — no data returned.
        // -----------------------------------------------------------------

        /// <summary>
        /// HTTP 204 No Content: Updates a product.
        /// PUT returns 204 because the client already has the updated data
        /// (they sent it), so no need to return it again.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            // 400 — ID mismatch
            if (id != product.Id)
                return BadRequest("ID in URL does not match ID in body");

            var exists = await _productService.ExistsAsync(id);

            // 404 — product not found
            if (!exists)
                return NotFound($"Product with ID {id} not found");

            await _productService.UpdateAsync(id, product);

            // 204 No Content — success, nothing to return
            return NoContent();
        }

        /// <summary>
        /// HTTP 204 No Content: Deletes a product.
        /// After deletion, there's nothing to return, so 204 is perfect.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var deleted = await _productService.DeleteAsync(id);

            if (!deleted)
                return NotFound($"Product with ID {id} not found");

            // 204 — product deleted, no body needed
            return NoContent();
        }
    }

    // =========================================================================
    // SECTION 3: HTTP 3xx REDIRECTION STATUS CODES
    // =========================================================================

    /// <summary>
    /// Demonstrates HTTP 301, 302, and 304 in Web API context.
    ///
    /// Note: Redirects are MORE common in MVC (HTML pages) than in pure APIs.
    /// But they are used in APIs for: URL versioning, canonical URLs, CDN caching.
    /// </summary>
    [ApiController]
    [Route("api")]
    public class RedirectController : ControllerBase
    {
        private readonly IMemoryCache _cache;

        public RedirectController(IMemoryCache cache)
        {
            _cache = cache;
        }

        // -----------------------------------------------------------------
        // HTTP 301 Moved Permanently
        //
        // When to use: The resource has permanently moved to a new URL.
        // Browsers/clients cache this redirect forever.
        //
        // Real analogy: A bank branch closed and permanently moved to a
        // new address. The sign on the old door says "We moved to 123 Main St."
        //
        // API use case: Old API version permanently retired, redirecting to v2.
        // -----------------------------------------------------------------

        /// <summary>
        /// HTTP 301 Moved Permanently:
        /// Old API endpoint redirects permanently to new versioned endpoint.
        /// SEO engines will update their index. Clients should update their bookmarks.
        /// </summary>
        [HttpGet("products-old")]                         // Old endpoint (deprecated)
        [ProducesResponseType(StatusCodes.Status301MovedPermanently)]
        public IActionResult OldProductEndpoint()
        {
            // Permanently redirect to the new endpoint
            // Location header will be set to: /api/v2/products
            return RedirectPermanent("/api/v2/products");

            // Alternatively: return RedirectToActionPermanent("GetAllProducts", "Product");
        }

        // -----------------------------------------------------------------
        // HTTP 302 Found (Temporary Redirect)
        //
        // When to use: The resource is TEMPORARILY at a different URL.
        // Clients should NOT cache this — they must check again next time.
        //
        // Real analogy: Bank branch is under renovation for 2 weeks.
        // "Visit our temporary branch at 456 Elm St. for now."
        //
        // API use case: Maintenance mode, A/B testing, load balancing
        // -----------------------------------------------------------------

        /// <summary>
        /// HTTP 302 Found (Temporary):
        /// During maintenance, temporarily redirect to a backup endpoint.
        /// </summary>
        [HttpGet("products-maintenance")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public IActionResult MaintenanceRedirect()
        {
            // Temporary redirect — client must not cache this
            return Redirect("/api/products-backup");

            // Alternatively: return RedirectToAction("GetAllProducts", "BackupProduct");
        }

        // -----------------------------------------------------------------
        // HTTP 304 Not Modified
        //
        // When to use: Client sends a conditional request (with ETag or
        // If-Modified-Since header). If data hasn't changed, return 304
        // instead of the full body. This SAVES BANDWIDTH.
        //
        // Real analogy: You ask the library "Has the Harry Potter book changed
        // since last Monday?" They say "No." — You reuse your own copy.
        //
        // Flow:
        // 1. Client GETs resource → Server returns 200 + ETag: "abc123"
        // 2. Client caches it.
        // 3. Client GETs again with If-None-Match: "abc123"
        // 4. Server checks: data unchanged → returns 304 (NO body, saving bandwidth)
        // 5. Client uses its cached copy.
        // -----------------------------------------------------------------

        /// <summary>
        /// HTTP 304 Not Modified:
        /// Implements ETag-based conditional GET for bandwidth optimization.
        /// Commonly used for: product catalogs, config files, static datasets.
        /// </summary>
        [HttpGet("v2/products")]
        [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        public IActionResult GetProductsWithCaching()
        {
            // Compute current ETag (in production: hash the data or use DB timestamp)
            var currentETag = "\"products-v1-etag-20240101\"";

            // Check if client sent a conditional request header
            var requestETag = Request.Headers.IfNoneMatch.ToString();

            if (!string.IsNullOrEmpty(requestETag) && requestETag == currentETag)
            {
                // Data hasn't changed — return 304 with NO body
                // This saves bandwidth — client reuses its cached copy
                Response.Headers.ETag = currentETag;
                return StatusCode(StatusCodes.Status304NotModified);
            }

            // Data changed (or first request) — return full data with 200
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Laptop", Price = 999m, Category = "Electronics" }
            };

            // Set ETag header so client can use it next time
            Response.Headers.ETag = currentETag;
            Response.Headers.CacheControl = "max-age=3600"; // Cache for 1 hour

            return Ok(products);
        }
    }

    // =========================================================================
    // SECTION 4: HTTP 4xx CLIENT ERROR STATUS CODES
    // =========================================================================

    /// <summary>
    /// Demonstrates HTTP 400, 401, 403, 404, 405.
    ///
    /// 4xx means: "YOU (the client) did something wrong."
    /// Not the server's fault. Fix your request.
    /// </summary>
    [ApiController]
    [Route("api/shop")]
    public class ClientErrorController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ClientErrorController> _logger;

        public ClientErrorController(IProductService productService, ILogger<ClientErrorController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // -----------------------------------------------------------------
        // HTTP 400 Bad Request
        //
        // When to use: The client sent an INVALID request.
        // - Missing required fields
        // - Invalid data types
        // - Business rule violations
        //
        // Real analogy: You go to the bank and fill the deposit form wrong.
        // Teller says: "Sir, you left the amount blank. Please fill it."
        // -----------------------------------------------------------------

        /// <summary>
        /// HTTP 400 Bad Request:
        /// With [ApiController], ASP.NET Core automatically returns 400
        /// when model validation fails. But here's how to return it manually.
        /// </summary>
        [HttpPost("products")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] CreateProductRequest request)
        {
            // [ApiController] handles basic model validation automatically.
            // For custom business rule validation:

            if (request.Price < 0)
            {
                // Manual 400 for business rule violation
                return BadRequest(new ValidationProblemDetails
                {
                    Title  = "Validation Failed",
                    Errors = { ["Price"] = new[] { "Price cannot be negative" } }
                });
            }

            if (request.Category == "Banned")
            {
                // Another custom business rule
                ModelState.AddModelError("Category", "This category is not allowed");
                return BadRequest(ModelState); // Returns 400 with all model errors
            }

            var created = await _productService.CreateAsync(request);
            return CreatedAtAction(nameof(GetProductById), new { id = created.Id }, created);
        }

        [HttpGet("products/{id:int}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return product == null ? NotFound() : Ok(product);
        }

        // -----------------------------------------------------------------
        // HTTP 401 Unauthorized (MISLEADING NAME: actually means "Not Authenticated")
        //
        // When to use: Request requires authentication and NONE was provided,
        // OR the provided credentials/token are invalid/expired.
        //
        // KEY DIFFERENCE from 403:
        //   401 = "Who are you? Prove your identity first."
        //   403 = "I know who you are. But you don't have permission."
        //
        // Real analogy: ATM asks for your PIN. You press OK without entering
        // anything. ATM says: "Please enter your PIN." (401)
        //
        // In production: JWT token is missing or expired → 401
        // -----------------------------------------------------------------

        /// <summary>
        /// HTTP 401 Unauthorized:
        /// Manually returning 401 for expired token scenario.
        /// In production, JWT middleware handles this automatically.
        /// </summary>
        [HttpGet("secure/profile")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetSecureProfile([FromHeader(Name = "Authorization")] string? authHeader)
        {
            // Simulate token validation
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                // 401 Unauthorized — no token provided
                // WWW-Authenticate header is REQUIRED by HTTP spec for 401 responses
                Response.Headers.WWWAuthenticate = "Bearer realm=\"api\", error=\"invalid_token\"";

                return Unauthorized(new
                {
                    Error   = "unauthorized",
                    Message = "Authentication token is required. Please login first.",
                    LoginUrl = "/api/auth/login"
                });
            }

            var token = authHeader["Bearer ".Length..].Trim();

            // Simulate token expiry check
            if (token == "expired-token")
            {
                Response.Headers.WWWAuthenticate =
                    "Bearer realm=\"api\", error=\"invalid_token\", error_description=\"Token has expired\"";

                return Unauthorized(new
                {
                    Error   = "token_expired",
                    Message = "Your session has expired. Please login again.",
                    LoginUrl = "/api/auth/login"
                });
            }

            // Authenticated successfully
            return Ok(new { UserId = 42, Email = "user@example.com", Role = "Customer" });
        }

        // -----------------------------------------------------------------
        // HTTP 403 Forbidden
        //
        // When to use: Authentication succeeded (user is known) BUT the
        // user does NOT have PERMISSION to access the resource.
        //
        // KEY DIFFERENCE from 401:
        //   401 = Authentication failed (identity unknown)
        //   403 = Authorization failed (identity known, permission denied)
        //
        // Real analogy: You show your bank ID card (authenticated ✔).
        // But you try to access the vault. Guard says: "You're not authorized
        // to enter the vault. Only bank managers can." (403)
        //
        // In production: Role-based access control (RBAC), policy authorization
        // -----------------------------------------------------------------

        /// <summary>
        /// HTTP 403 Forbidden:
        /// User is authenticated but lacks the required role/permission.
        /// In production, use [Authorize(Roles = "Admin")] or policies.
        /// </summary>
        [HttpDelete("admin/products/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AdminDeleteProduct(
            int id,
            [FromHeader(Name = "X-User-Role")] string? role)
        {
            // In production: extract role from JWT claims, not from header
            // This is just for demonstration

            // 401 — not authenticated
            if (string.IsNullOrEmpty(role))
                return Unauthorized(new { Message = "Please authenticate first" });

            // 403 — authenticated but not authorized
            if (role != "Admin" && role != "SuperAdmin")
            {
                _logger.LogWarning("Unauthorized delete attempt by role: {Role}", role);
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    Error   = "forbidden",
                    Message = "You do not have permission to delete products. Admin role required.",
                    YourRole = role,
                    RequiredRole = "Admin"
                });
            }

            var deleted = await _productService.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }

        // -----------------------------------------------------------------
        // HTTP 404 Not Found
        //
        // When to use:
        // 1. The specific resource doesn't exist (e.g., Product ID 999)
        // 2. The URL/route itself doesn't exist (framework returns this)
        //
        // Real analogy: You search for a product on Amazon that was removed.
        // Amazon shows: "Sorry, we couldn't find that product."
        //
        // BEST PRACTICE: Return descriptive 404 messages in APIs.
        // Don't just return empty 404 — tell the client WHAT was not found.
        // -----------------------------------------------------------------

        /// <summary>
        /// HTTP 404 Not Found:
        /// Product-specific not found vs. route not found.
        /// </summary>
        [HttpGet("products/{id:int}/details")]
        [ProducesResponseType(typeof(ApiResponse<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<Product>>> GetProductDetails(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
            {
                // Detailed 404 — tell the client exactly what wasn't found
                return NotFound(new ApiProblemDetails
                {
                    Type     = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Title    = "Product Not Found",
                    Status   = StatusCodes.Status404NotFound,
                    Detail   = $"Product with ID {id} was not found or has been deleted.",
                    Instance = HttpContext.Request.Path,
                    Extensions = new Dictionary<string, object>
                    {
                        ["productId"]  = id,
                        ["searchTip"]  = "Try browsing /api/products for all available products"
                    }
                });
            }

            return Ok(ApiResponse<Product>.Ok(product));
        }

        // -----------------------------------------------------------------
        // HTTP 405 Method Not Allowed
        //
        // When to use: The HTTP method used (GET/POST/PUT/DELETE) is not
        // allowed for that endpoint.
        //
        // Example: Endpoint only accepts GET, but client sends DELETE.
        //
        // Real analogy: You try to use the "Exit" door as an entrance.
        // The sign says: "Exit Only. Use the entrance on the other side."
        //
        // In ASP.NET Core: The framework returns 405 automatically when
        // a route exists but the HTTP method isn't mapped.
        // You can also return it manually for custom validation.
        //
        // REQUIRED: Include "Allow" header listing permitted methods.
        // -----------------------------------------------------------------

        /// <summary>
        /// HTTP 405 Method Not Allowed:
        /// This endpoint is read-only. Writing is not allowed.
        /// ASP.NET Core returns 405 automatically for unmapped methods,
        /// but here we demonstrate manual 405 with the Allow header.
        /// </summary>
        [HttpGet("readonly/products")]
        [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
        public async Task<ActionResult<IEnumerable<Product>>> GetReadOnlyProducts()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        /// <summary>
        /// Demonstrates returning 405 manually for a read-only resource.
        /// In production, ASP.NET Core handles this automatically.
        /// </summary>
        [HttpPost("readonly/products")]
        public IActionResult AttemptWriteToReadOnlyEndpoint()
        {
            // Manually set the Allow header listing what IS permitted
            Response.Headers.Allow = "GET, HEAD";

            return StatusCode(StatusCodes.Status405MethodNotAllowed, new
            {
                Error    = "method_not_allowed",
                Message  = "This endpoint is read-only. POST, PUT, DELETE are not supported.",
                Allowed  = new[] { "GET", "HEAD" }
            });
        }
    }

    // =========================================================================
    // SECTION 5: HTTP 5xx SERVER ERROR STATUS CODES
    // =========================================================================

    /// <summary>
    /// Demonstrates HTTP 500, 501, 503, 504.
    ///
    /// 5xx means: "WE (the server) messed up."
    /// The client's request was valid, but something on the server went wrong.
    ///
    /// GOLDEN RULE: Never expose internal exception details in 5xx responses!
    /// Log the full error internally, return a safe generic message to clients.
    /// </summary>
    [ApiController]
    [Route("api/server")]
    public class ServerErrorController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ServerErrorController> _logger;

        public ServerErrorController(IProductService productService, ILogger<ServerErrorController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // -----------------------------------------------------------------
        // HTTP 500 Internal Server Error
        //
        // When to use: An unexpected error occurred on the server.
        // The client did nothing wrong — the server had an unhandled exception.
        //
        // Real analogy: You press the ATM button correctly, but the ATM
        // has a hardware fault inside. "System error. Please try later."
        //
        // PRODUCTION RULE:
        // - Log the FULL exception (stack trace, inner exception, etc.)
        // - Return ONLY a safe generic message to the client
        // - Never leak database connection strings, stack traces, etc.
        // -----------------------------------------------------------------

        /// <summary>
        /// HTTP 500 Internal Server Error:
        /// Global exception handling is the preferred approach (via middleware),
        /// but here's how to handle it manually in a controller.
        /// </summary>
        [HttpGet("products/with-error")]
        [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsWithErrorHandling()
        {
            try
            {
                // Simulate a database call that throws unexpectedly
                var products = await _productService.GetAllAsync();

                // Simulate random server error (for demo)
                if (new Random().Next(10) == 0) // 10% chance of error
                    throw new InvalidOperationException("Database connection dropped unexpectedly");

                return Ok(products);
            }
            catch (Exception ex)
            {
                // IMPORTANT: Log the FULL exception internally
                _logger.LogError(ex,
                    "Unhandled error in {Controller}.{Action}. TraceId: {TraceId}",
                    nameof(ServerErrorController),
                    nameof(GetProductsWithErrorHandling),
                    HttpContext.TraceIdentifier);

                // Return SAFE generic message to client (no exception details!)
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiProblemDetails
                {
                    Type     = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Title    = "Internal Server Error",
                    Status   = StatusCodes.Status500InternalServerError,
                    Detail   = "An unexpected error occurred. Our team has been notified.",
                    Instance = HttpContext.Request.Path,
                    Extensions = new Dictionary<string, object>
                    {
                        // Give client a trace ID to quote in support tickets
                        ["traceId"] = HttpContext.TraceIdentifier
                    }
                });
            }
        }

        // -----------------------------------------------------------------
        // HTTP 501 Not Implemented
        //
        // When to use: The endpoint exists in the contract/interface but
        // the server has NOT yet implemented the functionality.
        //
        // Real analogy: You call the bank and ask "Can I open a crypto wallet?"
        // They say: "That feature is coming soon. Not available yet."
        //
        // Production use case:
        // - API-first design: define all endpoints upfront, implement later
        // - Microservice contracts defined before coding begins
        // - Feature flags for upcoming features
        // -----------------------------------------------------------------

        /// <summary>
        /// HTTP 501 Not Implemented:
        /// Feature is planned but not yet built.
        /// The endpoint exists in API spec (Swagger) but returns 501 until done.
        /// </summary>
        [HttpGet("products/recommendations")]
        [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult GetProductRecommendations(
            [FromQuery] int userId,
            [FromQuery] string algorithm = "collaborative-filtering")
        {
            // Feature is planned for Sprint 12 but not implemented yet
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                Error        = "not_implemented",
                Message      = "Product recommendations feature is under development.",
                PlannedRelease = "Q2 2025",
                AlternativeUrl = "/api/products?category=popular",
                Documentation  = "https://docs.myapi.com/upcoming/recommendations"
            });
        }

        // -----------------------------------------------------------------
        // HTTP 503 Service Unavailable
        //
        // When to use: The server is temporarily unable to handle requests.
        // Reasons: maintenance, overload, dependency down (DB, external API)
        //
        // Real analogy: Your bank's online banking is down for maintenance
        // at 2 AM Saturday. The page says: "We'll be back at 4 AM."
        //
        // IMPORTANT:
        // - Return Retry-After header to tell clients when to retry
        // - Load balancers use 503 to route traffic elsewhere
        // - Health check endpoints return 503 during unhealthy state
        // -----------------------------------------------------------------

        /// <summary>
        /// HTTP 503 Service Unavailable:
        /// Demonstrates service degradation pattern.
        /// In production, health checks return 503 when dependencies are down.
        /// </summary>
        [HttpGet("products/live")]
        [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetProductsLive()
        {
            // Check if the service is healthy (e.g., DB connection, cache available)
            var isAvailable = await _productService.IsServiceAvailableAsync();

            if (!isAvailable)
            {
                // 503 with Retry-After header
                // Tells client: "Try again in 30 seconds"
                Response.Headers.RetryAfter = "30"; // Seconds until retry

                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    Error     = "service_unavailable",
                    Message   = "The product service is currently unavailable due to maintenance.",
                    RetryAfterSeconds = 30,
                    StatusPage = "https://status.myservice.com",
                    MaintenanceWindow = new
                    {
                        Start = DateTime.UtcNow.ToString("o"),
                        End   = DateTime.UtcNow.AddMinutes(30).ToString("o")
                    }
                });
            }

            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        // -----------------------------------------------------------------
        // HTTP 504 Gateway Timeout
        //
        // When to use: Your server acted as a gateway/proxy and an UPSTREAM
        // service (another API, microservice, database) did not respond in time.
        //
        // Real analogy: You call the bank to transfer money. Your bank calls
        // the recipient's bank. Recipient bank doesn't respond in 30 seconds.
        // Your bank tells you: "The other bank isn't responding. Try later."
        //
        // Production context:
        // - API Gateway → Microservice timeout
        // - Web API → External Payment Gateway timeout
        // - Your service → Database query timeout
        // - Your service → Third-party API timeout
        // -----------------------------------------------------------------

        /// <summary>
        /// HTTP 504 Gateway Timeout:
        /// Your API calls an external service that takes too long.
        /// Use CancellationToken to enforce timeouts properly.
        /// </summary>
        [HttpPost("payments/process")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<IActionResult> ProcessPayment([FromBody] object paymentRequest)
        {
            // Use CancellationToken with timeout for external calls
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // 5s timeout

            try
            {
                // Simulate calling external payment gateway (Stripe, Razorpay, etc.)
                await SimulateExternalPaymentGatewayCall(cts.Token);

                return Ok(new
                {
                    TransactionId = Guid.NewGuid(),
                    Status = "Success",
                    ProcessedAt = DateTime.UtcNow
                });
            }
            catch (OperationCanceledException)
            {
                // External payment gateway didn't respond within 5 seconds
                _logger.LogError("Payment gateway timeout for request at {Time}", DateTime.UtcNow);

                return StatusCode(StatusCodes.Status504GatewayTimeout, new
                {
                    Error    = "gateway_timeout",
                    Message  = "The payment gateway did not respond in time. Please try again.",
                    TimeoutSeconds = 5,
                    RetryAdvice    = "Wait 30 seconds and retry the payment",
                    SupportEmail   = "payments@myservice.com"
                });
            }
        }

        // Helper: Simulates a slow external API call
        private static async Task SimulateExternalPaymentGatewayCall(CancellationToken ct)
        {
            // Simulate gateway taking 8 seconds (exceeds our 5s timeout)
            await Task.Delay(8000, ct);
        }
    }
}

#endregion

// =============================================================================
#region GlobalHttpMethodConfiguration
// =============================================================================
// Demonstrates how to configure allowed HTTP methods globally in ASP.NET Core.
// This is about restricting which HTTP verbs the entire application accepts.
//
// Use case: Security hardening — disable TRACE, OPTIONS, etc. globally
// to reduce attack surface. Only allow GET, POST, PUT, DELETE, PATCH.
// =============================================================================

namespace ReturnTypesAndStatusCodes.Configuration
{
    /// <summary>
    /// CONFIGURE ALLOWED HTTP METHODS GLOBALLY
    ///
    /// Several approaches to restrict HTTP methods at the application level:
    /// 1. Middleware-based filtering
    /// 2. Convention-based route constraints
    /// 3. CORS policy method restriction
    ///
    /// This class demonstrates middleware-based HTTP method restriction,
    /// which is the most powerful and flexible approach.
    /// </summary>
    public class HttpMethodFilterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpMethodFilterMiddleware> _logger;

        // Define the ONLY HTTP methods your API will accept globally
        private static readonly HashSet<string> _allowedMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            "GET",     // Retrieve resources
            "POST",    // Create resources
            "PUT",     // Full update of resources
            "PATCH",   // Partial update of resources
            "DELETE",  // Delete resources
            "OPTIONS", // CORS preflight (required for browser-based clients)
            "HEAD"     // Same as GET but no body (used for health checks)
            // NOT ALLOWED: TRACE (security risk), CONNECT, arbitrary custom methods
        };

        public HttpMethodFilterMiddleware(RequestDelegate next, ILogger<HttpMethodFilterMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var method = context.Request.Method;

            if (!_allowedMethods.Contains(method))
            {
                // Log the attempt — could indicate a security probe
                _logger.LogWarning(
                    "Rejected disallowed HTTP method: {Method} from IP: {IP}",
                    method,
                    context.Connection.RemoteIpAddress);

                // Return 405 with the Allow header listing accepted methods
                context.Response.StatusCode  = StatusCodes.Status405MethodNotAllowed;
                context.Response.Headers.Allow = string.Join(", ", _allowedMethods);
                context.Response.ContentType   = "application/json";

                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                {
                    Error   = "method_not_allowed",
                    Method  = method,
                    Message = $"HTTP method '{method}' is not allowed by this API",
                    Allowed = _allowedMethods.ToArray()
                }));
                return; // Stop pipeline — don't call next middleware
            }

            // Method is allowed — continue to next middleware / controller
            await _next(context);
        }
    }

    /// <summary>
    /// Extension method to register the middleware cleanly.
    /// This is the standard .NET pattern for middleware registration.
    /// </summary>
    public static class HttpMethodFilterMiddlewareExtensions
    {
        public static IApplicationBuilder UseHttpMethodFilter(this IApplicationBuilder app)
            => app.UseMiddleware<HttpMethodFilterMiddleware>();
    }
}

#endregion

// =============================================================================
#region GlobalExceptionHandlingMiddleware
// =============================================================================
// In production, you NEVER handle exceptions in every controller.
// Instead, use a GLOBAL exception handler that:
// 1. Catches all unhandled exceptions
// 2. Logs them fully
// 3. Returns safe 500 response to client
// 4. Assigns a unique trace ID for support
// =============================================================================

namespace ReturnTypesAndStatusCodes.Middleware
{
    /// <summary>
    /// PRODUCTION-GRADE GLOBAL EXCEPTION HANDLER
    ///
    /// This replaces try-catch in every controller action.
    /// Register this as the FIRST middleware in the pipeline (before everything).
    ///
    /// Flow: Request → GlobalExceptionHandler → Other Middlewares → Controller
    /// If any step throws → Caught here → Logged → Safe 500 returned
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next   = next;
            _logger = logger;
            _env    = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Execute the rest of the pipeline
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var traceId = context.TraceIdentifier;

            // ALWAYS log the full exception — never suppress it
            _logger.LogError(exception,
                "Unhandled exception. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
                traceId,
                context.Request.Path,
                context.Request.Method);

            // Determine appropriate status code based on exception type
            var statusCode = exception switch
            {
                ArgumentNullException      => StatusCodes.Status400BadRequest,
                ArgumentException          => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException       => StatusCodes.Status404NotFound,
                NotImplementedException    => StatusCodes.Status501NotImplemented,
                TimeoutException           => StatusCodes.Status504GatewayTimeout,
                OperationCanceledException => StatusCodes.Status504GatewayTimeout,
                _                          => StatusCodes.Status500InternalServerError
            };

            context.Response.StatusCode  = statusCode;
            context.Response.ContentType = "application/problem+json";

            // In Development: include details for debugging
            // In Production: only return safe generic message
            var responseBody = _env.IsDevelopment()
                ? new
                {
                    Type     = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Title    = exception.GetType().Name,
                    Status   = statusCode,
                    Detail   = exception.Message,     // OK in DEV
                    Instance = context.Request.Path.Value,
                    TraceId  = traceId,
                    StackTrace = exception.StackTrace  // Only in DEV!
                }
                : (object)new
                {
                    Type     = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Title    = "An error occurred",
                    Status   = statusCode,
                    Detail   = "An unexpected error occurred. Please contact support.",
                    Instance = context.Request.Path.Value,
                    TraceId  = traceId
                    // NO stack trace in production!
                };

            var json = System.Text.Json.JsonSerializer.Serialize(responseBody,
                new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

            await context.Response.WriteAsync(json);
        }
    }

    /// <summary>
    /// Extension method for clean middleware registration
    /// </summary>
    public static class GlobalExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
            => app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}

#endregion

// =============================================================================
#region ProgramSetup
// =============================================================================
// How to wire everything together in Program.cs (ASP.NET Core 6+)
// This is the entry point of your application.
// =============================================================================

namespace ReturnTypesAndStatusCodes
{
    using ReturnTypesAndStatusCodes.Configuration;
    using ReturnTypesAndStatusCodes.Interfaces;
    using ReturnTypesAndStatusCodes.Middleware;
    using ReturnTypesAndStatusCodes.Services;

    /// <summary>
    /// APPLICATION ENTRY POINT — Program.cs equivalent
    ///
    /// Shows the CORRECT order of middleware registration.
    /// Middleware order MATTERS — wrong order = security vulnerabilities!
    ///
    /// Correct Pipeline Order:
    /// 1. Exception Handling       ← catches ALL subsequent errors
    /// 2. HTTP Method Filter       ← rejects disallowed methods early
    /// 3. HTTPS Redirection        ← force secure connections
    /// 4. Security Headers         ← HSTS, etc.
    /// 5. Routing
    /// 6. CORS
    /// 7. Authentication           ← who are you?
    /// 8. Authorization            ← what can you do?
    /// 9. Controller endpoint      ← actual business logic
    /// </summary>
    public static class ProgramSetup
    {
        public static WebApplication BuildApplication(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ----------------------------------------------------------------
            // SERVICE REGISTRATION (Dependency Injection Container)
            // ----------------------------------------------------------------

            // Register controllers with API behavior (model validation → auto 400)
            builder.Services.AddControllers(options =>
            {
                // Configure allowed HTTP methods at MVC level
                // This affects route matching, not raw HTTP handling
                // For global HTTP method filtering, use the middleware above

                // Return 406 Not Acceptable when client requests unsupported format
                options.ReturnHttpNotAcceptable = true;
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                // Customize the default 400 response shape for model validation
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problems = new ValidationProblemDetails(context.ModelState)
                    {
                        Type     = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        Title    = "Validation Error",
                        Status   = StatusCodes.Status400BadRequest,
                        Instance = context.HttpContext.Request.Path
                    };
                    problems.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                    return new BadRequestObjectResult(problems)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

            // Register services with appropriate lifetimes
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddMemoryCache(); // For 304 ETag caching demo

            // Swagger / OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title   = "Return Types & Status Codes Demo API",
                    Version = "v1",
                    Description = "Production-grade demonstration of all HTTP status codes"
                });
            });

            var app = builder.Build();

            // ----------------------------------------------------------------
            // MIDDLEWARE PIPELINE — ORDER IS CRITICAL!
            // ----------------------------------------------------------------

            // 1. Global exception handler — MUST be first to catch all errors
            app.UseGlobalExceptionHandling();

            // 2. HTTP Method filtering — reject invalid methods early
            app.UseHttpMethodFilter();

            // 3. Development tools
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Status Codes API v1");
                    c.RoutePrefix = string.Empty; // Swagger at root URL
                });
            }

            // 4. HTTPS Redirection (returns 301/302 for HTTP → HTTPS)
            app.UseHttpsRedirection();

            // 5. Routing
            app.UseRouting();

            // 6. Authentication (401 handling)
            // app.UseAuthentication(); // Uncomment with JWT setup

            // 7. Authorization (403 handling)
            // app.UseAuthorization(); // Uncomment with role-based auth

            // 8. Map controllers
            app.MapControllers();

            return app;
        }
    }
}

#endregion

// =============================================================================
#region Summary
// =============================================================================
//
// QUICK REFERENCE: HTTP STATUS CODES AT A GLANCE
//
// ┌──────┬─────────────────────────┬─────────────────────────────────────────────┐
// │ Code │ Name                    │ When to Use in Web API                      │
// ├──────┼─────────────────────────┼─────────────────────────────────────────────┤
// │ 200  │ OK                      │ Successful GET / successful operation        │
// │ 201  │ Created                 │ POST created a resource (+ Location header) │
// │ 202  │ Accepted                │ Request queued for async processing          │
// │ 204  │ No Content              │ PUT / DELETE success, nothing to return      │
// ├──────┼─────────────────────────┼─────────────────────────────────────────────┤
// │ 301  │ Moved Permanently       │ Old URL → New URL (permanent, cache it)      │
// │ 302  │ Found (Temp Redirect)   │ Old URL → New URL (temporary, don't cache)   │
// │ 304  │ Not Modified            │ Conditional GET, client cache is still valid │
// ├──────┼─────────────────────────┼─────────────────────────────────────────────┤
// │ 400  │ Bad Request             │ Validation failed, malformed body            │
// │ 401  │ Unauthorized            │ Not authenticated (missing/invalid token)    │
// │ 403  │ Forbidden               │ Authenticated but lacks permission           │
// │ 404  │ Not Found               │ Resource doesn't exist                       │
// │ 405  │ Method Not Allowed      │ HTTP verb not supported for that route       │
// ├──────┼─────────────────────────┼─────────────────────────────────────────────┤
// │ 500  │ Internal Server Error   │ Unhandled exception — server's fault         │
// │ 501  │ Not Implemented         │ Feature exists in spec but not built yet     │
// │ 503  │ Service Unavailable     │ Server busy/down/maintenance (+ Retry-After) │
// │ 504  │ Gateway Timeout         │ Upstream service (DB/API) timed out          │
// └──────┴─────────────────────────┴─────────────────────────────────────────────┘
//
// RETURN TYPES QUICK REFERENCE:
//
// ┌──────────────────────┬──────────────────────────────────────────────────────┐
// │ Return Type          │ Best For                                             │
// ├──────────────────────┼──────────────────────────────────────────────────────┤
// │ Specific Type        │ Simple, internal-only APIs (no status code control)  │
// │ IActionResult        │ When type varies but needs [ProducesResponseType]    │
// │ ActionResult<T>      │ RECOMMENDED: type + status code control              │
// └──────────────────────┴──────────────────────────────────────────────────────┘
//
// PRODUCTION RULES:
//   ✔ Always use ActionResult<T> in controllers
//   ✔ Always use [ApiController] attribute
//   ✔ Always wrap 201 with CreatedAtAction() for Location header
//   ✔ Always include Allow header with 405 responses
//   ✔ Always include Retry-After header with 503 responses
//   ✔ Always include WWW-Authenticate header with 401 responses
//   ✔ Never expose stack traces in production 500 responses
//   ✔ Always log errors with TraceId for support traceability
//   ✔ Use GlobalExceptionHandlingMiddleware instead of try-catch everywhere
//   ✔ Use ProblemDetails (RFC 7807) standard for error response shapes
//
// =============================================================================
#endregion
