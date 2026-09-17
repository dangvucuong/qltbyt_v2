using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.DTOs.Device;

namespace QLTBYT.API.Services;

public interface IPdfSignatureService
{
    Task<DeviceFileDto?> SignDeviceFileAsync(int fileId, string signerName);
}

public class PdfSignatureService : IPdfSignatureService
{
    private readonly AppDbContext _context;
    private readonly IFileStorageService _files;

    public PdfSignatureService(AppDbContext context, IFileStorageService files, ICurrentUserService currentUser)
    {
        _context = context;
        _files = files;
    }

    public async Task<DeviceFileDto?> SignDeviceFileAsync(int fileId, string signerName)
    {
        var file = await _context.DeviceFiles.FirstOrDefaultAsync(f => f.Id == fileId);
        if (file is null || string.IsNullOrWhiteSpace(file.Path)) return null;

        var sourcePath = ResolvePhysicalPath(file.Path);
        if (!System.IO.File.Exists(sourcePath)) return null;

        var ext = System.IO.Path.GetExtension(sourcePath).ToLowerInvariant();
        if (ext != ".pdf") throw new InvalidOperationException("Chi ho tro ky so file PDF.");

        var signedName = System.IO.Path.GetFileNameWithoutExtension(sourcePath) + "_kyso.pdf";
        var signedPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(sourcePath)!, signedName);
        var stampText = $"Da ky so: {signerName} - {DateTime.Now:dd/MM/yyyy HH:mm}";

        using (var reader = new PdfReader(sourcePath))
        using (var writer = new PdfWriter(signedPath))
        using (var pdf = new PdfDocument(reader, writer))
        {
            var pages = pdf.GetNumberOfPages();
            var page = pdf.GetPage(pages);
            var pageSize = page.GetPageSize();
            var font = PdfFontFactory.CreateFont();
            var canvas = new PdfCanvas(page);
            canvas.BeginText()
                .SetFontAndSize(font, 10)
                .MoveText(40, 40)
                .ShowText(stampText)
                .EndText();
            pdf.Close();
        }

        var normalized = file.Path!.Replace('\\', '/');
        var slash = normalized.LastIndexOf('/');
        var newRelative = slash >= 0
            ? normalized.Substring(0, slash + 1) + signedName
            : System.IO.Path.Combine("FILELUU", file.DeviceId?.ToString() ?? "0", signedName).Replace('\\', '/');

        file.Path = newRelative;
        file.Description = (file.Description ?? "") + " [Ky so]";
        file.FileDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new DeviceFileDto { Id = file.Id, Path = file.Path, Description = file.Description, FileDate = file.FileDate };
    }

    private string ResolvePhysicalPath(string relativePath)
    {
        var root = _files.GetRootPath();
        var contentRoot = System.IO.Path.GetDirectoryName(root)!;
        var normalized = relativePath.Replace('\\', '/');
        if (normalized.StartsWith("FILELUU/", StringComparison.OrdinalIgnoreCase))
            normalized = normalized.Substring("FILELUU/".Length);
        if (normalized.StartsWith("Captures/", StringComparison.OrdinalIgnoreCase))
            return System.IO.Path.Combine(contentRoot, normalized.Replace('/', System.IO.Path.DirectorySeparatorChar));
        return System.IO.Path.Combine(root, normalized.Replace('/', System.IO.Path.DirectorySeparatorChar));
    }
}