namespace QLTBYT.API.Models.DTOs.Warehouse;

public class CommodityDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? ActiveIngredient { get; set; }
    public int StockOnHand { get; set; }
}

public class CreateCommodityRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ActiveIngredient { get; set; }
}

public class BatchDto
{
    public int Id { get; set; }
    public string? BatchCode { get; set; }
    public int? SupplierId { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateBatchRequest
{
    public string BatchCode { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
}

public class StockImportDto
{
    public int Id { get; set; }
    public int CommodityId { get; set; }
    public string? CommodityName { get; set; }
    public int? BatchId { get; set; }
    public string? BatchCode { get; set; }
    public int? Quantity { get; set; }
    public int? StockBalance { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateStockImportRequest
{
    public int CommodityId { get; set; }
    public int? BatchId { get; set; }
    public int Quantity { get; set; }
    public int? UnitPrice { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class CreateStockExportRequest
{
    public int ImportId { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
}

public class StockExportDto
{
    public int Id { get; set; }
    public int ImportId { get; set; }
    public int? Quantity { get; set; }
    public string? Note { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class WarehouseReportDto
{
    public int TotalCommodities { get; set; }
    public int TotalStockUnits { get; set; }
    public int ImportLines { get; set; }
    public int ExportLines { get; set; }
}