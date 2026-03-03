using Microsoft.AspNetCore.Mvc;
using _01_SampleWebApi.Models;

namespace _01_SampleWebApi.Controllers
{
    #region 🔹 What is Controller?

    /*
        A Controller handles HTTP requests (GET, POST, PUT, DELETE).

        Client (Browser/Postman)
                ↓
           HTTP Request
                ↓
        ProductController
                ↓
           Returns Response (JSON/Text/Status Code)

        Since this is a Web API project (no Views),
        we inherit from ControllerBase (recommended for APIs).
    */

    #endregion

    [ApiController]   // Enables automatic API behaviors (model validation, better binding, etc.)
    [Route("api/[controller]")]   // Route becomes: api/product
    public class ProductController : ControllerBase
    {

        #region 🔹 Sample In-Memory Data

        /*
            This is a temporary product list stored in memory.
            In real projects, data comes from Database.
        */

        public List<Product> products = new List<Product>
        {
            new Product { Id = 1, Name = "Laptop", Price = 999.99m },
            new Product { Id = 2, Name = "SmartPhone", Price = 777.7m },
            new Product { Id = 3, Name = "Tablet", Price = 499.5m }
        };

        #endregion

        #region --- 01. HttpGet - GetProduct() ---

        #region 🔹 GET ALL PRODUCTS
        /*
            What is HttpGet?

            [HttpGet] means this method handles HTTP GET request.

            URL:
            GET http://localhost:5155/api/product

            What is ActionResult<List<Product>>?

            ✔ It returns List<Product> (JSON)
            ✔ Also allows returning status codes (404, 400, etc.)
            ✔ Recommended for Web API
        */
        #endregion

        [HttpGet]
        public ActionResult<List<Product>> GetProduct()
        {
            if (products == null || products.Count == 0)
            {
                return NotFound("No products available");  // 404 Status
            }

            return products;   // Automatically returns 200 OK with JSON
        }

        #endregion

        #region --- 02. HttpPost - Insert() ---

        #region 🔹 INSERT NEW PRODUCT
        /*
            URL:
            POST api/product

            Body (JSON):
            {
                "id": 4,
                "name": "Monitor",
                "price": 300
            }
        */
        #endregion

        [HttpPost]
        public ActionResult<Product> Insert(Product newProduct)
        {
            if (newProduct == null)
            {
                return BadRequest("Product data is null"); // 400 Bad Request
            }

            return Ok(newProduct); // 200 OK with the created product
        }
        #endregion

        #region --- 03. HttpPut - Update() ---

        #region 🔹 UPDATE PRODUCT
        /*
            URL:
            PUT api/product/2

            Body (JSON):
            {
                "id": 2,
                "name": "Updated Phone",
                "price": 800
            }
        */
        #endregion

        [HttpPut("{id}")]
        public ActionResult Update(int id, Product updateProduct)
        {
            var product = products.Find(product => product.Id == id);

            if (product == null)
            {
                return NotFound("Product not found");
            }

            product.Name = updateProduct.Name;
            product.Price = updateProduct.Price;

            return Ok("Product updated successfully!");
        }

        #endregion

        #region --- 04. HttpDelete - Delete() --- 

        #region 🔹 DELETE PRODUCT
        /*
            URL:
            DELETE api/product/2
        */
        #endregion

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var product = products.Find(product => product.Id == id);

            if (product == null)
            {
                return NotFound("Product not found");

            }

            products.Remove(product);
            return Ok("Product deleted successfully!");
        }

        #endregion

        #region --- 05. HttpGet - GetById() ---

        #region 🔹 GET PRODUCT BY ID
        /*
            URL:
            GET api/product/2

            {id} is route parameter
        */
        #endregion

        [HttpGet("{id}")]
        public ActionResult<Product> GetById(int id)
        {
            var product = products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound("Product not found"); // 404
            }

            return product;  // 200 OK
        }

        #endregion

        #region --- 06. HttpPost - Complex Business Logic (Upsert) ---

        #region 🔹 INSERT OR UPDATE PRODUCT BY ID
        /*
            What is this method doing?

            This method performs multiple operations in one API call:

            ✔ If product does NOT exist → INSERT
            ✔ If product EXISTS → UPDATE

            This is called:

                UPSERT (Update + Insert)

            -----------------------------------------------------

            Why HttpPost?

            Because:
            - This is complex business logic
            - It modifies data
            - It is not purely Insert or purely Update
            - POST is recommended for custom operations

            -----------------------------------------------------

            URL:

                POST api/product/Upsert/5

            Body (JSON):

                {
                    "id": 5,
                    "name": "Monitor",
                    "price": 300
                }

            -----------------------------------------------------

            Possible Responses:

                201 Created → When new product inserted
                200 OK      → When product updated
        */

        #endregion

        [HttpPost("Upsert/{id}")]
        public ActionResult Upsert(int id, Product newProduct)
        {
            // Optional safety check
            if (id != newProduct.Id)
            {
                return BadRequest("ID mismatch");
            }

            var product = products.Find(p => p.Id == id);

            if (product == null)
            {
                // Insert new product
                products.Add(newProduct);

                return CreatedAtAction(nameof(GetById),
                    new { id = newProduct.Id },
                    newProduct);
            }
            else
            {
                // Update existing product
                product.Name = newProduct.Name;
                product.Price = newProduct.Price;

                return Ok("Product updated successfully!");
            }
        }

        #endregion

        #region --- 07. GetGreeting() ---

        #region 🔹 GET: api/product/greeting

        /*
            Why "greeting" inside HttpGet?

            Because if we write another [HttpGet] without route,
            it causes ERROR (same HTTP method + same route).

            Example: 

            [HttpGet]
            public string GetGreeting()
            {
                return "Hello World!";
            }

            So we change route like this:

            GET http://localhost:5155/api/product/greeting
        */

        #endregion

        [HttpGet("greeting")]
        public IActionResult GetGreeting()
        {
            return Ok("Hello World!"); // Returns 200 OK with string
        }

        #endregion

        #region --- 08. Post() ---

        #region 🔹 POST: api/product

        /*
            [HttpPost] handles HTTP POST request.

            URL:
            POST http://localhost:5155/api/product

            Why IActionResult?

            Because:
            ✔ We can return different status codes
            ✔ More flexible than returning just string

            Here also default route but this time not through ambiguity error
            Because use different HttpAttribute

            If call:
            POST http://localhost:5155/api/product  => Post()
            GET  http://localhost:5155/api/product  => GetProduct()
        */

        #endregion

        [HttpPost]
        public IActionResult Post()
        {
            return Ok("Product created successfully!");
        }

        #endregion




        #region -- Information About Controller ---

        #region 🔹 Understanding Return Types

        /*
        1️⃣ Returning string (Not Recommended for real projects)

            public string Get()
            {
                return "Hello";
            }

            Problem:
            ❌ Cannot return NotFound(), BadRequest(), etc.

        -----------------------------------------------------

        2️⃣ Returning Object Directly

            public List<Product> Get()

            ❌ Cannot return different status codes easily.

        -----------------------------------------------------

        3️⃣ Using IActionResult (Good)

            public IActionResult Get()

            ✔ Can return Ok(), NotFound(), BadRequest()

        -----------------------------------------------------

        4️⃣ Using ActionResult<T> (BEST Practice)

            public ActionResult<List<Product>> Get()

            ✔ Strongly typed
            ✔ Can return status codes
            ✔ Cleaner and recommended

        */

        #endregion

        #region 🔹 What is [ApiController]?

        /*
            [ApiController] provides:

            ✔ Automatic Model Validation
            ✔ Automatic 400 response for invalid input
            ✔ Better parameter binding
            ✔ Required for modern Web APIs
        */

        #endregion

        #region 🔹 What is [Route("api/[controller]")]?

        /*
            [controller] replaces controller name without "Controller".

            ProductController → product

            So final route becomes:

            api/product
        */

        #endregion

        #region 🔹 What is [Route("api/[controller]/[action]")]?

        /*
            [Route("api/[controller]/[action]")]

            This defines the base URL for the controller 
            AND includes the method name in the URL.

            🔹 [controller]
                Automatically replaces the controller name 
                without the word "Controller".

                Example:
                    ProductController → product

            🔹 [action]
                Automatically replaces the method name.

                Example:
                    public IActionResult GetAll()

                    → action name becomes: getall

            ----------------------------------------------------

            So if we have:

                public class ProductController

            And method:

                public IActionResult GetAll()

            The final route becomes:

                api/product/getall

            ----------------------------------------------------

            If application runs on:

                http://localhost:5155

            Full URL becomes:

                http://localhost:5155/api/product/getall

            ----------------------------------------------------

            Important:

            ✔ URL includes method name
            ✔ Easy for beginners to understand
            ❌ Not pure REST standard practice

            Professional REST APIs usually avoid [action]
            and use:

                [Route("api/[controller]")]
        */
        #endregion

        #region 🔹 What is [HttpGet]?

        /*
            [HttpGet] means this method handles HTTP GET request.

            GET is used to READ or FETCH data.

            Example:

                GET api/product
                GET api/product/1

            It should NOT modify data.
            It only retrieves information.

            Used for:
                ✔ Get All Records
                ✔ Get Record By Id
        */
        #endregion

        #region 🔹 What is [HttpPost]?

        /*
            [HttpPost] means this method handles HTTP POST request.

            POST is used to INSERT or CREATE new data.

            Example:

                POST api/product

            It usually takes data in request body (JSON).

            Used for:
                ✔ Create new record
                ✔ Complex business logic POST
        */
        #endregion

        #region 🔹 What is [HttpPut]?

        /*
            [HttpPut] means this method handles HTTP PUT request.

            PUT is used to UPDATE existing data.

            Example:

                PUT api/product/1

            It usually takes updated data in request body.

            Used for:
                ✔ Update entire record
        */
        #endregion

        #region 🔹 What is [HttpDelete]?

        /*
            [HttpDelete] means this method handles HTTP DELETE request.

            DELETE is used to REMOVE data.

            Example:

                DELETE api/product/1

            Used for:
                ✔ Delete record by Id
        */
        #endregion

        #region 🔹 What are HTTP Status Code Series?

        /*
            HTTP Status Codes tell the client what happened to the request.

            They are divided into 5 categories:

            ---------------------------------------------------

            1xx → Informational
                Request received, still processing.
                (Rarely used in Web API)

            2xx → Success
                Request was successful.

                200 OK            → Success (GET, PUT, DELETE)
                201 Created       → Resource created (POST)
                204 No Content    → Success but no data returned

            ---------------------------------------------------

            3xx → Redirection
                Client must take additional action.
                (Mostly used in web pages, not APIs)

                301 Moved Permanently
                302 Found

            ---------------------------------------------------

            4xx → Client Error
                Problem caused by client request.

                400 Bad Request   → Invalid input
                401 Unauthorized  → Not logged in
                403 Forbidden     → No permission
                404 Not Found     → Data not found

            ---------------------------------------------------

            5xx → Server Error
                Problem on server side.

                500 Internal Server Error
                503 Service Unavailable

        */
        #endregion

        #region 🔹 Controller vs ControllerBase

        /*
            Controller:
                ✔ Used in MVC (Views + API)
                ✔ Supports View()

            ControllerBase:
                ✔ Used only for APIs
                ✔ Lightweight
                ✔ Recommended for Web API

            Since this project is Web API,
            we use ControllerBase.
        */

        #endregion

        #endregion

    }
}
