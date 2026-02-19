// ================================================================
// HOW AN HTTP REQUEST TRAVELS THROUGH ASP.NET CORE
// Simple explanation with code — Step by Step
// ================================================================
//
// REAL WORLD ANALOGY:
// Think of a request like a VISITOR entering a COMPANY BUILDING
//
//  Client (Postman)
//      ↓
//  [Security Gate]       → Middleware Pipeline starts
//      ↓
//  [Reception Desk]      → Routing — "who are you looking for?"
//      ↓
//  [ID Check]            → Authentication — "prove who you are"
//      ↓
//  [Access Card Check]   → Authorization — "are you allowed in this room?"
//      ↓
//  [Rate Limit Counter]  → Rate Limiter — "you visited too many times today"
//      ↓
//  [The Actual Office]   → Controller Action runs
//      ↓
//  [Exit with Result]    → Response goes back to client
//
// ================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

// ================================================================
// STEP 1: CLIENT SENDS A REQUEST
// ================================================================
/*
 * A client (Postman, browser, mobile app) sends something like:
 *
 *   GET https://example.com/api/customers/5?search=abc
 *   Headers:
 *     Authorization: Basic YWRtaW46TXlLZXk=
 *     Content-Type: application/json
 *
 * The request contains:
 *   HTTP Method  → GET, POST, PUT, DELETE, PATCH
 *   URL          → https://example.com/api/customers/5
 *   Query String → ?search=abc  (optional — extra filters)
 *   Headers      → Authorization, Content-Type, etc.
 *   Body         → JSON data (only for POST, PUT, PATCH)
 *
 * This request now enters the MIDDLEWARE PIPELINE in Program.cs
*/

// ================================================================
// STEP 2: URL PARSING — Routing Middleware reads the URL
// ================================================================
/*
 * The URL https://example.com/api/customers/5?search=abc
 * is broken into parts:
 *
 *   Scheme       → https
 *   Host         → example.com
 *   Path         → /api/customers/5       ← used to find the controller
 *   Query String → search=abc             ← passed as method parameter
 *
 * Routing Middleware uses the PATH + HTTP METHOD to find
 * which controller action should handle this request.
*/

// ================================================================
// STEP 3: ENDPOINT MATCHING — Find the right controller action
// ================================================================
/*
 * ASP.NET Core keeps a list of all registered endpoints (routes).
 * It compares the incoming path against all route patterns.
 *
 * Registered routes in this app:
 *   GET  /api/customers         → GetAll()
 *   GET  /api/customers/{id}    → GetById(int id)    ← MATCH! id=5
 *   POST /api/customers         → Create()
 *   PUT  /api/customers/{id}    → Update(int id)
 *   DELETE /api/customers/{id}  → Delete(int id)
 *
 * For GET /api/customers/5:
 *   ✅ Pattern match: /api/customers/{id} → id = 5
 *   ✅ Method match:  GET
 *   → Selected endpoint: GetById(int id)
 *   → Route values stored: { controller="Customers", action="GetById", id="5" }
 *
 * If NO match found:
 *   → Pipeline continues with no endpoint assigned
 *   → Framework returns 404 Not Found
*/

// ================================================================
// STEP 4: POLICY MIDDLEWARES RUN (Auth, CORS, Rate Limiting)
// These run AFTER routing found the endpoint but BEFORE it executes
// ================================================================
/*
 * Now that we know WHICH endpoint was matched, policy middlewares
 * check if this request is ALLOWED to reach that endpoint.
 *
 * ORDER in Program.cs matters — they run top to bottom:
 *
 * ┌─────────────────────────────────────────────────────────────┐
 * │ app.UseCors()          → Is this origin allowed?            │
 * │ app.UseAuthentication() → Who are you? (reads auth header)  │
 * │ app.UseAuthorization()  → Are you allowed? (checks [Authorize]) │
 * │ app.UseRateLimiter()    → Too many requests? (429)          │
 * └─────────────────────────────────────────────────────────────┘
 *
 * UseAuthentication():
 *   Reads the Authorization header
 *   Decodes credentials → populates HttpContext.User with claims
 *   Does NOT block the request — just identifies the user
 *
 * UseAuthorization():
 *   Checks if HttpContext.User has permission for this endpoint
 *   Endpoint has [Authorize] → user must be authenticated
 *   Endpoint has [Authorize(Roles="Admin")] → user must have Admin role
 *   ❌ Not authenticated → 401 Unauthorized
 *   ❌ Wrong role        → 403 Forbidden
 *   ✅ Allowed           → request continues
 *
 * UseRateLimiter():
 *   Counts requests from this client in the time window
 *   ❌ Exceeded limit → 429 Too Many Requests
 *   ✅ Within limit   → request continues
*/

// ================================================================
// STEP 5: ENDPOINT EXECUTION — Controller runs
// ================================================================
/*
 * The request finally reaches the controller action.
 * The framework performs these steps automatically:
 *
 * 1. CONTROLLER CREATION
 *    DI container creates an instance of CustomersController
 *    Constructor parameters (ICustomerService etc.) are auto-injected
 *
 * 2. MODEL BINDING
 *    Framework extracts values from:
 *      Route      → /api/customers/5     → id = 5
 *      Query      → ?search=abc          → search = "abc"
 *      Body       → { "name": "John" }   → Customer object
 *      Headers    → Authorization: ...   → used by auth handler
 *    These are mapped to your method parameters automatically
 *
 * 3. MODEL VALIDATION
 *    [Required], [Range], [StringLength] attributes are checked
 *    ❌ Validation fails → 400 Bad Request (automatic with [ApiController])
 *    ✅ Validation passes → action method runs
 *
 * 4. FILTER PIPELINE (runs around the action)
 *    Authorization Filters  → final auth check
 *    Resource Filters       → caching, performance
 *    Action Filters         → before/after action logic
 *    Exception Filters      → catch unhandled errors
 *
 * 5. ACTION METHOD RUNS
 *    Your actual business logic executes here
 *    Calls services, reads/writes database, processes data
*/

// ================================================================
// STEP 6: RESPONSE GENERATION
// ================================================================
/*
 * After the action runs, it returns a result:
 *
 *   return Ok(customer)         → 200 OK      + JSON body
 *   return NotFound()           → 404 Not Found
 *   return BadRequest()         → 400 Bad Request
 *   return CreatedAtAction(...) → 201 Created  + Location header
 *   return NoContent()          → 204 No Content
 *   return Forbid()             → 403 Forbidden
 *
 * Framework sets:
 *   Status Code   → 200, 201, 400, 404, 500 etc.
 *   Headers       → Content-Type: application/json
 *   Body          → serialized JSON data
 *
 * Response travels back through middleware (in reverse order)
 * → back to Kestrel web server → back to the client
*/

// ================================================================
// FULL PIPELINE VISUAL SUMMARY
// ================================================================
/*
 *
 *  CLIENT REQUEST
 *       │
 *       ▼
 *  ┌─────────────────────────────────────────┐
 *  │  Custom Middleware (app.Use)            │  ← Logs request coming in
 *  │  app.UseCors()                          │  ← Is origin allowed?
 *  │  app.UseRouting()                       │  ← Match URL to endpoint
 *  │       │                                 │
 *  │       ├── No match → 404               │
 *  │       │                                 │
 *  │       └── Match found ↓                │
 *  │  app.UseAuthentication()               │  ← Who are you?
 *  │       ├── Fail → 401 Unauthorized      │
 *  │       └── Pass ↓                       │
 *  │  app.UseAuthorization()                │  ← Are you allowed?
 *  │       ├── Fail → 403 Forbidden         │
 *  │       └── Pass ↓                       │
 *  │  app.UseRateLimiter()                  │  ← Too many requests?
 *  │       ├── Fail → 429 Too Many          │
 *  │       └── Pass ↓                       │
 *  │  app.MapControllers()                  │  ← Run the action
 *  └─────────────────────────────────────────┘
 *       │
 *       ▼
 *  ┌─────────────────────────────────────────┐
 *  │  CONTROLLER                             │
 *  │  1. DI injects services                 │
 *  │  2. Model Binding (route/query/body)    │
 *  │  3. Model Validation ([Required] etc.)  │
 *  │       ├── Fail → 400 Bad Request        │
 *  │       └── Pass ↓                        │
 *  │  4. Filter Pipeline                     │
 *  │  5. Action Method runs                  │
 *  └─────────────────────────────────────────┘
 *       │
 *       ▼
 *  ┌─────────────────────────────────────────┐
 *  │  RESPONSE                               │
 *  │  Status Code + Headers + JSON Body      │
 *  │  Travels back through middleware        │
 *  └─────────────────────────────────────────┘
 *       │
 *       ▼
 *  CLIENT RECEIVES RESPONSE
 *
*/

// ================================================================
// MODEL
// ================================================================
#region Model

namespace RequestPipeline.Models
{
    public class Customer
    {
        public int Id { get; set; }

        // [Required] = ModelBinding checks this during Step 5
        // If name is missing → ModelState.IsValid = false → 400
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
        public int Age { get; set; }
    }
}

#endregion

// ================================================================
// CONTROLLER — Shows each step with comments
// ================================================================
#region Controller

namespace RequestPipeline.Controllers
{
    // [ApiController]
    //   → Enables automatic model validation (Step 5, point 3)
    //   → Without this, you must manually check ModelState
    //
    // [Route("api/[controller]")]
    //   → Registered as endpoint during Step 3
    //   → CustomersController → /api/customers
    //
    // [Authorize]
    //   → Checked during Step 4 (UseAuthorization)
    //   → All methods require valid authentication
    //
    // [EnableRateLimiting("fixed")]
    //   → Checked during Step 4 (UseRateLimiter)
    //   → All methods limited to 5 req/min

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class CustomersController : ControllerBase
    {
        // Step 5.1 — DI Container injects this service automatically
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // ── GET /api/customers ────────────────────────────────────────
        // Step 3: Pattern = /api/customers, Method = GET → matches here
        // Step 4: [Authorize] checked → must be logged in
        // Step 5: No model binding needed (no parameters)
        // Step 6: Returns 200 OK with list of customers
        [HttpGet]
        public ActionResult<IEnumerable<RequestPipeline.Models.Customer>> GetAll()
        {
            return Ok(_customerService.GetAll());
        }

        // ── GET /api/customers/5 ──────────────────────────────────────
        // Step 3: Pattern = /api/customers/{id:int}, id=5 extracted
        // Step 5.2 Model Binding: id=5 bound from route automatically
        // Step 6: Returns 200 OK or 404 Not Found
        [HttpGet("{id:int}")]
        public ActionResult<RequestPipeline.Models.Customer> GetById(int id)
        {
            // id came from URL path /api/customers/5 → id = 5
            var customer = _customerService.GetById(id);

            if (customer is null)
                return NotFound(new { message = $"Customer {id} not found" });

            return Ok(customer);
        }

        // ── GET /api/customers?search=abc ─────────────────────────────
        // Step 5.2 Model Binding: search="abc" bound from QUERY STRING
        // URL: /api/customers/search?name=john&maxAge=30
        [HttpGet("search")]
        public ActionResult<IEnumerable<RequestPipeline.Models.Customer>> Search(
            string? name,       // ← from ?name=john
            int? maxAge)        // ← from ?maxAge=30
        {
            var results = _customerService.Search(name, maxAge);
            return Ok(results);
        }

        // ── POST /api/customers ───────────────────────────────────────
        // Step 5.2 Model Binding: Customer object bound from REQUEST BODY
        // Step 5.3 Validation:
        //   [Required], [EmailAddress], [Range] checked automatically
        //   Bad body  → 400 Bad Request (automatic, no code needed)
        //   Good body → action runs
        // Step 6: Returns 201 Created with Location header
        [HttpPost]
        [Authorize(Roles = "Admin")]    // Only Admin role — checked in Step 4
        public ActionResult<RequestPipeline.Models.Customer> Create(
            [FromBody] RequestPipeline.Models.Customer customer)  // ← from JSON body
        {
            // [ApiController] already rejected bad data before reaching here
            // So at this point, customer is guaranteed to be valid
            var created = _customerService.Add(customer);

            // 201 Created + Location: /api/customers/{newId}
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ── PUT /api/customers/5 ──────────────────────────────────────
        // Two sources of model binding:
        //   id     → from ROUTE  /api/customers/5
        //   customer → from BODY  { "name": "John" }
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public ActionResult<RequestPipeline.Models.Customer> Update(
            int id,                                                // ← from route
            [FromBody] RequestPipeline.Models.Customer customer)  // ← from body
        {
            var updated = _customerService.Update(id, customer);

            if (updated is null)
                return NotFound(new { message = $"Customer {id} not found" });

            return Ok(updated);
        }

        // ── DELETE /api/customers/5 ───────────────────────────────────
        // Step 6: Returns 204 No Content (success, nothing to return)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            var deleted = _customerService.Delete(id);

            if (!deleted)
                return NotFound(new { message = $"Customer {id} not found" });

            return NoContent();   // 204 — success but no body
        }

        // ── Health check — no auth needed ─────────────────────────────
        // [AllowAnonymous] overrides class-level [Authorize]
        // Step 4: Authorization skipped for this endpoint
        [HttpGet("/health")]
        [AllowAnonymous]
        [DisableRateLimiting]
        public ActionResult Health() =>
            Ok(new { status = "Running", time = DateTime.UtcNow });
    }

    // ================================================================
    // SERVICE — Business logic layer (injected into controller)
    // ================================================================

    public interface ICustomerService
    {
        IEnumerable<RequestPipeline.Models.Customer> GetAll();
        IEnumerable<RequestPipeline.Models.Customer> Search(string? name, int? maxAge);
        RequestPipeline.Models.Customer? GetById(int id);
        RequestPipeline.Models.Customer Add(RequestPipeline.Models.Customer customer);
        RequestPipeline.Models.Customer? Update(int id, RequestPipeline.Models.Customer customer);
        bool Delete(int id);
    }

    public class CustomerService : ICustomerService
    {
        private readonly List<RequestPipeline.Models.Customer> _customers = new()
        {
            new() { Id = 1, Name = "Alice", Email = "alice@mail.com", Age = 28 },
            new() { Id = 2, Name = "Bob",   Email = "bob@mail.com",   Age = 35 },
            new() { Id = 3, Name = "Carol", Email = "carol@mail.com", Age = 22 },
        };

        private int _nextId = 4;

        public IEnumerable<RequestPipeline.Models.Customer> GetAll() => _customers;

        public IEnumerable<RequestPipeline.Models.Customer> Search(string? name, int? maxAge) =>
            _customers.Where(c =>
                (name == null || c.Name.Contains(name, StringComparison.OrdinalIgnoreCase)) &&
                (maxAge == null || c.Age <= maxAge));

        public RequestPipeline.Models.Customer? GetById(int id) =>
            _customers.FirstOrDefault(c => c.Id == id);

        public RequestPipeline.Models.Customer Add(RequestPipeline.Models.Customer customer)
        {
            customer.Id = _nextId++;
            _customers.Add(customer);
            return customer;
        }

        public RequestPipeline.Models.Customer? Update(int id, RequestPipeline.Models.Customer customer)
        {
            var existing = _customers.FirstOrDefault(c => c.Id == id);
            if (existing is null) return null;

            existing.Name = customer.Name;
            existing.Email = customer.Email;
            existing.Age = customer.Age;
            return existing;
        }

        public bool Delete(int id)
        {
            var customer = _customers.FirstOrDefault(c => c.Id == id);
            if (customer is null) return false;

            _customers.Remove(customer);
            return true;
        }
    }
}

#endregion

// ================================================================
// PROGRAM.CS — The middleware pipeline definition
// ================================================================
#region Program.cs

namespace RequestPipeline
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Register Services (DI Container) ──────────────────────
            builder.Services.AddControllers();
            builder.Services.AddSingleton<Controllers.ICustomerService, Controllers.CustomerService>();
            builder.Services.AddAuthentication("BasicAuth");   // your BasicAuthHandler here
            builder.Services.AddAuthorization();
            builder.Services.AddCors(o => o.AddPolicy("AllowAll",
                p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
            builder.Services.AddRateLimiter(o =>
                o.AddFixedWindowLimiter("fixed", opt =>
                {
                    opt.PermitLimit = 5;
                    opt.Window = TimeSpan.FromMinutes(1);
                }));

            var app = builder.Build();

            // ================================================================
            // MIDDLEWARE PIPELINE — ORDER IS CRITICAL
            // Each request passes through these in TOP → BOTTOM order
            // Each response passes back in BOTTOM → TOP order
            // ================================================================

            // STEP 2 happens here ↓
            // Custom logger — wraps everything, first in / last out
            app.Use(async (context, next) =>
            {
                // Runs on the way IN (request)
                Console.WriteLine($"[→ IN]  {context.Request.Method} {context.Request.Path}");

                await next();   // pass to next middleware

                // Runs on the way OUT (response)
                Console.WriteLine($"[← OUT] {context.Response.StatusCode}");
            });

            // STEP 2 — CORS: check origin before anything else
            app.UseCors("AllowAll");

            // STEP 3 — ROUTING: match URL to an endpoint
            // After this line, HttpContext knows which controller action was matched
            app.UseRouting();

            // STEP 4 — POLICY MIDDLEWARES (run after routing found the endpoint)

            // Reads Authorization header → populates HttpContext.User
            // Does NOT block — just identifies who the user is
            app.UseAuthentication();

            // Checks [Authorize] on the matched endpoint
            // ❌ No valid user  → 401 Unauthorized
            // ❌ Wrong role     → 403 Forbidden
            // ✅ Authorized     → continues
            app.UseAuthorization();

            // Checks request count for this client
            // ❌ Too many requests → 429 Too Many Requests
            // ✅ Within limit     → continues
            app.UseRateLimiter();

            // STEP 5 + 6 — ENDPOINT EXECUTION
            // Runs controller action → generates response
            app.MapControllers();

            app.Run();
        }
    }
}

#endregion

// ================================================================
// ALL STATUS CODES — What each step can return
// ================================================================
/*
 * ┌──────┬─────────────────────┬────────────────────────────────────────┐
 * │ Code │ Name                │ When it happens                        │
 * ├──────┼─────────────────────┼────────────────────────────────────────┤
 * │ 200  │ OK                  │ Request succeeded, returns data        │
 * │ 201  │ Created             │ POST succeeded, new resource created   │
 * │ 204  │ No Content          │ DELETE succeeded, nothing to return    │
 * │ 400  │ Bad Request         │ Model validation failed                │
 * │ 401  │ Unauthorized        │ No valid credentials (auth failed)     │
 * │ 403  │ Forbidden           │ Valid user but wrong role              │
 * │ 404  │ Not Found           │ No route match OR resource missing     │
 * │ 429  │ Too Many Requests   │ Rate limit exceeded                    │
 * │ 500  │ Internal Server Err │ Unhandled exception in action          │
 * └──────┴─────────────────────┴────────────────────────────────────────┘
 *
 * POSTMAN TESTING:
 * ─────────────────
 * GET    http://localhost:5000/api/customers         → 200 all customers
 * GET    http://localhost:5000/api/customers/1       → 200 one customer
 * GET    http://localhost:5000/api/customers/999     → 404 not found
 * GET    http://localhost:5000/api/customers/search?name=alice&maxAge=30 → 200
 * POST   http://localhost:5000/api/customers         → 201 created
 * PUT    http://localhost:5000/api/customers/1       → 200 updated
 * DELETE http://localhost:5000/api/customers/1       → 204 deleted
 * GET    http://localhost:5000/health                → 200 (no auth needed)
 *
 * Auth: Authorization tab → Basic Auth
 *   Username: admin  → Admin role  → all endpoints work
 *   Username: user   → ApiUser     → GET only, POST/PUT/DELETE = 403
 *   Password: MySuperSecretKey123!
*/