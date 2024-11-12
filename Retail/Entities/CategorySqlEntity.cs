using System.ComponentModel.DataAnnotations;

namespace Retail.Entities
{
    public class CategorySqlEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}