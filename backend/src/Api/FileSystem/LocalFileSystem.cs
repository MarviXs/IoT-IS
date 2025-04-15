
using Fei.Is.Api.Data.Models.InformationSystem;

namespace Fei.Is.Api.FileSystem
{
    public class LocalFileSystem : IFileSystemService
    {
        private readonly string _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fei.Is");
        private readonly string _timeStampFormat = "yyyyMMdd_HHmmssfff";
        public string SaveFile(IFormFile file, UserFile userFile)
        {
            string newFileName = GetCurrentLocalFileName(file);
            string newFilePath = Path.Combine(_basePath, newFileName);

            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }

            using (Stream fileStream = new FileStream(newFilePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            if (!string.IsNullOrEmpty(userFile.LocalFileName))
            {
                string oldFilePath = Path.Combine(_basePath, userFile.LocalFileName);

                File.Delete(oldFilePath);
            }

            return newFileName;
        }

        public FileStream GetFile(UserFile userFile)
        {
            if (string.IsNullOrEmpty(userFile.LocalFileName))
            {
                throw new ArgumentException("Local file name is null or empty.");
            }

            string filePath = Path.Combine(_basePath, userFile.LocalFileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        private string GetCurrentLocalFileName(IFormFile file)
        {
            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
            string fileExtension = Path.GetExtension(file.FileName);

            string now = DateTime.Now.ToString(_timeStampFormat);

            return $"{fileName}.{now}{fileExtension}";
        }
    }
}
