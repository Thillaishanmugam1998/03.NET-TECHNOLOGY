using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace DemoAPI.Controllers
{
    #region 🔷 CONTROLLER SETUP

    /*
        [ApiController] enables:
        ✔ Automatic Model Binding Source Inference
        ✔ Automatic Model Validation (400)
        ✔ Better API conventions

        Base Route:
        api/CompleteModelBindingWithContentTypeGuide
    */
    #endregion

    [ApiController]
    [Route("api/[controller]")]
    public class CompleteModelBindingWithContentTypeGuide : ControllerBase
    {

        #region 1️⃣ FROM ROUTE - Resource Identification

        /*
        REAL USE CASE:
        ----------------
        When you want to fetch/update/delete a specific resource.

        Example:
        Get Employee by ID
        Delete Order by ID
        Update Product by ID

        WHY ROUTE?
        ----------
        Because ID is part of resource path.

        CALL:
        GET /api/CompleteModelBindingWithContentTypeGuide/employee/10
        */

        [HttpGet("employee/{id}")]
        public IActionResult GetEmployee(int id)
        {
            return Ok($"Employee fetched with ID: {id}");
        }

        #endregion

        #region 2️⃣ FROM QUERY - Filtering / Searching / Pagination

        /*
        REAL USE CASE:
        ----------------
        Searching employees
        Filtering orders
        Pagination

        Example:
        GET /employees?department=IT&page=1&pageSize=10

        WHY QUERY?
        ----------
        Because these are optional filtering parameters.

        CALL:
        GET /api/CompleteModelBindingWithContentTypeGuide/search?name=John&age=30
        */

        [HttpGet("search")]
        public IActionResult Search(string name, int age)
        {
            return Ok($"Searching Name: {name}, Age: {age}");
        }

        #endregion

        #region 3️⃣ FROM BODY - Complex Object (MOST COMMON IN POST)

        /*
        REAL USE CASE:
        ----------------
        Creating a new employee
        Updating a product
        Saving customer details

        WHY BODY?
        ----------
        Because large structured data should go in request body.

        CONTENT-TYPE MUST BE:
        application/json

        CALL:
        POST /api/CompleteModelBindingWithContentTypeGuide/create

        Headers:
        Content-Type: application/json

        BODY:
        {
            "name": "Thillai",
            "salary": 50000,
            "email": "test@gmail.com"
        }
        */

        [HttpPost("create")]
        public IActionResult CreateEmployee(EmployeeRequest request)
        {
            return Ok(request);
        }

        #endregion

        #region 4️⃣ FROM FORM - File Upload & HTML Form Submission

        /*
        🔥 VERY IMPORTANT SECTION

        WHAT IS FromForm?
        ------------------
        It binds data from:
        multipart/form-data

        Used when:
        ✔ Uploading files
        ✔ Submitting HTML forms
        ✔ Sending text + file together

        WHY NOT JSON?
        -------------
        Because JSON cannot carry file binary data directly.

        CONTENT-TYPE:
        multipart/form-data

        REAL USE CASE:
        --------------
        Upload Profile Image
        Upload Invoice PDF
        Upload Excel Sheet

        CALL:
        POST /api/CompleteModelBindingWithContentTypeGuide/upload

        Postman:
        Body → form-data

        Key          Type
        ------------------
        name         Text
        file         File
        */

        [HttpPost("upload")]
        public IActionResult UploadFile(
            [FromForm] string name,
            [FromForm] IFormFile file)
        {
            return Ok(new
            {
                Message = "File uploaded successfully",
                UserName = name,
                FileName = file?.FileName
            });
        }

        /*
        WHEN NOT TO USE FromForm?
        --------------------------
        ❌ Normal JSON CRUD APIs
        ❌ Simple POST requests
        ❌ When no file is involved

        USE FromBody instead for JSON APIs.
        */

        #endregion

        #region 5️⃣ FROM HEADER - Metadata & Authentication

        /*
        🔥 VERY IMPORTANT SECTION

        WHAT IS FromHeader?
        -------------------
        It binds value from HTTP Request Header.

        Headers contain metadata, NOT business payload.

        REAL USE CASE:
        --------------
        ✔ API Key
        ✔ Authorization Token
        ✔ CorrelationId
        ✔ TenantId
        ✔ Versioning

        CALL:
        GET /api/CompleteModelBindingWithContentTypeGuide/secure

        Headers:
        X-API-KEY: 12345
        */

        [HttpGet("secure")]
        public IActionResult SecureEndpoint(
            [FromHeader(Name = "X-API-KEY")] string apiKey)
        {
            if (apiKey != "12345")
                return Unauthorized("Invalid API Key");

            return Ok("Access Granted");
        }

        /*
        WHEN NOT TO USE FromHeader?
        ----------------------------
        ❌ For large data
        ❌ For form input
        ❌ For JSON object
        ❌ For file upload

        Headers are only for metadata.
        */

        #endregion

        #region 🔷 CONTENT NEGOTIATION IN ASP.NET CORE WEB API

        /*
        🔥 WHAT IS CONTENT NEGOTIATION?
        --------------------------------
        Content Negotiation means:

        The server decides what format to return
        based on the client's "Accept" header.

        The client says:
        "I want response in this format"

        Server checks available Output Formatters
        and returns best match.


        --------------------------------------------
        🔥 HOW IT WORKS INTERNALLY?
        --------------------------------------------
        1. Client sends Accept header.
        2. ASP.NET Core checks Output Formatters.
        3. Best match is selected.
        4. Response is formatted (JSON / XML).


        --------------------------------------------
        🔥 REAL PRODUCTION USE CASE
        --------------------------------------------
        ✔ Public APIs
        ✔ Government APIs
        ✔ Enterprise integrations
        ✔ Systems supporting XML + JSON

        ❌ Most internal microservices use JSON only.


        --------------------------------------------
        🔥 ENABLE XML SUPPORT (Program.cs)
        --------------------------------------------
        builder.Services.AddControllers()
               .AddXmlSerializerFormatters();


        --------------------------------------------
        🔥 SAMPLE CALL
        --------------------------------------------
        GET /api/CompleteModelBindingWithContentTypeGuide/negotiation

        Headers:
        Accept: application/json
        OR
        Accept: application/xml
        */

        [HttpGet("negotiation")]
        public IActionResult GetWithNegotiation()
        {
            var response = new
            {
                Message = "Content Negotiation Working",
                GeneratedAt = DateTime.Now
            };

            return Ok(response);
        }

        #endregion

        #region 🔷 PRODUCES ATTRIBUTE

        /*
        🔥 WHAT IS [Produces]?
        --------------------------------
        Specifies the response content type
        that this API will return.

        It controls response format.


        --------------------------------------------
        🔥 WHY USED?
        --------------------------------------------
        ✔ API contract enforcement
        ✔ Swagger documentation clarity
        ✔ Public API standardization


        --------------------------------------------
        🔥 REAL PRODUCTION USE CASE
        --------------------------------------------
        Banking API returning only JSON
        Even if XML formatter enabled,
        it forces JSON.


        --------------------------------------------
        🔥 SAMPLE CALL
        --------------------------------------------
        GET /api/CompleteModelBindingWithContentTypeGuide/produce

        Response Content-Type:
        application/json
        */

        [Produces("application/json")]
        [HttpGet("produce")]
        public IActionResult GetProducesExample()
        {
            return Ok(new
            {
                Status = "Success",
                Info = "Produces attribute enforced JSON response"
            });
        }

        #endregion

        #region 🔷 CONSUMES ATTRIBUTE

        /*
        🔥 WHAT IS [Consumes]?
        --------------------------------
        Specifies what Content-Type
        the API accepts from client.

        It validates request body format.


        --------------------------------------------
        🔥 WHY USED?
        --------------------------------------------
        ✔ Security
        ✔ Prevent incorrect content type
        ✔ Strict API contract
        ✔ Prevent 3rd party misuse


        --------------------------------------------
        🔥 REAL PRODUCTION USE CASE
        --------------------------------------------
        Payment Processing API
        Login API
        Banking Transaction API


        --------------------------------------------
        🔥 SAMPLE CALL
        --------------------------------------------
        POST /api/CompleteModelBindingWithContentTypeGuide/consume

        Headers:
        Content-Type: application/json

        BODY:
        {
            "name": "Thillai",
            "salary": 50000,
            "email": "test@gmail.com"
        }

        If client sends:
        Content-Type: text/plain

        RESULT:
        415 Unsupported Media Type
        */

        [Consumes("application/json")]
        [HttpPost("consume")]
        public IActionResult PostConsumesExample([FromBody] EmployeeRequest request)
        {
            return Ok(new
            {
                Message = "Consumes attribute validated successfully",
                Data = request
            });
        }

        #endregion
    }



    #region 🔷 DTO MODEL WITH VALIDATION

    /*
        Data Transfer Object (DTO)
        Used for input validation
    */

    public class EmployeeRequest
    {
        [Required]
        public string Name { get; set; }

        [Range(1000, 100000)]
        public decimal Salary { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [BindNever]
        public string InternalCode { get; set; }
    }

    #endregion

   

    }
