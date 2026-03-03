// ============================================================
//  FILE: RoutePrefixInAspNetCore.cs
//  TOPIC: Route Prefix in ASP.NET Core Web API
//  AUTHOR: Senior .NET Developer (15+ Years Experience)
//  AUDIENCE: Freshers → Experienced Developers
// ============================================================
//
//  TABLE OF CONTENTS
//  -----------------
//  1.  What is a Route?  (Fresher Level)
//  2.  What is a Route PREFIX? (Fresher Level)
//  3.  [Route] Attribute – Controller Level (Basic)
//  4.  [Route] with [controller] token (Intermediate)
//  5.  [Route] with [action] token (Intermediate)
//  6.  Versioned Route Prefix  (Experienced)
//  7.  Combining Controller + Action + Parameter Routes
//  8.  Route Prefix via Conventions (MapControllerRoute)
//  9.  Route Prefix with Areas
//  10. Attribute Routing vs Convention Routing – when to use what
//  11. Common Mistakes & Best Practices
//
// ============================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

// ──────────────────────────────────────────────────────────────
// SECTION 1 – What is a Route? (Fresher Level)
// ──────────────────────────────────────────────────────────────
// A ROUTE is the URL pattern that maps an HTTP request to a
// specific Controller Action (method).
//
//  Browser sends:  GET  https://localhost:5000/api/products
//                                              ↑
//                                        This part is the ROUTE
//
// The framework reads the URL and decides which C# method to call.

// ──────────────────────────────────────────────────────────────
// SECTION 2 – What is a Route PREFIX? (Fresher Level)
// ──────────────────────────────────────────────────────────────
// A ROUTE PREFIX is a common URL segment placed at the TOP of a
// controller so that every action inside that controller
// automatically inherits it.
//
//  Instead of writing:
//      [Route("api/products")]       ← repeated on every action
//      [Route("api/products/{id}")]  ← repeated again
//
//  You write ONCE at the controller level:
//      [Route("api/products")]       ← PREFIX (defined ONCE)
//
//  Then each action adds only its own unique piece:
//      [HttpGet]          → GET  api/products
//      [HttpGet("{id}")]  → GET  api/products/5
//      [HttpPost]         → POST api/products
//
// Think of it like a FOLDER path:
//   Route Prefix = "C:\Users\John\"
//   Action Route = "Documents\file.txt"
//   Full Path    = "C:\Users\John\Documents\file.txt"

// ──────────────────────────────────────────────────────────────
// SECTION 3 – [Route] Attribute at Controller Level (Basic)
// ──────────────────────────────────────────────────────────────

namespace Demo.Section3_BasicRoutePrefix
{
    // [Route("api/products")] is the ROUTE PREFIX.
    // Every action in this controller will start with "api/products".
    [ApiController]
    [Route("api/products")]          // ← ROUTE PREFIX
    public class ProductsController : ControllerBase
    {
        // GET api/products
        // Full URL = prefix + nothing extra
        [HttpGet]
        public IActionResult GetAll() =>
            Ok("Returns all products");

        // GET api/products/5
        // Full URL = prefix + "/{id}"
        [HttpGet("{id}")]
        public IActionResult GetById(int id) =>
            Ok($"Returns product with id = {id}");

        // POST api/products
        [HttpPost]
        public IActionResult Create([FromBody] object product) =>
            Created("", product);

        // PUT api/products/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] object product) =>
            Ok($"Updated product {id}");

        // DELETE api/products/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) =>
            Ok($"Deleted product {id}");
    }
}

// ──────────────────────────────────────────────────────────────
// SECTION 4 – [Route] with [controller] Token (Intermediate)
// ──────────────────────────────────────────────────────────────
// ASP.NET Core has a special token [controller] that is replaced
// at runtime with the controller class name (minus "Controller").
//
//  Class Name: OrdersController
//  [controller] resolves to: "orders"  (lowercase by default)
//
// BENEFIT: If you rename the class, the route updates automatically.
//          No need to manually update the [Route] string.

namespace Demo.Section4_ControllerToken
{
    [ApiController]
    [Route("api/[controller]")]      // ← [controller] = "orders"
    public class OrdersController : ControllerBase
    {
        // GET api/orders
        [HttpGet]
        public IActionResult GetAll() => Ok("All orders");

        // GET api/orders/10
        [HttpGet("{id:int}")]        // ← :int is a ROUTE CONSTRAINT
        public IActionResult GetById(int id) => Ok($"Order {id}");

        // GET api/orders/customer/42
        // Adding extra segments after the prefix
        [HttpGet("customer/{customerId}")]
        public IActionResult GetByCustomer(int customerId) =>
            Ok($"Orders for customer {customerId}");
    }

    // -------------------------------------------------------
    // ANOTHER EXAMPLE – rename-safe prefix
    // If we rename this to InvoicesController, route becomes
    // "api/invoices" automatically. No code change needed!
    // -------------------------------------------------------
    [ApiController]
    [Route("api/[controller]")]      // resolves to "api/invoices"
    public class InvoicesController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("All invoices");
    }
}

// ──────────────────────────────────────────────────────────────
// SECTION 5 – [Route] with [action] Token (Intermediate)
// ──────────────────────────────────────────────────────────────
// [action] resolves to the method name (lowercase).
// Useful when you don't want to manually specify action segments.
//
//  Method Name: GetSummary
//  [action] resolves to: "getsummary"
//
// NOTE: This pattern is more common in MVC (views) than Web API.
//       In Web API, prefer explicit HTTP verb attributes instead.

namespace Demo.Section5_ActionToken
{
    [ApiController]
    [Route("api/[controller]/[action]")]   // prefix + action name
    public class ReportsController : ControllerBase
    {
        // GET api/reports/getsummary
        [HttpGet]
        public IActionResult GetSummary() => Ok("Summary report");

        // GET api/reports/getdetails
        [HttpGet]
        public IActionResult GetDetails() => Ok("Detailed report");

        // POST api/reports/generate
        [HttpPost]
        public IActionResult Generate() => Ok("Report generated");
    }
}

// ──────────────────────────────────────────────────────────────
// SECTION 6 – Versioned Route Prefix  (Experienced)
// ──────────────────────────────────────────────────────────────
// Real-world APIs need versioning. Route prefix is the CLEANEST
// way to manage breaking changes while keeping old clients working.
//
//  v1: api/v1/customers  → original behaviour
//  v2: api/v2/customers  → new behaviour (breaking change)

namespace Demo.Section6_VersionedPrefix
{
    // VERSION 1 – Original contract, never change this!
    [ApiController]
    [Route("api/v1/[controller]")]   // prefix = "api/v1/customers"
    public class CustomersV1Controller : ControllerBase
    {
        // GET api/v1/customers
        [HttpGet]
        public IActionResult Get() =>
            Ok(new { version = "v1", data = "Basic customer list" });
    }

    // VERSION 2 – New contract with extra fields / new behaviour
    [ApiController]
    [Route("api/v2/[controller]")]   // prefix = "api/v2/customers"
    public class CustomersV2Controller : ControllerBase
    {
        // GET api/v2/customers
        [HttpGet]
        public IActionResult Get() =>
            Ok(new { version = "v2", data = "Enhanced customer list with pagination" });

        // GET api/v2/customers/5/orders
        // Nested resource – only exists in v2
        [HttpGet("{id}/orders")]
        public IActionResult GetOrders(int id) =>
            Ok($"Orders for customer {id} (v2 only)");
    }

    // -------------------------------------------------------
    // PRO TIP: For large projects, use the
    // Microsoft.AspNetCore.Mvc.Versioning NuGet package which
    // provides [ApiVersion] attribute and centralised version
    // management.  The Route Prefix approach shown here is the
    // SIMPLEST and works without any extra package.
    // -------------------------------------------------------
}

// ──────────────────────────────────────────────────────────────
// SECTION 7 – Combining Controller + Action + Parameter Routes
// ──────────────────────────────────────────────────────────────
// Real APIs combine all of the above. This is what you'll see
// in production codebases every day.

namespace Demo.Section7_CombinedRoutes
{
    // Route Prefix: api/v1/catalog/products
    [ApiController]
    [Route("api/v1/catalog/[controller]")]
    public class ProductsController : ControllerBase
    {
        // GET  api/v1/catalog/products
        [HttpGet]
        public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
            Ok($"Page {page}, Size {pageSize}");

        // GET  api/v1/catalog/products/5
        [HttpGet("{id:int}")]               // :int = only match integers
        public IActionResult GetById(int id) => Ok($"Product {id}");

        // GET  api/v1/catalog/products/slug/wireless-headphones
        [HttpGet("slug/{slug}")]            // custom sub-route on top of prefix
        public IActionResult GetBySlug(string slug) => Ok($"Product slug: {slug}");

        // GET  api/v1/catalog/products/5/reviews
        [HttpGet("{id:int}/reviews")]       // nested resource
        public IActionResult GetReviews(int id) => Ok($"Reviews for product {id}");

        // POST api/v1/catalog/products
        [HttpPost]
        public IActionResult Create([FromBody] object dto) => Created("", dto);

        // PUT  api/v1/catalog/products/5
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] object dto) => NoContent();

        // PATCH api/v1/catalog/products/5/price
        [HttpPatch("{id:int}/price")]       // partial update sub-resource
        public IActionResult UpdatePrice(int id, [FromBody] object priceDto) => Ok();

        // DELETE api/v1/catalog/products/5
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id) => NoContent();
    }
}

// ──────────────────────────────────────────────────────────────
// SECTION 8 – Route Prefix via Conventions (MapControllerRoute)
// ──────────────────────────────────────────────────────────────
// This is the OLDER, Convention-Based approach.
// Used in MVC applications and older Web API patterns.
// Attribute Routing (Sections 3-7) is PREFERRED for Web APIs.

namespace Demo.Section8_ConventionRouting
{
    public class ConventionRoutingStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                // Convention-based: adds "api/" prefix globally
                // Template: api/{controller}/{action}/{id?}
                endpoints.MapControllerRoute(
                    name: "api",
                    pattern: "api/{controller}/{action}/{id?}");

                // Default MVC route
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    // With convention routing the controller does NOT need [Route] attribute.
    // The template above automatically makes:
    //   /api/employees/getall
    //   /api/employees/getbyid/5
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        public IActionResult GetAll() => Ok("All employees (convention routing)");
        public IActionResult GetById(int id) => Ok($"Employee {id}");
    }
}

// ──────────────────────────────────────────────────────────────
// SECTION 9 – Route Prefix with AREAS
// ──────────────────────────────────────────────────────────────
// Areas let you split a large application into named sections.
// Each area gets its own route prefix automatically.
//
//  Area: Admin  → routes under /admin/...
//  Area: Store  → routes under /store/...

namespace Demo.Section9_Areas
{
    // Decorate with [Area] so the area name is injected into routing
    [Area("admin")]
    [ApiController]
    [Route("[area]/api/[controller]")]    // → admin/api/users
    public class UsersController : ControllerBase
    {
        // GET admin/api/users
        [HttpGet]
        public IActionResult GetAll() => Ok("Admin: all users");

        // POST admin/api/users
        [HttpPost]
        public IActionResult Create() => Ok("Admin: user created");
    }

    [Area("store")]
    [ApiController]
    [Route("[area]/api/[controller]")]    // → store/api/products
    public class StoreProductsController : ControllerBase
    {
        // GET store/api/storeproducts
        [HttpGet]
        public IActionResult GetAll() => Ok("Store: all products");
    }
    // Registration in Program.cs:
    // app.UseEndpoints(e => {
    //     e.MapControllerRoute("areas", "{area:exists}/{controller}/{action}/{id?}");
    //     e.MapControllers();
    // });
}

// ──────────────────────────────────────────────────────────────
// SECTION 10 – Attribute vs Convention Routing – When to Use What
// ──────────────────────────────────────────────────────────────
//
//  ┌──────────────────────────┬────────────────────────────────┐
//  │  Attribute Routing       │  Convention-Based Routing      │
//  ├──────────────────────────┼────────────────────────────────┤
//  │ [Route] on controller    │ MapControllerRoute template    │
//  │ & action                 │ in Program.cs / Startup.cs     │
//  ├──────────────────────────┼────────────────────────────────┤
//  │ RECOMMENDED for Web API  │ Common in MVC with views       │
//  ├──────────────────────────┼────────────────────────────────┤
//  │ Explicit, readable,      │ Less repetition but harder     │
//  │ self-documenting         │ to see the full route at a     │
//  │                          │ glance in the controller       │
//  ├──────────────────────────┼────────────────────────────────┤
//  │ Easier to version        │ Versioning is awkward          │
//  ├──────────────────────────┼────────────────────────────────┤
//  │ Works great with Swagger │ Works with Swagger but needs   │
//  │ / OpenAPI out of the box │ extra configuration            │
//  └──────────────────────────┴────────────────────────────────┘
//
//  RULE OF THUMB:
//  → Building a REST API?          USE Attribute Routing ✅
//  → Building an MVC web app?      USE Convention Routing ✅
//  → Mixed? You CAN use both, but be consistent within each controller.

// ──────────────────────────────────────────────────────────────
// SECTION 11 – Common Mistakes & Best Practices
// ──────────────────────────────────────────────────────────────

namespace Demo.Section11_BestPractices
{
    // ❌ MISTAKE 1: Hardcoding controller name in prefix
    // If you rename the class, the route becomes stale.
    [Route("api/customers")]            // ← HARD-CODED name
    public class CustomersController_Wrong : ControllerBase { }

    // ✅ FIX: Use [controller] token – always in sync with class name
    [Route("api/[controller]")]         // ← DYNAMIC token
    public class CustomersController_Right : ControllerBase { }

    // ──────────────────────────────────────────────────────────
    // ❌ MISTAKE 2: Mixing convention and attribute routing
    //    on the SAME controller without understanding the rules.
    //
    //    If [Route] is on the controller, ALL action routes
    //    must be defined via attributes too.
    //    Convention routes will be IGNORED for that controller.
    // ──────────────────────────────────────────────────────────

    // ──────────────────────────────────────────────────────────
    // ❌ MISTAKE 3: Forgetting [ApiController] or [controller] base
    //
    //    Without [ApiController], you lose:
    //      - Automatic 400 Bad Request on model validation failure
    //      - [FromBody] inference
    //      - Problem Details responses
    //
    //    Always add [ApiController] to Web API controllers!
    // ──────────────────────────────────────────────────────────

    // ──────────────────────────────────────────────────────────
    // ✅ BEST PRACTICE: Use route constraints to avoid ambiguity
    // ──────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class BestPracticeController : ControllerBase
    {
        // :int  – only matches integers
        // :guid – only matches GUIDs
        // :alpha – only matches alphabetic strings
        // :minlength(3) – string must be at least 3 chars

        [HttpGet("{id:int}")]           // GET api/bestpractice/5
        public IActionResult GetById(int id) => Ok(id);

        [HttpGet("{code:alpha}")]       // GET api/bestpractice/ABC
        public IActionResult GetByCode(string code) => Ok(code);

        [HttpGet("{uid:guid}")]         // GET api/bestpractice/3fa85f64-...
        public IActionResult GetByGuid(Guid uid) => Ok(uid);
    }

    // ──────────────────────────────────────────────────────────
    // ✅ BEST PRACTICE: Consistent prefix structure for REST APIs
    //
    //   Pattern:  api/v{version}/{resource}
    //   Example:  api/v1/orders
    //             api/v1/orders/{id}
    //             api/v1/orders/{id}/items
    //
    // This is the standard followed in most enterprise .NET projects.
    // ──────────────────────────────────────────────────────────

    // ──────────────────────────────────────────────────────────
    // ✅ BEST PRACTICE: Register controllers in Program.cs
    // ──────────────────────────────────────────────────────────
    //
    //  var builder = WebApplication.CreateBuilder(args);
    //  builder.Services.AddControllers();          // register controllers
    //
    //  var app = builder.Build();
    //  app.UseRouting();
    //  app.MapControllers();                       // discover attribute routes
    //  app.Run();
}

// ──────────────────────────────────────────────────────────────
// QUICK REFERENCE CARD
// ──────────────────────────────────────────────────────────────
//
//  TOKEN         RESOLVES TO              EXAMPLE
//  ──────────    ────────────────────     ──────────────────────
//  [controller]  Controller class name    OrdersController → "orders"
//  [action]      Method name              GetAll → "getall"
//  [area]        Area name               Admin → "admin"
//
//  CONSTRAINT    MEANING                  EXAMPLE
//  ──────────    ────────────────────     ──────────────────────
//  :int          Integer only             {id:int}
//  :guid         GUID only                {uid:guid}
//  :bool         Boolean only             {flag:bool}
//  :alpha        Letters only             {code:alpha}
//  :min(n)       Min value (numeric)      {age:min(18)}
//  :max(n)       Max value (numeric)      {score:max(100)}
//  :minlength(n) Min string length        {name:minlength(3)}
//  :maxlength(n) Max string length        {tag:maxlength(20)}
//  :length(n)    Exact string length      {zip:length(5)}
//  :range(x,y)   Numeric range            {month:range(1,12)}
//  :regex(expr)  Regex pattern            {ssn:regex(^\\d{{3}}-\\d{{2}}-\\d{{4}}$)}
//
// ──────────────────────────────────────────────────────────────
// END OF FILE
// ──────────────────────────────────────────────────────────────
