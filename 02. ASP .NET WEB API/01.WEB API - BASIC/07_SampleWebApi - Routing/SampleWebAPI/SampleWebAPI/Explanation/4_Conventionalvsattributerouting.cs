// ================================================================
// CONVENTIONAL ROUTING vs ATTRIBUTE ROUTING
// Complete Guide in One File — Simple and Clear
// ================================================================

// ================================================================
// SIMPLE EXPLANATION FIRST
// ================================================================
/*
 * There are TWO ways to define routes in ASP.NET Core:
 *
 * 1. CONVENTIONAL ROUTING
 *    → You define ONE template in Program.cs
 *    → ALL controllers follow that same template automatically
 *    → Like a COMPANY DRESS CODE — one rule, everyone follows it
 *
 * 2. ATTRIBUTE ROUTING
 *    → You define routes directly ON each controller / method
 *    → Each endpoint has its own specific URL
 *    → Like PERSONAL STYLE — each person chooses their own outfit
 *
 * ┌────────────────────┬──────────────────────┬────────────────────────┐
 * │                    │ Conventional          │ Attribute              │
 * ├────────────────────┼──────────────────────┼────────────────────────┤
 * │ Where defined      │ Program.cs            │ On controller/method   │
 * │ Control            │ Global (one rule)     │ Per endpoint           │
 * │ Used for           │ MVC websites          │ REST APIs              │
 * │ URL style          │ /Controller/Action/id │ /api/products/5        │
 * │ Flexibility        │ Less flexible         │ Very flexible          │
 * └────────────────────┴──────────────────────┴────────────────────────┘
*/

using Microsoft.AspNetCore.Mvc;

// ================================================================
// SECTION 1: CONVENTIONAL ROUTING
// ================================================================
/*
 * WHAT IS IT?
 * One URL template defined in Program.cs.
 * Framework automatically maps URL → Controller → Action → Id
 * by following the template pattern.
 *
 * HOW IT WORKS:
 * Template: "{controller=Home}/{action=Index}/{id?}"
 *
 * This means:
 *   1st segment → Controller name  (without "Controller" word)
 *   2nd segment → Action method name
 *   3rd segment → id parameter (optional — the ? means optional)
 *
 * URL EXAMPLES:
 *   /Products/Index        → ProductsController.Index()
 *   /Products/Details/5    → ProductsController.Details(5)
 *   /Orders/Create         → OrdersController.Create()
 *   /                      → HomeController.Index()  ← defaults
 *
 * The "=Home" and "=Index" are DEFAULT VALUES
 * If segment is missing, use this default:
 *   /             → controller=Home, action=Index
 *   /Products     → controller=Products, action=Index
 *   /Products/Details/5 → controller=Products, action=Details, id=5
*/

#region Conventional Routing — Program.cs Setup

/*
 * In Program.cs — define the template once:
 *
 *   app.MapControllerRoute(
 *       name: "default",
 *       pattern: "{controller=Home}/{action=Index}/{id?}"
 *   );
 *
 * You can add MULTIPLE templates for different URL styles:
 *
 *   // Template 1 — standard
 *   app.MapControllerRoute(
 *       name: "default",
 *       pattern: "{controller=Home}/{action=Index}/{id?}"
 *   );
 *
 *   // Template 2 — with area (e.g., Admin section)
 *   app.MapControllerRoute(
 *       name: "areas",
 *       pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
 *   );
 *
 *   // Template 3 — custom fixed prefix
 *   app.MapControllerRoute(
 *       name: "blog",
 *       pattern: "blog/{year}/{month}/{slug}",
 *       defaults: new { controller = "Blog", action = "Post" }
 *   );
*/

#endregion

#region Conventional Routing — Controller (NO route attributes needed)

namespace RoutingTypes.Conventional
{
    // ✅ NO [Route] attribute on the controller
    // ✅ NO [HttpGet] / [HttpPost] attributes on methods
    // The URL template in Program.cs handles everything automatically
    //
    // URL mapping (using template "{controller=Home}/{action=Index}/{id?}"):
    //   GET /Products/Index         → Index()
    //   GET /Products/Details/5     → Details(5)
    //   GET /Products/Create        → Create()
    //   POST /Products/Create       → Create(product)   ← same URL, different method
    //   GET /Products/Edit/3        → Edit(3)
    //   POST /Products/Edit/3       → Edit(3, product)

    public class ProductsController : Controller  // ← Controller (not ControllerBase)
    {
        // GET /Products/Index  OR  /Products  (Index is default action)
        public IActionResult Index()
        {
            // Returns a VIEW (HTML page) — conventional routing is mainly for MVC websites
            return View();
        }

        // GET /Products/Details/5
        // id comes from URL segment — bound automatically by the template
        public IActionResult Details(int id)
        {
            return View(id);
        }

        // GET /Products/Create  → shows the empty form
        public IActionResult Create()
        {
            return View();
        }

        // POST /Products/Create → receives the submitted form data
        [HttpPost]   // same URL as above but different HTTP method
        public IActionResult Create(string productName)
        {
            // save product...
            return RedirectToAction("Index");
        }

        // GET /Products/Edit/3
        public IActionResult Edit(int id)
        {
            return View(id);
        }

        // POST /Products/Edit/3
        [HttpPost]
        public IActionResult Edit(int id, string productName)
        {
            // update product...
            return RedirectToAction("Index");
        }

        // GET /Products/Delete/3
        public IActionResult Delete(int id)
        {
            return View(id);
        }
    }

    // Another controller — SAME template, different controller name
    // GET /Orders/Index      → Index()
    // GET /Orders/Details/7  → Details(7)
    public class OrdersController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult Details(int id) => View(id);
    }

    // GET /       → HomeController.Index()  (both segments use defaults)
    // GET /Home   → HomeController.Index()  (action uses default)
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult About() => View();  // GET /Home/About
        public IActionResult Contact() => View(); // GET /Home/Contact
    }
}

#endregion

// ================================================================
// SECTION 2: ATTRIBUTE ROUTING
// ================================================================
/*
 * WHAT IS IT?
 * Routes are defined DIRECTLY on controllers and action methods
 * using attributes like [Route], [HttpGet], [HttpPost] etc.
 *
 * HOW IT WORKS:
 * You explicitly write the URL for EACH controller and method.
 * No global template — each endpoint is fully in control of its URL.
 *
 * WHY USE IT?
 * → REST APIs need clean URLs like /api/products/5
 * → Full control over every URL
 * → Different methods can have completely different URL shapes
 * → Versioning is easy: /api/v1/products, /api/v2/products
*/

#region Attribute Routing — Controller

namespace RoutingTypes.AttributeBased
{
    // [Route] on the CLASS sets the BASE URL for all methods inside
    // [controller] token = class name minus "Controller"
    // ProductsController → "products"
    // So base URL = /api/products

    [ApiController]
    [Route("api/[controller]")]   // base: /api/products
    public class ProductsController : ControllerBase
    {
        // ── BASIC ATTRIBUTE ROUTES ────────────────────────────────────

        // [HttpGet] alone = no extra segment added
        // GET /api/products
        [HttpGet]
        public ActionResult GetAll() => Ok("All products");

        // [HttpGet("active")] = fixed word added to base URL
        // GET /api/products/active
        [HttpGet("active")]
        public ActionResult GetActive() => Ok("Active products");

        // {id:int} = parameter from URL, must be integer
        // GET /api/products/5
        [HttpGet("{id:int}")]
        public ActionResult GetById(int id) => Ok($"Product {id}");

        // Fixed word + parameter combined
        // GET /api/products/5/details
        [HttpGet("{id:int}/details")]
        public ActionResult GetDetails(int id) => Ok($"Details for {id}");

        // Fixed word before parameter
        // GET /api/products/search/laptop
        [HttpGet("search/{name}")]
        public ActionResult Search(string name) => Ok($"Search: {name}");

        // POST to base URL
        // POST /api/products
        [HttpPost]
        public ActionResult Create([FromBody] object product) => Ok("Created");

        // PUT with id
        // PUT /api/products/5
        [HttpPut("{id:int}")]
        public ActionResult Update(int id, [FromBody] object product) => Ok($"Updated {id}");

        // DELETE with id
        // DELETE /api/products/5
        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id) => Ok($"Deleted {id}");
    }

    // ── VERSIONED API — Attribute routing makes this very easy ───────
    //
    // V1 and V2 controllers exist side by side
    // Client chooses which version to call

    [ApiController]
    [Route("api/v1/[controller]")]   // base: /api/v1/products
    public class ProductsV1Controller : ControllerBase
    {
        [HttpGet]
        public ActionResult GetAll() => Ok("V1: All products (basic response)");

        [HttpGet("{id:int}")]
        public ActionResult GetById(int id) => Ok($"V1: Product {id}");
    }

    [ApiController]
    [Route("api/v2/[controller]")]   // base: /api/v2/products
    public class ProductsV2Controller : ControllerBase
    {
        [HttpGet]
        public ActionResult GetAll() => Ok("V2: All products (with extra fields)");

        [HttpGet("{id:int}")]
        public ActionResult GetById(int id) => Ok($"V2: Product {id} (with reviews)");
    }

    // ── CUSTOM BASE URL — Hardcode any URL you want ───────────────────

    [ApiController]
    [Route("catalog/items")]   // hardcoded — does NOT use class name
    public class CatalogController : ControllerBase
    {
        // GET /catalog/items
        [HttpGet]
        public ActionResult GetAll() => Ok("Catalog items");

        // GET /catalog/items/5
        [HttpGet("{id:int}")]
        public ActionResult GetById(int id) => Ok($"Catalog item {id}");
    }

    // ── [Route] WITH [action] TOKEN ───────────────────────────────────
    // [action] = automatically uses the method name as URL segment

    [ApiController]
    [Route("api/[controller]/[action]")]  // includes method name in URL
    public class ReportsController : ControllerBase
    {
        // GET /api/reports/monthly
        [HttpGet]
        public ActionResult Monthly() => Ok("Monthly report");

        // GET /api/reports/weekly
        [HttpGet]
        public ActionResult Weekly() => Ok("Weekly report");

        // GET /api/reports/annual
        [HttpGet]
        public ActionResult Annual() => Ok("Annual report");
    }

    // ── MULTIPLE [Route] ON SAME CONTROLLER ──────────────────────────
    // Both URLs point to the same controller

    [ApiController]
    [Route("api/products")]    // /api/products/...
    [Route("api/items")]       // /api/items/...   ← alias
    public class MultiRouteController : ControllerBase
    {
        // Works for BOTH:
        // GET /api/products
        // GET /api/items
        [HttpGet]
        public ActionResult GetAll() => Ok("Products or Items — same thing");
    }

    // ── OVERRIDE BASE URL WITH LEADING SLASH ─────────────────────────
    // "/" at the start = ignore the controller [Route] completely

    [ApiController]
    [Route("api/[controller]")]
    public class UtilityController : ControllerBase
    {
        // Leading "/" ignores "api/utility" prefix
        // GET /health   (NOT /api/utility/health)
        [HttpGet("/health")]
        public ActionResult Health() => Ok("Running");

        // GET /version  (NOT /api/utility/version)
        [HttpGet("/version")]
        public ActionResult Version() => Ok("1.0.0");

        // No leading slash — uses the base route normally
        // GET /api/utility/status
        [HttpGet("status")]
        public ActionResult Status() => Ok("Status OK");
    }
}

#endregion

// ================================================================
// SECTION 3: USING BOTH TOGETHER IN ONE APP
// ================================================================
/*
 * You CAN use both conventional and attribute routing in the same app.
 * They do NOT conflict with each other.
 *
 * Common pattern:
 *   → Conventional routing for MVC website pages (returns Views/HTML)
 *   → Attribute routing for REST API endpoints (returns JSON)
 *
 * In Program.cs:
 *
 *   // Attribute routing — for [Route] decorated controllers
 *   app.MapControllers();
 *
 *   // Conventional routing — for MVC website controllers
 *   app.MapControllerRoute(
 *       name: "default",
 *       pattern: "{controller=Home}/{action=Index}/{id?}"
 *   );
*/

// ================================================================
// SECTION 4: SIDE BY SIDE COMPARISON
// Same feature, both routing styles
// ================================================================
/*
 *
 * SAME ENDPOINT — TWO DIFFERENT WAYS TO WRITE IT:
 *
 * ┌──────────────────────────────────────────────────────────────┐
 * │ GOAL: GET a product by ID → GET /api/products/5             │
 * ├──────────────────────┬───────────────────────────────────────┤
 * │ CONVENTIONAL         │ ATTRIBUTE                             │
 * ├──────────────────────┼───────────────────────────────────────┤
 * │ Program.cs:          │ On the controller:                    │
 * │  app.MapController   │  [Route("api/[controller]")]          │
 * │  Route(              │                                       │
 * │   "products",        │ On the method:                        │
 * │   "api/products/{id}"│  [HttpGet("{id:int}")]                │
 * │  );                  │  public ActionResult GetById(int id)  │
 * │                      │                                       │
 * │ No attribute on      │ Route is self-contained               │
 * │ controller or method │ on the method itself                  │
 * └──────────────────────┴───────────────────────────────────────┘
 *
 *
 * WHEN TO USE WHICH:
 *
 * Use CONVENTIONAL when:
 *   ✅ Building a website with Razor Views (MVC)
 *   ✅ All URLs follow the same pattern /Controller/Action/Id
 *   ✅ You want one rule for the whole app
 *   ✅ Simple CRUD pages for internal tools
 *
 * Use ATTRIBUTE when:
 *   ✅ Building a REST API (returns JSON)
 *   ✅ URLs need to be clean: /api/products/5/reviews
 *   ✅ You need API versioning: /api/v1/ and /api/v2/
 *   ✅ Different controllers need different URL shapes
 *   ✅ [Authorize], [EnableCors] per endpoint control
 *
 *
 * KEY DIFFERENCE SUMMARY:
 *
 *  Conventional → "Follow the company template"
 *    URL shape is decided by the TEMPLATE in Program.cs
 *    Controller just needs to exist — routing is automatic
 *
 *  Attribute → "You write your own URL on every controller/method"
 *    URL shape is decided by [Route] and [HttpGet] etc.
 *    Full control — nothing is automatic
 *
 *
 * ROUTE TOKENS CHEAT SHEET:
 *
 *  [controller]  → replaced by controller class name (minus "Controller")
 *  [action]      → replaced by method name
 *  {id}          → route parameter (any value)
 *  {id:int}      → route parameter (integers only)
 *  {id?}         → optional route parameter
 *  {id:int=1}    → route parameter with default value
 *  /prefix       → leading slash overrides controller base route
 *
*/

// ================================================================
// SECTION 5: POSTMAN TEST URLS — BOTH ROUTING STYLES
// ================================================================
/*
 * CONVENTIONAL ROUTES (MVC — returns Views):
 *   GET  /Products/Index
 *   GET  /Products/Details/5
 *   GET  /Products/Create
 *   POST /Products/Create
 *   GET  /Products/Edit/3
 *   POST /Products/Edit/3
 *   GET  /Orders/Index
 *   GET  /                      → HomeController.Index (defaults)
 *
 * ATTRIBUTE ROUTES (API — returns JSON):
 *   GET    /api/products
 *   GET    /api/products/active
 *   GET    /api/products/5
 *   GET    /api/products/5/details
 *   GET    /api/products/search/laptop
 *   POST   /api/products
 *   PUT    /api/products/5
 *   DELETE /api/products/5
 *
 * VERSIONED API:
 *   GET /api/v1/products
 *   GET /api/v2/products
 *   GET /api/v1/products/5
 *   GET /api/v2/products/5
 *
 * CUSTOM BASE URL:
 *   GET /catalog/items
 *   GET /catalog/items/5
 *
 * ACTION TOKEN ROUTES:
 *   GET /api/reports/monthly
 *   GET /api/reports/weekly
 *   GET /api/reports/annual
 *
 * OVERRIDE PREFIX:
 *   GET /health         ← no api/utility prefix
 *   GET /version        ← no api/utility prefix
 *   GET /api/utility/status  ← uses prefix normally
*/