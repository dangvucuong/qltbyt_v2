namespace QLTBYT.API.Models.DTOs.Legal;

public class LegalDocumentDto
{
    public int Id { get; set; }
    public string? InspectionCertificate { get; set; }
    public string? ImportPermit { get; set; }
    public string? ImportPermitExpiry { get; set; }
    public string? CirculationPermit { get; set; }
    public string? CirculationPermitExpiry { get; set; }
    public string? CertificateOfOrigin { get; set; }
    public string? OriginCountryId { get; set; }
    public string? QualityCertificate { get; set; }
    public string? ManufactureYear { get; set; }
    public string? CustomsDeclaration { get; set; }
    public string? PackingList { get; set; }
    public string? BillOfLading { get; set; }
    public string? ImportInvoice { get; set; }
    public string? SalesInvoice { get; set; }
    public string? FilePath { get; set; }
    public string? BiddingInfo { get; set; }
    public double? PriceWithVat { get; set; }
    public double? PriceWithoutVat { get; set; }
    public string? DepreciationInfo { get; set; }
    public string? FundingSource { get; set; }

    /// <summary>Giá trị gốc cột MaThietbi (thường là ID thiết bị).</summary>
    public string? DeviceCodes { get; set; }

    /// <summary>Tên hiển thị thiết bị (giống thongtinhs.aspx CustomColumnDisplayText).</summary>
    public string? DeviceDisplayName { get; set; }

    /// <summary>Mã TB (MaTB) nếu tra được từ ID.</summary>
    public string? DeviceCode { get; set; }
}

public class CreateLegalDocumentRequest
{
    public string? InspectionCertificate { get; set; }
    public string? ImportPermit { get; set; }
    public string? ImportPermitExpiry { get; set; }
    public string? CirculationPermit { get; set; }
    public string? CirculationPermitExpiry { get; set; }
    public string? CertificateOfOrigin { get; set; }
    public string? OriginCountryId { get; set; }
    public string? QualityCertificate { get; set; }
    public string? ManufactureYear { get; set; }
    public string? CustomsDeclaration { get; set; }
    public string? PackingList { get; set; }
    public string? BillOfLading { get; set; }
    public string? ImportInvoice { get; set; }
    public string? SalesInvoice { get; set; }
    public string? BiddingInfo { get; set; }
    public double? PriceWithVat { get; set; }
    public double? PriceWithoutVat { get; set; }
    public string? DepreciationInfo { get; set; }
    public string? FundingSource { get; set; }

    /// <summary>ID thiết bị hoặc mã TB (legacy lưu ID).</summary>
    public string? DeviceCodes { get; set; }
}

public class UpdateLegalDocumentRequest : CreateLegalDocumentRequest
{
}

public class LinkDeviceLegalRequest
{
    public int? LegalDocumentId { get; set; }
}
