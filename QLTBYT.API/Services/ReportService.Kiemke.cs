using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Helpers;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Report;
using QLTBYT.API.Models.Domain;

namespace QLTBYT.API.Services;

public partial class ReportService
{
    public async Task<PagedResult<KiemkeAssetRowDto>> GetKiemkeAssetsAsync(KiemkeAssetQueryParams query)
    {
        var rows = await BuildKiemkeAssetRowsAsync(query);
        return Paginate(rows, query.Page, query.PageSize);
    }

    public async Task<byte[]> ExportKiemkeAssetsExcelAsync(KiemkeAssetQueryParams query)
    {
        var rows = await BuildKiemkeAssetRowsAsync(query);
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Kiem ke TBYT");
        sheet.Cell(1, 1).Value = "Ten TB";
        sheet.Cell(1, 2).Value = "Ma tai san";
        sheet.Cell(1, 3).Value = "Hang SX";
        sheet.Cell(1, 4).Value = "Model";
        sheet.Cell(1, 5).Value = "Serial";
        sheet.Cell(1, 6).Value = "So luong";
        sheet.Cell(1, 7).Value = "DVT";
        sheet.Cell(1, 8).Value = "Khoa phong";
        sheet.Cell(1, 9).Value = "Vi tri";
        sheet.Cell(1, 10).Value = "Nguon mua sam";
        sheet.Cell(1, 11).Value = "Nguyen gia";
        sheet.Cell(1, 12).Value = "Ngay tiep nhan";
        sheet.Cell(1, 13).Value = "Khau hao";
        sheet.Cell(1, 14).Value = "Ghi chu";
        var r = 2;
        foreach (var row in rows)
        {
            sheet.Cell(r, 1).Value = row.Name;
            sheet.Cell(r, 2).Value = row.IdentityCode;
            sheet.Cell(r, 3).Value = row.Manufacturer;
            sheet.Cell(r, 4).Value = row.ModelCode;
            sheet.Cell(r, 5).Value = row.Serial;
            sheet.Cell(r, 6).Value = row.Quantity;
            sheet.Cell(r, 7).Value = row.Unit;
            sheet.Cell(r, 8).Value = row.OrganizationName;
            sheet.Cell(r, 9).Value = row.LocationCode;
            sheet.Cell(r, 10).Value = row.ProcurementSourceLabel;
            sheet.Cell(r, 11).Value = row.TotalAmount;
            sheet.Cell(r, 12).Value = row.ReceivedDate?.ToString("dd/MM/yyyy");
            sheet.Cell(r, 13).Value = row.DepreciationAmount;
            sheet.Cell(r, 14).Value = row.Note;
            r++;
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private async Task<List<KiemkeAssetRowDto>> BuildKiemkeAssetRowsAsync(KiemkeAssetQueryParams query)
    {
        IQueryable<Device> queryable = ScopedDevices()
            .Include(d => d.Organization)
            .Where(d => d.TotalAmount != null && d.TotalAmount.Trim() != string.Empty);

        if (query.OrganizationId is > 0)
            queryable = queryable.Where(d => d.OrganizationId == query.OrganizationId);

        var procurement = NormalizeOptionalFilter(query.ProcurementSource);
        if (!string.IsNullOrEmpty(procurement))
            queryable = queryable.Where(d => d.ProcurementSource == procurement);

        if (query.ReceivedYear is > 1900)
            queryable = queryable.Where(d => d.ReceivedDate != null && d.ReceivedDate.Value.Year == query.ReceivedYear);

        queryable = ApplyLocationFilter(queryable, NormalizeOptionalFilter(query.LocationCode));

        var devices = await queryable.OrderBy(d => d.Name).ToListAsync();

        IEnumerable<Device> filtered = devices;
        if (query.ExcludeZeroAmount)
            filtered = filtered.Where(BaocaoHelper.HasAssetValue);

        if (query.MinAmount is > 0)
            filtered = filtered.Where(d => FinancialParseHelper.ParseLegacyAmount(d.TotalAmount) >= query.MinAmount.Value);

        if (query.MaxAmount is > 0)
            filtered = filtered.Where(d => FinancialParseHelper.ParseLegacyAmount(d.TotalAmount) <= query.MaxAmount.Value);

        return filtered.Select(MapKiemkeRow).ToList();
    }

    private static string? NormalizeOptionalFilter(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        if (string.Equals(trimmed, "null", StringComparison.OrdinalIgnoreCase)) return null;
        return trimmed;
    }

    private static KiemkeAssetRowDto MapKiemkeRow(Device device)
    {
        var amount = FinancialParseHelper.ParseLegacyAmount(device.TotalAmount);
        return new KiemkeAssetRowDto
        {
            Id = device.Id,
            Name = device.Name,
            IdentityCode = device.IdentityCode,
            Manufacturer = device.Manufacturer,
            ModelCode = device.ModelCode,
            Serial = device.Serial,
            Quantity = device.Quantity,
            Unit = device.Unit,
            OrganizationName = device.Organization?.Name,
            LocationCode = device.LocationCode,
            ProcurementSourceLabel = BaocaoHelper.ResolveProcurementLabel(device.ProcurementSource),
            TotalAmount = amount,
            ReceivedDate = device.ReceivedDate,
            DepreciationAmount = BaocaoHelper.CalculateDepreciation(amount, device.ReceivedDate),
            Note = device.Note
        };
    }
}
