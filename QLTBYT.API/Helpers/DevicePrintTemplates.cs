using QLTBYT.API.Models.Domain;

namespace QLTBYT.API.Helpers;

public static class DevicePrintTemplates
{
    public static string BuildRepairSlip(Device device, string? organizationName)
    {
        var now = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        return "<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><title>Phiếu sửa chữa</title>" +
            "<style>body{font-family:Segoe UI,Arial,sans-serif;padding:24px} h1{font-size:18px} table{width:100%;border-collapse:collapse} td,th{border:1px solid #ccc;padding:8px}</style></head><body>" +
            "<h1>PHIẾU YÊU CẦU / SỬA CHỮA THIẾT BỊ</h1><p>In lúc: " + now + "</p><table>" +
            "<tr><th>Mã TB</th><td>" + device.DeviceCode + "</td><th>Đơn vị</th><td>" + organizationName + "</td></tr>" +
            "<tr><th>Tên TB</th><td colspan=\"3\">" + device.Name + "</td></tr>" +
            "<tr><th>Ký hiệu</th><td>" + device.ModelCode + "</td><th>Serial</th><td>" + device.Serial + "</td></tr>" +
            "<tr><th>Hãng SX</th><td>" + device.Manufacturer + "</td><th>Tình trạng</th><td>" + device.ConditionNote + "</td></tr></table>" +
            "<p><strong>Nội dung yêu cầu / sửa chữa:</strong></p><div style=\"min-height:120px;border:1px solid #ccc;padding:8px\"></div>" +
            "<p>Kỹ thuật viên: _______________ &nbsp; Đơn vị nhận: _______________</p></body></html>";
    }

    public static string BuildInspectionChecklist(Device device, string? organizationName)
    {
        var now = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        var insp = device.InspectionExpiryDate?.ToString("dd/MM/yyyy") ?? "";
        return "<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><title>Biên bản kiểm tra</title>" +
            "<style>body{font-family:Segoe UI,Arial,sans-serif;padding:24px} h1{font-size:18px} table{width:100%;border-collapse:collapse} td,th{border:1px solid #ccc;padding:8px}</style></head><body>" +
            "<h1>BIÊN BẢN KIỂM TRA / KIỂM ĐỊNH THIẾT BỊ</h1><p>In lúc: " + now + "</p><table>" +
            "<tr><th>Mã TB</th><td>" + device.DeviceCode + "</td><th>Đơn vị</th><td>" + organizationName + "</td></tr>" +
            "<tr><th>Tên TB</th><td colspan=\"3\">" + device.Name + "</td></tr>" +
            "<tr><th>Hạn kiểm định</th><td>" + insp + "</td><th>Mốc bảo trì</th><td>" + device.MaintenanceDueMarker + "</td></tr></table>" +
            "<p><strong>Kết quả kiểm tra:</strong></p><div style=\"min-height:100px;border:1px solid #ccc;padding:8px\"></div></body></html>";
    }
}
