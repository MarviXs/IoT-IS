using Fei.Is.Api.Data.Models.InformationSystem;

namespace Fei.Is.Api.FileSystem
{
    public interface IFileSystemService
    {
        public string SaveFile(IFormFile file, UserFile userFile);
        public FileStream GetFile(UserFile userFile);
    }
}
