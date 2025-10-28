using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Fei.Is.Api.Services.DeviceFirmwares;

public class DeviceFirmwareFileService : IDeviceFirmwareFileService
{
    private readonly string _basePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fei.Is", "firmwares");

    public async Task<string> SaveAsync(IFormFile file, string? existingFileName, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_basePath);

        string newFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        string newFilePath = Path.Combine(_basePath, newFileName);

        await using (var fileStream = new FileStream(newFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(fileStream, cancellationToken);
        }

        Delete(existingFileName);

        return newFileName;
    }

    public Task<Stream> OpenReadAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_basePath, storedFileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Firmware file not found: {storedFileName}");
        }

        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public void Delete(string? storedFileName)
    {
        if (string.IsNullOrWhiteSpace(storedFileName))
        {
            return;
        }

        var filePath = Path.Combine(_basePath, storedFileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
