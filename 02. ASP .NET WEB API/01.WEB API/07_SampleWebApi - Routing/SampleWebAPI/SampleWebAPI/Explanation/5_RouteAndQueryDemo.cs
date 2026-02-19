// ============================================================
//  RouteAndQueryDemo.cs
//  TOPIC: Route Parameters & Query Strings in ASP.NET Core Web API
//  FOR  : 3.5 Year Experienced .NET Developer (Interview Prep)
// ============================================================

// -------------------------------------------------------
// QUICK THEORY RECAP (Read before code)
// -------------------------------------------------------
// URL Structure:
//   https://myapp.com/api/employees/5?department=HR&sortBy=name
//                                  ^^  ^^^^^^^^^^^^^^^^^^^^^
//                            Route Param     Query Strings
//
// Route Parameter  → Part of the URL PATH → Mandatory → Identifies a resource
// Query String     → After '?'            → Optional  → Filters/Sorts/Searches
//
// WHEN TO USE WHAT?
//   Route Param  → /api/orders/101        → "Give me order #101"
//   Query String → /api/orders?status=pending → "Give me orders filtered by status"
// -------------------------------------------------------

using Microsoft.AspNetCore.Mvc;

namespace RouteAndQueryDemo.Controllers
{
    // -------------------------------------------------------
    // MODEL - Simple Employee class for demo
    // -------------------------------------------------------
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public string City { get; set; }
        public decimal Salary { get; set; }
    }

    // -------------------------------------------------------
    // CONTROLLER - All Route & Query examples in ONE file
    // -------------------------------------------------------
    [ApiController]
    [Route("api/employees")]
    public class EmployeeController : ControllerBase
    {
        // --------------------------------------------------
        // FAKE DATA - Simulating a database
        // --------------------------------------------------
        private static List<Employee> _employees = new List<Employee>
        {
            new Employee { Id = 1, Name = "Ravi Kumar",   Department = "HR",  City = "Delhi",   Salary = 50000 },
            new Employee { Id = 2, Name = "Priya Sharma", Department = "IT",  City = "Mumbai",  Salary = 80000 },
            new Employee { Id = 3, Name = "Amit Singh",   Department = "HR",  City = "Delhi",   Salary = 60000 },
            new Employee { Id = 4, Name = "Sneha Patel",  Department = "IT",  City = "Chennai", Salary = 90000 },
            new Employee { Id = 5, Name = "Raj Verma",    Department = "Finance", City = "Mumbai", Salary = 70000 },
        };


        // ==================================================
        // SECTION 1: ROUTE PARAMETERS
        // ==================================================
        // Route: GET /api/employees/3
        // Route Parameter: {id}
        // Use Case: Fetch a SPECIFIC employee by ID
        //
        // Interview Point:
        //   - {id} in the route template is a route parameter
        //   - [FromRoute] is optional here (ASP.NET infers it)
        //   - URL: /api/employees/3  → id = 3
        // --------------------------------------------------
        [HttpGet("{id}")]
        public IActionResult GetEmployeeById([FromRoute] int id)
        {
            var employee = _employees.FirstOrDefault(e => e.Id == id);

            if (employee == null)
                return NotFound(new { Message = $"Employee with ID {id} not found." });

            return Ok(employee);
        }


        // ==================================================
        // SECTION 2: MULTIPLE ROUTE PARAMETERS
        // ==================================================
        // Route: GET /api/employees/department/HR/city/Delhi
        // Route Parameters: {department} and {city}
        // Use Case: Get employees filtered by BOTH department AND city
        //           (both are required, part of the path)
        //
        // Interview Point:
        //   - You can have multiple route params in one route
        //   - Order in the URL must match the template
        // --------------------------------------------------
        [HttpGet("department/{department}/city/{city}")]
        public IActionResult GetByDepartmentAndCity(string department, string city)
        {
            var result = _employees
                .Where(e => e.Department.Equals(department, StringComparison.OrdinalIgnoreCase)
                         && e.City.Equals(city, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!result.Any())
                return NotFound(new { Message = "No employees found for given department and city." });

            return Ok(result);
        }


        // ==================================================
        // SECTION 3: QUERY STRINGS (Basic)
        // ==================================================
        // Route: GET /api/employees/search?department=HR
        // Query String: department (optional filter)
        // Use Case: Filter employees, parameter is optional
        //
        // Interview Point:
        //   - Query strings come after '?' in the URL
        //   - [FromQuery] tells ASP.NET to read from query string
        //   - Default values make them truly optional
        //   - URL: /api/employees/search?department=HR
        // --------------------------------------------------
        [HttpGet("search")]
        public IActionResult SearchByDepartment([FromQuery] string department = null)
        {
            // If no department passed, return all employees
            var result = string.IsNullOrEmpty(department)
                ? _employees
                : _employees.Where(e => e.Department.Equals(department, StringComparison.OrdinalIgnoreCase));

            return Ok(result.ToList());
        }


        // ==================================================
        // SECTION 4: MULTIPLE QUERY STRINGS
        // ==================================================
        // Route: GET /api/employees/filter?department=IT&city=Mumbai&sortBy=salary
        // All params are optional → classic filtering scenario
        //
        // Interview Point:
        //   - Real-world APIs use multiple query strings for filtering + sorting
        //   - Client can pass any combination or none at all
        //   - URL examples:
        //       /api/employees/filter                          → all employees
        //       /api/employees/filter?department=IT            → IT employees only
        //       /api/employees/filter?city=Mumbai&sortBy=name  → Mumbai, sorted by name
        // --------------------------------------------------
        [HttpGet("filter")]
        public IActionResult FilterEmployees(
            [FromQuery] string department = null,
            [FromQuery] string city = null,
            [FromQuery] string sortBy = null)
        {
            var query = _employees.AsQueryable();

            // Apply department filter if provided
            if (!string.IsNullOrEmpty(department))
                query = query.Where(e => e.Department.Equals(department, StringComparison.OrdinalIgnoreCase));

            // Apply city filter if provided
            if (!string.IsNullOrEmpty(city))
                query = query.Where(e => e.City.Equals(city, StringComparison.OrdinalIgnoreCase));

            // Apply sorting if provided
            query = sortBy?.ToLower() switch
            {
                "name"       => query.OrderBy(e => e.Name),
                "salary"     => query.OrderBy(e => e.Salary),
                "department" => query.OrderBy(e => e.Department),
                _            => query   // No sorting if invalid/null
            };

            return Ok(query.ToList());
        }


        // ==================================================
        // SECTION 5: COMBINING ROUTE PARAM + QUERY STRINGS
        // ==================================================
        // Route: GET /api/employees/5/orders?status=pending&pageSize=10
        // Route Param: {id}    → WHO (employee #5)
        // Query Strings: status, pageSize → HOW (filter + pagination)
        //
        // Interview Point:
        //   - This is the most COMMON real-world pattern!
        //   - Route param identifies the resource
        //   - Query strings add optional behavior (filter/page/sort)
        // --------------------------------------------------
        [HttpGet("{id}/details")]
        public IActionResult GetEmployeeDetails(
            [FromRoute] int id,
            [FromQuery] bool includeSalary = false)
        {
            var employee = _employees.FirstOrDefault(e => e.Id == id);

            if (employee == null)
                return NotFound();

            // Return salary only if explicitly requested via query string
            // URL: /api/employees/2/details              → no salary shown
            // URL: /api/employees/2/details?includeSalary=true → salary shown
            var result = new
            {
                employee.Id,
                employee.Name,
                employee.Department,
                employee.City,
                Salary = includeSalary ? employee.Salary : (decimal?)null
            };

            return Ok(result);
        }


        // ==================================================
        // SECTION 6: ROUTE CONSTRAINTS
        // ==================================================
        // Route: GET /api/employees/byid/5
        // Constraint: {id:int} ensures only integers are accepted
        //
        // Interview Point:
        //   - Constraints prevent wrong data types from hitting your action
        //   - Common constraints: int, guid, bool, minlength, range
        //   - /api/employees/byid/abc → will return 404 automatically (not matched)
        // --------------------------------------------------
        [HttpGet("byid/{id:int}")]      // :int is the constraint
        public IActionResult GetWithConstraint([FromRoute] int id)
        {
            // This action will NEVER receive a non-integer id
            var employee = _employees.FirstOrDefault(e => e.Id == id);
            return employee != null ? Ok(employee) : NotFound();
        }


        // ==================================================
        // SECTION 7: OPTIONAL ROUTE PARAMETERS
        // ==================================================
        // Route: GET /api/employees/page        → returns page 1
        // Route: GET /api/employees/page/2      → returns page 2
        //
        // Interview Point:
        //   - Adding '?' after param name makes it optional
        //   - Default value must be assigned in method signature
        //   - Useful for pagination where page 1 is the default
        // --------------------------------------------------
        [HttpGet("page/{pageNumber:int?}")]   // '?' makes route param optional
        public IActionResult GetPaged(int pageNumber = 1)
        {
            int pageSize = 2;
            var result = _employees
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                Page = pageNumber,
                PageSize = pageSize,
                TotalRecords = _employees.Count,
                Data = result
            });
        }
    }
}


// ============================================================
//  INTERVIEW CHEAT SHEET - Key Differences
// ============================================================
//
//  Feature           | Route Parameter              | Query String
//  ------------------|------------------------------|-------------------------
//  Location in URL   | In the path (/api/emp/5)     | After ? (/api/emp?id=5)
//  Mandatory/Optional| Usually Mandatory            | Usually Optional
//  Use case          | Identify a resource          | Filter/Sort/Search/Page
//  Attribute         | [FromRoute]                  | [FromQuery]
//  URL Example       | /api/employees/5             | /api/employees?dept=HR
//  Can have default? | Yes (with '?' in template)   | Yes (default in method)
//  Constraints       | Yes ({id:int}, {id:guid})    | No built-in constraints
//
// ============================================================
//  COMMON INTERVIEW QUESTIONS & QUICK ANSWERS
// ============================================================
//
//  Q1. When to use Route Param vs Query String?
//  A.  Route Param → When value is part of IDENTITY (e.g., /orders/101)
//      Query String → When value is a FILTER or OPTIONAL (e.g., ?status=active)
//
//  Q2. What is [FromRoute] and [FromQuery]?
//  A.  They are binding source attributes.
//      [FromRoute] → reads value from URL path
//      [FromQuery] → reads value from query string after '?'
//      Without them, ASP.NET auto-infers the source (usually correct)
//
//  Q3. What are Route Constraints?
//  A.  Constraints restrict which values a route parameter accepts.
//      {id:int}   → only integers
//      {id:guid}  → only GUIDs
//      {name:minlength(3)} → minimum 3 characters
//      If constraint fails → 404 (route not matched), not 400!
//
//  Q4. Can we have both route params and query strings in one action?
//  A.  YES! Very common pattern:
//      GET /api/employees/5/orders?status=pending
//      Route Param: 5 (which employee)
//      Query String: status=pending (filter their orders)
//
//  Q5. What happens if route param type doesn't match?
//  A.  If constraint like {id:int} is used → 404 Not Found
//      If no constraint → model binding fails → 400 Bad Request
//
// ============================================================
