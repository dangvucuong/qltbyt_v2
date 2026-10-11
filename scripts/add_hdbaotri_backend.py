# -*- coding: utf-8 -*-
from pathlib import Path

API = Path(r"d:\DVC\PROJECT\QLTBYT_NEW\QLTBYT.API")

# 1) DeviceDetailDto - add LegalDocumentRef
detail_path = API / "Models/DTOs/Device/DeviceDetailDto.cs"
detail = detail_path.read_text(encoding="utf-8")
if "LegalDocumentRef" not in detail:
    detail = detail.replace(
        "    public string? SpecializedName { get; set; }\n\n    public List<DeviceFileDto> Files",
        "    public string? SpecializedName { get; set; }\n    public string? LegalDocumentRef { get; set; }\n\n    public List<DeviceFileDto> Files",
    )
    detail_path.write_text(detail, encoding="utf-8", newline="\n")
    print("DeviceDetailDto + LegalDocumentRef")

# 2) IFileStorageService
ifs = API / "Services/IFileStorageService.cs"
t = ifs.read_text(encoding="utf-8")
if "SaveMaintenanceContractAsync" not in t:
    t = t.replace(
        "    Task<string> SaveCaptureAsync(int deviceId, Stream content, string fileName);\n    string GetRootPath();",
        "    Task<string> SaveCaptureAsync(int deviceId, Stream content, string fileName);\n    Task<string> SaveMaintenanceContractAsync(int deviceId, Stream content, string fileName);\n    string GetRootPath();",
    )
    ifs.write_text(t, encoding="utf-8", newline="\n")
    print("IFileStorageService updated")

# 3) LocalFileStorageService
lfs = API / "Services/LocalFileStorageService.cs"
t = lfs.read_text(encoding="utf-8")
if "SaveMaintenanceContractAsync" not in t:
    method = '''
    public async Task<string> SaveMaintenanceContractAsync(int deviceId, Stream content, string fileName)
    {
        var hdRoot = Path.Combine(Path.GetDirectoryName(_root)!, "HDBaotri");
        Directory.CreateDirectory(hdRoot);
        var safeName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeName))
            throw new InvalidOperationException("Tên file không hợp lệ.");
        var folder = Path.Combine(hdRoot, deviceId.ToString());
        Directory.CreateDirectory(folder);
        var fullPath = Path.Combine(folder, safeName);
        await using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs);
        return Path.Combine("HDBaotri", deviceId.ToString(), safeName).Replace('\\\\', '/');
    }
'''
    # fix escape - in real file use single backslash in Replace
    method = method.replace("Replace('\\\\', '/')", "Replace('\\\\', '/')")
    method = '''
    public async Task<string> SaveMaintenanceContractAsync(int deviceId, Stream content, string fileName)
    {
        var hdRoot = Path.Combine(Path.GetDirectoryName(_root)!, "HDBaotri");
        Directory.CreateDirectory(hdRoot);
        var safeName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeName))
            throw new InvalidOperationException("Tên file không hợp lệ.");
        var folder = Path.Combine(hdRoot, deviceId.ToString());
        Directory.CreateDirectory(folder);
        var fullPath = Path.Combine(folder, safeName);
        await using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs);
        return Path.Combine("HDBaotri", deviceId.ToString(), safeName).Replace('\\', '/');
    }
'''
    t = t.rstrip() + "\n" + method + "}\n"
    # remove extra closing if duplicated
    # Original ends with }\n for class - we appended method before last }
    # Better: insert before last closing brace of class
    t = lfs.read_text(encoding="utf-8")
    insert = '''
    public async Task<string> SaveMaintenanceContractAsync(int deviceId, Stream content, string fileName)
    {
        var hdRoot = Path.Combine(Path.GetDirectoryName(_root)!, "HDBaotri");
        Directory.CreateDirectory(hdRoot);
        var safeName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeName))
            throw new InvalidOperationException("Tên file không hợp lệ.");
        var folder = Path.Combine(hdRoot, deviceId.ToString());
        Directory.CreateDirectory(folder);

        // Xóa file hợp đồng cũ (nếu còn trong thư mục và khác tên)
        foreach (var old in Directory.GetFiles(folder))
        {
            try { File.Delete(old); } catch { /* ignore */ }
        }

        var fullPath = Path.Combine(folder, safeName);
        await using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs);
        return Path.Combine("HDBaotri", deviceId.ToString(), safeName).Replace('\\', '/');
    }
'''
    # insert before final }
    idx = t.rfind('}')
    t = t[:idx] + insert + "\n}\n"
    lfs.write_text(t, encoding="utf-8", newline="\n")
    print("LocalFileStorageService updated")

# 4) IDeviceService
ids = API / "Services/IDeviceService.cs"
t = ids.read_text(encoding="utf-8")
if "UploadMaintenanceContractAsync" not in t:
    t = t.replace(
        "    Task<DeviceFileDto> AddFileWithPathAsync(int deviceId, string relativePath, string? description);",
        "    Task<DeviceFileDto> AddFileWithPathAsync(int deviceId, string relativePath, string? description);\n    Task<string> UploadMaintenanceContractAsync(int deviceId, Stream content, string fileName);",
    )
    ids.write_text(t, encoding="utf-8", newline="\n")
    print("IDeviceService updated")

# 5) DeviceService method - append before last closing of class in DeviceService.cs or Phase3
# Put in DeviceService.cs after AddFileWithPathAsync
ds = API / "Services/DeviceService.cs"
t = ds.read_text(encoding="utf-8")
if "UploadMaintenanceContractAsync" not in t:
    method = '''
    public async Task<string> UploadMaintenanceContractAsync(int deviceId, Stream content, string fileName)
    {
        var queryable = _context.Devices.Where(d => d.Id == deviceId);
        queryable = ApplyOrganizationScope(queryable);
        var entity = await queryable.FirstOrDefaultAsync();
        if (entity is null)
            throw new InvalidOperationException("Không tìm thấy thiết bị.");

        var relativePath = await _fileStorage.SaveMaintenanceContractAsync(deviceId, content, fileName);
        var storedName = Path.GetFileName(relativePath);
        entity.LegalDocumentRef = storedName;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return relativePath;
    }
'''
    # Find AddFileWithPathAsync end and insert after
    marker = "return _mapper.Map<DeviceFileDto>(file);"
    # there may be two - AddFileAsync and AddFileWithPathAsync
    pos = t.find("public async Task<DeviceFileDto> AddFileWithPathAsync")
    if pos < 0:
        raise SystemExit("AddFileWithPathAsync not found")
    # find closing of that method - next public or end
    rest = t[pos:]
    # insert before TransferDevice or next public after AddFileWithPath
    insert_at = t.find("\n    public async Task<DeviceDetailDto?> TransferDeviceAsync")
    if insert_at < 0:
        insert_at = t.find("\n    public async Task<DeviceDetailDto?> UpdateQrAsync")
    if insert_at < 0:
        # DeviceService.cs may not have Transfer - it's in Phase3
        # insert before ApplyOrganizationScope private
        insert_at = t.find("\n    private IQueryable<Device> ApplyOrganizationScope")
    if insert_at < 0:
        raise SystemExit("insert point not found")
    t = t[:insert_at] + "\n" + method + t[insert_at:]
    ds.write_text(t, encoding="utf-8", newline="\n")
    print("DeviceService UploadMaintenanceContractAsync added")

print("backend core ok")
