using System.Collections.Generic;
using System.Linq;

namespace Retail.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public double TotalPrice { get; set; }
    }

    public class CartItemViewModel
    {
        public ProductViewModel Product { get; set; }
        public int Quantity { get; set; }
    }
}
