﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Retail.Entities;
using Retail.Models;
using Retail.Services;
using System.Security.Claims;

namespace Retail.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserStorageService _userService;

        public AccountController(UserStorageService userService)
        {
            _userService = userService;
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userService.GetUserByUsernameAsync(model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Username is already taken.");
                    return View(model);
                }

                var user = new UserEntity
                {
                    PartitionKey = "Users",
                    RowKey = Guid.NewGuid().ToString(),
                    Username = model.Username,
                    PasswordHash = HashPassword(model.Password),
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Role = "Admin"
                };

                await _userService.AddUserAsync(user);
                await SignInUser(user);
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.GetUserByUsernameAsync(model.Username);
                if (user != null && VerifyPassword(user.PasswordHash, model.Password))
                {
                    await SignInUser(user);
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }




        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task SignInUser(UserEntity user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.RowKey),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                // Keeps the user signed in across sessions
                IsPersistent = true 
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
        }


        //Hash the password
        private string HashPassword(string password)
        {
            var passwordHasher = new PasswordHasher<UserEntity>();
            return passwordHasher.HashPassword(null, password);
        }

        private bool VerifyPassword(string hashedPassword, string enteredPassword)
        {
            var passwordHasher = new PasswordHasher<UserEntity>();
            var result = passwordHasher.VerifyHashedPassword(null, hashedPassword, enteredPassword);
            return result == PasswordVerificationResult.Success;
        }

        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var username = User.Identity.Name;
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }

            var model = new User
            {
                UserName = user.Username,
                Email = user.Email,
                Name = user.FirstName,
                Surname = user.LastName,
                PhoneNumber = user.PhoneNumber
                // Populate other fields as necessary
            };

            return View(model);
        }
    }
}
