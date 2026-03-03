// ============================================================================================================================
// FILE: TokenReplacementDemo.cs
// TOPIC: Token Replacement in ASP.NET Core Web API
// AUTHOR: Senior C# Professional | Microsoft | 15+ Years Experience
// AUDIENCE: Freshers & Experienced Developers
// ============================================================================================================================
//
// ┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
// │                          WHAT IS TOKEN REPLACEMENT IN ASP.NET CORE WEB API?                                            │
// │                                                                                                                         │
// │  Token Replacement is a feature of Attribute Routing in ASP.NET Core that allows you to use                            │
// │  special PLACEHOLDERS (called Tokens) inside route templates, which the framework automatically                         │
// │  replaces with actual values at runtime.                                                                                │
// │                                                                                                                         │
// │  Supported Tokens:                                                                                                      │
// │    [controller]  →  Replaced with the controller name (WITHOUT the "Controller" suffix)                                 │
// │    [action]      →  Replaced with the action method name                                                                │
// │    [area]        →  Replaced with the area name (used in MVC Area routing)                                              │
// │                                                                                                                         │
// │  REAL-WORLD ANALOGY:                                                                                                    │
// │    Think of a letter template:                                                                                          │
// │       "Dear [Name], your appointment is on [Date] at [Address]."                                                        │
// │    When printed, [Name], [Date], [Address] are auto-replaced with real values.                                          │
// │    Similarly, ASP.NET Core replaces [controller] and [action] with actual names at runtime.                             │
// └─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
//
// ============================================================================================================================
// NAMESPACE & USING DIRECTIVES
// ============================================================================================================================

using Microsoft.AspNetCore.Mvc;

namespace TokenReplacementDemo.Controllers
{
    // ===========================================================
    // ❌ APPROACH 1 (BAD PRACTICE): WITHOUT Token Replacement
    // ===========================================================
    //
    // PROBLEM: Routes are hardcoded manually.
    //
    // Why is this BAD?
    //   1. If you rename the controller from "Employee" to "Staff", you must
    //      manually update EVERY route string in EVERY method. One typo = broken API.
    //   2. If you rename the action "GetAllEmployees" to "FetchAllEmployees",
    //      you must hunt down and update the route string manually.
    //   3. In large projects with 50+ controllers and 200+ actions, this becomes
    //      a MAINTENANCE NIGHTMARE.
    //   4. High risk of inconsistency and runtime bugs.
    //
    // FRESHER TIP:  Hardcoded strings in routes = Technical Debt.
    //               Always prefer dynamic, maintainable solutions.
    //
    // EXPERIENCED TIP: In enterprise projects, hardcoded route strings violate
    //                  the DRY (Don't Repeat Yourself) principle and make refactoring
    //                  extremely risky. Token replacement is the industry standard.

    [ApiController]
    public class EmployeeWithoutTokenController : ControllerBase
    {
        // ❌ Route is hardcoded as a string literal
        // If controller name changes → this string becomes STALE and WRONG
        [Route("Employee/GetAllEmployees")]
        [HttpGet]
        public string GetAllEmployees()
        {
            // URL: GET /Employee/GetAllEmployees
            return "Response from GetAllEmployees Method (WITHOUT Token Replacement)";
        }

        // ❌ Again, hardcoded. Redundant and fragile.
        [Route("Employee/GetAllDepartment")]
        [HttpGet]
        public string GetAllDepartment()
        {
            // URL: GET /Employee/GetAllDepartment
            return "Response from GetAllDepartment Method (WITHOUT Token Replacement)";
        }

        // ❌ Simulate a future rename problem:
        // If this action was renamed to "FetchEmployeeById" tomorrow,
        // the route string below would still say "GetEmployeeById" unless
        // the developer manually finds and updates it. Easy to miss!
        [Route("Employee/GetEmployeeById")]
        [HttpGet]
        public string GetEmployeeById()
        {
            // URL: GET /Employee/GetEmployeeById
            return "Response from GetEmployeeById Method";
        }
    }


    // ===========================================================
    // ✅ APPROACH 2 (GOOD PRACTICE): WITH Token Replacement
    //    Using [controller] and [action] Tokens
    // ===========================================================
    //
    // HOW IT WORKS:
    //   At startup/runtime, ASP.NET Core's routing middleware scans all controllers
    //   and replaces:
    //      [controller] → "Employee"    (derived from "EmployeeController" by stripping "Controller")
    //      [action]     → the name of each action method (e.g., "GetAllEmployees")
    //
    // BENEFITS:
    //   ✅ Rename the controller? Token auto-updates the URL. Zero manual work.
    //   ✅ Rename an action?     Token auto-updates the URL. Zero manual work.
    //   ✅ Consistent URL patterns across the entire application.
    //   ✅ Cleaner, readable, self-documenting code.
    //   ✅ Reduces human error in route maintenance.
    //
    // FRESHER TIP:  [controller] does NOT include the word "Controller".
    //               "EmployeeController" → [controller] resolves to "Employee"
    //
    // EXPERIENCED TIP: Token replacement works at compile-time metadata level.
    //                  The route template is evaluated against ControllerActionDescriptor
    //                  at application startup via IApplicationModelConvention pipeline.
    //                  This means zero runtime overhead per request for token resolution.

    [ApiController]
    [Route("[controller]")]   // ✅ Token: Resolves to "Employee" at runtime
    public class EmployeeController : ControllerBase
    {
        // ✅ Token [action] resolves to "GetAllEmployees" at runtime
        // Effective Route: GET /Employee/GetAllEmployees
        [Route("[action]")]
        [HttpGet]
        public string GetAllEmployees()
        {
            return "Response from GetAllEmployees Method (WITH Token Replacement)";
        }

        // ✅ Token [action] resolves to "GetAllDepartment" at runtime
        // Effective Route: GET /Employee/GetAllDepartment
        [Route("[action]")]
        [HttpGet]
        public string GetAllDepartment()
        {
            return "Response from GetAllDepartment Method (WITH Token Replacement)";
        }

        // ✅ Token [action] resolves to "GetEmployeeById" at runtime
        // Effective Route: GET /Employee/GetEmployeeById/5
        [Route("[action]/{id:int}")]  // Also supports route constraints like {id:int}
        [HttpGet]
        public string GetEmployeeById(int id)
        {
            return $"Response from GetEmployeeById Method. EmployeeId = {id}";
        }
    }


    // ===========================================================
    // ✅ APPROACH 3 (BEST PRACTICE): Tokens at Controller Level
    //    Combining [controller] + [action] in a single base route
    // ===========================================================
    //
    // KEY INSIGHT (for Experienced Devs):
    //   Instead of repeating [action] on EVERY method, define the base route
    //   ONCE at the controller level using [Route("[controller]/[action]")].
    //   Each action method will automatically get:
    //       ControllerName/ActionMethodName
    //
    // This is the most common pattern in real enterprise ASP.NET Core projects.
    //
    // FRESHER TIP:  Think of [Route] on the controller as a "prefix" for all
    //               routes inside it. Individual actions can override or extend this.
    //
    // EXPERIENCED TIP: This pattern implements the "Convention over Configuration"
    //                  design principle. It's also fully compatible with Swagger/OpenAPI
    //                  documentation generation, as route templates are resolved correctly.

    [ApiController]
    [Route("[controller]/[action]")]  // ✅ Applies to ALL actions in this controller
    public class ProductController : ControllerBase
    {
        // Effective Route: GET /Product/GetAllProducts
        // No need to write [Route("[action]")] on each method!
        [HttpGet]
        public string GetAllProducts()
        {
            return "Response from GetAllProducts (Token at Controller Level)";
        }

        // Effective Route: GET /Product/GetProductById/10
        [HttpGet("{id:int}")]   // Extends the controller-level route with /{id}
        public string GetProductById(int id)
        {
            return $"Response from GetProductById. ProductId = {id}";
        }

        // Effective Route: POST /Product/CreateProduct
        [HttpPost]
        public string CreateProduct([FromBody] string productName)
        {
            return $"Product '{productName}' created successfully.";
        }

        // Effective Route: PUT /Product/UpdateProduct/10
        [HttpPut("{id:int}")]
        public string UpdateProduct(int id, [FromBody] string productName)
        {
            return $"Product with ID {id} updated to '{productName}'.";
        }

        // Effective Route: DELETE /Product/DeleteProduct/10
        [HttpDelete("{id:int}")]
        public string DeleteProduct(int id)
        {
            return $"Product with ID {id} deleted.";
        }
    }


    // ===========================================================
    // ✅ APPROACH 4: Mixing Token Routes with Custom Route Segments
    // ===========================================================
    //
    // Tokens can be combined with custom static segments, prefixes, or versioning.
    //
    // USE CASE (for Experienced Devs):
    //   API Versioning via route prefix is a common enterprise requirement.
    //   You might want URLs like: /api/v1/Order/GetAllOrders
    //
    // FRESHER TIP:  You can combine static text AND tokens freely inside [Route].
    //               The only rule: tokens must be in square brackets [].

    [ApiController]
    [Route("api/v1/[controller]/[action]")]  // ✅ Static prefix "api/v1/" + Tokens
    public class OrderController : ControllerBase
    {
        // Effective Route: GET /api/v1/Order/GetAllOrders
        [HttpGet]
        public string GetAllOrders()
        {
            return "Response from GetAllOrders (Versioned API with Tokens)";
        }

        // Effective Route: GET /api/v1/Order/GetOrderById/7
        [HttpGet("{id:int}")]
        public string GetOrderById(int id)
        {
            return $"Response from GetOrderById. OrderId = {id}";
        }

        // Effective Route: POST /api/v1/Order/PlaceOrder
        [HttpPost]
        public string PlaceOrder([FromBody] string orderDetails)
        {
            return $"Order placed: {orderDetails}";
        }
    }


    // ===========================================================
    // ✅ APPROACH 5: Using [area] Token (Advanced – For MVC Projects)
    // ===========================================================
    //
    // The [area] token is used when your application is organized into Areas.
    // Areas help divide a large app into smaller functional sections.
    //
    // EXPERIENCED DEV NOTE:
    //   For ASP.NET Core Web API, [area] is less common but valid.
    //   For ASP.NET Core MVC (with Views), Areas are widely used to separate
    //   Admin, Customer, Reports sections, etc.
    //
    // IMPORTANT: To use [area], the controller must be decorated with [Area("AreaName")].
    //            The route token [area] then resolves to that area name.
    //
    // FRESHER TIP:  Areas are like "sub-applications" inside your main app.
    //               Example: An e-commerce site might have "Admin" area and "Customer" area,
    //               each with their own controllers and routes.

    [Area("Admin")]   // Marks this controller as part of the "Admin" area
    [ApiController]
    [Route("[area]/[controller]/[action]")]  // Resolves to: Admin/Report/GetSalesReport
    public class ReportController : ControllerBase
    {
        // Effective Route: GET /Admin/Report/GetSalesReport
        [HttpGet]
        public string GetSalesReport()
        {
            return "Response from Admin Area - GetSalesReport";
        }

        // Effective Route: GET /Admin/Report/GetInventoryReport
        [HttpGet]
        public string GetInventoryReport()
        {
            return "Response from Admin Area - GetInventoryReport";
        }
    }


    // ===========================================================
    // ✅ APPROACH 6: Overriding Controller-Level Token Route on a Specific Action
    // ===========================================================
    //
    // Sometimes one action needs a COMPLETELY DIFFERENT route.
    // You can override the inherited controller-level route on a per-action basis.
    //
    // EXPERIENCED DEV NOTE:
    //   This is useful for backward compatibility — keeping legacy URLs alive
    //   while the rest of the API adopts the new token-based convention.
    //   Or for exposing a "health check" endpoint at a well-known fixed path.
    //
    // FRESHER TIP:  Adding [Route("...")] directly on an action OVERRIDES the
    //               controller-level route for that specific action only.

    [ApiController]
    [Route("[controller]/[action]")]  // Default for all actions: Customer/{actionName}
    public class CustomerController : ControllerBase
    {
        // Effective Route: GET /Customer/GetAllCustomers   (inherits controller route)
        [HttpGet]
        public string GetAllCustomers()
        {
            return "Response from GetAllCustomers";
        }

        // ✅ OVERRIDE: This action ignores the controller-level route
        // Effective Route: GET /customers/list   (custom, fixed route)
        // Use case: Legacy API endpoint or well-known public URL
        [Route("/customers/list")]  // Leading "/" means ABSOLUTE route (overrides controller prefix)
        [HttpGet]
        public string GetCustomerList()
        {
            return "Response from GetCustomerList (Custom Absolute Route - overrides token route)";
        }

        // Effective Route: GET /Customer/GetCustomerById/3   (inherits controller route)
        [HttpGet("{id:int}")]
        public string GetCustomerById(int id)
        {
            return $"Response from GetCustomerById. CustomerId = {id}";
        }
    }


    // ============================================================================================================================
    // SUMMARY TABLE — QUICK REFERENCE FOR TOKENS
    // ============================================================================================================================
    //
    //  ┌──────────────────────────────────┬──────────────────────────────────────────────────────────────────────────────────┐
    //  │  Route Template Used             │  Resolved URL (for EmployeeController → GetAllEmployees action)                 │
    //  ├──────────────────────────────────┼──────────────────────────────────────────────────────────────────────────────────┤
    //  │  "Employee/GetAllEmployees"      │  /Employee/GetAllEmployees    ❌ Hardcoded — fragile                            │
    //  │  "[controller]/[action]"         │  /Employee/GetAllEmployees    ✅ Auto-resolved via tokens                       │
    //  │  "api/v1/[controller]/[action]"  │  /api/v1/Employee/GetAllEmployees  ✅ Versioned + tokens                        │
    //  │  "[area]/[controller]/[action]"  │  /Admin/Employee/GetAllEmployees   ✅ Area + tokens (if Area = Admin)           │
    //  └──────────────────────────────────┴──────────────────────────────────────────────────────────────────────────────────┘
    //
    //
    // ============================================================================================================================
    // KEY RULES & GOTCHAS — IMPORTANT FOR INTERVIEWS & PRODUCTION CODE
    // ============================================================================================================================
    //
    //  RULE 1: [controller] strips the "Controller" suffix.
    //          "EmployeeController" → resolves to "Employee"   (NOT "EmployeeController")
    //
    //  RULE 2: [action] uses the exact method name as written in code.
    //          If method is "GetAllEmployees()" → resolves to "GetAllEmployees"
    //
    //  RULE 3: Tokens are case-insensitive in the URL by default.
    //          /employee/getallemployees and /Employee/GetAllEmployees both work.
    //
    //  RULE 4: A route starting with "/" on an action is ABSOLUTE.
    //          It completely ignores the controller-level route prefix.
    //          Example: [Route("/customers/list")] → /customers/list
    //                                                (NOT /Customer/customers/list)
    //
    //  RULE 5: [area] token ONLY works when [Area("...")] attribute is applied.
    //          Without [Area], using [area] token will throw a routing error.
    //
    //  RULE 6: Tokens work ONLY inside [Route(...)], [HttpGet(...)], [HttpPost(...)], etc.
    //          They do NOT work in string variables, ViewBag, or Razor templates.
    //
    //  GOTCHA (for Experienced Devs):
    //          If you have TWO actions with the same HTTP method and the same resolved route,
    //          ASP.NET Core will throw an AmbiguousMatchException at startup.
    //          Always ensure each action method has a UNIQUE effective route URL.
    //
    // ============================================================================================================================
    // HOW TO REGISTER ROUTING IN Program.cs (ASP.NET Core 6+)
    // ============================================================================================================================
    //
    //  var builder = WebApplication.CreateBuilder(args);
    //  builder.Services.AddControllers();    // Registers controller services
    //
    //  var app = builder.Build();
    //  app.MapControllers();                  // Enables attribute-based routing (tokens work here)
    //  app.Run();
    //
    //  NOTE: Token replacement requires app.MapControllers() or app.UseRouting() + app.UseEndpoints().
    //        WITHOUT these calls, NO routes (token-based or otherwise) will be registered.
    //
    // ============================================================================================================================
}

// ============================================================================================================================
// END OF FILE
// ============================================================================================================================
