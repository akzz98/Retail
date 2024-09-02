using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;
using Retail.Models.ViewModels;
using Retail.Services;
using System.Threading.Tasks;

namespace Retail.Controllers
{
    public class CartController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly QueueService _queueService; // Inject a queue service

        public CartController(TableStorageService tableStorageService, QueueService queueService)
        {
            _tableStorageService = tableStorageService;
            _queueService = queueService; // Initialize queue service
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = HttpContext.Session.Get<CartViewModel>("Cart") ?? new CartViewModel();
            cart.TotalPrice = cart.Items.Sum(i => i.Product.Price * i.Quantity);

            // Update the cart item count in ViewBag
            ViewBag.CartItemCount = cart.Items.Sum(i => i.Quantity);

            // Check for any TempData message
            ViewBag.Message = TempData["Message"]?.ToString();

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

            HttpContext.Session.Set("Cart", cart);
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
            var cart = HttpContext.Session.Get<CartViewModel>("Cart");

            if (cart != null && cart.Items.Any())
            {
                foreach (var item in cart.Items)
                {
                    var message = new InventoryUpdateMessage
                    {
                        PartitionKey = item.Product.PartitionKey,
                        RowKey = item.Product.RowKey,
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
