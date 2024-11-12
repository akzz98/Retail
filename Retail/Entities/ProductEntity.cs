using System.ComponentModel.DataAnnotations;

namespace Retail.Entities
{
    public class ProductEntity
    {
        [Key] // This attribute indicates that this property is the primary key
        public int Id { get; set; } // Primary key for SQL database

        // Product properties
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public int CategoryId { get; set; } // Foreign key to the Category table
    }
}