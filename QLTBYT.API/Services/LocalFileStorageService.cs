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

    public async Task<string> SaveMaintenanceContractAsync(int deviceId, Stream content, string fileName)
    {
        var hdRoot = Path.Combine(Path.GetDirectoryName(_root)!, "HDBaotri");
        Directory.CreateDirectory(hdRoot);
        var safeName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeName))
            throw new InvalidOperationException("Tên file không hợp lệ.");

        var folder = Path.Combine(hdRoot, deviceId.ToString());
        Directory.CreateDirectory(folder);

        // Xóa file hợp đồng cũ trong thư mục thiết bị (giống legacy)
        foreach (var old in Directory.GetFiles(folder))
        {
            try { File.Delete(old); } catch { /* ignore */ }
        }

        var fullPath = Path.Combine(folder, safeName);
        await using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs);
        return Path.Combine("HDBaotri", deviceId.ToString(), safeName).Replace('\\', '/');
    }

    public async Task<string> SaveIncidentFileAsync(int deviceId, string categoryFolder, Stream content, string fileName)
    {
        var folderName = categoryFolder switch
        {
            "Hinh_anh_su_co" => "Hinh_anh_su_co",
            "BAO_CAO_SU_CO" => "BAO_CAO_SU_CO",
            _ => throw new InvalidOperationException("Loại file sự cố không hợp lệ.")
        };

        var safeName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeName))
            throw new InvalidOperationException("Tên file không hợp lệ.");

        var folder = Path.Combine(_root, deviceId.ToString(), folderName);
        Directory.CreateDirectory(folder);
        var fullPath = Path.Combine(folder, safeName);
        await using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs);
        return Path.Combine("FILELUU", deviceId.ToString(), folderName, safeName).Replace('\\', '/');
    }
}
