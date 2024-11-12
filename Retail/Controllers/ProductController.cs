using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Retail.Models;
using Retail.Models.ViewModels;
using Retail.Services;
using System.Net.Http.Headers;
using System.Linq;
using Retail.Entities; // Ensure this is included for LINQ operations

namespace Retail.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly BlobStorageService _blobStorageService; // Keep existing Blob Storage service
        private readonly CategorySqlService _categorySqlService; // Use the new SQL service for categories
        private readonly ProductSqlService _productSqlService; // Use the new SQL service for products
        private readonly ILogger<ProductController> _logger;

        public ProductController(BlobStorageService blobStorageService, CategorySqlService categorySqlService, ProductSqlService productSqlService, ILogger<ProductController> logger)
        {
            _blobStorageService = blobStorageService; // Existing service
            _categorySqlService = categorySqlService; // New SQL service for categories
            _productSqlService = productSqlService; // New SQL service for products
            _logger = logger;
        }

        // GET: /Product and return view with list of products
        public async Task<IActionResult> Index()
        {
            // Retrieve all products from the SQL database
            var products = await _productSqlService.GetAllProductsAsync();

            // Retrieve all categories from the SQL database
            var categories = await _categorySqlService.GetAllCategoriesAsync();

            // Create a list of ProductViewModel to pass to the view
            var productViewModels = products.Select(product => new ProductViewModel
            {
                Id = product.Id, // Assuming you have an Id property in ProductViewModel
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                Quantity = product.Quantity,
                ImageUrl = product.ImageUrl,
                CategoryName = categories.FirstOrDefault(category => category.Id == product.CategoryId)?.Name // Match by CategoryId
            });

            return View(productViewModels);
        }

        // View Product Details
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productSqlService.GetProductAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: /Product/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _categorySqlService.GetAllCategoriesAsync(), "Id", "Name");
            return View();
        }

        // POST: /Product/Create
        [HttpPost]
        public async Task<IActionResult> Create(ProductEntity product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                string imageUrl = null;
                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        using var client = new HttpClient
                        {
                            Timeout = TimeSpan.FromMinutes(5) // Set timeout to 5 minutes
                        };
                        var content = new MultipartFormDataContent();
                        var fileContent = new StreamContent(imageFile.OpenReadStream());
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(imageFile.ContentType);
                        content.Add(fileContent, "image", imageFile.FileName);

                        // Log the start of the request
                        _logger.LogInformation("Starting image upload...");

                        var response = await client.PostAsync("http://localhost:7199/api/UploadImage", content);

                        // Log the end of the request
                        _logger.LogInformation("Image upload completed.");

                        if (response.IsSuccessStatusCode)
                        {
                            var responseData = await response.Content.ReadFromJsonAsync<dynamic>();
                            imageUrl = responseData.imageUrl;
                        }
                    }
                    catch (TaskCanceledException ex)
                    {
                        _logger.LogError("Image upload task was canceled: {Message}", ex.Message);
                        ModelState.AddModelError(string.Empty, "Image upload took too long and was canceled.");
                        return View(product);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("An error occurred during image upload: {Message}", ex.Message);
                        ModelState.AddModelError(string.Empty, "An error occurred during image upload.");
                        return View(product);
                    }
                }

                product.ImageUrl = imageUrl; // Set the image URL in the product entity

                await _productSqlService.AddProductAsync(product);
                return RedirectToAction("Index");
            }

            ViewBag.Categories = new SelectList(await _categorySqlService.GetAllCategoriesAsync(), "Id", "Name");
            return View(product);
        }

        // GET: /Product/Edit
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productSqlService.GetProductAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(await _categorySqlService.GetAllCategoriesAsync(), "Id", "Name");
            return View(product);
        }

        // POST: /Product/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(ProductEntity product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if a new image is uploaded
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete the old image if it exists
                        if (!string.IsNullOrEmpty(product.ImageUrl))
                        {
                            await _blobStorageService.DeleteImageAsync(product.ImageUrl);
                        }

                        // Upload the new image
                        using var stream = imageFile.OpenReadStream();
                        product.ImageUrl = await _blobStorageService.UploadImageAsync(stream, imageFile.FileName);
                    }

                    // Update the product in SQL
                    await _productSqlService.UpdateProductAsync(product);

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log the exception and handle the error
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the product.");
                }
            }
            ViewBag.Categories = new SelectList(await _categorySqlService.GetAllCategoriesAsync(), "Id", "Name");
            return View(product);
        }

        // DELETE: /Product/Delete
        public async Task<IActionResult> Delete(int id)
        {
            // Retrieve the product to get the ImageUrl
            var product = await _productSqlService.GetProductAsync(id);

            if (product != null)
            {
                // Delete the image from Blob Storage
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    await _blobStorageService.DeleteImageAsync(product.ImageUrl);
                }

                // Delete the product from SQL
                await _productSqlService.DeleteProductAsync(product.Id); // Assuming Id is the primary key
            }

            return RedirectToAction("Index");
        }
    }
}