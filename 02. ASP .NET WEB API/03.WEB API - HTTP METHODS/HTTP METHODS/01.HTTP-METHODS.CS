/*
================================================================================
                    HTTP METHODS IN ASP.NET CORE WEB API
================================================================================

HTTP Methods (also called HTTP Verbs) define what kind of operation 
a client wants to perform on a resource.

In ASP.NET Core Web API, these methods are mapped using attributes like:

    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpPatch]
    [HttpDelete]

These attributes are part of:
    using Microsoft.AspNetCore.Mvc;

================================================================================
1️⃣ WHY HTTP METHODS ARE NEEDED?
================================================================================

Web API follows REST (Representational State Transfer) principles.

REST works based on:

    ✔ Resources (Ex: Employees, Products, Orders)
    ✔ HTTP Methods (Action to perform on resource)
    ✔ Status Codes (Result of operation)

Without HTTP methods:
    → Server cannot understand the intention of the request.
    → Everything would become a generic POST call (bad design).
    → Violates REST standards.
    → Hard to maintain & scale.

================================================================================
2️⃣ TYPES OF HTTP METHODS
================================================================================

------------------------------------------------------------------------------
1. GET
------------------------------------------------------------------------------
Purpose:
    Retrieve data from server.

Example:
    GET /api/employees

ASP.NET Core:
    [HttpGet]
    public IActionResult GetAll() { }

Characteristics:
    ✔ Read-only
    ✔ Should NOT modify data
    ✔ Can be cached
    ✔ Can be bookmarked
    ✔ Safe method

Real-world:
    Fetch employees list
    Fetch product details

------------------------------------------------------------------------------
2. POST
------------------------------------------------------------------------------
Purpose:
    Create new resource.

Example:
    POST /api/employees

ASP.NET Core:
    [HttpPost]
    public IActionResult Create(Employee model) { }

Characteristics:
    ✔ Creates new record
    ✔ Not idempotent
    ✔ Data sent in body
    ✔ Unsafe method

Real-world:
    Add new employee
    Register new user

------------------------------------------------------------------------------
3. PUT
------------------------------------------------------------------------------
Purpose:
    Update entire resource.

Example:
    PUT /api/employees/5

ASP.NET Core:
    [HttpPut("{id}")]
    public IActionResult Update(int id, Employee model) { }

Characteristics:
    ✔ Replaces entire object
    ✔ Idempotent
    ✔ Unsafe method

Idempotent means:
    Calling multiple times gives same result.

------------------------------------------------------------------------------
4. PATCH
------------------------------------------------------------------------------
Purpose:
    Partial update of resource.

Example:
    PATCH /api/employees/5

ASP.NET Core:
    [HttpPatch("{id}")]
    public IActionResult PartialUpdate(int id) { }

Characteristics:
    ✔ Updates only specific fields
    ✔ Unsafe method

------------------------------------------------------------------------------
5. DELETE
------------------------------------------------------------------------------
Purpose:
    Remove resource.

Example:
    DELETE /api/employees/5

ASP.NET Core:
    [HttpDelete("{id}")]
    public IActionResult Delete(int id) { }

Characteristics:
    ✔ Removes record
    ✔ Idempotent
    ✔ Unsafe method

================================================================================
3️⃣ SAFE VS UNSAFE HTTP METHODS
================================================================================

------------------------------------------------------------------------------
SAFE METHODS
------------------------------------------------------------------------------

Definition:
    A method is SAFE if it does NOT modify server data.

Safe Methods:
    ✔ GET
    ✔ HEAD
    ✔ OPTIONS

Example:
    GET /api/employees

Even if you call GET 1000 times:
    → No data change.
    → Only reads data.

Why important?
    ✔ Browsers can cache safely.
    ✔ Search engines can crawl safely.
    ✔ No accidental data modification.

------------------------------------------------------------------------------
UNSAFE METHODS
------------------------------------------------------------------------------

Definition:
    A method is UNSAFE if it modifies server state.

Unsafe Methods:
    ✔ POST
    ✔ PUT
    ✔ PATCH
    ✔ DELETE

Example:
    POST /api/employees

Every call:
    → Creates new record
    → Changes database

These require:
    ✔ Validation
    ✔ Authentication
    ✔ Authorization
    ✔ Logging (Your Logger.cs useful here)

================================================================================
4️⃣ IDEMPOTENT VS NON-IDEMPOTENT
================================================================================

Idempotent Methods:
    PUT
    DELETE
    GET

Meaning:
    Multiple identical requests → Same final result.

Non-Idempotent:
    POST

Example:
    POST 5 times → 5 records created ❌
    PUT 5 times → 1 updated record ✔

================================================================================
5️⃣ HOW ASP.NET CORE HANDLES HTTP METHODS INTERNALLY
================================================================================

Request Flow:

Client → Kestrel → Middleware Pipeline → Routing → Controller Action

Routing checks:
    ✔ URL
    ✔ HTTP Method

Example:

[HttpGet("employees")]
[HttpPost("employees")]

Same URL.
Different methods.
Different action selected.

This is how Web API supports REST properly.

================================================================================
6️⃣ ARCHITECTURE BEST PRACTICES
================================================================================

✔ Use correct HTTP method
✔ Never use POST for everything
✔ Always return proper Status Codes
✔ Validate ModelState
✔ Secure unsafe methods
✔ Log unsafe operations (WriteErrorLog / WriteInfoLog)

Bad Practice:
    Using GET to delete data ❌

Good Practice:
    Use DELETE verb ✔

================================================================================
7️⃣ SUMMARY TABLE
================================================================================

METHOD      SAFE?      IDEMPOTENT?     PURPOSE
----------------------------------------------------------
GET         ✔ YES      ✔ YES           Read data
POST        ❌ NO       ❌ NO            Create data
PUT         ❌ NO       ✔ YES           Full update
PATCH       ❌ NO       Depends         Partial update
DELETE      ❌ NO       ✔ YES           Remove data

================================================================================
END OF FILE
================================================================================
*/
