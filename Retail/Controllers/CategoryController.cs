using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Retail.Entities;
using Retail.Models;
using Retail.Services;
using System.Text.Json;

[Authorize(Roles = "Admin")]
public class CategoryController : Controller
{
    private readonly CategorySqlService _categorySqlService;

    public CategoryController(CategorySqlService categorySqlService)
    {
        _categorySqlService = categorySqlService;
    }

    // GET: /Category
    public async Task<IActionResult> Index()
    {
        var categories = await _categorySqlService.GetAllCategoriesAsync();
        return View(categories);
    }


    // GET: /Category/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var category = await _categorySqlService.GetCategoryAsync(id);

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
    public async Task<IActionResult> Create(CategorySqlEntity category)
    {
        if (ModelState.IsValid)
        {
            await _categorySqlService.AddCategoryAsync(category);
            return RedirectToAction("Index");
        }

        return View(category);
    }


    // GET: /Category/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categorySqlService.GetCategoryAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }


    // POST: /Category/Edit
    [HttpPost]
    public async Task<IActionResult> Edit(CategorySqlEntity category)
    {
        if (ModelState.IsValid)
        {
            await _categorySqlService.UpdateCategoryAsync(category);
            return RedirectToAction("Index");
        }
        return View(category);
    }




    // GET: /Category/Delete/{id}
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categorySqlService.GetCategoryAsync(id);

        if (category != null)
        {
            await _categorySqlService.DeleteCategoryAsync(id);
        }

        return RedirectToAction("Index");
    }
}
