using Microsoft.AspNetCore.Mvc;
using Retail.Models;
using Retail.Models.ViewModels;
using Retail.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Retail.Controllers
{
    public class CartController : Controller
    {
        private readonly ProductSqlService _productSqlService; // Use ProductSqlService instead of TableStorageService
        private readonly QueueService _queueService; // Inject a queue service

        public CartController(ProductSqlService productSqlService, QueueService queueService) // Updated constructor
        {
            _productSqlService = productSqlService; // Initialize ProductSqlService
            _queueService = queueService; // Initialize queue service
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = HttpContext.Session.Get<CartViewModel>("Cart") ?? new CartViewModel(); // Provide the key "Cart"
            cart.TotalPrice = cart.Items.Sum(i => i.Product.Price * i.Quantity);

            // Update the cart item count in ViewBag
            ViewBag.CartItemCount = cart.Items.Sum(i => i.Quantity);

            // Check for any TempData message
            ViewBag.Message = TempData["Message"]?.ToString();

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity) // Changed to use productId
        {
            var product = await _productSqlService.GetProductAsync(productId); // Updated to use ProductSqlService
            if (product == null)
            {
                return NotFound();
            }

            var cart = HttpContext.Session.Get<CartViewModel>("Cart") ?? new CartViewModel(); // Provide the key "Cart"
            var cartItem = cart.Items.FirstOrDefault(i => i.Product.RowKey == product.RowKey); // Use RowKey or adjust as needed

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
                        PartitionKey = product.PartitionKey, // Assuming you have this property
                        RowKey = product.RowKey, // Assuming you have this property
                        Name = product.Name,
                        Price = product.Price,
                        Description = product.Description,
                        CategoryName = product.CategoryId.ToString(), // Assuming CategoryId is used
                        Quantity = product.Quantity,
                        ImageUrl = product.ImageUrl
                    },
                    Quantity = quantity
                });
            }

            HttpContext.Session.Set("Cart", cart); // Save the cart back to session
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(int productId) // Changed to use productId
        {
            var cart = HttpContext.Session.Get<CartViewModel>("Cart") ?? new CartViewModel(); // Provide the key "Cart"
            var cartItem = cart.Items.FirstOrDefault(i => i.Product.RowKey == productId.ToString()); // Use RowKey or adjust as needed

            if (cartItem != null)
            {
                cart.Items.Remove(cartItem);
                HttpContext.Session.Set("Cart", cart); // Save the updated cart back to session
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Index");
        }

        // Updated Checkout Action
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.Get<CartViewModel>("Cart"); // Provide the key "Cart"

            if (cart != null && cart.Items.Any())
            {
                foreach (var item in cart.Items)
                {
                    var message = new InventoryUpdateMessage
                    {
                        PartitionKey = item.Product.PartitionKey, // Assuming you have this property
                        RowKey = item.Product.RowKey, // Assuming you have this property
                        Quantity = item.Quantity
                    };

                    // Send message to Azure Queue
                    await _queueService.SendMessageAsync(message);
                }

                // Clear the cart after sending messages
                HttpContext.Session.Remove("Cart");

                // Set the success message
                TempData["Message"] = "Item purchased successfully";
            }

            return RedirectToAction("Index");
        }
    }
}