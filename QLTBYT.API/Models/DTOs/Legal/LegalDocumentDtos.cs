namespace QLTBYT.API.Models.DTOs.Legal;

public class LegalDocumentDto
{
    public int Id { get; set; }
    public string? InspectionCertificate { get; set; }
    public string? ImportPermit { get; set; }
    public string? CirculationPermit { get; set; }
    public string? FilePath { get; set; }
    public string? BiddingInfo { get; set; }
    public double? PriceWithVat { get; set; }
    public string? DeviceCodes { get; set; }
}

public class CreateLegalDocumentRequest
{
    public string? InspectionCertificate { get; set; }
    public string? ImportPermit { get; set; }
    public string? CirculationPermit { get; set; }
    public string? BiddingInfo { get; set; }
    public double? PriceWithVat { get; set; }
    public string? DeviceCodes { get; set; }
}

public class UpdateLegalDocumentRequest
{
    public string? InspectionCertificate { get; set; }
    public string? ImportPermit { get; set; }
    public string? CirculationPermit { get; set; }
    public string? BiddingInfo { get; set; }
    public double? PriceWithVat { get; set; }
    public string? DeviceCodes { get; set; }
}

public class LinkDeviceLegalRequest
{
    public int? LegalDocumentId { get; set; }
}