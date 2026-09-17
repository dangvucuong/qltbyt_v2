using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Device;

namespace QLTBYT.API.Services;

public interface IDeviceService
{
    Task<PagedResult<DeviceListDto>> GetDevicesAsync(DeviceQueryParams query);
    Task<DeviceDetailDto?> GetDeviceByIdAsync(int id);
    Task<DeviceDetailDto> CreateDeviceAsync(CreateDeviceRequest request);
    Task<DeviceDetailDto?> UpdateDeviceAsync(int id, UpdateDeviceRequest request);
    Task<bool> DeleteDeviceAsync(int id);
    Task<List<DeviceListDto>> GetChildrenAsync(int parentId);
    Task<DeviceFileDto> AddFileAsync(int deviceId, Stream content, string fileName, string? description);
    Task<DeviceFileDto> AddFileWithPathAsync(int deviceId, string relativePath, string? description);
    Task<DeviceDetailDto?> TransferDeviceAsync(int id, TransferDeviceRequest request);
    Task<List<DeviceHistoryItemDto>> GetDeviceHistoryAsync(int id);
    Task<DeviceDetailDto?> UpdateQrAsync(int id, UpdateDeviceQrRequest request);
    Task<List<DeviceSupplyDto>> GetSuppliesAsync(int deviceId);
    Task<DeviceSupplyDto> AddSupplyAsync(int deviceId, CreateDeviceSupplyRequest request);
    Task<bool> DeleteSupplyAsync(int deviceId, int supplyId);
    Task<int> ImportDevicesFromExcelAsync(Stream stream);
}
