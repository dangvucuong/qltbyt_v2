using System.Net;
using System.Text;

namespace QLTBYT.API.Helpers;

/// <summary>
/// Mẫu in HTML bám layout DevExpress cũ (rpPhieusuachua / prBCKT / rpPhieuNT).
/// </summary>
public static class DevicePrintTemplates
{
    public sealed class PrintModel
    {
        public string HospitalName { get; set; } = "BỆNH VIỆN ĐA KHOA ABC";
        public string DepartmentName { get; set; } = "PHÒNG VẬT TƯ, TRANG THIẾT BỊ Y TẾ";
        public string PrintedAt { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        public string? DeviceName { get; set; }
        public string? ModelCode { get; set; }
        public string? Serial { get; set; }
        public string? Manufacturer { get; set; }
        public string? CountryName { get; set; }
        public string? PutIntoUseDate { get; set; }
        public string? OrganizationName { get; set; }
        public string? StatusName { get; set; }

        // Section 2 — phiếu sửa chữa / yêu cầu (PSC)
        public string? RequestType { get; set; }
        public string? RequestContent { get; set; }
        public string? RequestDateTime { get; set; }
        public string? RequestAccount { get; set; }

        // Section 2 — phiếu ghi nhận xử lý (PNT)
        public string? ProcessType { get; set; } = "Đã xử lý";
        public string? ProcessContent { get; set; }
        public string? ProcessDateTime { get; set; }
        public string? ProcessAccount { get; set; }
    }

    /// <summary>type=0 — rpPhieusuachua — PHIẾU GHI NHẬN TÌNH TRẠNG THIẾT BỊ (…/PSC)</summary>
    public static string BuildRepairSlip(PrintModel m) =>
        BuildDocument(
            title: "PHIẾU GHI NHẬN TÌNH TRẠNG THIẾT BỊ",
            docNo: "Số:…………/PSC",
            showPrintedAt: true,
            m,
            section2Title: "2. Ghi nhận yêu cầu:",
            section2Rows: new[]
            {
                ("Loại yêu cầu:", m.RequestType),
                ("Nội dung yêu cầu:", m.RequestContent),
                ("Ngày giờ yêu cầu:", m.RequestDateTime),
                ("Tài khoản yêu cầu:", m.RequestAccount),
            },
            noteLines: 5,
            signatureLeft: ("Đại diện khoa/phòng sử dụng", "(Ký ghi rõ họ tên)"),
            signatureRight: ("Đại diện phòng vật tư, trang thiết bị y tế", "(Ký ghi rõ họ tên)"));

    /// <summary>type=1 — prBCKT — BIÊN BẢN BÀN GIAO (…/BCKT)</summary>
    public static string BuildHandoverMinutes(PrintModel m) =>
        BuildDocument(
            title: "BIÊN BẢN BÀN GIAO",
            docNo: "Số:…………/BCKT",
            showPrintedAt: false,
            m,
            section2Title: "2. Nội dung bàn giao",
            section2Rows: Array.Empty<(string, string?)>(),
            noteLines: 2,
            signatureLeft: ("BÊN NHẬN", ""),
            signatureRight: ("BÊN GIAO", ""));

    /// <summary>type=2 — rpPhieuNT — PHIẾU GHI NHẬN XỬ LÝ THIẾT BỊ (…/PNT)</summary>
    public static string BuildAcceptanceSlip(PrintModel m) =>
        BuildDocument(
            title: "PHIẾU GHI NHẬN XỬ LÝ THIẾT BỊ",
            docNo: "Số:…………/PNT",
            showPrintedAt: true,
            m,
            section2Title: "2. Ghi nhận xử lý:",
            section2Rows: new[]
            {
                ("Loại xử lý:", m.ProcessType),
                ("Nội dung xử lý:", m.ProcessContent),
                ("Ngày giờ xử lý:", m.ProcessDateTime),
                ("Tài khoản thực hiện:", m.ProcessAccount),
            },
            noteLines: 5,
            signatureLeft: ("Đại diện khoa/phòng sử dụng", "(Ký ghi rõ họ tên)"),
            signatureRight: ("Đại diện phòng vật tư, trang thiết bị y tế", "(Ký ghi rõ họ tên)"));

    // Giữ tên cũ cho tương thích test/API
    public static string BuildInspectionChecklist(PrintModel m) => BuildHandoverMinutes(m);

    private static string BuildDocument(
        string title,
        string docNo,
        bool showPrintedAt,
        PrintModel m,
        string section2Title,
        IReadOnlyList<(string Label, string? Value)> section2Rows,
        int noteLines,
        (string Title, string Sub) signatureLeft,
        (string Title, string Sub) signatureRight)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html lang=\"vi\"><head><meta charset=\"utf-8\"/>");
        sb.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"/>");
        sb.Append("<title>").Append(Html(title)).Append("</title>");
        sb.Append("<style>");
        sb.Append("@page{size:A4;margin:18mm 16mm}");
        sb.Append("body{font-family:'Times New Roman',Times,serif;font-size:13pt;color:#000;margin:0;padding:12px 20px;background:#fff}");
        sb.Append(".sheet{max-width:190mm;margin:0 auto}");
        sb.Append(".header{text-align:center;margin-bottom:8px}");
        sb.Append(".header .org{font-size:14pt;font-weight:700;text-transform:uppercase;letter-spacing:.3px}");
        sb.Append(".header .dept{font-size:12.5pt;font-weight:700;margin-top:2px}");
        sb.Append(".header .line{margin:4px 0 6px;letter-spacing:2px}");
        sb.Append(".header .docno{font-size:12pt}");
        sb.Append(".header .printed{font-size:11pt;margin-top:2px}");
        sb.Append(".title{text-align:center;font-size:16pt;font-weight:700;text-transform:uppercase;margin:14px 0 16px}");
        sb.Append(".sec{font-weight:700;margin:14px 0 8px}");
        sb.Append(".row{display:flex;gap:8px;margin:3px 0;line-height:1.45;align-items:baseline}");
        sb.Append(".row .lbl{min-width:210px;flex:0 0 210px}");
        sb.Append(".row .val{flex:1;border-bottom:1px dotted #666;min-height:1.2em;padding-left:4px}");
        sb.Append(".note-label{margin-top:10px}");
        sb.Append(".dots{border-bottom:1px dotted #666;height:22px;margin:4px 0}");
        sb.Append(".signs{display:flex;justify-content:space-between;gap:24px;margin-top:36px;text-align:center}");
        sb.Append(".signs .box{flex:1}");
        sb.Append(".signs .role{font-weight:700}");
        sb.Append(".signs .hint{font-style:italic;font-size:11.5pt;margin-top:2px}");
        sb.Append(".signs .space{height:72px}");
        sb.Append(".toolbar{position:sticky;top:0;background:#f8fafc;border:1px solid #e2e8f0;padding:8px 12px;margin:-12px -20px 16px;display:flex;gap:8px;justify-content:flex-end}");
        sb.Append(".toolbar button{font:12pt 'Segoe UI',Arial,sans-serif;padding:6px 14px;cursor:pointer}");
        sb.Append("@media print{.toolbar{display:none!important} body{padding:0}}");
        sb.Append("</style></head><body><div class=\"sheet\">");
        sb.Append("<div class=\"toolbar\"><button type=\"button\" onclick=\"window.print()\">In phiếu</button>");
        sb.Append("<button type=\"button\" onclick=\"window.close()\">Đóng</button></div>");

        sb.Append("<div class=\"header\">");
        sb.Append("<div class=\"org\">").Append(Html(m.HospitalName)).Append("</div>");
        sb.Append("<div class=\"dept\">").Append(Html(m.DepartmentName)).Append("</div>");
        sb.Append("<div class=\"line\">------------</div>");
        sb.Append("<div class=\"docno\">").Append(Html(docNo)).Append("</div>");
        if (showPrintedAt)
            sb.Append("<div class=\"printed\">Thời gian in phiếu: ").Append(Html(m.PrintedAt)).Append("</div>");
        sb.Append("</div>");

        sb.Append("<div class=\"title\">").Append(Html(title)).Append("</div>");

        sb.Append("<div class=\"sec\">1. Thông tin thiết bị</div>");
        AppendRow(sb, "Tên thiết bị:", m.DeviceName);
        AppendRow(sb, "Ký hiệu:", m.ModelCode);
        AppendRow(sb, "Số seri:", m.Serial);
        AppendRow(sb, "Hãng sản xuất:", m.Manufacturer);
        AppendRow(sb, "Nước sản xuất:", m.CountryName);
        AppendRow(sb, "Thời gian bắt đầu sử dụng:", m.PutIntoUseDate);
        AppendRow(sb, "Khoa/phòng sử dụng:", m.OrganizationName);
        AppendRow(sb, "Tình trạng hiện nay:", m.StatusName);

        sb.Append("<div class=\"sec\">").Append(Html(section2Title)).Append("</div>");
        foreach (var (label, value) in section2Rows)
            AppendRow(sb, label, value);

        sb.Append("<div class=\"note-label\">Ghi chú: ");
        sb.Append("<span style=\"border-bottom:1px dotted #666;display:inline-block;min-width:70%\">&nbsp;</span></div>");
        for (var i = 0; i < Math.Max(0, noteLines - 1); i++)
            sb.Append("<div class=\"dots\"></div>");

        sb.Append("<div class=\"signs\">");
        sb.Append("<div class=\"box\"><div class=\"role\">").Append(Html(signatureLeft.Title)).Append("</div>");
        if (!string.IsNullOrEmpty(signatureLeft.Sub))
            sb.Append("<div class=\"hint\">").Append(Html(signatureLeft.Sub)).Append("</div>");
        sb.Append("<div class=\"space\"></div><div>......................................................</div></div>");
        sb.Append("<div class=\"box\"><div class=\"role\">").Append(Html(signatureRight.Title)).Append("</div>");
        if (!string.IsNullOrEmpty(signatureRight.Sub))
            sb.Append("<div class=\"hint\">").Append(Html(signatureRight.Sub)).Append("</div>");
        sb.Append("<div class=\"space\"></div><div>......................................................</div></div>");
        sb.Append("</div>");

        sb.Append("</div></body></html>");
        return sb.ToString();
    }

    private static void AppendRow(StringBuilder sb, string label, string? value)
    {
        sb.Append("<div class=\"row\"><div class=\"lbl\">").Append(Html(label)).Append("</div>");
        sb.Append("<div class=\"val\">").Append(Html(value)).Append("</div></div>");
    }

    private static string Html(string? value) =>
        WebUtility.HtmlEncode(value ?? string.Empty);
}
