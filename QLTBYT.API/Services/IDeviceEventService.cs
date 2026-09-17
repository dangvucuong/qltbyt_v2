using QLTBYT.API.Models.Common;
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
}
