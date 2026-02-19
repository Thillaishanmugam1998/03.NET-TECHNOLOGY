// ============================================================================================================================
// FILE: RoutingInAspNetCoreWebAPI.cs
// AUTHOR: Senior .NET Developer (15+ Years Experience)
// TOPIC: How Routing Works in ASP.NET Core Web API — From Startup to Request Execution
// AUDIENCE: Freshers + Experienced .NET Developers
// .NET VERSION: .NET 8 (concepts apply to .NET 6/7/8+)
// ============================================================================================================================
//
// TABLE OF CONTENTS
// -----------------
//  PART 1 : What Is Routing?
//  PART 2 : Core Routing Components (EndpointDataSource, RouteEndpoint, RouteMatcher, LinkGenerator)
//  PART 3 : Application Startup — How Routes Are BUILT & STORED
//  PART 4 : Middleware Pipeline — How a Request Flows
//  PART 5 : What Happens When a Client Sends a Request (Step-by-Step Internals)
//  PART 6 : Middleware Ordering in ASP.NET Core's Minimal Hosting Model
//  PART 7 : Conventional Routing vs Attribute Routing
//  PART 8 : Route Constraints, Defaults, and Optional Parameters
//  PART 9 : Route Parameters — Simple, Complex, Catch-All
//  PART 10: Minimal API Routing (Program.cs style — .NET 6+)
//  PART 11: Custom Middleware + Routing Together (Real-world example)
//  PART 12: Common Mistakes & Best Practices
// ============================================================================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

// ============================================================================================================================
// PART 1: WHAT IS ROUTING?
// ============================================================================================================================
//
//  Routing is the mechanism ASP.NET Core uses to MATCH incoming HTTP requests to the correct
//  endpoint (controller action, Razor page, Minimal API handler, etc.).
//
//  Think of it like a POST OFFICE:
//  - The incoming request is a LETTER.
//  - The URL + HTTP Method is the ADDRESS on the letter.
//  - The routing system is the SORTING MACHINE that reads the address
//    and delivers the letter to the correct HANDLER (endpoint).
//
//  Two phases in routing:
//  ┌────────────────────────────────────────────────────────────────────────────────┐
//  │  PHASE 1 — Route MATCHING  (UseRouting / EndpointRoutingMiddleware)           │
//  │     Looks at the incoming URL and HTTP method.                                │
//  │     Selects the best matching RouteEndpoint.                                  │
//  │     Stores the selected endpoint in HttpContext.Features                      │
//  │     (IEndpointFeature).                                                       │
//  │                                                                               │
//  │  PHASE 2 — Route EXECUTION (UseEndpoints / EndpointMiddleware)                │
//  │     Reads the endpoint from HttpContext.Features.                             │
//  │     Executes the endpoint delegate (calls your controller action).            │
//  └────────────────────────────────────────────────────────────────────────────────┘
//
// ============================================================================================================================

// ============================================================================================================================
// PART 2: CORE ROUTING COMPONENTS
// ============================================================================================================================
//
//  ┌─────────────────────────┬───────────────────────────────────────────────────────────────────────────────┐
//  │ Component               │ Role                                                                          │
//  ├─────────────────────────┼───────────────────────────────────────────────────────────────────────────────┤
//  │ EndpointDataSource      │ Holds the COMPLETE list of registered endpoints (built during startup).       │
//  │                         │ Think of it as the "route database."                                          │
//  ├─────────────────────────┼───────────────────────────────────────────────────────────────────────────────┤
//  │ RouteEndpoint           │ Represents a SINGLE endpoint — has a route pattern, HTTP method              │
//  │                         │ constraints, metadata (auth, CORS, etc.), and the RequestDelegate to invoke. │
//  ├─────────────────────────┼───────────────────────────────────────────────────────────────────────────────┤
//  │ RouteMatcher /          │ At runtime, for each incoming request, scans all RouteEndpoints and           │
//  │ EndpointSelector        │ determines the BEST MATCH using a priority algorithm.                        │
//  ├─────────────────────────┼───────────────────────────────────────────────────────────────────────────────┤
//  │ LinkGenerator           │ Reverse routing — given an endpoint name/action/controller, generates a URL. │
//  ├─────────────────────────┼───────────────────────────────────────────────────────────────────────────────┤
//  │ IEndpointRouteBuilder   │ The object you interact with in UseEndpoints / MapControllers / MapGet etc.   │
//  │                         │ to ADD endpoints to the data source.                                          │
//  ├─────────────────────────┼───────────────────────────────────────────────────────────────────────────────┤
//  │ RouteValueDictionary    │ A dictionary of parsed route values extracted from the URL segment.           │
//  │                         │ e.g. /api/products/42 → { controller="Products", action="Get", id=42 }       │
//  └─────────────────────────┴───────────────────────────────────────────────────────────────────────────────┘
//
//  Internal Inheritance Chain (simplified):
//
//      IEndpointMetadataCollection  ← metadata attached to each endpoint (auth, CORS, swagger, etc.)
//              ↑
//          Endpoint                 ← base class with DisplayName + RequestDelegate
//              ↑
//          RouteEndpoint            ← adds RoutePattern (parsed pattern) + Order (priority)
//
// ============================================================================================================================

// ============================================================================================================================
// PART 3: APPLICATION STARTUP — HOW ROUTES ARE BUILT & STORED
// ============================================================================================================================
//
//  When your ASP.NET Core app starts, here is EXACTLY what happens (step by step):
//
//  ┌──────────────────────────────────────────────────────────────────────────────────────────────┐
//  │  STEP 1: WebApplication.CreateBuilder(args) or Host.CreateDefaultBuilder(args)               │
//  │          Creates the DI container, configuration, logging.                                   │
//  │                                                                                              │
//  │  STEP 2: builder.Services.AddControllers()                                                   │
//  │          - Registers MVC services into the DI container.                                     │
//  │          - Internally calls AddMvcCore() which registers:                                    │
//  │              • ControllerFeatureProvider (scans assemblies for controllers)                  │
//  │              • ActionDescriptorCollectionProvider (builds list of all actions)               │
//  │              • IActionSelector (selects the right action at runtime)                         │
//  │                                                                                              │
//  │  STEP 3: var app = builder.Build()                                                           │
//  │          - Finalizes the DI container.                                                       │
//  │          - No routes are registered YET at this point.                                       │
//  │                                                                                              │
//  │  STEP 4: app.UseRouting()           ← Adds EndpointRoutingMiddleware to the pipeline         │
//  │                                                                                              │
//  │  STEP 5: app.UseAuthorization()     ← Runs BETWEEN routing and endpoint execution            │
//  │                                                                                              │
//  │  STEP 6: app.MapControllers()       ← THIS is where routes get registered                    │
//  │          - Internally calls ControllerActionEndpointDataSource                               │
//  │          - Scans all controllers/actions decorated with [Route], [HttpGet], etc.             │
//  │          - Creates a RouteEndpoint for EACH action.                                          │
//  │          - Adds them all to the EndpointDataSource.                                          │
//  │                                                                                              │
//  │  STEP 7: app.Run()                                                                           │
//  │          - Starts the Kestrel HTTP server.                                                   │
//  │          - The EndpointDataSource is now FROZEN (read-only at runtime).                      │
//  └──────────────────────────────────────────────────────────────────────────────────────────────┘
//
//  KEY INSIGHT: Routes are registered LAZILY — they are fully compiled into a
//  "DFA (Deterministic Finite Automaton)" matcher the FIRST time a request comes in.
//  After that, matching is O(log n) or better.
//
// ============================================================================================================================

// ============================================================================================================================
// PART 4: MIDDLEWARE PIPELINE — HOW A REQUEST FLOWS
// ============================================================================================================================
//
//  The middleware pipeline is a chain of components. Each one can:
//    - Process the request.
//    - Pass it to the NEXT middleware (call next(context)).
//    - Short-circuit the pipeline (NOT call next).
//    - Process the response on the WAY BACK OUT.
//
//  Visual representation:
//
//  HTTP Request
//       │
//       ▼
//  ┌────────────────────────────┐
//  │  1. ExceptionHandler       │  (wraps everything — catches unhandled exceptions)
//  └────────────┬───────────────┘
//               │
//  ┌────────────▼───────────────┐
//  │  2. HTTPS Redirection      │
//  └────────────┬───────────────┘
//               │
//  ┌────────────▼───────────────┐
//  │  3. Static Files           │  (short-circuits for .js/.css/.html — never reaches routing)
//  └────────────┬───────────────┘
//               │
//  ┌────────────▼───────────────────────────────────────────────────────────┐
//  │  4. UseRouting()  — EndpointRoutingMiddleware                          │
//  │     • Reads HttpContext.Request.Path + Method                          │
//  │     • Runs route matching against EndpointDataSource                   │
//  │     • Populates HttpContext.Features.Get<IEndpointFeature>().Endpoint  │
//  │     • Populates HttpContext.Request.RouteValues                        │
//  └────────────┬───────────────────────────────────────────────────────────┘
//               │
//  ┌────────────▼───────────────┐
//  │  5. CORS Middleware        │  (can inspect the matched endpoint's CORS policy)
//  └────────────┬───────────────┘
//               │
//  ┌────────────▼───────────────┐
//  │  6. Authentication         │
//  └────────────┬───────────────┘
//               │
//  ┌────────────▼───────────────┐
//  │  7. Authorization          │  (reads [Authorize] metadata from the matched endpoint)
//  └────────────┬───────────────┘
//               │
//  ┌────────────▼───────────────────────────────────────────────────────────┐
//  │  8. UseEndpoints() — EndpointMiddleware                                │
//  │     • Reads the endpoint from IEndpointFeature                         │
//  │     • Invokes endpoint.RequestDelegate(httpContext)                    │
//  │     • Which calls the MVC action invoker → your controller method      │
//  └────────────┴───────────────────────────────────────────────────────────┘
//               │
//          HTTP Response returned to client
//
// ============================================================================================================================

// ============================================================================================================================
// PART 5: WHAT HAPPENS WHEN A CLIENT SENDS A REQUEST — FULL INTERNAL WALK-THROUGH
// ============================================================================================================================
//
//  Example: GET /api/products/42
//
//  ────────────────────────────────────────────────────────────────────────────────
//  CLIENT → Kestrel (TCP/TLS)
//  ────────────────────────────────────────────────────────────────────────────────
//  1. Client sends: GET /api/products/42 HTTP/1.1
//  2. Kestrel parses the raw bytes into an HttpContext object.
//     HttpContext contains:
//       • HttpContext.Request.Method  = "GET"
//       • HttpContext.Request.Path    = "/api/products/42"
//       • HttpContext.Request.Headers = { ... }
//
//  ────────────────────────────────────────────────────────────────────────────────
//  Kestrel → Middleware Pipeline
//  ────────────────────────────────────────────────────────────────────────────────
//  3. UseRouting() middleware fires.
//
//     INTERNALLY:
//     a. EndpointRoutingMiddleware gets the CompositeEndpointDataSource
//        (which aggregates all registered EndpointDataSources).
//
//     b. It calls EndpointSelector.SelectAsync(httpContext, candidateSet).
//        The selector uses a compiled DFA tree to walk "/api/products/42"
//        segment by segment:
//          segment 1: "api"      → literal match
//          segment 2: "products" → literal match
//          segment 3: "42"       → matches route parameter {id:int}, constraint int passes ✓
//
//     c. Selects the best RouteEndpoint:
//          Pattern   : "api/products/{id:int}"
//          Controller: ProductsController
//          Action    : GetById(int id)
//          HTTP verb : GET ✓
//
//     d. Stores selected endpoint:
//          httpContext.Features.Set<IEndpointFeature>(new EndpointFeature { Endpoint = selectedEndpoint });
//
//     e. Populates route values:
//          httpContext.Request.RouteValues["id"] = "42"
//          httpContext.Request.RouteValues["controller"] = "Products"
//          httpContext.Request.RouteValues["action"] = "GetById"
//
//  4. Authorization middleware fires.
//     Reads metadata: endpoint.Metadata.GetMetadata<IAuthorizeData>()
//     If [Authorize] present and user not authenticated → 401/403 returned HERE.
//     Our action has no [Authorize] → passes through.
//
//  5. UseEndpoints() fires.
//     Reads: httpContext.GetEndpoint()  (reads IEndpointFeature)
//     Calls: await endpoint.RequestDelegate(httpContext)
//
//     INTERNALLY for MVC:
//     a. ControllerFactory creates an instance of ProductsController.
//        Injects constructor dependencies (e.g., IProductService) via DI.
//
//     b. Model binding:
//          ActionContext is created.
//          The value "42" from RouteValues["id"] is bound to int id parameter.
//
//     c. Action filters run (OnActionExecuting).
//
//     d. Your method executes: GetById(42) → returns Ok(product)
//
//     e. Action filters run (OnActionExecuted).
//
//     f. Result filters run (OnResultExecuting).
//
//     g. Result execution:
//          OkObjectResult → ObjectResultExecutor
//          → Content negotiation (picks JSON by default)
//          → JsonOutputFormatter serializes the product object
//          → Writes JSON to httpContext.Response.Body
//          → Sets Content-Type: application/json
//          → Sets Status: 200 OK
//
//  6. Response flows back UP through middleware (response phase).
//  7. Kestrel sends the HTTP response bytes back to the client.
//
// ============================================================================================================================

// ============================================================================================================================
// PART 6: MIDDLEWARE ORDERING IN ASP.NET CORE'S MINIMAL HOSTING MODEL
// ============================================================================================================================
//
//  In .NET 6+ Minimal Hosting Model (Program.cs without Startup.cs), ordering is CRITICAL.
//  Wrong order = subtle bugs (e.g., CORS not applied, Authorization bypassed).
//
//  CORRECT ORDER:
//
//  var builder = WebApplication.CreateBuilder(args);
//  builder.Services.AddControllers();
//  builder.Services.AddAuthentication(...);
//  builder.Services.AddAuthorization();
//
//  var app = builder.Build();
//
//  app.UseExceptionHandler("/error");        // 1. Must be FIRST — catches all exceptions
//  app.UseHsts();                            // 2. HTTPS security header
//  app.UseHttpsRedirection();               // 3. Redirect HTTP → HTTPS
//  app.UseStaticFiles();                    // 4. Serve .css/.js before routing overhead
//  app.UseRouting();                        // 5. ROUTE MATCHING — must come before Auth
//  app.UseCors();                           // 6. CORS — must be after UseRouting (needs endpoint metadata)
//                                           //    but before UseAuthorization
//  app.UseAuthentication();                 // 7. Who are you?
//  app.UseAuthorization();                  // 8. Are you allowed? (needs routing result + auth)
//  app.MapControllers();                    // 9. Register controller endpoints (also triggers UseEndpoints internally)
//  app.Run();
//
//
//  WHY DOES ORDER MATTER?
//
//  ┌─────────────────────────────┬─────────────────────────────────────────────────────────────────┐
//  │ Mistake                     │ Result                                                          │
//  ├─────────────────────────────┼─────────────────────────────────────────────────────────────────┤
//  │ UseAuthorization BEFORE     │ Authorization runs before routing selects an endpoint.          │
//  │ UseRouting                  │ It can't read endpoint's [Authorize] metadata → always 200 OK   │
//  │                             │ (security hole!) or always 401 depending on global policy.      │
//  ├─────────────────────────────┼─────────────────────────────────────────────────────────────────┤
//  │ UseCors BEFORE UseRouting   │ CORS can't read the endpoint's CORS policy metadata.            │
//  │                             │ Either all CORS fails or you fall back to global policy only.   │
//  ├─────────────────────────────┼─────────────────────────────────────────────────────────────────┤
//  │ UseStaticFiles AFTER        │ Static files go through full routing overhead unnecessarily.    │
//  │ UseRouting                  │ Performance degradation.                                        │
//  └─────────────────────────────┴─────────────────────────────────────────────────────────────────┘
//
//  NOTE: In .NET 6+ with app.MapControllers(), you do NOT need explicit UseEndpoints().
//  MapControllers() automatically adds the EndpointMiddleware at the end of the pipeline.
//  But if you do call UseEndpoints() explicitly, MapControllers() must be INSIDE it.
//
// ============================================================================================================================

// ============================================================================================================================
// PART 7: CONVENTIONAL ROUTING vs ATTRIBUTE ROUTING
// ============================================================================================================================

namespace RoutingDemo.ConventionalRouting
{
    // ─────────────────────────────────────────────────────────────────────────────
    // CONVENTIONAL ROUTING (mostly used in MVC apps, not Web API)
    // ─────────────────────────────────────────────────────────────────────────────
    //
    // Defined centrally in Program.cs or Startup.cs:
    //
    //   app.MapControllerRoute(
    //       name: "default",
    //       pattern: "{controller=Home}/{action=Index}/{id?}");
    //
    // For a request to GET /products/details/5:
    //   controller = "Products"   → ProductsController
    //   action     = "Details"    → Details(int id)
    //   id         = 5
    //
    // PROBLEMS with Conventional Routing in Web API:
    //  - Doesn't map well to REST (GET/POST/PUT/DELETE verbs on same URL)
    //  - Ambiguity when multiple actions have the same name
    //  - Therefore, Web API almost always uses ATTRIBUTE ROUTING.
    //
    // ─────────────────────────────────────────────────────────────────────────────
    // ATTRIBUTE ROUTING (recommended for Web API)
    // ─────────────────────────────────────────────────────────────────────────────
    //
    // Routes are defined directly ON the controller/action using attributes.
    // This makes routes explicit, predictable, and co-located with the code.

    [ApiController]
    [Route("api/[controller]")]   // "api/products" — [controller] token replaced with class name minus "Controller"
    public class ProductsController : ControllerBase
    {
        // GET api/products
        // Route: inherited from class → "api/products"
        // HTTP verb: [HttpGet] attribute
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(new[] { "Laptop", "Phone", "Tablet" });
        }

        // GET api/products/42
        // Full route: "api/products" + "/{id:int}" = "api/products/{id:int}"
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            return Ok(new { Id = id, Name = "Laptop" });
        }

        // POST api/products
        [HttpPost]
        public IActionResult Create([FromBody] string name)
        {
            return CreatedAtAction(nameof(GetById), new { id = 99 }, new { Id = 99, Name = name });
        }

        // PUT api/products/42
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] string name)
        {
            return Ok(new { Id = id, Name = name, Updated = true });
        }

        // DELETE api/products/42
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            return NoContent(); // 204
        }

        // ─────────────────────────────────────────────────────────────────────
        // OVERRIDING class-level route on a specific action
        // Full route: "api/featured" (ignores class-level "api/[controller]")
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("/api/featured")]  // Leading "/" means ABSOLUTE route — ignores [Route] on class
        public IActionResult GetFeatured()
        {
            return Ok("This is the featured product!");
        }

        // ─────────────────────────────────────────────────────────────────────
        // MULTIPLE ROUTES on one action
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("search")]          // GET api/products/search?q=laptop
        [HttpGet("find")]            // GET api/products/find?q=laptop  (same action, two URLs)
        public IActionResult Search([FromQuery] string q)
        {
            return Ok($"Searching for: {q}");
        }
    }
}

// ============================================================================================================================
// PART 8: ROUTE CONSTRAINTS, DEFAULTS, AND OPTIONAL PARAMETERS
// ============================================================================================================================

namespace RoutingDemo.Constraints
{
    [ApiController]
    [Route("api/[controller]")]
    public class DemoController : ControllerBase
    {
        // ─────────────────────────────────────────────────────────────────────
        // ROUTE CONSTRAINTS
        // ─────────────────────────────────────────────────────────────────────
        // Syntax: {paramName:constraintType}
        //
        //  ┌────────────────────┬───────────────────────────────────────────────────────┐
        //  │ Constraint         │ Example          │ Matches                            │
        //  ├────────────────────┼──────────────────┼────────────────────────────────────┤
        //  │ int                │ {id:int}         │ 42, -7, 0                          │
        //  │ long               │ {id:long}        │ 9999999999                         │
        //  │ double             │ {price:double}   │ 3.14                               │
        //  │ bool               │ {active:bool}    │ true, false                        │
        //  │ guid               │ {token:guid}     │ a1b2c3d4-...                       │
        //  │ datetime           │ {date:datetime}  │ 2024-01-15                         │
        //  │ alpha              │ {name:alpha}     │ only letters                       │
        //  │ length(n)          │ {code:length(5)} │ exactly 5 chars                    │
        //  │ minlength(n)       │ {code:minlength(2)} │ at least 2 chars               │
        //  │ maxlength(n)       │ {code:maxlength(10)} │ at most 10 chars              │
        //  │ min(n)             │ {age:min(18)}    │ integer >= 18                      │
        //  │ max(n)             │ {age:max(120)}   │ integer <= 120                     │
        //  │ range(min,max)     │ {score:range(1,100)} │ integer between 1 and 100     │
        //  │ regex(pattern)     │ {zip:regex(^\\d{{5}}$)} │ 5-digit zip code           │
        //  └────────────────────┴──────────────────┴────────────────────────────────────┘

        // Only matches if id is an integer
        [HttpGet("{id:int}")]
        public IActionResult GetByInt(int id) => Ok($"int id: {id}");

        // Only matches if id is a GUID
        [HttpGet("{id:guid}")]
        public IActionResult GetByGuid(Guid id) => Ok($"guid: {id}");

        // Range constraint: age must be 18–120
        [HttpGet("user/{age:range(18,120)}")]
        public IActionResult GetUserByAge(int age) => Ok($"age: {age}");

        // Regex constraint: zip code must be 5 digits
        [HttpGet("zip/{zip:regex(^\\d{{5}}$)}")]
        public IActionResult GetByZip(string zip) => Ok($"zip: {zip}");

        // ─────────────────────────────────────────────────────────────────────
        // OPTIONAL PARAMETERS: append "?" to the segment
        // Route matches BOTH:  GET api/demo/page        (pageNumber = null)
        //                      GET api/demo/page/3      (pageNumber = 3)
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("page/{pageNumber:int?}")]
        public IActionResult GetPage(int? pageNumber = 1)
        {
            return Ok($"Page: {pageNumber ?? 1}");
        }

        // ─────────────────────────────────────────────────────────────────────
        // DEFAULT VALUES in conventional routing (not directly in attribute routing)
        // In attribute routing, use optional parameters + default in method signature.
        // ─────────────────────────────────────────────────────────────────────

        // ─────────────────────────────────────────────────────────────────────
        // COMBINING CONSTRAINTS
        // ─────────────────────────────────────────────────────────────────────
        // {id:int:min(1):max(999)}  — must be int, between 1 and 999
        [HttpGet("item/{id:int:min(1):max(999)}")]
        public IActionResult GetItem(int id) => Ok($"item id (1-999): {id}");
    }
}

// ============================================================================================================================
// PART 9: ROUTE PARAMETERS — SIMPLE, COMPLEX, CATCH-ALL
// ============================================================================================================================

namespace RoutingDemo.RouteParameters
{
    [ApiController]
    [Route("api")]
    public class FileController : ControllerBase
    {
        // ─────────────────────────────────────────────────────────────────────
        // SIMPLE ROUTE PARAMETER
        // GET api/file/report.pdf
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("file/{name}")]
        public IActionResult GetFile(string name)
        {
            return Ok($"File: {name}");
        }

        // ─────────────────────────────────────────────────────────────────────
        // CATCH-ALL PARAMETER: {**rest} or {*rest}
        // Matches everything after the prefix, including "/" characters.
        //
        // GET api/files/2024/january/report.pdf
        //   → path = "2024/january/report.pdf"
        //
        // This is perfect for file paths, nested resource URLs, etc.
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("files/{**path}")]
        public IActionResult GetFilePath(string path)
        {
            // path = "2024/january/report.pdf"
            return Ok($"Full path: {path}");
        }

        // ─────────────────────────────────────────────────────────────────────
        // MULTIPLE PARAMETERS IN ONE SEGMENT
        // GET api/product/electronics-laptop-42
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("product/{category}-{name}-{id:int}")]
        public IActionResult GetProduct(string category, string name, int id)
        {
            return Ok(new { category, name, id });
        }

        // ─────────────────────────────────────────────────────────────────────
        // ROUTE VALUES vs QUERY STRING vs BODY
        //
        //  [FromRoute]  → from URL segment:  /api/orders/5
        //  [FromQuery]  → from query string: /api/orders?status=pending
        //  [FromBody]   → from request body: JSON/XML payload
        //  [FromHeader] → from HTTP header:  X-Api-Key: abc123
        //  [FromForm]   → from form data
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("orders/{id:int}")]
        public IActionResult GetOrder(
            [FromRoute] int id,
            [FromQuery] string status = "all",
            [FromHeader(Name = "X-Request-Id")] string? requestId = null)
        {
            return Ok(new { id, status, requestId });
        }
    }
}

// ============================================================================================================================
// PART 10: MINIMAL API ROUTING (.NET 6+ Program.cs style)
// ============================================================================================================================
//
//  In .NET 6+, you can register endpoints DIRECTLY in Program.cs without controllers.
//  These still go through the same EndpointDataSource / middleware pipeline.
//
//  Below is a complete minimal API example in a class to keep this file compilable.

namespace RoutingDemo.MinimalApi
{
    public static class MinimalApiExample
    {
        public static void RegisterRoutes(WebApplication app)
        {
            // ─────────────────────────────────────────────────────────────────
            // Basic verbs
            // ─────────────────────────────────────────────────────────────────
            app.MapGet("/api/hello", () => "Hello, World!");

            app.MapGet("/api/items/{id:int}", (int id) =>
                Results.Ok(new { id, name = "Sample" }));

            app.MapPost("/api/items", ([FromBody] string name) =>
                Results.Created($"/api/items/1", new { id = 1, name }));

            app.MapPut("/api/items/{id:int}", (int id, [FromBody] string name) =>
                Results.Ok(new { id, name, updated = true }));

            app.MapDelete("/api/items/{id:int}", (int id) =>
                Results.NoContent());

            // ─────────────────────────────────────────────────────────────────
            // Route Groups — reduces repetition (.NET 7+)
            // All routes inside automatically prefixed with "/api/v1/products"
            // ─────────────────────────────────────────────────────────────────
            var products = app.MapGroup("/api/v1/products")
                              .RequireAuthorization()          // apply to all in group
                              .WithTags("Products");            // Swagger tag for all

            products.MapGet("/", () => Results.Ok("All products"));
            products.MapGet("/{id:int}", (int id) => Results.Ok($"Product {id}"));
            products.MapPost("/", ([FromBody] string name) => Results.Created($"/api/v1/products/1", name));

            // ─────────────────────────────────────────────────────────────────
            // Named endpoints — for URL generation (reverse routing)
            // ─────────────────────────────────────────────────────────────────
            app.MapGet("/api/orders/{id:int}", (int id) =>
                Results.Ok(new { id }))
               .WithName("GetOrderById");  // Name used with LinkGenerator

            // ─────────────────────────────────────────────────────────────────
            // Using LinkGenerator to build URLs (reverse routing)
            // ─────────────────────────────────────────────────────────────────
            app.MapPost("/api/orders", (LinkGenerator linker) =>
            {
                int newId = 99; // pretend we created order 99
                // Generates: "/api/orders/99"
                string? url = linker.GetPathByName("GetOrderById", new { id = newId });
                return Results.Created(url ?? "/api/orders", new { id = newId });
            });

            // ─────────────────────────────────────────────────────────────────
            // Catch-all in Minimal API
            // ─────────────────────────────────────────────────────────────────
            app.MapGet("/api/docs/{**slug}", (string slug) =>
                Results.Ok($"Documentation path: {slug}"));
        }
    }
}

// ============================================================================================================================
// PART 11: CUSTOM MIDDLEWARE + ROUTING TOGETHER (Real-World Example)
// ============================================================================================================================

namespace RoutingDemo.CustomMiddleware
{
    // ─────────────────────────────────────────────────────────────────────────
    // Custom middleware that runs AFTER routing but BEFORE endpoint execution.
    // It can inspect the MATCHED endpoint (read metadata like [Authorize], custom attributes).
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Example: API Key validation middleware.
    /// Runs after UseRouting so it can check if the endpoint
    /// has [RequiresApiKey] metadata before enforcing the check.
    /// </summary>
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiKeyMiddleware> _logger;
        private const string API_KEY_HEADER = "X-Api-Key";
        private const string VALID_KEY = "my-secret-key"; // In real apps: read from configuration

        public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // ── Read the endpoint selected by UseRouting ──────────────────
            var endpoint = context.GetEndpoint();

            if (endpoint is null)
            {
                // No endpoint matched — let pipeline handle (404 later)
                await _next(context);
                return;
            }

            // ── Check if endpoint has our custom metadata ─────────────────
            var requiresKey = endpoint.Metadata.GetMetadata<RequiresApiKeyAttribute>();

            if (requiresKey is not null)
            {
                // This endpoint requires an API key
                if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var apiKey)
                    || apiKey != VALID_KEY)
                {
                    _logger.LogWarning("Unauthorized API key attempt for {Path}", context.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { error = "Invalid or missing API key." });
                    return; // Short-circuit — do NOT call _next
                }
            }

            // ── Valid or not required — continue pipeline ─────────────────
            await _next(context);
        }
    }

    // Custom attribute to mark endpoints requiring an API key
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiresApiKeyAttribute : Attribute { }

    // ─────────────────────────────────────────────────────────────────────────
    // Controller using the custom attribute
    // ─────────────────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class SecureDataController : ControllerBase
    {
        // Public — no API key needed
        [HttpGet("public")]
        public IActionResult GetPublic() => Ok("Anyone can see this.");

        // Requires API key (middleware reads this attribute)
        [HttpGet("private")]
        [RequiresApiKey]
        public IActionResult GetPrivate() => Ok("Super secret data! ✅");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Extension method for clean registration
    // ─────────────────────────────────────────────────────────────────────────
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKey(this IApplicationBuilder app)
            => app.UseMiddleware<ApiKeyMiddleware>();
    }
}

// ============================================================================================================================
// PART 12: COMMON MISTAKES & BEST PRACTICES
// ============================================================================================================================
//
//  ┌─────────────────────────────────────────────────────────────────────────────────────────────────────┐
//  │ MISTAKE 1: Ambiguous Routes                                                                         │
//  │                                                                                                     │
//  │  [HttpGet("{id}")]       → matches ANY string                                                       │
//  │  [HttpGet("{name}")]     → ALSO matches any string                                                  │
//  │                                                                                                     │
//  │  Result: AmbiguousMatchException at runtime!                                                        │
//  │                                                                                                     │
//  │  FIX: Use constraints to disambiguate:                                                              │
//  │  [HttpGet("{id:int}")]   → only integers                                                            │
//  │  [HttpGet("{name:alpha}")] → only letters                                                           │
//  └─────────────────────────────────────────────────────────────────────────────────────────────────────┘
//
//  ┌─────────────────────────────────────────────────────────────────────────────────────────────────────┐
//  │ MISTAKE 2: UseAuthorization before UseRouting                                                       │
//  │                                                                                                     │
//  │  app.UseAuthorization();  // ← WRONG! No endpoint selected yet                                     │
//  │  app.UseRouting();        //   Authorization can't read [Authorize] metadata                        │
//  │                                                                                                     │
//  │  FIX:                                                                                               │
//  │  app.UseRouting();        // First: select endpoint                                                 │
//  │  app.UseAuthorization();  // Then: check authorization metadata                                    │
//  └─────────────────────────────────────────────────────────────────────────────────────────────────────┘
//
//  ┌─────────────────────────────────────────────────────────────────────────────────────────────────────┐
//  │ MISTAKE 3: Forgetting [ApiController] attribute                                                     │
//  │                                                                                                     │
//  │  Without [ApiController]:                                                                           │
//  │    - [FromBody] is NOT automatically inferred for complex types                                     │
//  │    - Model validation errors do NOT automatically return 400                                        │
//  │    - [FromQuery]/[FromRoute] NOT automatically inferred                                             │
//  │                                                                                                     │
//  │  Always use [ApiController] on Web API controllers!                                                 │
//  └─────────────────────────────────────────────────────────────────────────────────────────────────────┘
//
//  ┌─────────────────────────────────────────────────────────────────────────────────────────────────────┐
//  │ MISTAKE 4: Mixing conventional + attribute routing on same controller                               │
//  │                                                                                                     │
//  │  If a controller has [Route] attribute, conventional routes are IGNORED for that controller.        │
//  │  You cannot mix both styles on one controller.                                                      │
//  └─────────────────────────────────────────────────────────────────────────────────────────────────────┘
//
//  ┌─────────────────────────────────────────────────────────────────────────────────────────────────────┐
//  │ MISTAKE 5: Not versioning APIs                                                                      │
//  │                                                                                                     │
//  │  Always version your APIs from day one:                                                             │
//  │  [Route("api/v{version:apiVersion}/[controller]")]                                                  │
//  │  or simply:                                                                                         │
//  │  [Route("api/v1/[controller]")]                                                                     │
//  └─────────────────────────────────────────────────────────────────────────────────────────────────────┘
//
//  ╔═════════════════════════════════════════════════════════════════════════════════════════╗
//  ║  BEST PRACTICES SUMMARY                                                                ║
//  ╠═════════════════════════════════════════════════════════════════════════════════════════╣
//  ║  ✅ Always use [ApiController] + [Route] on Web API controllers                        ║
//  ║  ✅ Use attribute routing for Web APIs (explicit and RESTful)                          ║
//  ║  ✅ Use [controller] and [action] tokens to reduce duplication                         ║
//  ║  ✅ Use route constraints ({id:int}) to avoid ambiguous matches                        ║
//  ║  ✅ Keep middleware in the correct order: Routing → CORS → Auth → Authorization        ║
//  ║  ✅ Use [FromRoute]/[FromQuery]/[FromBody] explicitly for clarity                      ║
//  ║  ✅ Version your APIs from day one: /api/v1/...                                        ║
//  ║  ✅ Use named endpoints + LinkGenerator for reverse routing (no hardcoded URLs)        ║
//  ║  ✅ Use MapGroup() in Minimal APIs to avoid route prefix repetition                   ║
//  ║  ✅ Place custom middleware that needs endpoint metadata AFTER UseRouting()             ║
//  ╚═════════════════════════════════════════════════════════════════════════════════════════╝

// ============================================================================================================================
// COMPLETE PROGRAM.CS EXAMPLE — Putting It All Together
// ============================================================================================================================

namespace RoutingDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Service Registration ─────────────────────────────────────────
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer(); // for Swagger
            builder.Services.AddSwaggerGen();
            builder.Services.AddAuthentication();       // configure your scheme
            builder.Services.AddAuthorization();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });
            builder.Services.AddLogging();

            var app = builder.Build();

            // ── Middleware Pipeline (ORDER MATTERS!) ─────────────────────────
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler("/error");       // 1. Exception handling first
                app.UseHsts();                           // 2. HTTPS security header
            }

            app.UseHttpsRedirection();                   // 3. HTTP → HTTPS
            app.UseStaticFiles();                        // 4. Static files (skip routing overhead)
            app.UseRouting();                            // 5. *** ROUTE MATCHING ***
            app.UseCors("AllowAll");                     // 6. CORS (after routing, needs endpoint metadata)
            app.UseAuthentication();                     // 7. Who are you?
            app.UseAuthorization();                      // 8. Are you allowed?

            // Custom middleware — placed AFTER UseRouting so it can read endpoint metadata
            app.UseApiKey();                             // 9. Custom: API key validation

            // ── Endpoint Registration ────────────────────────────────────────
            app.MapControllers();                        // 10. MVC Controller endpoints

            // Minimal API endpoints
            MinimalApi.MinimalApiExample.RegisterRoutes(app);

            // ── Health Check ────────────────────────────────────────────────
            app.MapGet("/health", () => Results.Ok(new { status = "Healthy", time = DateTime.UtcNow }))
               .AllowAnonymous()
               .WithTags("Health");

            app.Run();
        }
    }
}

// ============================================================================================================================
// QUICK REFERENCE CHEAT SHEET
// ============================================================================================================================
//
//  ROUTE TOKENS:
//    [controller]  → class name minus "Controller" suffix  (ProductsController → products)
//    [action]      → method name                           (GetById → GetById)
//    [area]        → area name (for MVC areas)
//
//  HTTP VERB ATTRIBUTES:
//    [HttpGet]     → GET
//    [HttpPost]    → POST
//    [HttpPut]     → PUT
//    [HttpPatch]   → PATCH
//    [HttpDelete]  → DELETE
//    [HttpHead]    → HEAD
//    [HttpOptions] → OPTIONS
//    [AcceptVerbs("GET","POST")] → multiple verbs
//
//  SOURCE BINDING:
//    [FromRoute]   → /api/items/{id}
//    [FromQuery]   → /api/items?page=2
//    [FromBody]    → JSON/XML body
//    [FromHeader]  → X-Custom-Header: value
//    [FromForm]    → multipart/form-data
//    [FromServices] → inject from DI inside action parameter
//
//  RESULT HELPERS (ControllerBase):
//    Ok(data)                → 200
//    Created(uri, data)      → 201
//    CreatedAtAction(...)    → 201 with Location header
//    Accepted()              → 202
//    NoContent()             → 204
//    BadRequest(errors)      → 400
//    Unauthorized()          → 401
//    Forbid()                → 403
//    NotFound()              → 404
//    Conflict()              → 409
//    UnprocessableEntity()   → 422
//    StatusCode(500, msg)    → any code
//
//  MINIMAL API RESULTS:
//    Results.Ok(data)        → 200
//    Results.Created(uri, data) → 201
//    Results.NoContent()     → 204
//    Results.NotFound()      → 404
//    Results.BadRequest(msg) → 400
//    Results.Problem(...)    → RFC 7807 ProblemDetails
//    Results.Json(obj)       → custom JSON response
//    Results.File(bytes,...) → file download
//    Results.Redirect(url)   → 302 redirect
//
// ============================================================================================================================
// END OF FILE
// ============================================================================================================================
