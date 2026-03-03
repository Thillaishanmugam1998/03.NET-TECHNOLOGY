using EmployeeAPI.Models;
using EmployeeAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]   // Base URL: /api/employees
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;

        public EmployeesController(IEmployeeService service)
        {
            _service = service;
        }

        #region ✅ 200 OK
        /*
        ============================================================
        200 OK
        ============================================================

        MEANING:
        Request successful.

        USED FOR:
        ✔ GET
        ✔ PUT
        ✔ DELETE
        ✔ Login
        ✔ Search


        REAL WORLD EXAMPLE:

        GET /api/employees

        Response:
        200 OK
        [
            { employee data }
        ]

        ============================================================
        */

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetAll()
        {
            var employees = _service.GetAll();

            return Ok(employees); // 200 OK
        }


        #endregion

        #region ✅ 201 Created
        /*
        ============================================================
        201 Created
        ============================================================

        MEANING:
        New resource successfully created

        MUST INCLUDE:
        Location Header

        USED FOR:
        POST

        ============================================================
        */

        [HttpPost]
        [Route("[action]")]
        public IActionResult Create([FromBody] Employee employee)
        {
            var created = _service.Create(employee);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created); // 201 Created
        }

        #endregion

        #region ✅ 202 Accepted
        /*
        ============================================================
        202 Accepted
        ============================================================

        MEANING:
        Request accepted but processing not completed yet

        USED FOR:
        Background jobs
        Async processing

        MUST INCLUDE:
        Polling URL

        ============================================================
        */

        [HttpPost("process-payroll")]
        public IActionResult ProcessPayroll()
        {
            var trackingId = Guid.NewGuid();

            return AcceptedAtAction(
                nameof(CheckPayrollStatus),
                new { id = trackingId },
                new
                {
                    Message = "Payroll processing started",
                    TrackingId = trackingId
                });
        }

        [HttpGet("process-payroll/{id}")]
        public IActionResult CheckPayrollStatus(Guid id)
        {
            return Ok(new
            {
                TrackingId = id,
                Status = "Processing"
            });
        }

        #endregion

        #region ✅ 204 No Content
        /*
        ============================================================
        204 No Content
        ============================================================

        MEANING:
        Success but no response body

        USED FOR:
        PUT
        DELETE

        ============================================================
        */

        [HttpDelete]
        [Route("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            var deleted = _service.Delete(id);

            if (!deleted)
                return NotFound();

            return NoContent(); // 204
        }

        #endregion

        #region ❌ 400 Bad Request
        /*
        ============================================================
        400 Bad Request
        ============================================================

        MEANING:
        Client sent invalid data

        ============================================================
        */

        [HttpPost("validate")]
        public IActionResult ValidateEmployee(Employee employee)
        {
            if (string.IsNullOrEmpty(employee.EmployeeName))
            {
                return BadRequest("Employee name required");
            }

            return Ok();
        }

        #endregion

        #region ❌ 401 Unauthorized
        /*
        ============================================================
        401 Unauthorized
        ============================================================

        MEANING:
        User not authenticated

        ============================================================
        */

        [HttpGet("secure-data")]
        public IActionResult SecureData()
        {
            return Unauthorized();
        }

        #endregion

        #region ❌ 403 Forbidden
        /*
        ============================================================
        403 Forbidden
        ============================================================

        MEANING:
        User authenticated but no permission

        ============================================================
        */

        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Forbid();
        }

        #endregion

        #region ❌ 404 Not Found
        /*
        ============================================================
        404 Not Found
        ============================================================

        MEANING:
        Resource does not exist

        ============================================================
        */

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var employee = _service.GetById(id);

            if (employee == null)
                return NotFound("Employee not found");

            return Ok(employee);
        }

        #endregion

        #region ❌ 405 Method Not Allowed
        /*
        ============================================================
        405 Method Not Allowed
        ============================================================

        MEANING:
        HTTP verb not supported

        MUST INCLUDE:
        Allow header

        ============================================================
        */

        [HttpPost("readonly")]
        public IActionResult ReadOnly()
        {
            Response.Headers.Add("Allow", "GET");

            return StatusCode(405, "Only GET allowed");
        }

        #endregion

        #region ❌ 500 Internal Server Error
        /*
        ============================================================
        500 Internal Server Error
        ============================================================

        MEANING:
        Server crash

        ============================================================
        */

        [HttpGet("crash")]
        public IActionResult Crash()
        {
            try
            {
                throw new Exception("Database crashed");
            }
            catch
            {
                return StatusCode(500,
                    "Internal Server Error. Contact support.");
            }
        }

        #endregion

        #region ❌ 501 Not Implemented
        /*
        ============================================================
        501 Not Implemented
        ============================================================

        MEANING:
        Feature not built yet

        ============================================================
        */

        [HttpGet("future-feature")]
        public IActionResult FutureFeature()
        {
            return StatusCode(501,
                "Feature not implemented yet");
        }

        #endregion

        #region ❌ 503 Service Unavailable
        /*
        ============================================================
        503 Service Unavailable
        ============================================================

        MEANING:
        Server temporarily down

        MUST INCLUDE:
        Retry-After Header

        ============================================================
        */

        [HttpGet("maintenance")]
        public IActionResult Maintenance()
        {
            Response.Headers.Add("Retry-After", "60");

            return StatusCode(503,
                "Service unavailable. Try after 60 seconds.");
        }

        #endregion

        #region ❌ 504 Gateway Timeout
        /*
        ============================================================
        504 Gateway Timeout
        ============================================================

        MEANING:
        External service timeout

        ============================================================
        */

        [HttpGet("timeout")]
        public IActionResult Timeout()
        {
            return StatusCode(504,
                "Database timeout. Try again later.");
        }

        #endregion

    }
}
