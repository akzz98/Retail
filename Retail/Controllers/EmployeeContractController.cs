using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Retail.Models;
using Retail.Services.Functions;

namespace Retail.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeeContractController : Controller
    {
        private readonly EmployeeContractFunctionService _employeeContractFunctionService;
        private readonly ILogger<EmployeeContractController> _logger;

        public EmployeeContractController(EmployeeContractFunctionService employeeContractFunctionService, ILogger<EmployeeContractController> logger)
        {
            _employeeContractFunctionService = employeeContractFunctionService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var contracts = await _employeeContractFunctionService.ListEmployeeContractsAsync();
            return View(contracts ?? new List<string>());
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(EmployeeContract model)
        {
            if (ModelState.IsValid)
            {
                await _employeeContractFunctionService.UploadEmployeeContractAsync(model);
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            try
            {
                var stream = await _employeeContractFunctionService.DownloadEmployeeContractAsync(fileName);
                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading file '{fileName}': {ex.Message}");
                return StatusCode(500, "Error downloading file.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            try
            {
                await _employeeContractFunctionService.DeleteEmployeeContractAsync(fileName);
                TempData["Message"] = "File deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting file: {ex.Message}");
                TempData["Error"] = $"Error deleting file: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}