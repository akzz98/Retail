using Retail.Models;

namespace Retail.Services.Functions
{
    public class EmployeeContractFunctionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _functionBaseUrl;

        public EmployeeContractFunctionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _functionBaseUrl = configuration["AzureFunctionSettings:BaseUrl1"];
        }

        public async Task<List<string>> ListEmployeeContractsAsync()
        {
            var response = await _httpClient.GetAsync($"{_functionBaseUrl}/contracts");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<string>>();
        }

        public async Task UploadEmployeeContractAsync(EmployeeContract employeeContract)
        {
            using (var formContent = new MultipartFormDataContent())
            {
                formContent.Add(new StringContent(employeeContract.EmployeeName), "EmployeeName");
                formContent.Add(new StreamContent(employeeContract.File.OpenReadStream()), "file", employeeContract.FileName);

                var response = await _httpClient.PostAsync($"{_functionBaseUrl}/contracts/upload", formContent);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task DeleteEmployeeContractAsync(string fileName)
        {
            var response = await _httpClient.DeleteAsync($"{_functionBaseUrl}/contracts/{fileName}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<Stream> DownloadEmployeeContractAsync(string fileName)
        {
            var response = await _httpClient.GetAsync($"{_functionBaseUrl}/contracts/download/{fileName}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
    }
}

