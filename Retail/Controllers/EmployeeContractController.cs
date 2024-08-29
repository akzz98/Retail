using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Retail.Models;

namespace Retail.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeeContractController : Controller
    {
        private readonly FileStorageService _fileStorageService;
        private readonly ILogger<FileStorageService> _logger;

        public EmployeeContractController(FileStorageService fileStorageService, ILogger<FileStorageService> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var contracts = await _fileStorageService.ListFilesAsync("employeecontracts");
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
                using (var stream = new MemoryStream())
                {
                    await model.File.CopyToAsync(stream);
                    stream.Position = 0;

                    await _fileStorageService.UploadFileAsync("contracts", model.FileName, stream);
                }
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string employeeName, IFormFile contractFile)
        {
            if (contractFile != null && contractFile.Length > 0)
            {
                var fileName = $"{employeeName}-{contractFile.FileName}";
                await _fileStorageService.UploadFileAsync("employeecontracts", fileName, contractFile.OpenReadStream());
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Please provide a valid file.");
            return View();
        }

        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                _logger.LogError("File name parameter is null or empty.");
                return BadRequest("File name is required.");
            }

            try
            {
                var stream = await _fileStorageService.DownloadFileAsync("employeecontracts", fileName);
                if (stream == null)
                {
                    _logger.LogError($"File '{fileName}' could not be downloaded as the stream is null.");
                    return NotFound();
                }

                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while downloading file '{fileName}': {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error downloading file.");
            }
        }

        // GET: EmployeeContract/Edit
        [HttpGet]
        public IActionResult Edit(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            // Pass the file name to the view
            return View(model: fileName);
        }

        // POST: EmployeeContract/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(string originalFileName, IFormFile newFile)
        {
            if (newFile == null || newFile.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                return RedirectToAction("Edit", new { fileName = originalFileName });
            }

            try
            {
                // Delete the original file
                await _fileStorageService.DeleteFileAsync("employeecontracts", originalFileName);

                // Upload the new file with the original file name
                await _fileStorageService.UploadFileAsync("employeecontracts", originalFileName, newFile.OpenReadStream());

                TempData["Message"] = "File updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["Error"] = $"Error updating file: {ex.Message}";
                return RedirectToAction("Edit", new { fileName = originalFileName });
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
                await _fileStorageService.DeleteFileAsync("employeecontracts", fileName);
                TempData["Message"] = "File deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting file: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

    }
}
