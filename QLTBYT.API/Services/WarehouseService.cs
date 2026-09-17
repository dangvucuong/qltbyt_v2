using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.Domain.Warehouse;
using QLTBYT.API.Models.DTOs.Warehouse;

namespace QLTBYT.API.Services;

public interface IWarehouseService
{
    Task<List<CommodityDto>> GetCommoditiesAsync();
    Task<CommodityDto> CreateCommodityAsync(CreateCommodityRequest request);
    Task<List<BatchDto>> GetBatchesAsync();
    Task<BatchDto> CreateBatchAsync(CreateBatchRequest request);
    Task<List<StockImportDto>> GetImportsAsync();
    Task<StockImportDto> ImportStockAsync(CreateStockImportRequest request);
    Task<List<StockExportDto>> GetExportsAsync();
    Task<StockExportDto> ExportStockAsync(CreateStockExportRequest request);
    Task<WarehouseReportDto> GetReportAsync();
    Task<byte[]> ExportStockReportExcelAsync(DateTime from, DateTime to);
}

public class WarehouseService : IWarehouseService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public WarehouseService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<CommodityDto>> GetCommoditiesAsync()
    {
        var commodities = await _context.MedicineCommodities.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        var balances = await _context.StockImports.AsNoTracking()
            .GroupBy(i => i.CommodityId)
            .Select(g => new { CommodityId = g.Key, Stock = g.Sum(x => x.StockBalance ?? 0) })
            .ToListAsync();
        var map = balances.ToDictionary(x => x.CommodityId, x => x.Stock);
        return commodities.Select(c => new CommodityDto
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name,
            ActiveIngredient = c.ActiveIngredient,
            StockOnHand = map.GetValueOrDefault(c.Id)
        }).ToList();
    }

    public async Task<CommodityDto> CreateCommodityAsync(CreateCommodityRequest request)
    {
        var nextId = await _context.MedicineCommodities.MaxAsync(c => (int?)c.Id) ?? 0;
        var entity = new MedicineCommodity
        {
            Id = nextId + 1,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            ActiveIngredient = request.ActiveIngredient?.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = _currentUser.UserId
        };
        _context.MedicineCommodities.Add(entity);
        await _context.SaveChangesAsync();
        return new CommodityDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            ActiveIngredient = entity.ActiveIngredient,
            StockOnHand = 0
        };
    }

    public async Task<List<BatchDto>> GetBatchesAsync()
    {
        return await _context.MedicineBatches.AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BatchDto
            {
                Id = b.Id,
                BatchCode = b.BatchCode,
                SupplierId = b.SupplierId,
                CreatedAt = b.CreatedAt
            }).ToListAsync();
    }

    public async Task<BatchDto> CreateBatchAsync(CreateBatchRequest request)
    {
        var nextId = await _context.MedicineBatches.MaxAsync(b => (int?)b.Id) ?? 0;
        var entity = new MedicineBatch
        {
            Id = nextId + 1,
            BatchCode = request.BatchCode.Trim(),
            SupplierId = request.SupplierId,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = _currentUser.UserId
        };
        _context.MedicineBatches.Add(entity);
        await _context.SaveChangesAsync();
        return new BatchDto { Id = entity.Id, BatchCode = entity.BatchCode, SupplierId = entity.SupplierId, CreatedAt = entity.CreatedAt };
    }

    public async Task<List<StockImportDto>> GetImportsAsync()
    {
        var imports = await _context.StockImports.AsNoTracking().OrderByDescending(i => i.CreatedAt).Take(200).ToListAsync();
        var commodityNames = await _context.MedicineCommodities.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.Name ?? "");
        var batchCodes = await _context.MedicineBatches.AsNoTracking().ToDictionaryAsync(b => b.Id, b => b.BatchCode ?? "");
        return imports.Select(i => new StockImportDto
        {
            Id = i.Id,
            CommodityId = i.CommodityId,
            CommodityName = commodityNames.GetValueOrDefault(i.CommodityId),
            BatchId = i.BatchId,
            BatchCode = i.BatchId.HasValue ? batchCodes.GetValueOrDefault(i.BatchId.Value) : null,
            Quantity = i.Quantity,
            StockBalance = i.StockBalance,
            ExpiryDate = i.ExpiryDate,
            CreatedAt = i.CreatedAt
        }).ToList();
    }

    public async Task<StockImportDto> ImportStockAsync(CreateStockImportRequest request)
    {
        if (request.Quantity <= 0) throw new InvalidOperationException("So luong nhap phai lon hon 0.");
        var nextId = await _context.StockImports.MaxAsync(i => (int?)i.Id) ?? 0;
        var entity = new StockImport
        {
            Id = nextId + 1,
            CommodityId = request.CommodityId,
            BatchId = request.BatchId,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            ExpiryDate = request.ExpiryDate,
            StockBalance = request.Quantity,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = _currentUser.UserId
        };
        _context.StockImports.Add(entity);
        await _context.SaveChangesAsync();
        var name = await _context.MedicineCommodities.AsNoTracking().Where(c => c.Id == entity.CommodityId).Select(c => c.Name).FirstOrDefaultAsync();
        return new StockImportDto
        {
            Id = entity.Id,
            CommodityId = entity.CommodityId,
            CommodityName = name,
            BatchId = entity.BatchId,
            Quantity = entity.Quantity,
            StockBalance = entity.StockBalance,
            ExpiryDate = entity.ExpiryDate,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task<List<StockExportDto>> GetExportsAsync()
    {
        return await _context.StockExports.AsNoTracking()
            .OrderByDescending(e => e.CreatedAt)
            .Take(200)
            .Select(e => new StockExportDto { Id = e.Id, ImportId = e.ImportId, Quantity = e.Quantity, Note = e.Note, CreatedAt = e.CreatedAt })
            .ToListAsync();
    }

    public async Task<StockExportDto> ExportStockAsync(CreateStockExportRequest request)
    {
        if (request.Quantity <= 0) throw new InvalidOperationException("So luong xuat phai lon hon 0.");
        var import = await _context.StockImports.FirstOrDefaultAsync(i => i.Id == request.ImportId);
        if (import is null) throw new InvalidOperationException("Khong tim thay dong nhap kho.");
        var balance = import.StockBalance ?? 0;
        if (request.Quantity > balance) throw new InvalidOperationException("So luong xuat vuot ton kho.");
        import.StockBalance = balance - request.Quantity;
        var nextId = await _context.StockExports.MaxAsync(e => (int?)e.Id) ?? 0;
        var export = new StockExport
        {
            Id = nextId + 1,
            ImportId = request.ImportId,
            Quantity = request.Quantity,
            Note = request.Note?.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = _currentUser.UserId
        };
        _context.StockExports.Add(export);
        await _context.SaveChangesAsync();
        return new StockExportDto { Id = export.Id, ImportId = export.ImportId, Quantity = export.Quantity, Note = export.Note, CreatedAt = export.CreatedAt };
    }

    public async Task<WarehouseReportDto> GetReportAsync()
    {
        var commodities = await _context.MedicineCommodities.CountAsync();
        var stock = await _context.StockImports.SumAsync(i => (int?)i.StockBalance) ?? 0;
        var imports = await _context.StockImports.CountAsync();
        var exports = await _context.StockExports.CountAsync();
        return new WarehouseReportDto
        {
            TotalCommodities = commodities,
            TotalStockUnits = stock,
            ImportLines = imports,
            ExportLines = exports
        };
    }

    public async Task<byte[]> ExportStockReportExcelAsync(DateTime from, DateTime to)
    {
        var end = to.Date.AddDays(1).AddTicks(-1);
        var imports = await _context.StockImports.AsNoTracking()
            .Where(i => i.CreatedAt >= from && i.CreatedAt <= end)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
        var names = await _context.MedicineCommodities.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.Name ?? "");
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var sheet = workbook.Worksheets.Add("Nhap kho");
        sheet.Cell(1, 1).Value = "ID";
        sheet.Cell(1, 2).Value = "Hang hoa";
        sheet.Cell(1, 3).Value = "SL nhap";
        sheet.Cell(1, 4).Value = "Ton";
        sheet.Cell(1, 5).Value = "Ngay";
        int r = 2;
        foreach (var i in imports)
        {
            sheet.Cell(r, 1).Value = i.Id;
            sheet.Cell(r, 2).Value = names.GetValueOrDefault(i.CommodityId);
            sheet.Cell(r, 3).Value = i.Quantity;
            sheet.Cell(r, 4).Value = i.StockBalance;
            sheet.Cell(r, 5).Value = i.CreatedAt?.ToString("dd/MM/yyyy HH:mm");
            r++;
        }
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}