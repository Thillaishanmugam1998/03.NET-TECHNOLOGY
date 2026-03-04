using System.ComponentModel.DataAnnotations;
namespace ECommerceApp.Models
{
    // Dependent in the 1:1 relationship with Customer
    // PK = FK (same property) is the canonical required 1:1 pattern.
    public class Profile : BaseAuditableEntity
    {
        // Both Primary Key and Foreign Key to Customer
        [Key] // Keep this to make the PK explicit for readers
        public int CustomerId { get; set; }

        // Required 1:1 nav back to principal Customer
        public virtual Customer Customer { get; set; } = null!;

        // Extra profile info
        public string DisplayName { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
    }

    /*
    SQL real table structure (concept):

    CREATE TABLE Profiles (
        CustomerId INT PRIMARY KEY,      -- also Foreign Key
        DisplayName NVARCHAR(100) NOT NULL,
        Gender NVARCHAR(20) NOT NULL,
        DateOfBirth DATE NOT NULL,
        CONSTRAINT FK_Profiles_Customers_CustomerId
            FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
            ON DELETE CASCADE
    );

    Why CustomerId is both PK and FK?
    - PK: one unique profile row.
    - FK: profile must belong to an existing customer.
    - So this enforces exactly one profile per customer (1:1 pattern).
    */
}
