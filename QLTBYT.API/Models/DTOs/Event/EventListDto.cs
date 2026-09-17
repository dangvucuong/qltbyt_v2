namespace QLTBYT.API.Models.DTOs.Event;

public class EventListDto
{
    public int Id { get; set; }
    public int? DeviceId { get; set; }
    public string? DeviceCode { get; set; }
    public string? DeviceName { get; set; }
    public int? TypeId { get; set; }
    public string? TypeName { get; set; }
    public string? Content { get; set; }
    public DateTime? EventDateTime { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public string? Keyword { get; set; }
    public double? Cost { get; set; }
    public string? AccountUsername { get; set; }
}

public class EventDetailDto : EventListDto
{
    public int? ManagerUserId { get; set; }
    public string? ManagerName { get; set; }
}
