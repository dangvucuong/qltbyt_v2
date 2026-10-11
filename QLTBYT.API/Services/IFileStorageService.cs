namespace QLTBYT.API.Services;

public interface IFileStorageService
{
    Task<string> SaveDeviceFileAsync(int deviceId, Stream content, string fileName);
    Task<string> SaveLegalDocumentFileAsync(int legalDocumentId, Stream content, string fileName);
    Task<string> SaveCaptureAsync(int deviceId, Stream content, string fileName);
    Task<string> SaveMaintenanceContractAsync(int deviceId, Stream content, string fileName);
    /// <summary>
    /// Lưu file sự cố theo thư mục legacy: Hinh_anh_su_co | BAO_CAO_SU_CO.
    /// </summary>
    Task<string> SaveIncidentFileAsync(int deviceId, string categoryFolder, Stream content, string fileName);
    string GetRootPath();
}