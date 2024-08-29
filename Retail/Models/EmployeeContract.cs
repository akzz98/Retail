namespace Retail.Models
{
    public class EmployeeContract
    {
        public string EmployeeName { get; set; }
        public string FileName { get; set; }
        public IFormFile File { get; set; }
        public DateTime UploadedDate { get; set; }
    }
}
