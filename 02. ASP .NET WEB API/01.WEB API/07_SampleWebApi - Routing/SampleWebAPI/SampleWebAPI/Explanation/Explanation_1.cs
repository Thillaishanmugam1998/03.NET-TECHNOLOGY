// ============================================================
// COMPLETE GUIDE: ASP.NET Core Web API Concepts
// Everything in one file with clear explanations
// ============================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;

// ============================================================
// SECTION 1: MODEL
// A Model is just a class that holds data.
// DataAnnotations = rules that say "this field must be X"
// ModelState checks these rules automatically when data arrives
// ============================================================
#region Model with Validation Rules

namespace ConceptGuide.Models
{
    public class Product
    {
        public int Id { get; set; }  // Auto-assigned by service, no validation needed

        // [Required] = Name cannot be null or empty string
        // If someone sends: { "name": "" } → ModelState.IsValid = FALSE → 400 Bad Request
        // If someone sends: { "name": "Laptop" } → ModelState.IsValid = TRUE → continues
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be 2-100 characters")]
        public string Name { get; set; } = string.Empty;

        // [StringLength] = controls max/min length of text
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        // [Range] = value must be between min and max
        // Wrong data example: { "price": -50 }  → 400 Bad Request
        // Correct data example: { "price": 99.99 } → passes validation
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 99999.99, ErrorMessage = "Price must be between 0.01 and 99999.99")]
        public decimal Price { get; set; }

        // Stock cannot be negative
        // Wrong: { "stock": -5 }  → 400 Bad Request
        // Correct: { "stock": 10 } → passes
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }
    }
}

/*
 * MODELSTATE EXPLAINED WITH EXAMPLES:
 * =====================================
 * 
 * ModelState is like a validator that checks incoming JSON data against your model rules.
 * [ApiController] attribute runs this check AUTOMATICALLY before your method even starts.
 * 
 * CORRECT REQUEST BODY (passes all rules):
 * {
 *   "name": "Laptop",         ✅ has value, length is fine
 *   "description": "Gaming",  ✅ under 500 chars
 *   "price": 1299.99,         ✅ between 0.01 and 99999.99
 *   "stock": 10               ✅ not negative
 * }
 * Result: ModelState.IsValid = TRUE → method runs normally
 * 
 * WRONG REQUEST BODY (fails rules):
 * {
 *   "name": "",               ❌ empty string — [Required] fails
 *   "description": "Gaming",
 *   "price": -50,             ❌ negative — [Range] fails
 *   "stock": -1               ❌ negative — [Range] fails
 * }
 * Result: ModelState.IsValid = FALSE → automatic 400 Bad Request response:
 * {
 *   "errors": {
 *     "Name":  ["Name is required"],
 *     "Price": ["Price must be between 0.01 and 99999.99"],
 *     "Stock": ["Stock cannot be negative"]
 *   }
 * }
 * 
 * WITHOUT [ApiController] → you must check manually:
 *   if (!ModelState.IsValid) return BadRequest(ModelState);
 * 
 * WITH [ApiController] → this check happens automatically, no code needed.
 */

#endregion

// ============================================================
// SECTION 2: SERVICE INTERFACE + IMPLEMENTATION
// Interface = the CONTRACT (what methods exist)
// Implementation = the ACTUAL CODE (how methods work)
// This pattern allows you to swap implementations easily
// e.g., swap in-memory → database without changing the controller
// ============================================================
#region Service Layer

namespace ConceptGuide.Services
{
    // The interface — defines what operations are available
    // Controller only knows about this interface, not the actual class
    public interface IProductService
    {
        IEnumerable<ConceptGuide.Models.Product> GetAll();
        ConceptGuide.Models.Product? GetById(int id);
        ConceptGuide.Models.Product Add(ConceptGuide.Models.Product product);
    }

    // The actual implementation — in-memory list as our "database"
    // Registered as Singleton in Program.cs = one instance for entire app lifetime
    public class ProductService : IProductService
    {
        private readonly List<ConceptGuide.Models.Product> _products = new()
        {
            new() { Id = 1, Name = "Laptop",   Description = "Gaming laptop",        Price = 1299.99m, Stock = 10 },
            new() { Id = 2, Name = "Mouse",    Description = "Wireless mouse",       Price = 49.99m,   Stock = 50 },
            new() { Id = 3, Name = "Keyboard", Description = "Mechanical keyboard",  Price = 89.99m,   Stock = 30 },
        };

        private int _nextId = 4;  // Auto-increment ID tracker

        public IEnumerable<ConceptGuide.Models.Product> GetAll() => _products;

        public ConceptGuide.Models.Product? GetById(int id) =>
            _products.FirstOrDefault(p => p.Id == id);

        public ConceptGuide.Models.Product Add(ConceptGuide.Models.Product product)
        {
            product.Id = _nextId++;
            _products.Add(product);
            return product;
        }
    }
}

#endregion

// ============================================================
// SECTION 3: AUTHENTICATION HANDLER
// Basic Auth flow:
// 1. Client sends: Authorization: Basic base64("username:password")
// 2. Handler decodes it
// 3. Compares password to ApiKey in appsettings.json
// 4. If match → user is authenticated, assigns role claims
// 5. If no match → 401 Unauthorized
// ============================================================
#region Authentication Handler

namespace ConceptGuide.Auth
{
    public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConfiguration _configuration;

        public BasicAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IConfiguration configuration)
            : base(options, logger, encoder)
        {
            _configuration = configuration;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Step 1: Check the Authorization header exists at all
            // If missing → 401 immediately
            if (!Request.Headers.ContainsKey("Authorization"))
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header."));

            string authHeader = Request.Headers["Authorization"].ToString();

            // Step 2: Must start with "Basic " — other schemes like "Bearer" are rejected
            if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(AuthenticateResult.Fail("Use Basic authentication scheme."));

            // Step 3: Decode the Base64 encoded "username:password"
            // Example: "YWRtaW46TXlLZXk=" → "admin:MyKey"
            string encodedCredentials = authHeader["Basic ".Length..].Trim();
            string decodedCredentials;
            try
            {
                decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Base64 encoding."));
            }

            // Step 4: Split "admin:MyKey" into username="admin" and password="MyKey"
            int colonIndex = decodedCredentials.IndexOf(':');
            if (colonIndex < 0)
                return Task.FromResult(AuthenticateResult.Fail("Credentials must be username:password format."));

            string username = decodedCredentials[..colonIndex];
            string password = decodedCredentials[(colonIndex + 1)..];

            // Step 5: Compare password against ApiKey from appsettings.json
            // appsettings.json → "ApiSettings": { "ApiKey": "MySuperSecretKey123!" }
            string? expectedKey = _configuration["ApiSettings:ApiKey"];
            if (string.IsNullOrEmpty(expectedKey) || password != expectedKey)
                return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));

            // Step 6: Password matched! Now assign role based on username
            // "admin" username gets Admin role → can POST (add products)
            // anyone else gets ApiUser role → can only GET
            string role = username.ToLower() == "admin" ? "Admin" : "ApiUser";

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}

#endregion

// ============================================================
// SECTION 4: CONTROLLER
//
// [ApiController]
//   → Enables automatic ModelState validation (400 if invalid)
//   → Automatically infers [FromBody], [FromRoute] sources
//   → Without this, you must manually check ModelState everywhere
//
// [Route("api/[controller]")]
//   → [controller] is replaced by class name minus "Controller"
//   → ProductsController → /api/products
//   → You can also write: [Route("api/v1/[controller]")] for versioning
//
// [Authorize]
//   → Applied at CLASS level = every single method inside is protected
//   → Without valid credentials → 401 Unauthorized before method runs
//   → Override per method using [AllowAnonymous] to make one method public
//
// [EnableRateLimiting("fixed")]
//   → Applied at CLASS level = every method gets 5 req/min limit
//   → Uses the "fixed" policy registered in Program.cs
//   → Override per method using [DisableRateLimiting] to remove limit
//
// ============================================================
#region Controller

namespace ConceptGuide.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class ProductsController : ControllerBase
    {
        private readonly ConceptGuide.Services.IProductService _productService;

        // Constructor Injection — DI container provides IProductService automatically
        public ProductsController(ConceptGuide.Services.IProductService productService)
        {
            _productService = productService;
        }

        // ── ROUTING PATTERNS EXPLAINED ───────────────────────────────────────
        //
        // [HttpGet]                    → GET /api/products
        // [HttpGet("{id:int}")]        → GET /api/products/5
        // [HttpGet("search/{name}")]   → GET /api/products/search/laptop
        // [HttpPost]                   → POST /api/products
        // [HttpPut("{id:int}")]        → PUT /api/products/5
        // [HttpDelete("{id:int}")]     → DELETE /api/products/5
        // [HttpGet("/products")]       → GET /products  (leading / overrides api/ prefix!)
        //
        // ── RETURN TYPE: ActionResult<T> vs IActionResult vs Direct Type ─────
        //
        // Direct Type:       public List<Product> GetAll()
        //   ✅ Simple
        //   ❌ Can ONLY return 200 OK — cannot return NotFound, BadRequest etc.
        //
        // IActionResult:     public IActionResult GetAll()
        //   ✅ Can return any status code (Ok, NotFound, BadRequest, etc.)
        //   ❌ Swagger does not know what type the response body is
        //
        // ActionResult<T>:   public ActionResult<List<Product>> GetAll()  ← BEST
        //   ✅ Can return any status code
        //   ✅ Swagger knows the exact response type
        //   ✅ Can return T directly without wrapping in Ok() — implicit conversion
        //
        // ────────────────────────────────────────────────────────────────────

        // GET /api/products
        // Both Admin and ApiUser can view products
        [HttpGet]
        [Authorize(Roles = "Admin,ApiUser")]
        public ActionResult<IEnumerable<ConceptGuide.Models.Product>> GetAll()
        {
            return Ok(_productService.GetAll());
        }

        // GET /api/products/5
        // Returns 404 with a clear message if product does not exist
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,ApiUser")]
        public ActionResult<ConceptGuide.Models.Product> GetById(int id)
        {
            var product = _productService.GetById(id);

            if (product is null)
                return NotFound(new { message = $"Product with ID {id} was not found." });

            // Implicit conversion: return product directly, ActionResult<T> wraps as 200 OK
            return product;
        }

        // POST /api/products
        // ONLY Admin role can add products
        // [ApiController] automatically validates the Product model using DataAnnotations
        // If validation fails → 400 Bad Request is returned before this method even runs
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult<ConceptGuide.Models.Product> Add([FromBody] ConceptGuide.Models.Product product)
        {
            // No need to check ModelState manually — [ApiController] handles it
            // If name is empty, price is negative etc. → already rejected with 400

            var created = _productService.Add(product);

            // 201 Created + Location header: GET /api/products/{newId}
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

#endregion

// ============================================================
// SECTION 5: PROGRAM.CS — PUTTING IT ALL TOGETHER
// Two phases:
//   Phase 1 (Services): Register everything into the DI container
//   Phase 2 (Middleware): Define the request pipeline order
// ============================================================
#region Program Entry Point

namespace ConceptGuide
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==============================================================
            // PHASE 1: SERVICE REGISTRATION
            // Think of this as setting up tools BEFORE the app starts.
            // ==============================================================

            // Controllers — enables routing to controller action methods
            builder.Services.AddControllers();

            // ProductService as Singleton — same instance lives for app lifetime
            // Use AddScoped for database-connected services (one per request)
            builder.Services.AddSingleton<ConceptGuide.Services.IProductService, ConceptGuide.Services.ProductService>();

            // Basic Auth — our custom handler reads ApiSettings:ApiKey from appsettings.json
            builder.Services.AddAuthentication("BasicAuth")
                .AddScheme<AuthenticationSchemeOptions, ConceptGuide.Auth.BasicAuthHandler>("BasicAuth", null);

            // Authorization — enables [Authorize] and [Authorize(Roles="...")] attributes
            builder.Services.AddAuthorization();

            // CORS — allows browser-based clients from any origin to call this API
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            // Rate Limiter — 5 requests per minute per client
            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("fixed", opt =>
                {
                    opt.PermitLimit = 5;
                    opt.Window = TimeSpan.FromMinutes(1);
                });
            });

            var app = builder.Build();

            // ==============================================================
            // PHASE 2: MIDDLEWARE PIPELINE
            // Order matters — each middleware runs top to bottom on every request.
            // ==============================================================

            // 1. Custom logger — first in, last out (wraps everything)
            app.Use(async (context, next) =>
            {
                Console.WriteLine($"[→] {context.Request.Method} {context.Request.Path}");
                await next();
                Console.WriteLine($"[←] {context.Response.StatusCode}");
            });

            // 2. CORS — must be before routing to handle OPTIONS preflight requests
            app.UseCors("AllowAll");

            // 3. Routing — matches URL to a controller action
            app.UseRouting();

            // 4. Authentication — reads the Authorization header, identifies the user
            //    Must be AFTER routing, BEFORE authorization
            app.UseAuthentication();

            // 5. Authorization — checks if the identified user has permission
            //    Must be AFTER authentication
            app.UseAuthorization();

            // 6. Rate Limiting — placed after auth so limits can be per-user if needed
            app.UseRateLimiter();

            // 7. Map controllers — final handler that runs the actual action method
            app.MapControllers();

            app.Run();
        }
    }
}

#endregion

/*
 * ============================================================
 * POSTMAN TESTING GUIDE
 * ============================================================
 *
 * Setup Auth in Postman:
 *   Tab: Authorization → Type: Basic Auth
 *   Username: admin       → gets Admin role  → all 3 endpoints work
 *   Username: user        → gets ApiUser role → only GET endpoints work
 *   Password: MySuperSecretKey123!  (from appsettings.json)
 *
 * ── ENDPOINTS ──────────────────────────────────────────────
 *
 * 1. GET ALL PRODUCTS
 *    GET http://localhost:5000/api/products
 *    Auth: admin or user → 200 OK
 *
 * 2. GET BY ID
 *    GET http://localhost:5000/api/products/1
 *    Auth: admin or user → 200 OK
 *    GET http://localhost:5000/api/products/999 → 404 Not Found
 *
 * 3. ADD PRODUCT
 *    POST http://localhost:5000/api/products
 *    Auth: admin only (user → 403 Forbidden)
 *    Body (raw JSON):
 *    {
 *      "name": "Monitor",
 *      "description": "4K display",
 *      "price": 399.99,
 *      "stock": 15
 *    }
 *    → 201 Created
 *
 *    Wrong body (triggers ModelState validation):
 *    {
 *      "name": "",        ← empty, Required fails
 *      "price": -10       ← negative, Range fails
 *    }
 *    → 400 Bad Request with error messages
 *
 * 4. HEALTH CHECK (no auth needed)
 *    GET http://localhost:5000/api/products/health → 200 OK
 *
 * ── STATUS CODE REFERENCE ──────────────────────────────────
 *
 *  200 OK           → success, returns data
 *  201 Created      → product added successfully
 *  400 Bad Request  → model validation failed (wrong data)
 *  401 Unauthorized → missing or wrong password
 *  403 Forbidden    → correct password but wrong role
 *  404 Not Found    → product ID does not exist
 *  429 Too Many     → exceeded 5 requests per minute
 *
 * ============================================================
 */