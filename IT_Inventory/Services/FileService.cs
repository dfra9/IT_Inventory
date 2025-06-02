using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace IT_Inventory.Services
{
    public class FileService : IFileService
    {
        public string ProcessUploadedFiles(List<HttpPostedFileBase> files, string existingImagePaths, string uploadPath)
        {
            List<string> imagePaths = new List<string>();

            if (!string.IsNullOrEmpty(existingImagePaths))
            {
                imagePaths.AddRange(existingImagePaths.Split(';').Where(p => !string.IsNullOrWhiteSpace(p)));
            }

            if (files != null && files.Any(f => f != null && f.ContentLength > 0))
            {
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                foreach (var uploadedFile in files.Where(f => f != null && f.ContentLength > 0))
                {
                    string originalFileName = uploadedFile.FileName;
                    string filePath = Path.Combine(uploadPath, originalFileName);

                    uploadedFile.SaveAs(filePath);
                    imagePaths.Add(originalFileName);
                }
            }
            return string.Join(";", imagePaths);
        }
    }
}