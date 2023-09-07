namespace CRM_mvc.Services
{
    public interface UploadFile
    {
        Task<string> UploadFile(IFormFile file, string folderName);
    }
}
