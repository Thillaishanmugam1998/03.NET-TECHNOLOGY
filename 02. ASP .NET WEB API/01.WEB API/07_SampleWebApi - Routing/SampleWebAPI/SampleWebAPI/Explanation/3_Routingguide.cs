// ================================================================
// COMPLETE ASP.NET CORE ROUTING PATTERNS — All in One File
// ================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RoutingGuide.Controllers
{
    // ================================================================
    // SECTION 1: CONTROLLER-LEVEL ROUTE OPTIONS
    // ================================================================
    /*
     * Option A — Default (most common)
     * [Route("api/[controller]")]
     * [controller] = class name minus "Controller" word
     * ProductsController → /api/products
     *
     * Option B — Versioned API
     * [Route("api/v1/[controller]")]
     * → /api/v1/products
     *
     * Option C — Action name in URL
     * [Route("api/[controller]/[action]")]
     * GetAll method  → /api/products/getall
     * GetById method → /api/products/getbyid
     * Add method     → /api/products/add
     *
     * Option D — Hardcoded (ignores class name completely)
     * [Route("api/items")]
     * → always /api/items even if you rename the class
     *
     * Option E — Multiple routes on same controller (both URLs work)
     * [Route("api/products")]
     * [Route("api/items")]
     * → /api/products AND /api/items both hit this controller
    */

    [ApiController]
    [Route("api/[controller]")]   // ProductsController → /api/products
    public class ProductsController : ControllerBase
    {
        // ================================================================
        // SECTION 2: BASIC HTTP METHOD ROUTING
        // ================================================================
        #region 2 — Basic HTTP Methods

        [HttpGet]                  // GET    /api/products
        public ActionResult GetAll() => Ok("Get All Products");

        [HttpPost]                 // POST   /api/products
        public ActionResult Add() => Ok("Add Product");

        [HttpPut]                  // PUT    /api/products
        public ActionResult UpdateAll() => Ok("Update All");

        [HttpDelete]               // DELETE /api/products
        public ActionResult DeleteAll() => Ok("Delete All");

        [HttpPatch]                // PATCH  /api/products
        public ActionResult PatchAll() => Ok("Patch All");

        #endregion

        // ================================================================
        // SECTION 3: FIXED SEGMENT ROUTING
        // A hardcoded word added to the URL path
        // ================================================================
        #region 3 — Fixed Segment Routes

        [HttpGet("all")]           // GET /api/products/all
        public ActionResult GetAllFixed() => Ok("Fixed: all");

        [HttpGet("list")]          // GET /api/products/list
        public ActionResult GetList() => Ok("Fixed: list");

        [HttpGet("active")]        // GET /api/products/active
        public ActionResult GetActive() => Ok("Fixed: active");

        [HttpGet("featured")]      // GET /api/products/featured
        public ActionResult GetFeatured() => Ok("Fixed: featured");

        #endregion

        // ================================================================
        // SECTION 4: ROUTE PARAMETER — value comes from the URL path
        // {param}  = required
        // {param?} = optional
        // ================================================================
        #region 4 — Route Parameters (No Type Constraint)

        [HttpGet("{id}")]                 // GET /api/products/5      → id = "5"
        public ActionResult GetById(string id) => Ok($"Id = {id}");

        [HttpGet("{name}")]               // GET /api/products/laptop → name = "laptop"
        public ActionResult GetByName(string name) => Ok($"Name = {name}");

        #endregion

        // ================================================================
        // SECTION 5: TYPE CONSTRAINTS
        // Constraint = only accept this data type, reject others with 404
        // Format: {param:type}
        // ================================================================
        #region 5 — Route Parameters with Type Constraints

        // :int — only accepts whole numbers
        // GET /api/products/byid/5    ✅ accepted
        // GET /api/products/byid/abc  ❌ 404 Not Found
        [HttpGet("byid/{id:int}")]
        public ActionResult GetByIdInt(int id) => Ok($"Int Id = {id}");

        // :alpha — only accepts letters (no numbers or symbols)
        // GET /api/products/byname/laptop  ✅
        // GET /api/products/byname/123     ❌ 404
        [HttpGet("byname/{name:alpha}")]
        public ActionResult GetByNameAlpha(string name) => Ok($"Alpha Name = {name}");

        // :decimal — only accepts decimal numbers
        // GET /api/products/byprice/9.99  ✅
        // GET /api/products/byprice/abc   ❌ 404
        [HttpGet("byprice/{price:decimal}")]
        public ActionResult GetByPrice(decimal price) => Ok($"Price = {price}");

        // :guid — only accepts GUID format
        // GET /api/products/byguid/3fa85f64-5717-4562-b3fc-2c963f66afa6  ✅
        // GET /api/products/byguid/123  ❌ 404
        [HttpGet("byguid/{id:guid}")]
        public ActionResult GetByGuid(Guid id) => Ok($"Guid = {id}");

        // :bool — only accepts true or false
        // GET /api/products/bystatus/true   ✅
        // GET /api/products/bystatus/hello  ❌ 404
        [HttpGet("bystatus/{inStock:bool}")]
        public ActionResult GetByStatus(bool inStock) => Ok($"InStock = {inStock}");

        #endregion

        // ================================================================
        // SECTION 6: RANGE & LENGTH CONSTRAINTS
        // Add extra rules on top of type constraints
        // Format: {param:type:rule(value)}
        // ================================================================
        #region 6 — Range and Length Constraints

        // :min(1) — int must be 1 or more
        // GET /api/products/minid/5  ✅
        // GET /api/products/minid/0  ❌ 404
        [HttpGet("minid/{id:int:min(1)}")]
        public ActionResult GetByMinId(int id) => Ok($"Min(1) Id = {id}");

        // :max(100) — int must be 100 or less
        // GET /api/products/maxid/50   ✅
        // GET /api/products/maxid/200  ❌ 404
        [HttpGet("maxid/{id:int:max(100)}")]
        public ActionResult GetByMaxId(int id) => Ok($"Max(100) Id = {id}");

        // :range(1,100) — int must be between 1 and 100
        // GET /api/products/range/50   ✅
        // GET /api/products/range/0    ❌ 404
        // GET /api/products/range/101  ❌ 404
        [HttpGet("range/{id:int:range(1,100)}")]
        public ActionResult GetByRange(int id) => Ok($"Range(1,100) Id = {id}");

        // :minlength(3) — string must be 3 or more characters
        // GET /api/products/minname/laptop  ✅
        // GET /api/products/minname/ab      ❌ 404
        [HttpGet("minname/{name:minlength(3)}")]
        public ActionResult GetByMinName(string name) => Ok($"MinLength(3) Name = {name}");

        // :maxlength(5) — string must be 5 or fewer characters
        // GET /api/products/maxname/abc      ✅
        // GET /api/products/maxname/toolong  ❌ 404
        [HttpGet("maxname/{name:maxlength(5)}")]
        public ActionResult GetByMaxName(string name) => Ok($"MaxLength(5) Name = {name}");

        // :length(5) — string must be EXACTLY 5 characters
        // GET /api/products/code/AB123  ✅
        // GET /api/products/code/AB12   ❌ 404
        [HttpGet("code/{code:length(5)}")]
        public ActionResult GetByCode(string code) => Ok($"Length(5) Code = {code}");

        #endregion

        // ================================================================
        // SECTION 7: COMBINED FIXED + PARAMETER ROUTES
        // Mix hardcoded words and dynamic values in the same URL
        // ================================================================
        #region 7 — Combined Routes

        // Fixed word "details" after the id
        // GET /api/products/5/details
        [HttpGet("{id:int}/details")]
        public ActionResult GetDetails(int id) => Ok($"Details for Id = {id}");

        // Fixed word "reviews" after the id
        // GET /api/products/5/reviews
        [HttpGet("{id:int}/reviews")]
        public ActionResult GetReviews(int id) => Ok($"Reviews for Id = {id}");

        // Fixed word "search" before the name
        // GET /api/products/search/laptop
        [HttpGet("search/{name}")]
        public ActionResult SearchByName(string name) => Ok($"Search = {name}");

        // Fixed word "category" before the category name
        // GET /api/products/category/electronics
        [HttpGet("category/{cat}")]
        public ActionResult GetByCategory(string cat) => Ok($"Category = {cat}");

        // Two parameters in one route
        // GET /api/products/5/related/3
        [HttpGet("{id:int}/related/{count:int}")]
        public ActionResult GetRelated(int id, int count) => Ok($"Id = {id}, Count = {count}");

        // Two fixed words + two parameters
        // GET /api/products/category/electronics/price/999.99
        [HttpGet("category/{cat}/price/{max:decimal}")]
        public ActionResult GetByCategoryAndPrice(string cat, decimal max) =>
            Ok($"Category = {cat}, MaxPrice = {max}");

        #endregion

        // ================================================================
        // SECTION 8: QUERY STRING PARAMETERS
        // Values come after ? in the URL — not part of the path
        // ================================================================
        #region 8 — Query String Parameters

        // Query string — values come after ? in the URL
        // GET /api/products/search?name=laptop
        // GET /api/products/search?name=laptop&maxPrice=1000
        [HttpGet("filter")]
        public ActionResult Filter(string? name, decimal? maxPrice)
            => Ok($"Name = {name}, MaxPrice = {maxPrice}");

        // Pagination using query string
        // GET /api/products/page?number=2&size=10
        [HttpGet("page")]
        public ActionResult GetPage(int number = 1, int size = 10)
            => Ok($"Page {number}, Size {size}");

        // Mix of route param + query string
        // GET /api/products/5/related?count=3
        [HttpGet("{id:int}/extras")]
        public ActionResult GetExtras(int id, int count = 5)
            => Ok($"Id = {id}, Count = {count}");

        #endregion

        // ================================================================
        // SECTION 9: OPTIONAL ROUTE PARAMETER
        // {param?} = segment is not required in the URL
        // ================================================================
        #region 9 — Optional Parameters

        // ? makes the segment optional
        // GET /api/products/browse          → name = null
        // GET /api/products/browse/laptop   → name = "laptop"
        [HttpGet("browse/{name?}")]
        public ActionResult Browse(string? name)
            => Ok(name is null ? "All products" : $"Browsing: {name}");

        #endregion

        // ================================================================
        // SECTION 10: DEFAULT VALUE IN ROUTE
        // If segment missing, use the default value
        // ================================================================
        #region 10 — Default Values

        // GET /api/products/paged         → number = 1 (default)
        // GET /api/products/paged/3       → number = 3
        [HttpGet("paged/{number:int=1}")]
        public ActionResult GetPaged(int number)
            => Ok($"Page number = {number}");

        #endregion

        // ================================================================
        // SECTION 11: MULTIPLE ROUTES ON SAME METHOD
        // Both URLs call the exact same method
        // ================================================================
        #region 11 — Multiple Routes on Same Method

        [HttpGet("everything")]
        [HttpGet("complete")]
        // GET /api/products/everything  ✅
        // GET /api/products/complete    ✅ — both work!
        public ActionResult GetEverything() => Ok("Multiple routes, same method");

        #endregion

        // ================================================================
        // SECTION 12: OVERRIDE CONTROLLER PREFIX WITH LEADING SLASH
        // Starting route with "/" ignores the controller [Route] completely
        // ================================================================
        #region 12 — Override Controller Prefix

        // Leading slash "/" = skip "api/products" prefix entirely
        // GET /health   (NOT /api/products/health)
        [HttpGet("/health")]
        [AllowAnonymous]
        public ActionResult Health() => Ok(new { status = "Running", time = DateTime.UtcNow });

        // GET /version
        [HttpGet("/version")]
        [AllowAnonymous]
        public ActionResult Version() => Ok(new { version = "1.0.0" });

        #endregion

        // ================================================================
        // SECTION 13: ALL PATTERNS — QUICK REFERENCE SUMMARY
        // ================================================================
        /*
         * ┌─────────────────────────────────────────────────────────────────┐
         * │ PATTERN                          │ EXAMPLE URL                  │
         * ├─────────────────────────────────────────────────────────────────┤
         * │ [HttpGet]                         │ GET /api/products            │
         * │ [HttpGet("all")]                  │ GET /api/products/all        │
         * │ [HttpGet("{id}")]                 │ GET /api/products/5          │
         * │ [HttpGet("{id:int}")]             │ GET /api/products/5          │
         * │ [HttpGet("{name:alpha}")]         │ GET /api/products/laptop     │
         * │ [HttpGet("{id:int:min(1)}")]      │ GET /api/products/5          │
         * │ [HttpGet("{id:int:range(1,100)")] │ GET /api/products/50         │
         * │ [HttpGet("{code:length(5)}")]     │ GET /api/products/AB123      │
         * │ [HttpGet("{id:int}/details")]     │ GET /api/products/5/details  │
         * │ [HttpGet("search/{name}")]        │ GET /api/products/search/abc │
         * │ [HttpGet("{id:int}/rel/{n:int}")] │ GET /api/products/5/rel/3    │
         * │ [HttpGet("filter")]               │ GET /api/products/filter?... │
         * │ [HttpGet("browse/{name?}")]       │ GET /api/products/browse     │
         * │ [HttpGet("paged/{n:int=1}")]      │ GET /api/products/paged      │
         * │ [HttpGet("/health")]              │ GET /health                  │
         * └─────────────────────────────────────────────────────────────────┘
         *
         * STATUS CODES:
         * ✅ Route matched  → method runs
         * ❌ Type mismatch  → 404 Not Found  (constraint rejected the value)
         * ❌ No route match → 404 Not Found
         *
         * CONSTRAINT CHEAT SHEET:
         * {id:int}              only whole numbers
         * {name:alpha}          only letters
         * {price:decimal}       only decimal numbers
         * {id:guid}             only GUID format
         * {flag:bool}           only true/false
         * {id:int:min(1)}       int >= 1
         * {id:int:max(100)}     int <= 100
         * {id:int:range(1,100)} int between 1 and 100
         * {name:minlength(3)}   string 3+ characters
         * {name:maxlength(5)}   string max 5 characters
         * {code:length(5)}      string exactly 5 characters
        */
    }
}