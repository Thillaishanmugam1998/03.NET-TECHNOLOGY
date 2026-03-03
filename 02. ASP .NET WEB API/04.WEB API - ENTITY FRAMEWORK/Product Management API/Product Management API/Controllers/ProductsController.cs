using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Product_Management_API.Data;
using Product_Management_API.Models;

namespace Product_Management_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        #region 01. Constructor 
        public ProductsController(AppDbContext context)
        {
            _context = context;
        }
        #endregion

        // GET: api/products
        // Retrieve all products
        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetAll()
        {
            // Fetch all products from database
            var products = await _context.Products.ToListAsync();
            // Return HTTP 200 OK with data
            return Ok(products);
        }
        
        
        // GET: api/products/{id}
        // Retrieve product by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            // Find product by primary key
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { Message = $"Product with ID {id} not found." });
            return Ok(product);
        }
       
        
        // POST: api/products
        // Create new product
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            if (product == null)
                return BadRequest(new { Message = "Invalid product data." });
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            // Return 201 Created with location header
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        
        
        // PUT: api/products/{id}
        // Update existing product
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product updatedProduct)
        {
            if (id != updatedProduct.Id)
                return BadRequest(new { Message = "Product ID mismatch." });
            // Check if product exists
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { Message = $"Product with ID {id} not found." });
            // Update fields
            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            product.Stock = updatedProduct.Stock;
            product.Description = updatedProduct.Description;
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Product updated successfully.", Product = product });
        }
        
        // DELETE: api/products/{id}
        // Delete existing product
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { Message = $"Product with ID {id} not found." });
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Product deleted successfully." });
        }
    }
}
