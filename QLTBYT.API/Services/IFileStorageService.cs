namespace QLTBYT.API.Services;

public interface IFileStorageService
{
    Task<string> SaveDeviceFileAsync(int deviceId, Stream content, string fileName);
    Task<string> SaveLegalDocumentFileAsync(int legalDocumentId, Stream content, string fileName);
    Task<string> SaveCaptureAsync(int deviceId, Stream content, string fileName);
    string GetRootPath();
}