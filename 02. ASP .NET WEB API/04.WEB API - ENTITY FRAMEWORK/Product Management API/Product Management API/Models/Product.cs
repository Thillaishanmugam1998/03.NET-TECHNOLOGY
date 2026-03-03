using System.ComponentModel.DataAnnotations.Schema;

namespace Product_Management_API.Models
{
    public class Product
    {
        public int Id { get; set; }                // Primary key
        
        public string Name { get; set; } = null!;  // Product name
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }         // Product price
        
        public int Stock { get; set; }             // Available stock quantity
        
        public string? Description { get; set; }   // Optional description field

        // New properties added
        public string? CreatedBy { get; set; }     // Name of the user who added the product
        public DateTime CreatedDate { get; set; }  // When the product was added
    }
}
