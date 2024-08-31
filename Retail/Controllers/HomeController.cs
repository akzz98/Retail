using Microsoft.AspNetCore.Mvc;
using Retail.Models.ViewModels;
using Retail.Services;

namespace Retail.Controllers
{
    public class HomeController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly CategoryStorageService _categoryStorageService;

        public HomeController(TableStorageService tableStorageService, CategoryStorageService categoryStorageService)
        {
            _tableStorageService = tableStorageService;
            _categoryStorageService = categoryStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _tableStorageService.GetAllProductsAsync();
            var categories = await _categoryStorageService.GetAllCategoriesAsync();

            var productViewModels = products.Select(product => new ProductViewModel
            {
                PartitionKey = product.PartitionKey,
                RowKey = product.RowKey,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                CategoryName = categories.FirstOrDefault(category => category.RowKey == product.CategoryRowKey)?.Name,
                Quantity = product.Quantity,
                ImageUrl = product.ImageUrl
            });
            return View(productViewModels);
        }

        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            if (partitionKey == null || rowKey == null)
            {
                return NotFound();
            }

            var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
            var categories = await _categoryStorageService.GetAllCategoriesAsync();

            if (product == null)
            {
                return NotFound();
            }

            var model = new ProductViewModel
            {
                PartitionKey = product.PartitionKey,
                RowKey = product.RowKey,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                CategoryName = categories.FirstOrDefault(category => category.RowKey == product.CategoryRowKey)?.Name,
                Quantity = product.Quantity,
                ImageUrl = product.ImageUrl
            };

            return View(model);
        }



    }
}