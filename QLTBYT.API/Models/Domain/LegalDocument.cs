using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

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

    [Column("Magiayphepluuhanh")]
    public string? CirculationPermit { get; set; }

    [Column("FileHS")]
    public string? FilePath { get; set; }

    [Column("Thongtingoithau")]
    public string? BiddingInfo { get; set; }

    [Column("GiacoVAT")]
    public double? PriceWithVat { get; set; }

    [Column("MaThietbi")]
    public string? DeviceCodes { get; set; }
}
