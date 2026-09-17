using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("Goithau")]
public class BiddingPackage
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("idGoithau")]
    public string? PackageCode { get; set; }

    [Column("Tengoithau")]
    public string? Name { get; set; }

    [Column("Tinhtrang")]
    public string? Status { get; set; }

    [Column("Donvitrungthau")]
    public string? WinningVendor { get; set; }

    [Column("MST")]
    public string? TaxCode { get; set; }

    [Column("SoHD")]
    public string? ContractNumber { get; set; }

    [Column("Giatri")]
    public double? Value { get; set; }
}
