namespace CRM_mvc.Services
{
    public class LocalFileUploadService : UploadFile
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public LocalFileUploadService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<string> UploadFile(IFormFile file, string folderName)
        {
            var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, @$"wwwroot\{folderName}");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, folderPath, file.FileName);
            using var fileStreem = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStreem);
            return filePath;
        }
    }
}
