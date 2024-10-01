using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Retail.Models;
using Retail.Models.ViewModels;
using Retail.Services;
using System.Net.Http.Headers;

namespace Retail.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly BlobStorageService _blobStorageService;
        private readonly CategoryStorageService _categoryStorageService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(TableStorageService tableStorageService, BlobStorageService blobStorageService, CategoryStorageService categoryStorageService, ILogger<ProductController> logger) // Add logger to constructor
        {
            _tableStorageService = tableStorageService;
            _blobStorageService = blobStorageService;
            _categoryStorageService = categoryStorageService;
            _logger = logger;
        }

        // GET: /Product and return view with list of products
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
                Quantity = product.Quantity,
                ImageUrl = product.ImageUrl,
                CategoryName = categories.FirstOrDefault(category => category.RowKey == product.CategoryRowKey)?.Name
            });

            return View(productViewModels);
        }

        //View Product Details
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: /Product/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _categoryStorageService.GetAllCategoriesAsync(), "RowKey", "Name");
            return View();
        }

        // POST: /Product/Create
        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
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

                var productEntity = new ProductEntity
                {
                    PartitionKey = "Products",
                    RowKey = Guid.NewGuid().ToString(),
                    Name = product.Name,
                    Price = product.Price,
                    Description = product.Description,
                    Quantity = product.Quantity,
                    CategoryRowKey = product.CategoryRowKey,
                    ImageUrl = imageUrl
                };

                await _tableStorageService.AddProductAsync(productEntity);
                return RedirectToAction("Index");
            }

            return View(product);
        }


        //Get: /Product/Edit
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(await _categoryStorageService.GetAllCategoriesAsync(), "RowKey", "Name");
            return View(product);
        }

        //Post: /Product/Edit
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

                    // Update the product in Table Storage
                    await _tableStorageService.UpdateProductAsync(product);

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log the exception and handle the error
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the product.");
                }
            }
            return View(product);
        }

        //Delete: /Product/Delete
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            // Retrieve the product to get the ImageUrl
            var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);

            if (product != null)
            {
                // Delete the image from Blob Storage
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    await _blobStorageService.DeleteImageAsync(product.ImageUrl);
                }

                // Delete the product from Table Storage
                await _tableStorageService.DeleteProductAsync(partitionKey, rowKey);
            }

            return RedirectToAction("Index");
        }
    }
}