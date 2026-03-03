using EmployeeAPI.Models;

namespace EmployeeAPI.Services
{
    // EmployeeService holds all employee data in a simple List.
    // Registered as Singleton in Program.cs so the list stays alive
    // for the entire lifetime of the application.
    public class EmployeeService : IEmployeeService
    {
        private readonly List<Employee> _employees = new();
        private int _nextId = 1;

        public EmployeeService()
        {
            // Add sample data so the API has something to show on first run
            _employees.Add(new Employee { Id = _nextId++, EmployeeName = "Alice Johnson", Address = "12 Elm Street, New York", MobileNumber = "+1-212-555-0101", Email = "alice@company.com", DateOfBirth = new DateTime(1990, 4, 15), Department = "Engineering", Salary = 95000, YearsOfExperience = 7 });
            _employees.Add(new Employee { Id = _nextId++, EmployeeName = "Bob Martinez",  Address = "45 Oak Avenue, Chicago",  MobileNumber = "+1-312-555-0202", Email = "bob@company.com",   DateOfBirth = new DateTime(1985, 9, 22), Department = "HR",          Salary = 72000, YearsOfExperience = 12 });
            _employees.Add(new Employee { Id = _nextId++, EmployeeName = "Carol Singh",   Address = "78 Maple Road, Austin",   MobileNumber = "+1-512-555-0303", Email = "carol@company.com", DateOfBirth = new DateTime(1995, 1,  8), Department = "Finance",     Salary = 88000, YearsOfExperience = 4 });
        }

        // Return all employees
        public List<Employee> GetAll()
        {
            return _employees;
        }

        // Return one employee by ID, or null if not found
        public Employee? GetById(int id)
        {
            return _employees.FirstOrDefault(e => e.Id == id);
        }

        // Add a new employee and return it with the assigned ID
        public Employee Create(Employee employee)
        {
            employee.Id = _nextId++;
            _employees.Add(employee);
            return employee;
        }

        // Update an existing employee. Returns null if ID not found.
        public Employee? Update(int id, Employee updated)
        {
            var existing = _employees.FirstOrDefault(e => e.Id == id);
            if (existing == null) return null;

            existing.EmployeeName      = updated.EmployeeName;
            existing.Address           = updated.Address;
            existing.MobileNumber      = updated.MobileNumber;
            existing.Email             = updated.Email;
            existing.DateOfBirth       = updated.DateOfBirth;
            existing.Department        = updated.Department;
            existing.Salary            = updated.Salary;
            existing.YearsOfExperience = updated.YearsOfExperience;

            return existing;
        }

        // Delete an employee. Returns true if deleted, false if not found.
        public bool Delete(int id)
        {
            var employee = _employees.FirstOrDefault(e => e.Id == id);
            if (employee == null) return false;

            _employees.Remove(employee);
            return true;
        }
    }
}
