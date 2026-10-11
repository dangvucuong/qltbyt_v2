using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

/// <summary>
/// Bảng ThongtinHS — hồ sơ pháp lý.
/// Lưu ý legacy: cột MaThietbi thường lưu <b>ID thiết bị</b> (Quanlythietbi.ID), không phải MaTB.
/// </summary>
[Table("ThongtinHS")]
public class LegalDocument
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Magiaykiemdinh")]
    public string? InspectionCertificate { get; set; }

    [Column("Magiaynhapkhau")]
    public string? ImportPermit { get; set; }

    [Column("Thoihangiaynhapkhau")]
    public string? ImportPermitExpiry { get; set; }

    [Column("Magiayphepluuhanh")]
    public string? CirculationPermit { get; set; }

    [Column("Thoihangiayphepluuhanh")]
    public string? CirculationPermitExpiry { get; set; }

    [Column("MagiayCO")]
    public string? CertificateOfOrigin { get; set; }

    [Column("XuatxuCO")]
    public string? OriginCountryId { get; set; }

    [Column("MaCQ")]
    public string? QualityCertificate { get; set; }

    [Column("NamsxCQ")]
    public string? ManufactureYear { get; set; }

    [Column("MatokhaiHQ")]
    public string? CustomsDeclaration { get; set; }

    [Column("Packinglist")]
    public string? PackingList { get; set; }

    [Column("Mavandon")]
    public string? BillOfLading { get; set; }

    [Column("MaInvoiceNK")]
    public string? ImportInvoice { get; set; }

    [Column("MahoadonBH")]
    public string? SalesInvoice { get; set; }

    [Column("FileHS")]
    public string? FilePath { get; set; }

    [Column("Thongtingoithau")]
    public string? BiddingInfo { get; set; }

    [Column("GiacoVAT")]
    public double? PriceWithVat { get; set; }

    [Column("GiatruVAT")]
    public double? PriceWithoutVat { get; set; }

    [Column("Thongtinkhauhao")]
    public string? DepreciationInfo { get; set; }

    [Column("Nguonvon")]
    public string? FundingSource { get; set; }

    /// <summary>Legacy: thường là ID thiết bị (số), đôi khi '0' hoặc danh sách.</summary>
    [Column("MaThietbi")]
    public string? DeviceCodes { get; set; }
}
