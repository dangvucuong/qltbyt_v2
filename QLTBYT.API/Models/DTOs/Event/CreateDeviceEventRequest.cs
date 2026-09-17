namespace QLTBYT.API.Models.DTOs.Event;

public class CreateDeviceEventRequest
{
    public int EventTypeId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Keyword { get; set; }
}
