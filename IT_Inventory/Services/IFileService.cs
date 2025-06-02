using System.Collections.Generic;
using System.Web;

namespace IT_Inventory.Services
{
    public interface IFileService
    {
        string ProcessUploadedFiles(List<HttpPostedFileBase> files, string existingImagePaths, string uploadPath);
    }
}