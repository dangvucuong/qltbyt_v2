namespace QLTBYT.API.Models.DTOs.Report;

public class BaocaoAssetQueryParams
{
    public string Section { get; set; } = "procurement";
    public string? Value { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class BaocaoOperationalQueryParams
{
    public string Section { get; set; } = "status";
    public string? Value { get; set; }
    public int DaysUntilInspection { get; set; } = 30;
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public double? MinAmount { get; set; }
    public double? MaxAmount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class BaocaoAssetRowDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? IdentityCode { get; set; }
    public string? Manufacturer { get; set; }
    public string? ModelCode { get; set; }
    public string? Serial { get; set; }
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? OrganizationName { get; set; }
    public string? FilterLabel { get; set; }
    public double? TotalAmount { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public double? DepreciationAmount { get; set; }
}

public class BaocaoOperationalRowDto
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? IdentityCode { get; set; }
    public string? Manufacturer { get; set; }
    public string? ModelCode { get; set; }
    public string? Serial { get; set; }
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? OrganizationName { get; set; }
    public string? FilterLabel { get; set; }
    public DateTime? InspectionExpiryDate { get; set; }
    public DateTime? BreakdownStartedAt { get; set; }
    public string? RepairedAt { get; set; }
    public string? DowntimeNote { get; set; }
    public double? TotalAmount { get; set; }
    public int? UsageHours { get; set; }
    public int? UsageSessions { get; set; }
}

public class BaocaoAmountOptionDto
{
    public double Amount { get; set; }
    public string Display { get; set; } = string.Empty;
}
