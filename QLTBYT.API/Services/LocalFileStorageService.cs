using Microsoft.AspNetCore.Hosting;

namespace QLTBYT.API.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _root;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _root = Path.Combine(env.ContentRootPath, "FILELUU");
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveCaptureAsync(int deviceId, Stream content, string fileName)
    {
        var capturesRoot = Path.Combine(Path.GetDirectoryName(_root)!, "Captures");
        Directory.CreateDirectory(capturesRoot);
        var safeName = Path.GetFileName(fileName);
        var folder = Path.Combine(capturesRoot, deviceId.ToString());
        Directory.CreateDirectory(folder);
        var fullPath = Path.Combine(folder, safeName);
        await using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs);
        return Path.Combine("Captures", deviceId.ToString(), safeName).Replace('\\', '/');
    }

    public string GetRootPath() => _root;

    public async Task<string> SaveDeviceFileAsync(int deviceId, Stream content, string fileName)
    {
        var safeName = Path.GetFileName(fileName);
        var folder = Path.Combine(_root, deviceId.ToString());
        Directory.CreateDirectory(folder);
        var fullPath = Path.Combine(folder, safeName);
        await using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs);
        return Path.Combine("FILELUU", deviceId.ToString(), safeName).Replace('\\', '/');
    }

    public async Task<string> SaveLegalDocumentFileAsync(int legalDocumentId, Stream content, string fileName)
    {
        var safeName = Path.GetFileName(fileName);
        var folder = Path.Combine(_root, "legal", legalDocumentId.ToString());
        Directory.CreateDirectory(folder);
        var fullPath = Path.Combine(folder, safeName);
        await using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs);
        return Path.Combine("FILELUU", "legal", legalDocumentId.ToString(), safeName).Replace('\\', '/');
    }
}