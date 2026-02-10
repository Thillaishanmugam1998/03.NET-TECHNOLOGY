using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using _02_SampleWebApi.Models;

namespace _02_SampleWebApi.Controllers
{
    /// <summary>
    /// Handles all Student related API operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        #region In-Memory Data Source
        /*
         * This list acts as a temporary database.
         * Data will be lost when the application restarts.
         */
        private static readonly List<Student> students = new List<Student>
        {
            new Student { Id = 1, Name = "Thillai", Department = "IT" },
            new Student { Id = 2, Name = "Tamizh", Department = "CSE" }
        };
        #endregion

        #region GET APIs

        /// <summary>
        /// Gets all students
        /// URL: GET /api/students
        /// </summary>
        [HttpGet]
        public IActionResult GetStudents()
        {
            return Ok(students);
        }

        /// <summary>
        /// Gets a student by Id
        /// URL: GET /api/students/{id}
        /// </summary>
        /// <param name="id">Student Id</param>
        [HttpGet("{id}")]
        public IActionResult GetStudentById(int id)
        {
            var student = students.Find(s => s.Id == id);

            if (student == null)
            {
                return NotFound($"Student with Id {id} not found.");
            }

            return Ok(student);
        }

        #endregion

        #region POST APIs

        /// <summary>
        /// Adds a new student
        /// URL: POST /api/students
        /// </summary>
        /// <param name="student">Student object</param>
        [HttpPost]
        public IActionResult AddStudent([FromBody] Student student)
        {
            if (student == null)
            {
                return BadRequest("Student data is required.");
            }

            students.Add(student);

            return CreatedAtAction(
                nameof(GetStudentById),
                new { id = student.Id },
                student
            );
        }

        #endregion
    }
}
