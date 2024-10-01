using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Retail.Entities;
using Retail.Models;
using System.Text.Json;

[Authorize(Roles = "Admin")]
public class CategoryController : Controller
{
    private readonly CategoryFunctionService _categoryFunctionService;

    public CategoryController(CategoryFunctionService categoryFunctionService)
    {
        _categoryFunctionService = categoryFunctionService;
    }

    // GET: /Category
    public async Task<IActionResult> Index()
    {
        var categories = await _categoryFunctionService.GetAllCategoriesAsync();
        return View(categories);
    }

    // GET: /Category/Details/{partitionKey}/{rowKey}
    public async Task<IActionResult> Details(string partitionKey, string rowKey)
    {
        var category = await _categoryFunctionService.GetCategoryAsync(partitionKey, rowKey);

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
        if (ModelState.IsValid)
        {
            var categoryEntity = new CategoryEntity
            {
                PartitionKey = "Categories",
                RowKey = Guid.NewGuid().ToString(), // Auto-generate RowKey
                Name = category.Name
            };

            await _categoryFunctionService.CreateCategoryAsync(categoryEntity);
            return RedirectToAction("Index");
        }

        return View(category);
    }

    // GET: /Category/Edit/{partitionKey}/{rowKey}
    [HttpGet]
    public async Task<IActionResult> Edit(string partitionKey, string rowKey)
    {
        var category = await _categoryFunctionService.GetCategoryAsync(partitionKey, rowKey);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    // POST: /Category/Edit
    [HttpPost]
    public async Task<IActionResult> Edit(CategoryEntity category)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _categoryFunctionService.UpdateCategoryAsync(category);
                return RedirectToAction("Index");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Failed to update category: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Failed to update category. Please try again.");
            }
        }
        return View(category);
    }




    // GET: /Category/Delete/
    public async Task<IActionResult> Delete(string partitionKey, string rowKey)
    {
        var category = await _categoryFunctionService.GetCategoryAsync(partitionKey, rowKey);

        if (category != null)
        {
            await _categoryFunctionService.DeleteCategoryAsync(partitionKey, rowKey);
        }

        return RedirectToAction("Index");
    }
}
