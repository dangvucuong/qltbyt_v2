namespace QLTBYT.API.Models.DTOs.Event;

public class EventQueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? StatusId { get; set; }
    public int? DeviceId { get; set; }
    public string? Search { get; set; }
}
