using EmployeeAPI.Models;
using EmployeeAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]   // base URL: /api/employees
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;

        // ASP.NET Core injects IEmployeeService automatically
        public EmployeesController(IEmployeeService service)
        {
            _service = service;
        }

        // GET /api/employees  →  returns all employees
        [HttpGet]
        public IActionResult GetAll()
        {
            var employees = _service.GetAll();
            return Ok(employees);
        }

        // GET /api/employees/1  →  returns one employee or 404
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var employee = _service.GetById(id);
            if (employee == null)
                return NotFound($"Employee with ID {id} not found.");

            return Ok(employee);
        }

        // POST /api/employees  →  creates a new employee
        [HttpPost]
        public IActionResult Create([FromBody] Employee employee)
        {
            var created = _service.Create(employee);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT /api/employees/1  →  updates an existing employee
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Employee employee)
        {
            var updated = _service.Update(id, employee);
            if (updated == null)
                return NotFound($"Employee with ID {id} not found.");

            return Ok(updated);
        }

        // DELETE /api/employees/1  →  deletes an employee
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var deleted = _service.Delete(id);
            if (!deleted)
                return NotFound($"Employee with ID {id} not found.");

            return Ok($"Employee {id} deleted successfully.");
        }
    }
}
