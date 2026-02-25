using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace DemoAPI.Controllers
{

    #region 🌍 01. WHAT IS WEB API RETURN TYPE?

    /*
     
    =============================================
    WHAT IS RETURN TYPE?
    =============================================

    Return Type means:

        What the Controller Action Method sends back to Client.

    Client can be:

        • Browser
        • Postman
        • Mobile App
        • Angular / React App
        • Another API

    Client always receives:

        ✔ Status Code
        ✔ Data (optional)
        ✔ Headers (optional)



    =============================================
    REAL WORLD EXAMPLE
    =============================================

    Client Request:

        GET /api/products/1


    Server Response:

        Status Code: 200 OK

        Body:

        {
            "id": 1,
            "name": "Laptop",
            "price": 50000
        }



    =============================================
    ASP.NET CORE INTERNAL FLOW
    =============================================

    Client
        ↓

    Kestrel Server
        ↓

    Middleware
        ↓

    Routing
        ↓

    Controller
        ↓

    Action Method
        ↓

    RETURN TYPE decides:

        ✔ Status Code
        ✔ Response Body


    =============================================

    */

    #endregion

    #region 🌍 02. MODEL CLASS

    /*
     
    Model represents Database Table.

    Example:

        Database Table: Products

    Columns:

        Id
        Name
        Price

    */

    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }
    }

    #endregion

    #region 🌍 03. FAKE DATABASE

    /*
     
    This simulates Database.

    Real world:

        SQL Server
        MySQL
        Oracle

    */

    public static class FakeDatabase
    {
        public static List<Product> products = new List<Product>()
        {
            new Product{ Id = 1, Name = "Laptop", Price = 50000 },
            new Product{ Id = 2, Name = "Mobile", Price = 20000 }
        };
    }

    #endregion

    [ApiController]
    [Route("api/[controller]")]
    public class ReturnTypesAndStatusCodesController : ControllerBase
    {

        #region 🌍 04. RETURNING NORMAL OBJECT

        /*
         
        =============================================
        RETURN TYPE: Product
        =============================================

        Example:

            public Product Get()

        =============================================
        WHAT HAPPENS INTERNALLY?
        =============================================

        Step 1:

            Method returns Product Object

        Step 2:

            ASP.NET Core converts object to JSON

        Step 3:

            ASP.NET Core automatically assigns:

                Status Code = 200 OK


        =============================================
        PROBLEM
        =============================================

        If Product = NULL

        Still response:

            Status Code = 200

            Body = null

        This is WRONG.


        Correct status should be:

            404 Not Found


        =============================================
        CONCLUSION

        Never use normal return type in production


        */

        [HttpGet("NormalReturn/{id}")]
        public Product NormalReturn(int id)
        {
            return FakeDatabase.products.FirstOrDefault(x => x.Id == id);
        }


        #endregion

        #region 🌍 05. RETURN TYPE: IActionResult

        /*
         
        =============================================
        IActionResult is INTERFACE
        =============================================

        It gives full control over response.


        You can return:

            Ok()

            NotFound()

            BadRequest()

            Created()

            StatusCode()


        =============================================
        BEST USE CASE:

        When multiple status codes possible

        =============================================

        */

        [HttpGet("IActionResult/{id}")]
        public IActionResult IActionResultExample(int id)
        {

            var product = FakeDatabase.products.FirstOrDefault(x => x.Id == id);


            if (product == null)
            {
                return NotFound();
            }


            return Ok(product);

        }

        #endregion

        #region 🌍 06. RETURN TYPE: ActionResult<T>  ⭐ BEST PRACTICE

        /*
         
        =============================================
        BEST RETURN TYPE IN ASP.NET CORE
        =============================================

        Combines BOTH:

            ✔ Strong typing
            ✔ Status Code Control


        =============================================
        WHY BEST?

        ✔ Swagger shows correct model

        ✔ Clean code

        ✔ Production ready


        */

        [HttpGet("ActionResult/{id}")]
        public ActionResult<Product> ActionResultExample(int id)
        {

            var product = FakeDatabase.products.FirstOrDefault(x => x.Id == id);


            if (product == null)
            {
                return NotFound();
            }


            return product;

        }


        #endregion

        #region 🌍 07. 200 OK STATUS CODE

        /*
         
        =============================================
        MEANING:

        SUCCESS

        =============================================

        Used for:

        GET

        PUT

        DELETE


        */

        [HttpGet("200")]
        public IActionResult Status200()
        {
            return Ok(FakeDatabase.products);
        }


        #endregion

        #region 🌍 08. 201 CREATED

        /*
         
        =============================================

        MEANING:

        Resource Created

        Used for:

        POST

        =============================================

        IMPORTANT:

        Must return Location Header

        */

        [HttpPost("201")]
        public IActionResult Status201(Product product)
        {

            product.Id = FakeDatabase.products.Max(x => x.Id) + 1;

            FakeDatabase.products.Add(product);


            return CreatedAtAction(nameof(Status200), new { id = product.Id }, product);

        }


        #endregion

        #region 🌍 09. 204 NO CONTENT

        /*
         
        =============================================

        SUCCESS

        BUT NO DATA

        Used for:

        DELETE

        */

        [HttpDelete("204/{id}")]
        public IActionResult Status204(int id)
        {

            var product = FakeDatabase.products.FirstOrDefault(x => x.Id == id);

            FakeDatabase.products.Remove(product);


            return NoContent();

        }

        #endregion

        #region 🌍 10. 400 BAD REQUEST

        /*
         
        CLIENT ERROR

        Client sends wrong data

        */

        [HttpGet("400/{id}")]
        public IActionResult Status400(int id)
        {

            if (id <= 0)
            {
                return BadRequest("Invalid ID");
            }


            return Ok();

        }

        #endregion

        #region 🌍 11. 401 UNAUTHORIZED

        /*
         
        Authentication Failed

        */

        [HttpGet("401")]
        public IActionResult Status401()
        {

            return Unauthorized();

        }


        #endregion

        #region 🌍 12. 403 FORBIDDEN

        /*
         
        Authentication success

        But no permission

        */

        [HttpGet("403")]
        public IActionResult Status403()
        {

            return Forbid();

        }

        #endregion

        #region 🌍 13. 404 NOT FOUND

        /*
         
        Resource not found

        */

        [HttpGet("404/{id}")]
        public IActionResult Status404(int id)
        {

            return NotFound();

        }

        #endregion

        #region 🌍 14. 500 INTERNAL SERVER ERROR

        /*
         
        Server crash

        */

        [HttpGet("500")]
        public IActionResult Status500()
        {

            return StatusCode(500);

        }


        #endregion

        #region 🌍 15. 503 SERVICE UNAVAILABLE

        /*
         
        Server temporarily down

        */

        [HttpGet("503")]
        public IActionResult Status503()
        {

            return StatusCode(503);

        }

        #endregion
        
        
        #region 16. FULL STATUS CODE
        /*
            200	OK	Successful GET, search, login, or action-based POST
            201	Created	POST created a new resource — include Location header
            202	Accepted	Async request queued — include polling URL
            204	No Content	PUT / DELETE success — no response body allowed
            301	Moved Permanently	Resource permanently moved — clients should update bookmarks
            302	Found	Temporarily at different URL — clients must check again next time
            304	Not Modified	Client cache is still valid — save bandwidth, no body returned
            400	Bad Request	Client sent invalid request — validation failed or malformed
            401	Unauthorized	Not authenticated — provide valid credentials first
            403	Forbidden	Authenticated but lacks permission — contact admin
            404	Not Found	Resource does not exist — wrong ID or deleted
            405	Method Not Allowed	HTTP verb not supported here — include Allow header
            500	Internal Server Error	Server crashed — log details, return safe message
            501	Not Implemented	Feature planned but not yet built
            503	Service Unavailable	Server temporarily down — include Retry-After header
            504	Gateway Timeout	Upstream service (DB/API) did not respond in time
        */
        #endregion


}
}
