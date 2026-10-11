namespace QLTBYT.API.Models.DTOs.Audit;

public class SystemAuditLogDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string? UserFullName { get; set; }
    public string? Content { get; set; }
    public DateTime? Timestamp { get; set; }
    public int? DeviceId { get; set; }
    public string? DeviceCode { get; set; }
    public string? DeviceName { get; set; }
}

public class SystemAuditQueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public int? UserId { get; set; }
    public int? DeviceId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
