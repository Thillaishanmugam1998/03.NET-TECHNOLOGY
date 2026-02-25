using EmployeeAPI.Models;

namespace EmployeeAPI.Services
{
    // Interface defines the contract – what operations EmployeeService must provide.
    // The controller depends on this interface, not the concrete class.
    // This makes it easy to swap implementations later (e.g. database version).
    public interface IEmployeeService
    {
        List<Employee> GetAll();
        Employee? GetById(int id);
        Employee Create(Employee employee);
        Employee? Update(int id, Employee employee);
        bool Delete(int id);
    }
}
