namespace ECommerceApp.Models
{
    // Principal for Profile, Addresses, and Orders
    public class Customer : BaseAuditableEntity
    {
        // Primary Key (use explicit name to be clear)
        public int CustomerId { get; set; }

        // Basic Customer Info (Required by default through non-nullable refs)
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;

        // 1:1 → A customer may have one Profile
        public virtual Profile? Profile { get; set; }

        // 1:M → A customer can have many addresses and orders
        public virtual ICollection<Address>? Addresses { get; set; }
        public virtual ICollection<Order>? Orders { get; set; }
    }

    /*
    SQL real table structure (concept):

    CREATE TABLE Customers (
        CustomerId INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(100) NOT NULL,
        Email NVARCHAR(150) NOT NULL,
        Phone NVARCHAR(20) NOT NULL
    );

    Relationship meaning:
    - Customers is the parent (principal) table.
    - One customer row can have 0 or 1 matching row in Profiles.
    - Match is done by CustomerId.
    */
}
