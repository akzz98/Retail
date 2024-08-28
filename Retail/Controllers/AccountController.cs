using Microsoft.AspNetCore.Mvc;
using Retail.Entities;
using Retail.Models;
using Retail.Services;
using System.Security.Cryptography;
using System.Text;

namespace Retail.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserStorageService _userService;

        public AccountController(UserStorageService userService)
        {
            _userService = userService;
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Hash the password
                var passwordHash = ComputeHash(model.Password);

                var user = new UserEntity
                {
                    PartitionKey = "Users",
                    RowKey = Guid.NewGuid().ToString(),
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = passwordHash,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Role = "User"
                };

                await _userService.AddUserAsync(user);
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                // Hash the password
                var passwordHash = ComputeHash(model.Password);

                var user = await _userService.ValidateUserAsync(model.Username, passwordHash);

                if (user != null)
                {
                    // Store the user information in session or cookie as needed
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        private string ComputeHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
