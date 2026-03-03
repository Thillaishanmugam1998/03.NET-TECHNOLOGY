namespace EmployeeAPI.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Department { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public int YearsOfExperience { get; set; }
    }
}
