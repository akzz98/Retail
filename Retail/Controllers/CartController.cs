using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Retail.Models;
using Retail.Services;

public class CartController : Controller
{
    private readonly TableStorageService _tableStorageService;

    public CartController(TableStorageService tableStorageService)
    {
        _tableStorageService = tableStorageService;
    }

    private List<ProductEntity> GetCart()
    {
        var cartJson = HttpContext.Session.GetString("Cart");
        if (cartJson == null)
        {
            return new List<ProductEntity>();
        }
        return JsonConvert.DeserializeObject<List<ProductEntity>>(cartJson);
    }

    private void SaveCart(List<ProductEntity> cart)
    {
        var cartJson = JsonConvert.SerializeObject(cart);
        HttpContext.Session.SetString("Cart", cartJson);
    }

    public async Task<IActionResult> Index()
    {
        var cart = GetCart();
        if (cart.Count == 0)
        {
            return View(new List<ProductEntity>());
        }

        // Retrieve product details for items in the cart
        var cartProducts = await Task.WhenAll(cart.Select(async p =>
            await _tableStorageService.GetProductAsync("Products", p.RowKey)));

        return View(cartProducts);
    }

    public async Task<IActionResult> AddToCart(int id, int quantity)
    {
        var product = await _tableStorageService.GetProductAsync("Products", id.ToString());

        if (product == null)
        {
            return NotFound();
        }

        var cart = GetCart();
        var cartItem = cart.FirstOrDefault(p => p.RowKey == id.ToString());

        if (cartItem != null)
        {
            cartItem.Quantity += quantity;
        }
        else
        {
            product.Quantity = quantity;
            cart.Add(product);
        }

        SaveCart(cart);

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> RemoveFromCart(int id)
    {
        var cart = GetCart();
        var product = cart.FirstOrDefault(p => p.RowKey == id.ToString());

        if (product != null)
        {
            cart.Remove(product);
            SaveCart(cart);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCart(int id, int quantity)
    {
        var cart = GetCart();
        var product = cart.FirstOrDefault(p => p.RowKey == id.ToString());

        if (product != null)
        {
            product.Quantity = quantity;
            SaveCart(cart);

            // Optionally update the product in Table Storage if needed
            await _tableStorageService.UpdateProductAsync(product);
        }

        return RedirectToAction("Index");
    }
}
