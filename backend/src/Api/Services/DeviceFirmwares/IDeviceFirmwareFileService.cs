using System.IO;
using Microsoft.AspNetCore.Http;

namespace Fei.Is.Api.Services.DeviceFirmwares;

public interface IDeviceFirmwareFileService
{
    Task<string> SaveAsync(IFormFile file, string? existingFileName, CancellationToken cancellationToken = default);
    Task SaveStreamAsAsync(Stream stream, string storedFileName, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string storedFileName, CancellationToken cancellationToken = default);
    void Delete(string? storedFileName);
}
