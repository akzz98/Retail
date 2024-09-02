using Microsoft.AspNetCore.Mvc;
using Retail.Models.ViewModels;
using Retail.Services;

namespace Retail.Controllers
{
    public class CartController : Controller
    {
        private readonly TableStorageService _tableStorageService;

        public CartController(TableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = HttpContext.Session.Get<CartViewModel>("Cart") ?? new CartViewModel();
            cart.TotalPrice = cart.Items.Sum(i => i.Product.Price * i.Quantity);

            // Update the cart item count in ViewBag
            ViewBag.CartItemCount = cart.Items.Sum(i => i.Quantity);

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Add(string partitionKey, string rowKey, int quantity)
        {
            var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
            if (product == null)
            {
                return NotFound();
            }

            var cart = HttpContext.Session.Get<CartViewModel>("Cart") ?? new CartViewModel();
            var cartItem = cart.Items.FirstOrDefault(i => i.Product.RowKey == rowKey);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItemViewModel
                {
                    Product = new ProductViewModel
                    {
                        PartitionKey = product.PartitionKey,
                        RowKey = product.RowKey,
                        Name = product.Name,
                        Price = product.Price,
                        Description = product.Description,
                        CategoryName = product.CategoryRowKey,
                        Quantity = product.Quantity,
                        ImageUrl = product.ImageUrl
                    },
                    Quantity = quantity
                });
            }

            // Update the cart in session
            HttpContext.Session.Set("Cart", cart);

            // Update the cart item count in ViewBag
            ViewBag.CartItemCount = cart.Items.Sum(i => i.Quantity);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(string productId)
        {
            var cart = HttpContext.Session.Get<CartViewModel>("Cart") ?? new CartViewModel();
            var cartItem = cart.Items.FirstOrDefault(i => i.Product.RowKey == productId);

            if (cartItem != null)
            {
                cart.Items.Remove(cartItem);
                HttpContext.Session.Set("Cart", cart);
            }

            // Update the cart item count in ViewBag
            ViewBag.CartItemCount = cart.Items.Sum(i => i.Quantity);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove("Cart");

            // Clear the cart item count in ViewBag
            ViewBag.CartItemCount = 0;

            return RedirectToAction("Index");
        }
    }
}
