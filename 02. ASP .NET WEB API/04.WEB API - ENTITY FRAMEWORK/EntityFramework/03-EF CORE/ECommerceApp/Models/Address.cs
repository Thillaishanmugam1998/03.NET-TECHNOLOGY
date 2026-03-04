namespace ECommerceApp.Models
{
    // Dependent entity in Customer (1:M) relationship
    public class Address : BaseAuditableEntity
    {
        // Primary Key
        public int AddressId { get; set; }

        public string Line1 { get; set; } = null!;
        public string? Line2 { get; set; }
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string Country { get; set; } = null!;

        // Foreign Key to Customer
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;
    }
}
