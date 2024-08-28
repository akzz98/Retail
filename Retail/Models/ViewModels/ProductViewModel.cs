namespace Retail.Models.ViewModels
{
    public class ProductViewModel
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public string CategoryName { get; set; }
    }
}