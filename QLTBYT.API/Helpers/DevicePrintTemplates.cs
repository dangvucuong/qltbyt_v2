using QLTBYT.API.Models.Domain;

namespace QLTBYT.API.Helpers;

public static class DevicePrintTemplates
{
    public static string BuildRepairSlip(Device device, string? organizationName)
    {
        var now = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        return "<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><title>Phieu sua chua</title>" +
            "<style>body{font-family:Segoe UI,Arial,sans-serif;padding:24px} h1{font-size:18px} table{width:100%;border-collapse:collapse} td,th{border:1px solid #ccc;padding:8px}</style></head><body>" +
            "<h1>PHIEU YEU CAU / SUA CHUA THIET BI</h1><p>In luc: " + now + "</p><table>" +
            "<tr><th>Ma TB</th><td>" + device.DeviceCode + "</td><th>Don vi</th><td>" + organizationName + "</td></tr>" +
            "<tr><th>Ten TB</th><td colspan=\"3\">" + device.Name + "</td></tr>" +
            "<tr><th>Ky hieu</th><td>" + device.ModelCode + "</td><th>Serial</th><td>" + device.Serial + "</td></tr>" +
            "<tr><th>Hang SX</th><td>" + device.Manufacturer + "</td><th>Tinh trang</th><td>" + device.ConditionNote + "</td></tr></table>" +
            "<p><strong>Noi dung yeu cau / sua chua:</strong></p><div style=\"min-height:120px;border:1px solid #ccc;padding:8px\"></div>" +
            "<p>Ky thuat vien: _______________ &nbsp; Don vi nhan: _______________</p></body></html>";
    }

    public static string BuildInspectionChecklist(Device device, string? organizationName)
    {
        var now = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        var insp = device.InspectionExpiryDate?.ToString("dd/MM/yyyy") ?? "";
        return "<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><title>Bien ban kiem tra</title>" +
            "<style>body{font-family:Segoe UI,Arial,sans-serif;padding:24px} h1{font-size:18px} table{width:100%;border-collapse:collapse} td,th{border:1px solid #ccc;padding:8px}</style></head><body>" +
            "<h1>BIEN BAN KIEM TRA / KIEM DINH THIET BI</h1><p>In luc: " + now + "</p><table>" +
            "<tr><th>Ma TB</th><td>" + device.DeviceCode + "</td><th>Don vi</th><td>" + organizationName + "</td></tr>" +
            "<tr><th>Ten TB</th><td colspan=\"3\">" + device.Name + "</td></tr>" +
            "<tr><th>Han kiem dinh</th><td>" + insp + "</td><th>Moc bao tri</th><td>" + device.MaintenanceDueMarker + "</td></tr></table>" +
            "<p><strong>Ket qua kiem tra:</strong></p><div style=\"min-height:100px;border:1px solid #ccc;padding:8px\"></div></body></html>";
    }
}