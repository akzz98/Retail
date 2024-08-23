using Microsoft.AspNetCore.Mvc;
using Retail.Models;
using Retail.Services;

namespace Retail.Controllers
{
    public class ProductController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly BlobStorageService _blobStorageService;

        public ProductController(TableStorageService tableStorageService, BlobStorageService blobStorageService)
        {
            _tableStorageService = tableStorageService;
            _blobStorageService = blobStorageService;
        }

        // GET: /Product and return view with list of products
        public async Task<IActionResult> Index()
        {
            var products = await _tableStorageService.GetAllProductsAsync();
            return View(products);
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
        public IActionResult Create()
        {
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
                    using var stream = imageFile.OpenReadStream();
                    imageUrl = await _blobStorageService.UploadImageAsync(stream, imageFile.FileName);
                }

                var productEntity = new ProductEntity
                {
                    PartitionKey = "Products",
                    RowKey = Guid.NewGuid().ToString(),
                    Name = product.Name,
                    Price = product.Price,
                    Description = product.Description,
                    ImageUrl = imageUrl,
                    Quantity = product.Quantity
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

            // Pass the product entity to the view for editing
            return View(product);
        }

        //Edit: /Product/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(ProductEntity product, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if a new image is uploaded
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        // Delete the old image if it exists
                        if (!string.IsNullOrEmpty(product.ImageUrl))
                        {
                            await _blobStorageService.DeleteImageAsync(product.ImageUrl);
                        }

                        // Upload the new image
                        using var stream = ImageFile.OpenReadStream();
                        product.ImageUrl = await _blobStorageService.UploadImageAsync(stream, ImageFile.FileName);
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
