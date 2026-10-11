using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Models.DTOs.Event;

namespace QLTBYT.API.Services;

public interface IDeviceEventService
{
    Task<PagedResult<EventListDto>> GetEventsAsync(EventQueryParams query);
    Task<EventDetailDto?> GetEventByIdAsync(int id);
    Task<EventDetailDto> CreateForDeviceAsync(int deviceId, CreateDeviceEventRequest request);
    Task<EventDetailDto?> ResolveAsync(int id, ResolveEventRequest request);
    Task<List<EventKeywordDto>> GetKeywordsAsync();
    Task<EventKeywordDto> AddKeywordAsync(CreateEventKeywordRequest request);
    Task<DeviceFileDto> AddIncidentFileForEventAsync(int eventId, Stream content, string fileName, string kind);
    Task<List<DeviceFileDto>> GetIncidentFilesForEventAsync(int eventId);
    Task<DeviceFileDto> AddIncidentFileForDeviceAsync(int deviceId, Stream content, string fileName, string kind);
    Task<List<DeviceFileDto>> GetIncidentFilesForDeviceAsync(int deviceId, string? kind);
}
