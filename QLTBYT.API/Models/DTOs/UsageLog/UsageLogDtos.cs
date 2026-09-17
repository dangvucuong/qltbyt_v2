namespace QLTBYT.API.Models.DTOs.UsageLog;

public class UsageLogDto
{
    public int Id { get; set; }
    public int? DeviceId { get; set; }
    public string DeviceCode { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public int? Hours { get; set; }
    public int? Sessions { get; set; }
    public DateTime InputDate { get; set; }
    public int? UserId { get; set; }
}

public class CreateUsageLogRequest
{
    public string DeviceCode { get; set; } = string.Empty;
    public int? Hours { get; set; }
    public int? Sessions { get; set; }
    public DateTime InputDate { get; set; }
}

public class UsageLogQueryParams
{
    public string? DeviceCode { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

