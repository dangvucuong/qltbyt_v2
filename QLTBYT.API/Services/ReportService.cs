using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Helpers;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Models.DTOs.Report;
using QLTBYT.API.Models.Domain;

namespace QLTBYT.API.Services;

public interface IReportService
{
    Task<DeviceReportSummaryDto> GetDeviceReportSummaryAsync(int expiringWithinDays = 30);
    Task<PagedResult<DeviceReportRowDto>> GetDeviceReportGridAsync(DeviceQueryParams query);
    Task<byte[]> ExportDevicesExcelAsync();
    Task<byte[]> ExportExpiringInspectionExcelAsync(int withinDays = 90);
    Task<byte[]> ExportAssetsExcelAsync();
    Task<byte[]> ExportEventsExcelAsync();
    Task<string?> GetDeviceRepairPrintHtmlAsync(int deviceId);
    Task<string?> GetDeviceInspectionPrintHtmlAsync(int deviceId);
    Task<PagedResult<BaocaoAssetRowDto>> GetBaocaoAssetStatisticsAsync(BaocaoAssetQueryParams query);
    Task<PagedResult<BaocaoOperationalRowDto>> GetBaocaoOperationalStatisticsAsync(BaocaoOperationalQueryParams query);
    Task<List<BaocaoAmountOptionDto>> GetBaocaoUsageAmountOptionsAsync(DateTime from, DateTime to);
    Task<byte[]> ExportBaocaoAssetStatisticsExcelAsync(BaocaoAssetQueryParams query);
    Task<byte[]> ExportBaocaoOperationalStatisticsExcelAsync(BaocaoOperationalQueryParams query);
    Task<PagedResult<KiemkeAssetRowDto>> GetKiemkeAssetsAsync(KiemkeAssetQueryParams query);
    Task<byte[]> ExportKiemkeAssetsExcelAsync(KiemkeAssetQueryParams query);
}

public partial class ReportService : IReportService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ReportService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<DeviceReportSummaryDto> GetDeviceReportSummaryAsync(int expiringWithinDays = 30)
    {
        var q = ScopedDevices();
        var list = await q.Select(d => new { d.StatusId, d.InspectionExpiryDate, d.MaintenanceDueMarker }).ToListAsync();
        var thresholdInspect = DateTime.Today.AddDays(expiringWithinDays);
        var thresholdMaint = DateTime.Today.AddDays(expiringWithinDays);
        var total = list.Count;
        var expiring = list.Count(d => d.InspectionExpiryDate != null && d.InspectionExpiryDate <= thresholdInspect);
        var maintenanceDue = list.Count(d =>
        {
            var m = FinancialParseHelper.ParseLegacyDate(d.MaintenanceDueMarker);
            return m != null && m <= thresholdMaint;
        });
        var active = list.Count(d => d.StatusId == 1);
        var inactive = list.Count(d => d.StatusId == 2);
        return new DeviceReportSummaryDto
        {
            TotalDevices = total,
            ExpiringInspectionCount = expiring,
            MaintenanceDueCount = maintenanceDue,
            ByStatusActive = active,
            ByStatusInactive = inactive
        };
    }

    public async Task<PagedResult<DeviceReportRowDto>> GetDeviceReportGridAsync(DeviceQueryParams query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;
        IQueryable<Device> queryable = ScopedDevices();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            queryable = queryable.Where(d =>
                (d.Name != null && d.Name.Contains(term)) ||
                (d.DeviceCode != null && d.DeviceCode.Contains(term)) ||
                (d.IdentityCode != null && d.IdentityCode.Contains(term)));
        }
        if (query.OrganizationId.HasValue) queryable = queryable.Where(d => d.OrganizationId == query.OrganizationId);
        if (query.StatusId.HasValue) queryable = queryable.Where(d => d.StatusId == query.StatusId);
        if (query.TypeId.HasValue) queryable = queryable.Where(d => d.TypeId == query.TypeId);
        if (query.ExpiringWithinDays is > 0)
        {
            var threshold = DateTime.Today.AddDays(query.ExpiringWithinDays.Value);
            queryable = queryable.Where(d => d.InspectionExpiryDate != null && d.InspectionExpiryDate <= threshold);
        }

        var total = await queryable.CountAsync();

        var sortRows = await queryable.Select(d => new { d.Id, d.DeviceCode }).ToListAsync();
        var pageIds = sortRows
            .OrderBy(x => x.DeviceCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.Id)
            .ToList();

        if (pageIds.Count == 0)
        {
            return new PagedResult<DeviceReportRowDto>
            {
                Items = new List<DeviceReportRowDto>(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        var rows = await queryable
            .Include(d => d.Organization)
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceStatus)
            .Where(d => pageIds.Contains(d.Id))
            .ToListAsync();

        rows = pageIds.Select(id => rows.First(r => r.Id == id)).ToList();

        var items = rows.Select(d => new DeviceReportRowDto
        {
            Id = d.Id,
            DeviceCode = d.DeviceCode,
            IdentityCode = d.IdentityCode,
            Name = d.Name,
            TypeName = d.DeviceType?.Name,
            OrganizationName = d.Organization?.Name,
            StatusName = d.DeviceStatus?.Name,
            Manufacturer = d.Manufacturer,
            ModelCode = d.ModelCode,
            Serial = d.Serial,
            ReceivedDate = d.ReceivedDate,
            InspectionExpiryDate = d.InspectionExpiryDate,
            MaintenanceDueDate = FinancialParseHelper.ParseLegacyDate(d.MaintenanceDueMarker),
            Classification = d.Classification,
            TotalAmount = FinancialParseHelper.ParseLegacyAmount(d.TotalAmount)
        }).ToList();

        return new PagedResult<DeviceReportRowDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<byte[]> ExportDevicesExcelAsync()
    {
        var rows = await ScopedDevices()
            .Include(d => d.Organization)
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceStatus)
            .OrderBy(d => d.DeviceCode)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Thiet bi");
        sheet.Cell(1, 1).Value = "Ma TB";
        sheet.Cell(1, 2).Value = "Ten TB";
        sheet.Cell(1, 3).Value = "Loai";
        sheet.Cell(1, 4).Value = "Don vi";
        sheet.Cell(1, 5).Value = "Trang thai";
        sheet.Cell(1, 6).Value = "Han kiem dinh";
        sheet.Cell(1, 7).Value = "Thanh tien";
        sheet.Cell(1, 8).Value = "Moc bao tri";
        int r = 2;
        foreach (var d in rows)
        {
            sheet.Cell(r, 1).Value = d.DeviceCode;
            sheet.Cell(r, 2).Value = d.Name;
            sheet.Cell(r, 3).Value = d.DeviceType?.Name;
            sheet.Cell(r, 4).Value = d.Organization?.Name;
            sheet.Cell(r, 5).Value = d.DeviceStatus?.Name;
            sheet.Cell(r, 6).Value = d.InspectionExpiryDate?.ToString("dd/MM/yyyy");
            sheet.Cell(r, 7).Value = d.TotalAmount;
            sheet.Cell(r, 8).Value = d.MaintenanceDueMarker;
            r++;
        }
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportExpiringInspectionExcelAsync(int withinDays = 90)
    {
        var threshold = DateTime.Today.AddDays(withinDays);
        var rows = await ScopedDevices()
            .Where(d => d.InspectionExpiryDate != null && d.InspectionExpiryDate <= threshold)
            .OrderBy(d => d.InspectionExpiryDate)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Sap het han KD");
        sheet.Cell(1, 1).Value = "Ma TB";
        sheet.Cell(1, 2).Value = "Ten TB";
        sheet.Cell(1, 3).Value = "Han kiem dinh";
        int r = 2;
        foreach (var d in rows)
        {
            sheet.Cell(r, 1).Value = d.DeviceCode;
            sheet.Cell(r, 2).Value = d.Name;
            sheet.Cell(r, 3).Value = d.InspectionExpiryDate?.ToString("dd/MM/yyyy");
            r++;
        }
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportAssetsExcelAsync()
    {
        var rows = await ScopedDevices()
            .Include(d => d.Organization)
            .Include(d => d.DeviceType)
            .OrderBy(d => d.DeviceCode)
            .ToListAsync();
        var legalList = await _context.LegalDocuments.AsNoTracking().ToListAsync();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Tai san TB");
        sheet.Cell(1, 1).Value = "Ma TB";
        sheet.Cell(1, 2).Value = "Ten TB";
        sheet.Cell(1, 3).Value = "Loai";
        sheet.Cell(1, 4).Value = "Don vi";
        sheet.Cell(1, 5).Value = "Ngay tiep nhan";
        sheet.Cell(1, 6).Value = "Goi thau";
        sheet.Cell(1, 7).Value = "Gia co VAT (HS)";
        sheet.Cell(1, 8).Value = "Thanh tien";
        int r = 2;
        foreach (var d in rows)
        {
            var legal = ResolveLegalForAsset(d, legalList);
            sheet.Cell(r, 1).Value = d.DeviceCode;
            sheet.Cell(r, 2).Value = d.Name;
            sheet.Cell(r, 3).Value = d.DeviceType?.Name;
            sheet.Cell(r, 4).Value = d.Organization?.Name;
            sheet.Cell(r, 5).Value = d.ReceivedDate?.ToString("dd/MM/yyyy");
            sheet.Cell(r, 6).Value = d.BiddingPackageCode;
            sheet.Cell(r, 7).Value = legal?.PriceWithVat;
            sheet.Cell(r, 8).Value = FinancialParseHelper.ParseLegacyAmount(d.TotalAmount);
            r++;
        }
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportEventsExcelAsync()
    {
        var events = await _context.DeviceEvents.AsNoTracking()
            .OrderByDescending(e => e.EventDateTime)
            .Take(500)
            .ToListAsync();
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Su co");
        sheet.Cell(1, 1).Value = "ID";
        sheet.Cell(1, 2).Value = "TB ID";
        sheet.Cell(1, 3).Value = "Noi dung";
        sheet.Cell(1, 4).Value = "Thoi gian";
        sheet.Cell(1, 5).Value = "Trang thai";
        int r = 2;
        foreach (var e in events)
        {
            sheet.Cell(r, 1).Value = e.Id;
            sheet.Cell(r, 2).Value = e.DeviceId;
            sheet.Cell(r, 3).Value = e.Content;
            sheet.Cell(r, 4).Value = e.EventDateTime?.ToString("dd/MM/yyyy HH:mm");
            sheet.Cell(r, 5).Value = e.StatusId;
            r++;
        }
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<string?> GetDeviceRepairPrintHtmlAsync(int deviceId)
    {
        var d = await ScopedDevices().Include(x => x.Organization).FirstOrDefaultAsync(x => x.Id == deviceId);
        if (d is null) return null;
        return DevicePrintTemplates.BuildRepairSlip(d, d.Organization?.Name);
    }

    public async Task<string?> GetDeviceInspectionPrintHtmlAsync(int deviceId)
    {
        var d = await ScopedDevices().Include(x => x.Organization).FirstOrDefaultAsync(x => x.Id == deviceId);
        if (d is null) return null;
        return DevicePrintTemplates.BuildInspectionChecklist(d, d.Organization?.Name);
    }

    private static LegalDocument? ResolveLegalForAsset(Device d, List<LegalDocument> legalList)
    {
        if (!string.IsNullOrWhiteSpace(d.LegalDocumentRef) && int.TryParse(d.LegalDocumentRef.Trim(), out var id))
        {
            var linked = legalList.FirstOrDefault(l => l.Id == id);
            if (linked is not null) return linked;
        }
        var code = d.DeviceCode?.Trim();
        if (string.IsNullOrEmpty(code)) return null;
        return legalList.FirstOrDefault(l => l.DeviceCodes != null && l.DeviceCodes.Contains(code));
    }

    private IQueryable<Device> ScopedDevices()
    {
        var q = _context.Devices.AsNoTracking();
        if (!_currentUser.MustFilterByOrganization) return q;
        var allowed = _currentUser.OrganizationIds;
        return q.Where(d => d.OrganizationId != null && allowed.Contains(d.OrganizationId.Value));
    }
}