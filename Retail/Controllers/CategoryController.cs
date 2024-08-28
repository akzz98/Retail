using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Retail.Entities;
using Retail.Models;
using Retail.Services;


[Authorize(Roles = "Admin")]
public class CategoryController : Controller
{
    
    private readonly CategoryStorageService _categoryStorageService;

    public CategoryController(CategoryStorageService categoryStorageService)
    {
        _categoryStorageService = categoryStorageService;
    }

    // GET: /Category
    public async Task<IActionResult> Index()
    {
        var categories = await _categoryStorageService.GetAllCategoriesAsync();
        return View(categories);
    }

    // GET: /Category/Details/{partitionKey}/{rowKey}
    public async Task<IActionResult> Details(string partitionKey, string rowKey)
    {
        var category = await _categoryStorageService.GetCategoryAsync(partitionKey, rowKey);

        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    // GET: /Category/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Category/Create
    [HttpPost]
    public async Task<IActionResult> Create(Category category)
    {
        // Log the received category
        Console.WriteLine($"Received Category: Name={category.Name}");

        if (ModelState.IsValid)
        {
            var categoryEntity = new CategoryEntity
            {
                PartitionKey = "Categories",
                RowKey = Guid.NewGuid().ToString(), // Generate a unique RowKey
                Name = category.Name
            };

            // Log the category entity
            Console.WriteLine($"CategoryEntity: PartitionKey={categoryEntity.PartitionKey}, RowKey={categoryEntity.RowKey}, Name={categoryEntity.Name}");

            await _categoryStorageService.AddCategoryAsync(categoryEntity);
            return RedirectToAction("Index");
        }

        // Log ModelState errors
        foreach (var error in ModelState)
        {
            Console.WriteLine($"ModelState Error: Key={error.Key}, Value={string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
        }

        return View(category);
    }

    // Edit: /Category/Edit (GET)
    public async Task<IActionResult> Edit(string partitionKey, string rowKey)
    {
        var category = await _categoryStorageService.GetCategoryAsync(partitionKey, rowKey);

        if (category == null)
        {
            return NotFound();
        }

        // Pass the CategoryEntity directly to the view
        return View(category);
    }

    // POST: /Category/Edit
    [HttpPost]
    public async Task<IActionResult> Edit(CategoryEntity category)
    {
        if (ModelState.IsValid)
        {
            await _categoryStorageService.UpdateCategoryAsync(category);
            return RedirectToAction("Index");
        }

        return View(category);
    }

    // GET: /Category/Delete/
    public async Task<IActionResult> Delete(string partitionKey, string rowKey)
    {
        var category = await _categoryStorageService.GetCategoryAsync(partitionKey, rowKey);

        if (category != null)
        {
            await _categoryStorageService.DeleteCategoryAsync(partitionKey, rowKey);
        }

        return RedirectToAction("Index");
    }
}